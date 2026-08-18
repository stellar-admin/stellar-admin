import { LitElement } from "lit";
import { customElement } from "lit/decorators.js";

@customElement("sel-collapsible")
export class Collapsible extends LitElement {
  static override get observedAttributes() {
    return [...super.observedAttributes, "hidden"];
  }

  override createRenderRoot() {
    return this;
  }

  override connectedCallback() {
    super.connectedCallback();
    this.addEventListener("command", this.#onCommand);
    this.#syncState();
  }

  override disconnectedCallback() {
    super.disconnectedCallback();
    this.removeEventListener("command", this.#onCommand);
  }

  override attributeChangedCallback(name: string, old: string | null, value: string | null) {
    super.attributeChangedCallback(name, old, value);
    if (name === "hidden") {
      this.#syncState();
    }
  }

  #syncState() {
    this.dataset.state = this.hidden ? "closed" : "open";
    this.#syncInvokers();
  }

  #syncInvokers() {
    if (!this.id) {
      return;
    }
    // Resolved on every sync rather than cached once, so triggers rendered later (htmx swaps,
    // late-rendered buttons) are kept in step too.
    const expanded = String(!this.hidden);
    for (const invoker of document.querySelectorAll(`[commandfor="${CSS.escape(this.id)}"]`)) {
      invoker.setAttribute("aria-expanded", expanded);
    }
  }

  #onCommand = (event: Event) => {
    switch ((event as Event & { command: string }).command) {
      case "--show":
        this.show();
        break;
      case "--hide":
        this.hide();
        break;
      case "--toggle":
        this.toggle();
        break;
    }
  };

  toggle() {
    this.hidden = !this.hidden;
  }

  show() {
    this.hidden = false;
  }

  hide() {
    this.hidden = true;
  }
}
