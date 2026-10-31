const TAB = '[role="tab"]';

/**
 * Tabs web component. Wrap it around a `[role="tablist"]` of tab buttons and the
 * `[role="tabpanel"]`s they control; each tab names its panel with `aria-controls`.
 * A tab selects itself with the Invoker Commands API: `command="--select-tab"`
 * and `commandfor` set to this element's id.
 *
 * All state lives in the DOM — `aria-selected`, `tabindex` and `data-active` on the
 * tabs, `hidden` on the panels — so the server picks the initial tab and tabs that
 * htmx adds need no re-initialization.
 *
 * The arrow keys move between the enabled tabs along the list's `aria-orientation`
 * (Home and End jump to the first and last) and select the tab they move to. They skip
 * hidden tabs, which a `sel-tab-list` offers in its More menu instead.
 *
 * Selecting a tab raises a bubbling `tabhide` event on the panel it hides, then
 * `tabshow` on the panel it shows (detail: `{ tab, panel }`), after the change, so
 * the shown panel is visible when the event fires. The initial state raises no
 * events. Because the event fires on the panel, htmx attributes on the panel need
 * no `from:` modifier: `hx-get="…" hx-trigger="tabshow once"`.
 */
export class Tabs extends HTMLElement {
  connectedCallback() {
    this.addEventListener("command", this.#onCommand);
    this.addEventListener("keydown", this.#onKeyDown);
  }

  disconnectedCallback() {
    this.removeEventListener("command", this.#onCommand);
    this.removeEventListener("keydown", this.#onKeyDown);
  }

  /**
   * The tabs this element controls (excluding those of nested tabs).
   * @returns {HTMLElement[]}
   */
  get tabs() {
    return Array.from(/** @type {NodeListOf<HTMLElement>} */ (this.querySelectorAll(TAB))).filter(
      (tab) => this.#owns(tab),
    );
  }

  /**
   * The selected tab.
   * @returns {HTMLElement | null}
   */
  get selectedTab() {
    return this.tabs.find((tab) => tab.getAttribute("aria-selected") === "true") ?? null;
  }

  /**
   * Selects a tab and shows its panel.
   * @param {HTMLElement} tab
   */
  select(tab) {
    if (!this.#owns(tab) || this.#isDisabled(tab)) return;

    const previous = this.selectedTab;
    if (previous === tab) return;

    for (const other of this.tabs) {
      const selected = other === tab;
      other.setAttribute("aria-selected", String(selected));
      other.tabIndex = selected ? 0 : -1;
      other.toggleAttribute("data-active", selected);
      const panel = this.#panelFor(other);
      if (panel) panel.hidden = !selected;
    }

    const previousPanel = previous && this.#panelFor(previous);
    if (previousPanel) this.#dispatch("tabhide", previous, previousPanel);

    const panel = this.#panelFor(tab);
    if (panel) this.#dispatch("tabshow", tab, panel);
  }

  /** @param {Element} element */
  #owns(element) {
    return element.closest("sel-tabs") === this;
  }

  /** @param {HTMLElement} tab */
  #isDisabled(tab) {
    return tab.matches(":disabled, [aria-disabled='true']");
  }

  /**
   * @param {Element} tab
   * @returns {HTMLElement | null}
   */
  #panelFor(tab) {
    const controls = tab.getAttribute("aria-controls");
    return controls ? document.getElementById(controls) : null;
  }

  /**
   * @param {"tabshow" | "tabhide"} type
   * @param {HTMLElement} tab
   * @param {HTMLElement} panel
   */
  #dispatch(type, tab, panel) {
    panel.dispatchEvent(new CustomEvent(type, { bubbles: true, detail: { tab, panel } }));
  }

  /** @param {Event} event */
  #onCommand = (event) => {
    const { command, source } = /** @type {Event & { command: string; source: Element | null }} */ (
      event
    );
    if (command !== "--select-tab" || !(source instanceof HTMLElement) || !source.matches(TAB)) {
      return;
    }

    this.select(source);
  };

  /** @param {KeyboardEvent} event */
  #onKeyDown = (event) => {
    const target = event.target;
    if (!(target instanceof HTMLElement) || !target.matches(TAB) || !this.#owns(target)) return;
    if (event.altKey || event.ctrlKey || event.metaKey) return;

    const list = target.closest('[role="tablist"]');
    const vertical = list?.getAttribute("aria-orientation") === "vertical";
    const rtl = getComputedStyle(target).direction === "rtl";
    const tabs = this.tabs.filter((tab) => tab.closest('[role="tablist"]') === list);
    // A tab that its sel-tab-list moved into the More menu is hidden
    const enabled = tabs.filter((tab) => !tab.hidden && !this.#isDisabled(tab));
    const index = enabled.indexOf(target);
    if (enabled.length === 0) return;

    /** @type {HTMLElement | undefined} */
    let next;
    switch (event.key) {
      case vertical ? "ArrowUp" : rtl ? "ArrowRight" : "ArrowLeft":
        next = enabled[(index - 1 + enabled.length) % enabled.length];
        break;
      case vertical ? "ArrowDown" : rtl ? "ArrowLeft" : "ArrowRight":
        next = enabled[(index + 1) % enabled.length];
        break;
      case "Home":
        next = enabled[0];
        break;
      case "End":
        next = enabled[enabled.length - 1];
        break;
      default:
        return;
    }

    event.preventDefault();
    if (!next) return;

    next.focus();
    this.select(next);
  };
}

if (!customElements.get("sel-tabs")) customElements.define("sel-tabs", Tabs);
