/** @typedef {import("../web-components/sel-toaster.js").Toaster} Toaster */
/** @typedef {import("../web-components/sel-toaster.js").ToastOptions} ToastOptions */

/** The response header that carries toasts queued on the server during an AJAX request. */
const HEADER_NAME = "SA-Toasts";

/** @typedef {Omit<ToastOptions, "title" | "type">} ToastDetails */
/**
 * @template T
 * @typedef {string | ((value: T) => string)} PromiseMessage
 */

/** @returns {Toaster} */
function toaster() {
  const el = /** @type {Toaster | null} */ (document.querySelector("sel-toaster"));
  if (!el) throw new Error("Toaster not found: add <sa-toaster /> to the layout.");

  return el;
}

// The typed shortcuts take a description, as the server's Success(title, description) does,
// or the remaining options.
/** @param {ToastOptions["type"]} type */
function typed(type) {
  /**
   * @param {string} title
   * @param {string | ToastDetails} [details]
   */
  const show = (title, details) =>
    add({
      ...(typeof details === "string" ? { description: details } : details),
      title,
      type,
    });

  return show;
}

/**
 * @param {ToastOptions} options
 * @returns {string}
 */
function add(options) {
  return toaster().addToast(options);
}

/** @typedef {Response | Headers | XMLHttpRequest | string | null | undefined} ResponseSource */

/**
 * @param {ResponseSource} source
 * @returns {string | null}
 */
function readHeader(source) {
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

  /**
   * Changes an open toast. Options not given keep their current values.
   * @param {string} id
   * @param {Partial<ToastOptions>} options
   */
  update(id, options) {
    toaster().updateToast(id, options);
  },

  /**
   * Closes a toast, resolving once its exit animation has finished.
   * @param {string} id
   * @returns {Promise<void>}
   */
  close(id) {
    return toaster().closeToast(id);
  },

  /**
   * Shows a loading toast until the promise settles, then turns it into a success or error
   * toast. Returns the promise, so it can still be awaited.
   * @template T
   * @param {Promise<T>} promise
   * @param {{ loading: string; success: PromiseMessage<T>; error: PromiseMessage<any> }} messages
   * @returns {Promise<T>}
   */
  promise(promise, messages) {
    const id = add({ title: messages.loading, type: "loading" });
    /**
     * @template V
     * @param {"success" | "error"} type
     * @param {PromiseMessage<V>} message
     * @param {V} value
     */
    const settle = (type, message, value) =>
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
   * @param {ResponseSource} source
   * @returns {string[]}
   */
  fromResponse(source) {
    const header = readHeader(source);
    if (!header) return [];

    /** @type {ToastOptions[]} */
    let toasts;
    try {
      toasts = JSON.parse(header);
    } catch {
      console.warn(`stellar-admin-ui: the ${HEADER_NAME} header is not valid JSON.`);
      return [];
    }

    return Array.isArray(toasts) ? toasts.map(add) : [];
  },
};
