import { commandScore, RELATIVE_SCORE_CUTOFF } from "./command-score.js";

const ITEM = '[data-slot="command-item"]';
const GROUP = '[data-slot="command-group"]';
const SEPARATOR = '[data-slot="command-separator"]';

/**
 * Command palette web component rendered by the `sa-command` tag helper. Focus stays in the
 * search input while the arrow keys move a highlighted item, announced to assistive technology
 * through `aria-activedescendant`.
 *
 *   - `data-filter="client"` (default) fuzzy-filters and sorts the items as the user types
 *   - `data-filter="none"` leaves the items alone so the developer can filter them, for example
 *     on the server
 *   - `data-loop="true"` wraps keyboard navigation at either end of the list
 *
 * Items added or replaced later (for example by an htmx swap of the list contents) are picked up
 * automatically: they get IDs, are filtered against the current search in client mode, and the
 * empty state and active item are updated.
 *
 * Enter or a click activates an item (links navigate) and dispatches a bubbling `itemselect`
 * event with the item's value in `detail.value`.
 */
export class Command extends HTMLElement {
  /** @type {HTMLDialogElement | null} */
  #dialog = null;
  #itemCount = 0;
  #observer = new MutationObserver((records) => this.#refresh(true, records));
  // Children of each container reordered by sorting, in their original order.
  /** @type {Map<Element, Element[]>} */
  #originalOrder = new Map();
  /** @type {HTMLElement[]} */
  #selectable = [];
  /** @type {HTMLElement | null} */
  #selected = null;
  /** @type {WeakMap<HTMLElement, string>} */
  #values = new WeakMap();

  connectedCallback() {
    this.addEventListener("input", this.#onInput);
    this.addEventListener("keydown", this.#onKeydown);
    this.addEventListener("click", this.#onClick);
    this.addEventListener("mousedown", this.#onMousedown);
    this.addEventListener("pointermove", this.#onPointerMove);
    this.#dialog = this.closest("dialog");
    this.#dialog?.addEventListener("close", this.#onDialogClose);
    this.#refresh();
    this.#observer.observe(this, { childList: true, subtree: true });
  }

  disconnectedCallback() {
    this.#observer.disconnect();
    this.removeEventListener("input", this.#onInput);
    this.removeEventListener("keydown", this.#onKeydown);
    this.removeEventListener("click", this.#onClick);
    this.removeEventListener("mousedown", this.#onMousedown);
    this.removeEventListener("pointermove", this.#onPointerMove);
    this.#dialog?.removeEventListener("close", this.#onDialogClose);
    this.#dialog = null;
  }

  get #input() {
    return /** @type {HTMLInputElement | null} */ (
      this.querySelector('[data-slot="command-input"]')
    );
  }

  get #list() {
    return /** @type {HTMLElement | null} */ (this.querySelector('[data-slot="command-list"]'));
  }

  get #clientFiltering() {
    return this.dataset.filter !== "none";
  }

  #items() {
    return Array.from(/** @type {NodeListOf<HTMLElement>} */ (this.querySelectorAll(ITEM)));
  }

  /** Visible, enabled items in display order. */
  #selectableItems() {
    return this.#items().filter((item) => !item.hidden && !this.#isDisabled(item));
  }

  /** @param {HTMLElement} item */
  #isDisabled(item) {
    return item.dataset.disabled === "true";
  }

  /**
   * The item's `data-value`, or its text without any shortcut.
   * @param {HTMLElement} item
   */
  #valueOf(item) {
    if (item.dataset.value) {
      return item.dataset.value;
    }
    let value = this.#values.get(item);
    if (value === undefined) {
      const copy = /** @type {HTMLElement} */ (item.cloneNode(true));
      copy.querySelectorAll('[data-slot="command-shortcut"]').forEach((el) => el.remove());
      value = (copy.textContent ?? "").replace(/\s+/g, " ").trim();
      this.#values.set(item, value);
    }
    return value;
  }

  /**
   * Assigns missing item IDs and re-filters. After a change to the list made outside this
   * component (`fromMutation`), the active item is kept when it is still selectable, and when it was
   * removed, the first item added in the same change takes over, such as the next page swapped in
   * for a "load more" item. An active item removed with nothing added in its place hands over to
   * the item that followed it, or else the one before it, so the highlight stays where it was.
   * @param {MutationRecord[]} [records]
   */
  #refresh(fromMutation = false, records = []) {
    if (fromMutation) {
      this.#forgetStaleOrder();
    }
    const prefix = this.id || "sel-command";
    for (const item of this.#items()) {
      if (!item.id) {
        // Skip IDs already on the page, such as those in a copy of generated markup.
        let id;
        do {
          id = `${prefix}-item-${++this.#itemCount}`;
        } while (document.getElementById(id));
        item.id = id;
      }
    }
    this.#filter(fromMutation, this.#addedInPlaceOfSelected(records), this.#neighbors());
  }

  /** The previously selectable items nearest the active item: those after it, then those before it. */
  #neighbors() {
    const index = this.#selectable.indexOf(this.#selected);
    if (index < 0) {
      return [];
    }
    return [...this.#selectable.slice(index + 1), ...this.#selectable.slice(0, index).reverse()];
  }

  /**
   * The nodes added by the mutations when they removed the active item.
   * @param {MutationRecord[]} records
   */
  #addedInPlaceOfSelected(records) {
    const selected = this.#selected;
    const removed =
      selected &&
      records.some((record) =>
        Array.from(record.removedNodes).some((node) => node.contains(selected)),
      );
    return removed ? records.flatMap((record) => Array.from(record.addedNodes)) : [];
  }

  /**
   * @param {Node[]} [replacements]
   * @param {HTMLElement[]} [neighbors]
   */
  #filter(keepSelection = false, replacements = [], neighbors = []) {
    const search = this.#clientFiltering ? (this.#input?.value ?? "").trim() : "";
    const items = this.#items();

    if (search) {
      /** @type {Map<HTMLElement, number>} */
      const scores = new Map();
      for (const item of items) {
        scores.set(item, commandScore(this.#valueOf(item), search, item.dataset.keywords ?? ""));
      }
      const cutoff = Math.max(...scores.values(), 0) * RELATIVE_SCORE_CUTOFF;
      for (const [item, score] of scores) {
        item.hidden = score === 0 || score < cutoff;
      }
      this.#sort(scores);
    } else if (this.#clientFiltering) {
      for (const item of items) {
        item.hidden = false;
      }
      this.#restoreOrder();
    }

    if (this.#clientFiltering) {
      for (const group of /** @type {NodeListOf<HTMLElement>} */ (this.querySelectorAll(GROUP))) {
        group.hidden = !!search && !group.querySelector(`${ITEM}:not([hidden])`);
      }
      for (const separator of /** @type {NodeListOf<HTMLElement>} */ (
        this.querySelectorAll(SEPARATOR)
      )) {
        separator.hidden = !!search;
      }
    }

    const empty = /** @type {HTMLElement | null} */ (
      this.querySelector('[data-slot="command-empty"]')
    );
    if (empty) {
      empty.hidden = items.some((item) => !item.hidden);
    }

    const selectable = this.#selectableItems();
    const keep = keepSelection && this.#selected && selectable.includes(this.#selected);
    const replacement =
      selectable.find((item) => replacements.some((node) => node.contains(item))) ??
      neighbors.find((item) => selectable.includes(item));
    this.#select(keep ? this.#selected : (replacement ?? selectable[0] ?? null), !keep);
    this.#selectable = selectable;

    // Discard the records of this component's own reordering so it does not trigger a refresh.
    this.#observer.takeRecords();
  }

  /**
   * Sorts items by score, then groups by their best item's score. Each is sorted within the slots
   * it occupies in its container, so separators and other children keep their authored positions.
   * @param {Map<HTMLElement, number>} scores
   */
  #sort(scores) {
    this.#sortInSlots(scores);

    /** @type {Map<HTMLElement, number>} */
    const groupScores = new Map();
    for (const group of /** @type {NodeListOf<HTMLElement>} */ (this.querySelectorAll(GROUP))) {
      let best = 0;
      for (const item of /** @type {NodeListOf<HTMLElement>} */ (group.querySelectorAll(ITEM))) {
        best = Math.max(best, scores.get(item) ?? 0);
      }
      groupScores.set(group, best);
    }
    this.#sortInSlots(groupScores);
  }

  /**
   * Sorts elements by descending score within the slots they occupy in their container.
   * @param {Map<HTMLElement, number>} scores
   */
  #sortInSlots(scores) {
    /** @type {Map<Element, HTMLElement[]>} */
    const byContainer = new Map();
    for (const element of scores.keys()) {
      const container = element.parentElement;
      if (container) {
        byContainer.set(container, [...(byContainer.get(container) ?? []), element]);
      }
    }
    for (const [container, elements] of byContainer) {
      this.#rememberOrder(container);
      // Array.prototype.sort is stable, so equal scores keep their authored order.
      const sorted = [...elements].sort((a, b) => scores.get(b) - scores.get(a));
      const slots = elements.map((element) => {
        const slot = document.createComment("");
        element.before(slot);
        return slot;
      });
      slots.forEach((slot, index) => slot.replaceWith(sorted[index]));
    }
  }

  /** @param {Element} container */
  #rememberOrder(container) {
    if (!this.#originalOrder.has(container)) {
      this.#originalOrder.set(container, Array.from(container.children));
    }
  }

  /**
   * Drops remembered containers that have left the component and remembered children that have
   * left their container; children added since are restored after the remembered ones.
   */
  #forgetStaleOrder() {
    for (const [container, children] of this.#originalOrder) {
      if (!this.contains(container)) {
        this.#originalOrder.delete(container);
        continue;
      }
      const remaining = children.filter((child) => child.parentElement === container);
      const added = Array.from(container.children).filter((child) => !remaining.includes(child));
      this.#originalOrder.set(container, [...remaining, ...added]);
    }
  }

  #restoreOrder() {
    for (const [container, children] of this.#originalOrder) {
      container.append(...children.filter((child) => child.parentElement === container));
    }
    this.#originalOrder.clear();
  }

  /** @param {HTMLElement | null} item */
  #select(item, scroll = true) {
    if (this.#selected && this.#selected !== item) {
      this.#selected.removeAttribute("data-selected");
      this.#selected.setAttribute("aria-selected", "false");
    }
    this.#selected = item;

    const input = this.#input;
    if (!item) {
      input?.removeAttribute("aria-activedescendant");
      return;
    }

    item.setAttribute("data-selected", "true");
    item.setAttribute("aria-selected", "true");
    input?.setAttribute("aria-activedescendant", item.id);
    if (scroll) {
      this.#scrollIntoList(item);
    }
  }

  /**
   * Scrolls the list (never the page) so the item is visible, along with its whole group heading
   * when it is the group's first selectable item.
   * @param {HTMLElement} item
   */
  #scrollIntoList(item) {
    const list = this.#list;
    if (!list) {
      return;
    }

    const group = /** @type {HTMLElement | null} */ (item.closest(GROUP));
    const firstInGroup =
      group?.querySelector(`${ITEM}:not([hidden]):not([data-disabled="true"])`) === item;
    const target = group && firstInGroup ? group : item;

    const listRect = list.getBoundingClientRect();
    const top = target.getBoundingClientRect().top;
    const bottom = item.getBoundingClientRect().bottom;
    if (top < listRect.top) {
      list.scrollTop -= listRect.top - top;
    } else if (bottom > listRect.bottom) {
      list.scrollTop += bottom - listRect.bottom;
    }
  }

  /** @param {1 | -1} delta */
  #move(delta) {
    const items = this.#selectableItems();
    if (items.length === 0) {
      return;
    }

    const index = this.#selected ? items.indexOf(this.#selected) : -1;
    let next = index < 0 ? (delta > 0 ? 0 : items.length - 1) : index + delta;
    if (this.dataset.loop === "true") {
      next = (next + items.length) % items.length;
    } else {
      next = Math.min(Math.max(next, 0), items.length - 1);
    }
    this.#select(items[next]);
  }

  /** @param {Event} event */
  #onInput = (event) => {
    if (event.target === this.#input) {
      this.#filter();
    }
  };

  /** @param {KeyboardEvent} event */
  #onKeydown = (event) => {
    if (event.defaultPrevented || event.isComposing || event.keyCode === 229) {
      return;
    }

    let handled = true;
    switch (event.key) {
      case "ArrowDown":
        this.#move(1);
        break;
      case "ArrowUp":
        this.#move(-1);
        break;
      case "Home":
        this.#select(this.#selectableItems()[0] ?? null);
        break;
      case "End":
        this.#select(this.#selectableItems().at(-1) ?? null);
        break;
      case "n":
      case "j":
        handled = event.ctrlKey;
        if (handled) {
          this.#move(1);
        }
        break;
      case "p":
      case "k":
        handled = event.ctrlKey;
        if (handled) {
          this.#move(-1);
        }
        break;
      case "Enter":
        handled = !!this.#selected;
        this.#selected?.click();
        break;
      default:
        handled = false;
    }

    if (handled) {
      event.preventDefault();
    }
  };

  /** @param {MouseEvent} event */
  #onClick = (event) => {
    const item = /** @type {HTMLElement | null} */ (
      /** @type {Element | null} */ (event.target)?.closest(ITEM)
    );
    if (!item || !this.contains(item)) {
      return;
    }

    if (this.#isDisabled(item)) {
      event.preventDefault();
      return;
    }

    this.#select(item, false);
    item.dispatchEvent(
      new CustomEvent("itemselect", { detail: { value: this.#valueOf(item) }, bubbles: true }),
    );
  };

  // Keeps focus in the input when an item is pressed, so typing and arrow keys keep working.
  /** @param {MouseEvent} event */
  #onMousedown = (event) => {
    if (/** @type {Element | null} */ (event.target)?.closest(ITEM)) {
      event.preventDefault();
    }
  };

  /** @param {PointerEvent} event */
  #onPointerMove = (event) => {
    const item = /** @type {HTMLElement | null} */ (
      /** @type {Element | null} */ (event.target)?.closest(ITEM)
    );
    if (item && item !== this.#selected && !item.hidden && !this.#isDisabled(item)) {
      this.#select(item, false);
    }
  };

  // A dialog keeps its content between openings, so start each one with a fresh search.
  #onDialogClose = () => {
    const input = this.#input;
    if (input) {
      input.value = "";
    }
    this.#filter();
  };
}

if (!customElements.get("sel-command")) customElements.define("sel-command", Command);
