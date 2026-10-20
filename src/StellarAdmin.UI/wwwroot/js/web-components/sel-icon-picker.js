/** @typedef {{ name: string; svg: SVGSVGElement; keywords: string[] }} Icon */

// Cells rendered at a time; more render as the grid scrolls.
const BATCH = 240;
const SVG_NS = "http://www.w3.org/2000/svg";

/** @param {number} value */
const formatNumber = (value) => value.toLocaleString("en");

/**
 * Parses icon markup from an `icons-url` response. Only markup with an `<svg>` root is accepted,
 * and scripts, foreign objects, event handler attributes and `javascript:` links are removed.
 * @param {string} markup
 * @returns {SVGSVGElement | null}
 */
export function sanitizeSvg(markup) {
  const doc = new DOMParser().parseFromString(markup, "image/svg+xml");
  const root = doc.documentElement;
  if (
    root.namespaceURI !== SVG_NS ||
    root.localName !== "svg" ||
    doc.getElementsByTagName("parsererror").length > 0
  ) {
    return null;
  }

  for (const element of [root, ...root.querySelectorAll("*")]) {
    if (element.localName === "script" || element.localName === "foreignObject") {
      element.remove();
      continue;
    }

    for (const attribute of [...element.attributes]) {
      const name = attribute.name.toLowerCase();
      const isLink = name === "href" || name.endsWith(":href");
      if (name.startsWith("on") || (isLink && /^\s*javascript:/i.test(attribute.value))) {
        element.removeAttribute(attribute.name);
      }
    }
  }

  root.setAttribute("aria-hidden", "true");
  root.setAttribute("focusable", "false");

  return /** @type {SVGSVGElement} */ (/** @type {unknown} */ (document.importNode(root, true)));
}

/**
 * Icon picker web component. Rendered by the `sa-icon-picker` tag helper.
 *
 * In the picker layout, the trigger opens a popover with a search box, a grid of icons and a status
 * line. The icons come from the `<template>` the tag helper renders from its child icons, or from
 * `data-icons-url`, fetched once when the popover first opens. The grid renders `BATCH` cells at a
 * time and more as it scrolls. ↓ moves from the search box into the grid, the arrow keys move
 * through it, Enter picks, and ↑ from the first row goes back to the search box. Picking sets the
 * hidden input, which is all the form posts.
 *
 * In the inline layout, the icons are toggle group radios. Clicking the chosen tile again clears an
 * optional field: the hidden input then posts an empty value.
 */
export class IconPicker extends HTMLElement {
  /** @type {HTMLElement | null} */
  #clear = null;
  /** @type {HTMLElement} */
  #count;
  /** @type {HTMLElement} */
  #grid;
  /** @type {HTMLInputElement} */
  #hidden;
  /** @type {Icon[] | null} */
  #icons = null;
  /** @type {Icon[]} */
  #list = [];
  /** @type {Promise<void> | null} */
  #loading = null;
  /** @type {HTMLElement} */
  #name;
  /** @type {IntersectionObserver | null} */
  #observer = null;
  /** @type {HTMLElement} */
  #popover;
  #rendered = 0;
  /** @type {HTMLElement} */
  #scroll;
  /** @type {HTMLInputElement} */
  #search;
  /** @type {HTMLButtonElement} */
  #trigger;
  #wasChecked = false;

  connectedCallback() {
    this.#hidden = this.#part("icon-picker-value");
    if (this.dataset.layout === "inline") {
      this.addEventListener("pointerdown", this.#onTilePointerDown);
      this.addEventListener("click", this.#onTileClick);
      this.addEventListener("change", this.#onTileChange);
      return;
    }

    this.#trigger = this.#part("icon-picker-trigger");
    this.#popover = this.#part("icon-picker-popover");
    this.#search = /** @type {HTMLInputElement | null} */ (
      this.#popover.querySelector('input[type="search"]')
    );
    this.#scroll = this.#part("icon-picker-scroll");
    this.#grid = this.#part("icon-picker-grid");
    this.#name = this.#part("icon-picker-name");
    this.#count = this.#part("icon-picker-count");
    this.#clear = /** @type {HTMLElement | null} */ (this.querySelector('[data-action="clear"]'));

    this.#popover.addEventListener("toggle", this.#onToggle);
    this.#search.addEventListener("input", this.#onSearchInput);
    this.#search.addEventListener("keydown", this.#onSearchKeyDown);
    this.#grid.addEventListener("click", this.#onGridClick);
    this.#grid.addEventListener("keydown", this.#onGridKeyDown);
    this.#grid.addEventListener("pointerover", this.#onGridPointerOver);
    this.#grid.addEventListener("focusin", this.#onGridPointerOver);
    this.#scroll.addEventListener("pointerleave", this.#onGridPointerLeave);
    this.#clear?.addEventListener("click", this.#onClear);
  }

  disconnectedCallback() {
    this.#observer?.disconnect();
  }

  /** The name of the chosen icon, or an empty string when none is chosen. */
  get value() {
    return this.#hidden.value;
  }

  get #required() {
    return this.dataset.required === "true";
  }

  /**
   * @template {HTMLElement} T
   * @param {string} slot
   */
  #part(slot) {
    return /** @type {T | null} */ (this.querySelector(`[data-slot="${slot}"]`));
  }

  /** @param {string} name */
  #setValue(name) {
    this.#hidden.value = name;
    this.#hidden.dispatchEvent(new Event("input", { bubbles: true }));
    this.#hidden.dispatchEvent(new Event("change", { bubbles: true }));
  }

  // Inline layout -----------------------------------------------------------------------------

  /** @param {PointerEvent} event */
  #onTilePointerDown = (event) => {
    const input = /** @type {HTMLInputElement | null} */ (
      /** @type {Element} */ (event.target)
        .closest('[data-slot="toggle-group-item"]')
        ?.querySelector('input[type="radio"]')
    );
    this.#wasChecked = input?.checked ?? false;
  };

  /** @param {MouseEvent} event */
  #onTileClick = (event) => {
    const input = /** @type {HTMLInputElement} */ (event.target);
    if (input.type !== "radio") return;

    const wasChecked = this.#wasChecked;
    this.#wasChecked = false;
    if (!wasChecked || this.#required || input.disabled) return;

    input.checked = false;
    this.#hidden.disabled = false;
    this.#setValue("");
  };

  /** @param {Event} event */
  #onTileChange = (event) => {
    if (/** @type {HTMLInputElement} */ (event.target).type === "radio")
      this.#hidden.disabled = true;
  };

  // Picker layout -----------------------------------------------------------------------------

  /** @param {Event} event */
  #onToggle = (event) => {
    if (/** @type {ToggleEvent} */ (event).newState !== "open") {
      // Closing moves focus off a cell or the search box; it goes back to the trigger
      const active = document.activeElement;
      if (!active || active === document.body || this.#popover.contains(active)) {
        this.#trigger.focus();
      }

      return;
    }

    this.#search.value = "";
    this.#search.focus();
    void this.#load().then(() => this.#filter());
  };

  #load() {
    if (this.#icons) return Promise.resolve();

    this.#loading ??= this.#fetchIcons()
      .then((icons) => {
        this.#icons = icons;
        this.#updateTrigger();
      })
      .catch(() => {
        this.#status("Couldn't load the icons.");
      })
      .finally(() => {
        this.#loading = null;
      });

    return this.#loading;
  }

  /** @returns {Promise<Icon[]>} */
  async #fetchIcons() {
    const template = /** @type {HTMLTemplateElement | null} */ (
      this.querySelector('[data-slot="icon-picker-icons"]')
    );
    if (template) {
      return [
        .../** @type {NodeListOf<SVGSVGElement>} */ (
          template.content.querySelectorAll("svg[data-icon-name]")
        ),
      ].map((svg) => ({
        name: svg.dataset.iconName,
        svg: document.importNode(svg, true),
        keywords: (svg.dataset.iconKeywords ?? "")
          .split(",")
          .filter(Boolean)
          .map((keyword) => keyword.toLowerCase()),
      }));
    }

    this.#status("Loading icons…");
    const response = await fetch(this.dataset.iconsUrl, {
      headers: { Accept: "application/json" },
      credentials: "same-origin",
    });
    if (!response.ok) throw new Error(`The icons request failed with ${response.status}.`);

    const payload = await response.json();
    if (!Array.isArray(payload?.icons)) throw new Error("The icons response has no icons array.");

    /** @type {Icon[]} */
    const icons = [];
    /** @type {Set<string>} */
    const names = new Set();
    for (const item of payload.icons) {
      if (typeof item?.name !== "string" || !item.name || typeof item.svg !== "string") continue;
      if (names.has(item.name)) continue;

      const svg = sanitizeSvg(item.svg);
      if (!svg) continue;

      const keywords = Array.isArray(item.keywords)
        ? item.keywords
            .filter(/** @param {unknown} keyword */ (keyword) => typeof keyword === "string")
            .map(/** @param {string} keyword */ (keyword) => keyword.toLowerCase())
        : [];
      names.add(item.name);
      icons.push({ name: item.name, svg, keywords });
    }

    return icons;
  }

  /** @param {string} text */
  #status(text) {
    this.#grid.replaceChildren();
    this.#name.textContent = "";
    this.#name.removeAttribute("data-name");
    this.#count.textContent = text;
  }

  #onSearchInput = () => {
    if (this.#icons) this.#filter();
  };

  #filter() {
    const icons = this.#icons;
    const query = this.#search.value.trim().toLowerCase().replace(/\s+/g, " ");
    let list = query ? search(icons, query) : icons;

    // With no search, the chosen icon comes first so it's in view
    const current = icons.find((icon) => icon.name === this.value);
    if (!query && current) list = [current, ...list.filter((icon) => icon !== current)];

    this.#list = list;
    this.#rendered = 0;
    this.#observer?.disconnect();
    this.#grid.replaceChildren();
    this.#scroll.scrollTop = 0;
    this.#count.textContent = query
      ? `${formatNumber(list.length)} of ${formatNumber(icons.length)}`
      : `${formatNumber(list.length)} icons`;
    this.#showName(null);

    if (list.length === 0) {
      const empty = document.createElement("div");
      empty.className = "sa-icon-picker-empty";
      empty.textContent = query ? `No icons match “${this.#search.value.trim()}”.` : "No icons.";
      this.#grid.append(empty);
      return;
    }

    this.#renderMore();
  }

  #renderMore() {
    const start = this.#rendered;
    const end = Math.min(this.#list.length, start + BATCH);
    if (start >= end) return;

    this.#grid.querySelector("[data-sentinel]")?.remove();
    const cells = this.#list.slice(start, end).map((icon) => {
      const cell = document.createElement("button");
      cell.type = "button";
      cell.className = "sa-icon-picker-cell";
      cell.tabIndex = -1;
      cell.dataset.icon = icon.name;
      cell.title = icon.name;
      cell.setAttribute("role", "option");
      cell.setAttribute("aria-label", icon.name);
      cell.setAttribute("aria-selected", String(icon.name === this.value));
      cell.append(icon.svg.cloneNode(true));
      return cell;
    });
    this.#grid.append(...cells);
    this.#rendered = end;

    if (end < this.#list.length) {
      const sentinel = document.createElement("div");
      sentinel.dataset.sentinel = "";
      sentinel.style.gridColumn = "1 / -1";
      sentinel.style.height = "1px";
      this.#grid.append(sentinel);
      this.#observer ??= new IntersectionObserver(
        (entries) => {
          if (entries.some((entry) => entry.isIntersecting)) this.#renderMore();
        },
        { root: this.#scroll, rootMargin: "200px" },
      );
      this.#observer.disconnect();
      this.#observer.observe(sentinel);
    }
  }

  #cells() {
    return [.../** @type {NodeListOf<HTMLElement>} */ (this.#grid.querySelectorAll("[data-icon]"))];
  }

  /** @param {KeyboardEvent} event */
  #onSearchKeyDown = (event) => {
    if (event.key === "ArrowDown") {
      event.preventDefault();
      this.#cells()[0]?.focus();
    } else if (event.key === "Enter") {
      // Enter picks the first match rather than submitting the form
      event.preventDefault();
      const first = this.#cells()[0];
      if (first) this.#pick(first.dataset.icon);
    }
  };

  /** @param {KeyboardEvent} event */
  #onGridKeyDown = (event) => {
    const cells = this.#cells();
    const index = cells.indexOf(/** @type {HTMLElement} */ (document.activeElement));
    if (index < 0) return;

    if (event.key === "Enter" || event.key === " ") {
      event.preventDefault();
      this.#pick(cells[index].dataset.icon);
      return;
    }

    const columns = getComputedStyle(this.#grid).gridTemplateColumns.split(" ").length;
    /** @type {Record<string, number>} */
    const moves = {
      ArrowRight: 1,
      ArrowLeft: -1,
      ArrowDown: columns,
      ArrowUp: -columns,
      Home: -index,
      End: cells.length - 1 - index,
    };
    const move = moves[event.key];
    if (move === undefined) return;

    event.preventDefault();
    const target = index + move;
    if (target < 0) {
      this.#search.focus();
      return;
    }

    if (target >= cells.length - columns) this.#renderMore();
    const next = this.#cells();
    next[Math.min(target, next.length - 1)]?.focus();
  };

  /** @param {MouseEvent} event */
  #onGridClick = (event) => {
    const cell = /** @type {HTMLElement | null} */ (
      /** @type {Element} */ (event.target).closest("[data-icon]")
    );
    if (cell) this.#pick(cell.dataset.icon);
  };

  /** @param {Event} event */
  #onGridPointerOver = (event) => {
    const cell = /** @type {HTMLElement | null} */ (
      /** @type {Element} */ (event.target).closest("[data-icon]")
    );
    if (cell) this.#showName(cell.dataset.icon);
  };

  #onGridPointerLeave = () => {
    if (!this.#grid.contains(document.activeElement)) this.#showName(null);
  };

  #onClear = () => {
    this.#pick("");
  };

  /** @param {string} name */
  #pick(name) {
    this.#setValue(name);
    this.#updateTrigger();
    this.#popover.hidePopover();
  }

  // The status line names the icon under the pointer or with focus, or else the chosen one
  /** @param {string | null} name */
  #showName(name) {
    const shown = name ?? this.value;
    this.#name.textContent = shown || "No icon";
    this.#name.toggleAttribute("data-name", !!shown);
  }

  #updateTrigger() {
    const text = /** @type {HTMLElement | null} */ (
      this.#trigger.querySelector('[data-slot="icon-picker-text"]')
    );
    const name = this.value;

    // Before the icons load, the trigger keeps the icon the server rendered
    if (name && !this.#icons) return;

    while (text.previousElementSibling) text.previousElementSibling.remove();
    if (!name) {
      text.dataset.placeholder = "true";
      text.textContent = this.dataset.placeholder ?? "";
      return;
    }

    const icon = this.#icons.find((candidate) => candidate.name === name);
    /** @type {Element} */
    let glyph;
    if (icon) {
      glyph = /** @type {Element} */ (icon.svg.cloneNode(true));
    } else {
      glyph = document.createElement("span");
      glyph.className = "sa-icon-missing";
      glyph.setAttribute("aria-hidden", "true");
    }

    text.before(glyph);
    delete text.dataset.placeholder;
    text.textContent = name;
  }
}

if (!customElements.get("sel-icon-picker")) customElements.define("sel-icon-picker", IconPicker);

// Matches every search term against the name or a keyword. Exact names come first, then names that
// start with the search, then names with a word that does, then the other name matches, then
// keyword matches; each alphabetical.
/**
 * @param {Icon[]} icons
 * @param {string} query
 */
function search(icons, query) {
  const terms = query.split(" ");
  const joined = terms.join("-");
  /** @param {Icon} icon */
  const rank = (icon) => {
    const name = icon.name.toLowerCase();
    const inName = terms.every((term) => name.includes(term));
    if (!inName) {
      const inKeywords = terms.every(
        (term) => name.includes(term) || icon.keywords.some((keyword) => keyword.includes(term)),
      );
      return inKeywords ? 4 : -1;
    }

    if (name === joined) return 0;
    if (name.startsWith(joined)) return 1;
    if (name.includes(`-${joined}`)) return 2;
    return 3;
  };

  return icons
    .map((icon) => ({ icon, rank: rank(icon) }))
    .filter((match) => match.rank >= 0)
    .sort((a, b) => a.rank - b.rank || a.icon.name.localeCompare(b.icon.name))
    .map((match) => match.icon);
}
