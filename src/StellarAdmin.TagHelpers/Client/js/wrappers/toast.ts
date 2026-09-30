import type { Toaster, ToastOptions } from "../web-components/sel-toaster";

/** The response header that carries toasts queued on the server during an AJAX request. */
const HEADER_NAME = "SA-Toasts";

type ToastDetails = Omit<ToastOptions, "title" | "type">;
type PromiseMessage<T> = string | ((value: T) => string);

function toaster(): Toaster {
  const el = document.querySelector("sel-toaster") as Toaster | null;
  if (!el) throw new Error("Toaster not found: add <sa-toaster /> to the layout.");

  return el;
}

// The typed shortcuts take a description, as the server's Success(title, description) does,
// or the remaining options.
function typed(type: ToastOptions["type"]) {
  return (title: string, details?: string | ToastDetails) =>
    add({
      ...(typeof details === "string" ? { description: details } : details),
      title,
      type,
    });
}

function add(options: ToastOptions): string {
  return toaster().addToast(options);
}

type ResponseSource = Response | Headers | XMLHttpRequest | string | null | undefined;

function readHeader(source: ResponseSource): string | null {
  if (source == null) return null;
  if (typeof source === "string") return source;
  if (source instanceof XMLHttpRequest) return source.getResponseHeader(HEADER_NAME);
  if (source instanceof Headers) return source.get(HEADER_NAME);

  return source.headers.get(HEADER_NAME);
}

/**
 * Shows toasts in the page's `<sa-toaster>`.
 *
 * ```ts
 * window.stellarAdmin.toast.success("Copied to clipboard");
 *
 * const id = window.stellarAdmin.toast.add({ title: "Uploading…", type: "loading" });
 * window.stellarAdmin.toast.update(id, { title: "Uploaded", type: "success" });
 *
 * // After an AJAX request, show the toasts the server queued for it
 * window.stellarAdmin.toast.fromResponse(response);
 * ```
 */
export const toast = {
  /** Shows a toast and returns its id. */
  add,

  success: typed("success"),
  info: typed("info"),
  warning: typed("warning"),
  error: typed("error"),

  /** Changes an open toast. Options not given keep their current values. */
  update(id: string, options: Partial<ToastOptions>) {
    toaster().updateToast(id, options);
  },

  /** Closes a toast, resolving once its exit animation has finished. */
  close(id: string): Promise<void> {
    return toaster().closeToast(id);
  },

  /**
   * Shows a loading toast until the promise settles, then turns it into a success or error
   * toast. Returns the promise, so it can still be awaited.
   */
  promise<T>(
    promise: Promise<T>,
    messages: { loading: string; success: PromiseMessage<T>; error: PromiseMessage<any> },
  ): Promise<T> {
    const id = add({ title: messages.loading, type: "loading" });
    const settle = <V>(type: "success" | "error", message: PromiseMessage<V>, value: V) =>
      // An undefined duration goes back to the toaster's default, which loading toasts skip.
      toaster().updateToast(id, {
        title: typeof message === "function" ? message(value) : message,
        type,
        duration: undefined,
      });

    promise.then(
      (value) => settle("success", messages.success, value),
      (error) => settle("error", messages.error, error),
    );

    return promise;
  },

  /**
   * Shows the toasts the server sent in the `SA-Toasts` response header and returns their ids.
   * Takes a fetch `Response`, its `Headers`, an `XMLHttpRequest`, or the header's value.
   */
  fromResponse(source: ResponseSource): string[] {
    const header = readHeader(source);
    if (!header) return [];

    let toasts: ToastOptions[];
    try {
      toasts = JSON.parse(header);
    } catch {
      console.warn(`stellar-admin-ui: the ${HEADER_NAME} header is not valid JSON.`);
      return [];
    }

    return Array.isArray(toasts) ? toasts.map(add) : [];
  },
};
