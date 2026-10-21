const DETAIL_ROW = 'tr[data-slot="table-row-detail"]';
const TOGGLE = '[data-slot="table-row-detail-toggle"]';

// Clicks on these keep their own behavior instead of toggling the row.
const INTERACTIVE =
  'a, button, input, select, textarea, label, summary, [role="button"], [contenteditable], [data-no-row-toggle]';

let idCounter = 0;

/**
 * @param {Element} element
 * @param {string} name
 * @param {string} value
 */
function setAttributeIfChanged(element, name, value) {
  if (element.getAttribute(name) !== value) element.setAttribute(name, value);
}

/**
 * Row details for a `sel-table` with `data-row-details`. In the table, an expandable
 * row is followed by a `tr[data-slot="table-row-detail"]`, and a toggle button
 * (`[data-slot="table-row-detail-toggle"]`) sits in a cell of the row. A toggle
 * with `aria-controls` controls that detail row instead; a toggle with
 * `data-all` expands or collapses every row.
 *
 * All state lives in the DOM — `hidden` and `data-state="open|closed"` on the
 * detail row, `aria-expanded` on the toggles — and the behavior listens on
 * the table element only. Rows that htmx (or any script) adds, replaces or removes need
 * no re-initialization, and server-rendered expanded rows are respected.
 *
 * Settings are read from the table element's data attributes on every interaction:
 *   - data-expand-mode="single|multiple"
 *   - data-row-click="true|false"
 *   - data-inset="aligned|bleed"  (aligned starts the content at the column after
 *     the toggle's column; bleed at the row's first cell)
 *
 * Expanding or collapsing a row raises a bubbling `rowdetailexpand` or
 * `rowdetailcollapse` event on the detail row (detail: `{ row, detailRow }`),
 * after the change, so the expanded content is visible when the event fires.
 * Server-rendered state raises no events. Because the event fires on the detail
 * row, htmx attributes on that row need no `from:` modifier:
 * `hx-get="…" hx-trigger="rowdetailexpand once" hx-target="find [data-slot=table-row-detail-content]"`.
 *
 * Rows with a `data-key` are remembered by key: when a script or htmx replaces a detail
 * row, the detail row that takes its place under a row with the same key is expanded or
 * collapsed as before, without animating, whatever the new markup says, and raises the
 * matching event so its content can load again. Rows with a key not seen before keep their
 * rendered state.
 */
export class TableRowDetails {
  /** @type {HTMLElement} */
  #host;
  // Re-syncs when rows are added or removed, or when a script shows or hides a detail row.
  // Attribute changes elsewhere (including the component's own writes) are ignored, so a
  // sync never triggers another.
  #mutationObserver = new MutationObserver((records) => {
    if (
      records.some(
        (record) =>
          record.type === "childList" ||
          (record.target instanceof Element && record.target.matches(DETAIL_ROW)),
      )
    ) {
      this.#scheduleSync();
    }
  });
  #resizeObserver = new ResizeObserver(() => this.#measure());
  // The detail rows of keyed rows present at the last sync, and whether each was open.
  /** @type {Map<string, { detailRow: HTMLTableRowElement; open: boolean }>} */
  #remembered = new Map();
  #syncScheduled = false;

  /** @param {HTMLElement} host */
  constructor(host) {
    this.#host = host;
  }

  connect() {
    this.#host.addEventListener("click", this.#onClick);
    this.#mutationObserver.observe(this.#host, {
      childList: true,
      subtree: true,
      attributes: true,
      attributeFilter: ["hidden"],
    });
    this.#resizeObserver.observe(this.#host);
    this.#sync();
  }

  disconnect() {
    this.#host.removeEventListener("click", this.#onClick);
    this.#mutationObserver.disconnect();
    this.#resizeObserver.disconnect();
  }

  /**
   * The detail rows this element controls (excluding those of nested tables).
   * @returns {HTMLTableRowElement[]}
   */
  get detailRows() {
    return Array.from(
      /** @type {NodeListOf<HTMLTableRowElement>} */ (this.#host.querySelectorAll(DETAIL_ROW)),
    ).filter((row) => this.#owns(row));
  }

  /**
   * Expands a detail row.
   * @param {HTMLTableRowElement} detailRow
   */
  expand(detailRow) {
    this.#setExpanded(detailRow, true);
  }

  /**
   * Collapses a detail row.
   * @param {HTMLTableRowElement} detailRow
   */
  collapse(detailRow) {
    this.#setExpanded(detailRow, false);
  }

  /**
   * Expands a collapsed detail row, or collapses an expanded one.
   * @param {HTMLTableRowElement} detailRow
   */
  toggle(detailRow) {
    this.#setExpanded(detailRow, !this.#isOpen(detailRow));
  }

  /** Expands every detail row. In single mode only the first row is expanded. */
  expandAll() {
    const rows = this.detailRows;
    if (this.#single) {
      if (rows[0]) this.expand(rows[0]);
      return;
    }
    for (const row of rows) this.#setExpanded(row, true);
  }

  /** Collapses every detail row. */
  collapseAll() {
    for (const row of this.detailRows) this.#setExpanded(row, false);
  }

  get #single() {
    return this.#host.dataset.expandMode === "single";
  }

  /** @param {Element} element */
  #owns(element) {
    return element.closest("sel-table") === this.#host;
  }

  /** @param {HTMLTableRowElement} detailRow */
  #isOpen(detailRow) {
    return detailRow.dataset.state === "open";
  }

  /**
   * The detail row a toggle controls: its aria-controls target, else the row after its own.
   * @param {Element} toggle
   * @returns {HTMLTableRowElement | null}
   */
  #detailRowFor(toggle) {
    const controls = toggle.getAttribute("aria-controls");
    if (controls) {
      const target = document.getElementById(controls);
      return target instanceof HTMLTableRowElement && target.matches(DETAIL_ROW) ? target : null;
    }
    const next = toggle.closest("tr")?.nextElementSibling;
    return next instanceof HTMLTableRowElement && next.matches(DETAIL_ROW) ? next : null;
  }

  /**
   * The expandable row a detail row belongs to: the row before it.
   * @param {HTMLTableRowElement} detailRow
   * @returns {HTMLTableRowElement | null}
   */
  #rowFor(detailRow) {
    const previous = detailRow.previousElementSibling;
    return previous instanceof HTMLTableRowElement && !previous.matches(DETAIL_ROW)
      ? previous
      : null;
  }

  /** @param {MouseEvent} event */
  #onClick = (event) => {
    const target = event.target;
    if (!(target instanceof Element)) return;

    const toggle = target.closest(TOGGLE);
    if (toggle && this.#owns(toggle)) {
      if (toggle.hasAttribute("data-all")) {
        if (toggle.getAttribute("aria-expanded") === "true") this.collapseAll();
        else this.expandAll();
        return;
      }
      const detailRow = this.#detailRowFor(toggle);
      if (detailRow) this.toggle(detailRow);
      return;
    }

    if (this.#host.dataset.rowClick !== "true") return;
    if (target.closest(INTERACTIVE)) return;
    if (window.getSelection()?.toString()) return;

    const row = target.closest("tr");
    if (!row || !this.#owns(row) || row.matches(DETAIL_ROW) || row.closest("thead, tfoot")) return;
    const next = row.nextElementSibling;
    if (next instanceof HTMLTableRowElement && next.matches(DETAIL_ROW)) this.toggle(next);
  };

  /**
   * @param {HTMLTableRowElement} detailRow
   * @param {boolean} open
   */
  #setExpanded(detailRow, open) {
    if (this.#isOpen(detailRow) === open) return;

    if (open && this.#single) {
      for (const other of this.detailRows) {
        if (other !== detailRow) this.#setExpanded(other, false);
      }
    }

    if (open) {
      detailRow.hidden = false;
      this.#measure();
      // Commit the collapsed layout before opening, so the reveal transitions.
      void detailRow.offsetHeight;
      detailRow.dataset.state = "open";
    } else {
      detailRow.dataset.state = "closed";
      // Hide once the collapse transition ends, or at once when there is none.
      const reveal = detailRow.querySelector(".sa-table-row-detail-reveal");
      const transitions = reveal?.getAnimations() ?? [];
      if (transitions.length === 0) {
        detailRow.hidden = true;
      } else {
        Promise.allSettled(transitions.map((animation) => animation.finished)).then(() => {
          if (detailRow.dataset.state === "closed") detailRow.hidden = true;
        });
      }
    }

    this.#syncToggles();
    this.#remember(detailRow);
    this.#dispatch(detailRow);
  }

  /** @param {HTMLTableRowElement} detailRow */
  #remember(detailRow) {
    const key = this.#rowFor(detailRow)?.dataset.key;
    if (key !== undefined) this.#remembered.set(key, { detailRow, open: this.#isOpen(detailRow) });
  }

  /**
   * Raises the event for the detail row's current state.
   * @param {HTMLTableRowElement} detailRow
   */
  #dispatch(detailRow) {
    detailRow.dispatchEvent(
      new CustomEvent(this.#isOpen(detailRow) ? "rowdetailexpand" : "rowdetailcollapse", {
        bubbles: true,
        detail: { row: this.#rowFor(detailRow), detailRow },
      }),
    );
  }

  #scheduleSync() {
    if (this.#syncScheduled) return;
    this.#syncScheduled = true;
    queueMicrotask(() => {
      this.#syncScheduled = false;
      this.#sync();
    });
  }

  /** Reconciles server-rendered or script-inserted rows with the component's conventions. */
  #sync() {
    /** @type {Map<string, { detailRow: HTMLTableRowElement; open: boolean }>} */
    const remembered = new Map();
    /** @type {HTMLTableRowElement[]} */
    const restored = [];
    for (const detailRow of this.detailRows) {
      if (!detailRow.id) detailRow.id = `--sa-row-detail-${++idCounter}`;
      // Rows inserted without a state take it from `hidden`; a hidden row is never open.
      // (A closed row that is not yet hidden is still animating closed.)
      if (!detailRow.dataset.state || (detailRow.hidden && this.#isOpen(detailRow))) {
        detailRow.dataset.state = detailRow.hidden ? "closed" : "open";
      }

      const row = this.#rowFor(detailRow);
      const key = row?.dataset.key;
      if (key !== undefined) {
        // A detail row that replaced the remembered one takes its state.
        const previous = this.#remembered.get(key);
        if (
          previous &&
          previous.detailRow !== detailRow &&
          previous.open !== this.#isOpen(detailRow)
        ) {
          detailRow.hidden = !previous.open;
          detailRow.dataset.state = previous.open ? "open" : "closed";
          restored.push(detailRow);
        }
        // A row still animating closed counts as closed.
        remembered.set(key, { detailRow, open: this.#isOpen(detailRow) });
      }

      const cell = detailRow.cells[0];
      if (cell && !cell.hasAttribute("colspan")) {
        const columns = this.#columnCount(detailRow);
        if (columns > 1) cell.colSpan = columns;
      }

      for (const toggle of row?.querySelectorAll(TOGGLE) ?? []) {
        if (!toggle.hasAttribute("aria-controls") && !toggle.hasAttribute("data-all")) {
          toggle.setAttribute("aria-controls", detailRow.id);
        }
      }
    }
    this.#remembered = remembered;
    this.#syncToggles();
    this.#measure();
    for (const detailRow of restored) this.#dispatch(detailRow);
  }

  /** @param {HTMLTableRowElement} detailRow */
  #columnCount(detailRow) {
    const table = detailRow.closest("table");
    const reference = table?.tHead?.rows[0] ?? this.#rowFor(detailRow);
    let count = 0;
    for (const cell of reference?.cells ?? []) count += cell.colSpan;
    return count;
  }

  #syncToggles() {
    const rows = this.detailRows;
    for (const toggle of /** @type {NodeListOf<HTMLElement>} */ (
      this.#host.querySelectorAll(TOGGLE)
    )) {
      if (!this.#owns(toggle)) continue;
      if (toggle.hasAttribute("data-all")) {
        if (toggle.hidden !== this.#single) toggle.hidden = this.#single;
        const allOpen = rows.length > 0 && rows.every((row) => this.#isOpen(row));
        setAttributeIfChanged(toggle, "aria-expanded", String(allOpen));
        continue;
      }
      const detailRow = this.#detailRowFor(toggle);
      setAttributeIfChanged(
        toggle,
        "aria-expanded",
        String(!!detailRow && this.#isOpen(detailRow)),
      );
    }
  }

  /**
   * Publishes the table container's visible width, which the detail content stays pinned to
   * when the table scrolls sideways, and the content's start and end padding: in line with
   * the row's first cell, or with the first column after a leading toggle column for the
   * aligned inset. Padding is measured rather than set in CSS so it follows each theme's
   * cell padding and any edge padding the table's container adds.
   */
  #measure() {
    const container =
      /** @type {HTMLElement | null} */ (
        this.#host.querySelector('[data-slot="table-container"]')
      ) ?? this.#host;
    this.#host.style.setProperty("--sa-table-row-detail-width", `${container.clientWidth}px`);

    // Measured from the first row that has a toggle: a row without one (a row a script
    // replaced, say) has no toggle column to align with.
    const detailRows = this.detailRows;
    const detailRow =
      detailRows.find((candidate) => this.#rowFor(candidate)?.querySelector(TOGGLE)) ??
      detailRows[0];
    const row = detailRow && this.#rowFor(detailRow);
    const detailCell = detailRow?.cells[0];
    const firstCell = row?.cells[0];
    const lastCell = row?.cells[row.cells.length - 1];
    if (!detailCell || !firstCell || !lastCell) return;

    const detailStyle = getComputedStyle(detailCell);
    // Aligned: the column after the toggle's column, when the toggle leads the row (it may
    // follow a selection column); a trailing toggle has no column after it.
    const cells = Array.from(row.cells);
    const toggleIndex = cells.findIndex((cell) => cell.querySelector(TOGGLE));
    const alignedCell =
      this.#host.dataset.inset === "aligned" && toggleIndex >= 0
        ? (cells[toggleIndex + 1] ?? null)
        : null;
    const startCell = alignedCell ?? firstCell;
    const start =
      startCell.offsetLeft +
      parseFloat(getComputedStyle(startCell).paddingLeft) -
      parseFloat(detailStyle.paddingLeft);
    const end =
      parseFloat(getComputedStyle(lastCell).paddingRight) - parseFloat(detailStyle.paddingRight);

    this.#host.style.setProperty("--sa-table-row-detail-inset-start", `${Math.max(start, 0)}px`);
    this.#host.style.setProperty("--sa-table-row-detail-inset-end", `${Math.max(end, 0)}px`);
  }
}
