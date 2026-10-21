export class Collapsible extends HTMLElement {
  static get observedAttributes() {
    return ["hidden"];
  }

  connectedCallback() {
    this.addEventListener("command", this.#onCommand);
    this.#syncState();
  }

  disconnectedCallback() {
    this.removeEventListener("command", this.#onCommand);
  }

  /**
   * @param {string} name
   * @param {string | null} old
   * @param {string | null} value
   */
  attributeChangedCallback(name, old, value) {
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

  /** @param {Event} event */
  #onCommand = (event) => {
    switch (/** @type {Event & { command: string }} */ (event).command) {
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

if (!customElements.get("sel-collapsible")) customElements.define("sel-collapsible", Collapsible);
