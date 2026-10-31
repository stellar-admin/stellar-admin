const TAB = '[data-slot="tabs-trigger"]';
const ITEM = '[role="menuitem"]';

// A link tab's attributes that belong to the tab, not to the link its menu item repeats
const TAB_ONLY_ATTRIBUTES = new Set([
  "aria-controls",
  "aria-disabled",
  "aria-selected",
  "class",
  "command",
  "commandfor",
  "data-active",
  "data-disabled",
  "data-slot",
  "hidden",
  "id",
  "role",
  "tabindex",
]);

/**
 * Tab list web component, rendered by `<sa-tab-list>` for a horizontal list with an overflow
 * menu. It keeps as many tabs as fit on the list's line and hides the rest, which the More
 * menu at the list's end offers instead. The selected tab is always a tab: it's placed first,
 * then the others in order while they fit.
 *
 * The menu's items are copies of the hidden tabs, rebuilt on every fit: a link tab's item is a
 * link with the tab's attributes, so htmx attributes carry over; a panel tab's item selects
 * the tab through its `sel-tabs`.
 *
 * It fits the tabs when it connects, when its width or its parent's width changes, when the
 * web fonts finish loading, and when the selected tab changes. Its parent's width matters
 * because a list that's only as wide as its tabs doesn't grow when there's room again.
 */
export class TabList extends HTMLElement {
  #resizeObserver = new ResizeObserver(() => this.fit());
  #mutationObserver = new MutationObserver(() => this.fit());

  connectedCallback() {
    this.#resizeObserver.observe(this);
    if (this.parentElement) this.#resizeObserver.observe(this.parentElement);
    this.#mutationObserver.observe(this, {
      attributes: true,
      attributeFilter: ["aria-selected"],
      subtree: true,
    });
    this.addEventListener("click", this.#onClick);
    this.fit();
    document.fonts.ready.then(() => this.isConnected && this.fit());
  }

  disconnectedCallback() {
    this.#resizeObserver.disconnect();
    this.#mutationObserver.disconnect();
    this.removeEventListener("click", this.#onClick);
  }

  /** Shows the tabs that fit and puts the others in the menu. */
  fit() {
    const list = /** @type {HTMLElement | null} */ (this.querySelector('[data-slot="tabs-list"]'));
    const more = /** @type {HTMLElement | null} */ (this.querySelector('[data-slot="tabs-more"]'));
    const menu = more && document.getElementById(more.getAttribute("popovertarget") ?? "");
    if (!list || !more || !menu) return;

    const tabs = Array.from(/** @type {NodeListOf<HTMLElement>} */ (list.querySelectorAll(TAB)));
    for (const tab of tabs) tab.hidden = false;
    more.hidden = true;

    const listStyle = getComputedStyle(list);
    const gap = parseFloat(listStyle.columnGap) || 0;
    // Fractional widths: the tabs' sum can be a fraction wider than the list they exactly fill
    const space =
      this.getBoundingClientRect().width +
      0.5 -
      (parseFloat(listStyle.paddingInlineStart) || 0) -
      (parseFloat(listStyle.paddingInlineEnd) || 0) -
      (parseFloat(listStyle.borderInlineStartWidth) || 0) -
      (parseFloat(listStyle.borderInlineEndWidth) || 0);
    const widths = tabs.map((tab) => tab.getBoundingClientRect().width);
    const total = widths.reduce((sum, width, index) => sum + width + (index ? gap : 0), 0);

    /** @type {Set<number>} */
    let shown = new Set(tabs.keys());
    if (total > space) {
      more.hidden = false;
      const selected = tabs.findIndex((tab) => tab.getAttribute("aria-selected") === "true");
      shown = new Set(selected < 0 ? [] : [selected]);
      let used = more.getBoundingClientRect().width + (selected < 0 ? 0 : gap + widths[selected]);
      for (let index = 0; index < tabs.length; index++) {
        if (index === selected) continue;
        if (used + gap + widths[index] > space) break;
        used += gap + widths[index];
        shown.add(index);
      }
    }

    tabs.forEach((tab, index) => (tab.hidden = !shown.has(index)));
    menu.replaceChildren(
      ...tabs.flatMap((tab, index) => (shown.has(index) ? [] : [this.#menuItem(tab, index)])),
    );
  }

  /**
   * @param {HTMLElement} tab
   * @param {number} index
   */
  #menuItem(tab, index) {
    const isLink = tab.localName === "a";
    const item = document.createElement(isLink ? "a" : "button");
    if (isLink) {
      for (const { name, value } of tab.attributes) {
        if (!TAB_ONLY_ATTRIBUTES.has(name)) item.setAttribute(name, value);
      }
    } else {
      item.setAttribute("type", "button");
    }

    item.setAttribute("role", "menuitem");
    item.tabIndex = -1;
    item.dataset.slot = "dropdown-menu-item";
    item.dataset.variant = "default";
    item.dataset.tabIndex = String(index);
    item.className = "sa-dropdown-menu-item";
    if (tab.matches(":disabled, [aria-disabled='true']")) {
      item.setAttribute("aria-disabled", "true");
      item.toggleAttribute("data-disabled", true);
    }

    item.append(...Array.from(tab.childNodes, (node) => node.cloneNode(true)));
    return item;
  }

  /** @param {MouseEvent} event */
  #onClick = (event) => {
    const item = /** @type {Element} */ (event.target)?.closest(ITEM);
    if (!(item instanceof HTMLButtonElement) || item.dataset.tabIndex === undefined) return;

    const tab = /** @type {HTMLElement | undefined} */ (
      this.querySelectorAll(TAB)[Number(item.dataset.tabIndex)]
    );
    const tabs = /** @type {(HTMLElement & { select?: (tab: HTMLElement) => void }) | null} */ (
      tab?.closest("sel-tabs") ?? null
    );
    if (!tab || !tabs?.select) return;

    // Selecting the tab fits the list again, which shows the tab
    tabs.select(tab);
    requestAnimationFrame(() => tab.focus());
  };
}

if (!customElements.get("sel-tab-list")) customElements.define("sel-tab-list", TabList);
