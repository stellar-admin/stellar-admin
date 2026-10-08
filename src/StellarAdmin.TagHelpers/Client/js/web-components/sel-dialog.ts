import { LitElement } from "lit";
import { customElement } from "lit/decorators.js";

@customElement("sel-dialog")
class Dialog extends LitElement {
  createRenderRoot() {
    return this;
  }

  connectedCallback() {
    super.connectedCallback();
    const dialog = this.querySelector("dialog");
    if (!dialog) return;

    // The page stays locked while any modal dialog is open, so closing one stacked over another keeps it locked. The
    // scrollbar is measured only when the lock starts, since the page has none while locked.
    const sync = () => {
      const isOpen = dialog.open;
      const isLocked = document.querySelector("dialog:modal") !== null;
      const { style } = document.body;

      if (isLocked && style.overflow !== "hidden") {
        style.paddingRight = `${window.innerWidth - document.documentElement.clientWidth}px`;
        style.overflow = "hidden";
      } else if (!isLocked) {
        style.overflow = "";
        style.paddingRight = "";
      }

      dialog.toggleAttribute("data-open", isOpen);
      dialog.toggleAttribute("data-closed", !isOpen);
    };

    new MutationObserver(sync).observe(dialog, { attributes: true, attributeFilter: ["open"] });
    sync();
  }
}
