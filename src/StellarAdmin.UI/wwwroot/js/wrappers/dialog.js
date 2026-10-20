/**
 * @template [TData=Record<string, any>]
 * @param {string | HTMLDialogElement} selector
 * @param {{ clearReturnValueOnOpen?: boolean }} [options]
 */
export function dialog(selector, { clearReturnValueOnOpen = true } = {}) {
  const el =
    typeof selector === "string"
      ? /** @type {HTMLDialogElement | null} */ (document.querySelector(selector))
      : selector;

  if (!el) throw new Error(`Dialog not found: ${selector}`);
  if (!(el instanceof HTMLDialogElement)) throw new Error(`Element is not a dialog: ${selector}`);

  /** @type {((result: { confirmed: boolean; returnValue: string; data: TData | null }) => void) | null} */
  let _resolve = null;

  return {
    /** @returns {Promise<{ confirmed: boolean; returnValue: string; data: TData | null }>} */
    showAsync() {
      return new Promise((resolve) => {
        let dismissed = false;
        _resolve = resolve;

        el.addEventListener(
          "cancel",
          () => {
            dismissed = true;
          },
          { once: true },
        );
        el.addEventListener(
          "close",
          () => {
            if (!_resolve) return;
            // A close without a returnValue (a command="close" button, the sheet's X) is a
            // dismissal, not a confirmation: only an explicit non-"cancel" value confirms.
            if (dismissed || el.returnValue === "" || el.returnValue === "cancel") {
              _resolve({ confirmed: false, returnValue: el.returnValue, data: null });
            } else {
              const form = el.querySelector("form");
              const data = form
                ? /** @type {TData} */ (Object.fromEntries(new FormData(form)))
                : null;

              _resolve({ confirmed: true, returnValue: el.returnValue, data });
            }
            _resolve = null;
          },
          { once: true },
        );

        if (clearReturnValueOnOpen == null || clearReturnValueOnOpen) el.returnValue = "";

        el.showModal();
      });
    },
    /** @param {TData} [data] */
    confirm(data) {
      if (!_resolve) throw new Error("No dialog is currently open.");

      if (!data) {
        const form = el.querySelector("form");
        data = form ? /** @type {TData} */ (Object.fromEntries(new FormData(form))) : undefined;
      }

      const resolve = _resolve;
      _resolve = null;

      el.close();

      resolve({ confirmed: true, returnValue: el.returnValue, data: data ?? null });
    },
    cancel() {
      if (!_resolve) throw new Error("No dialog is currently open.");

      const resolve = _resolve;
      _resolve = null;

      el.close();

      resolve({ confirmed: false, returnValue: el.returnValue, data: null });
    },
  };
}
