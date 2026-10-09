import { LitElement } from "lit";
import { customElement } from "lit/decorators.js";

const DETAIL_ROW = 'tr[data-slot="table-row-detail"]';
const TOGGLE = '[data-slot="table-row-detail-toggle"]';

// Clicks on these keep their own behavior instead of toggling the row.
const INTERACTIVE =
  'a, button, input, select, textarea, label, summary, [role="button"], [contenteditable], [data-no-row-toggle]';

let idCounter = 0;

function setAttributeIfChanged(element: Element, name: string, value: string) {
  if (element.getAttribute(name) !== value) element.setAttribute(name, value);
}

/**
 * Table row-details web component. Wrap it around a table in which an expandable
 * row is followed by a `tr[data-slot="table-row-detail"]`, and a toggle button
 * (`[data-slot="table-row-detail-toggle"]`) sits in a cell of the row. A toggle
 * with `aria-controls` controls that detail row instead; a toggle with
 * `data-all` expands or collapses every row.
 *
 * All state lives in the DOM — `hidden` and `data-state="open|closed"` on the
 * detail row, `aria-expanded` on the toggles — and the component listens on
 * itself only. Rows that htmx (or any script) adds, replaces or removes need no
 * re-initialization, and server-rendered expanded rows are respected.
 *
 * Settings are read from the element's data attributes on every interaction:
 *   - data-expand-mode="single|multiple"
 *   - data-row-click="true|false"
 *   - data-inset="aligned|bleed"  (aligned measures the first column after a
 *     leading toggle column into --sa-table-row-detail-inset)
 *
 * Expanding or collapsing a row raises a bubbling `row-detail-expand` or
 * `row-detail-collapse` event on the detail row (detail: `{ row, detailRow }`),
 * after the change, so the expanded content is visible when the event fires.
 * Server-rendered state raises no events. Because the event fires on the detail
 * row, htmx attributes on that row need no `from:` modifier:
 * `hx-get="…" hx-trigger="row-detail-expand once" hx-target="find [data-slot=table-row-detail-content]"`.
 */
@customElement("sel-table-row-details")
export class TableRowDetails extends LitElement {
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
  #syncScheduled = false;

  override createRenderRoot() {
    return this;
  }

  override connectedCallback() {
    super.connectedCallback();
    this.addEventListener("click", this.#onClick);
    this.#mutationObserver.observe(this, {
      childList: true,
      subtree: true,
      attributes: true,
      attributeFilter: ["hidden"],
    });
    this.#resizeObserver.observe(this);
    this.#sync();
  }

  override disconnectedCallback() {
    super.disconnectedCallback();
    this.removeEventListener("click", this.#onClick);
    this.#mutationObserver.disconnect();
    this.#resizeObserver.disconnect();
  }

  /** The detail rows this element controls (excluding those of nested tables). */
  get detailRows(): HTMLTableRowElement[] {
    return Array.from(this.querySelectorAll<HTMLTableRowElement>(DETAIL_ROW)).filter((row) =>
      this.#owns(row),
    );
  }

  /** Expands a detail row. */
  expand(detailRow: HTMLTableRowElement) {
    this.#setExpanded(detailRow, true);
  }

  /** Collapses a detail row. */
  collapse(detailRow: HTMLTableRowElement) {
    this.#setExpanded(detailRow, false);
  }

  /** Expands a collapsed detail row, or collapses an expanded one. */
  toggle(detailRow: HTMLTableRowElement) {
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
    return this.dataset.expandMode === "single";
  }

  #owns(element: Element) {
    return element.closest("sel-table-row-details") === this;
  }

  #isOpen(detailRow: HTMLTableRowElement) {
    return detailRow.dataset.state === "open";
  }

  /** The detail row a toggle controls: its aria-controls target, else the row after its own. */
  #detailRowFor(toggle: Element): HTMLTableRowElement | null {
    const controls = toggle.getAttribute("aria-controls");
    if (controls) {
      const target = document.getElementById(controls);
      return target instanceof HTMLTableRowElement && target.matches(DETAIL_ROW) ? target : null;
    }
    const next = toggle.closest("tr")?.nextElementSibling;
    return next instanceof HTMLTableRowElement && next.matches(DETAIL_ROW) ? next : null;
  }

  /** The expandable row a detail row belongs to: the row before it. */
  #rowFor(detailRow: HTMLTableRowElement): HTMLTableRowElement | null {
    const previous = detailRow.previousElementSibling;
    return previous instanceof HTMLTableRowElement && !previous.matches(DETAIL_ROW)
      ? previous
      : null;
  }

  #onClick = (event: MouseEvent) => {
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

    if (this.dataset.rowClick !== "true") return;
    if (target.closest(INTERACTIVE)) return;
    if (window.getSelection()?.toString()) return;

    const row = target.closest("tr");
    if (!row || !this.#owns(row) || row.matches(DETAIL_ROW) || row.closest("thead, tfoot")) return;
    const next = row.nextElementSibling;
    if (next instanceof HTMLTableRowElement && next.matches(DETAIL_ROW)) this.toggle(next);
  };

  #setExpanded(detailRow: HTMLTableRowElement, open: boolean) {
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
    detailRow.dispatchEvent(
      new CustomEvent(open ? "row-detail-expand" : "row-detail-collapse", {
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
    for (const detailRow of this.detailRows) {
      if (!detailRow.id) detailRow.id = `--sa-row-detail-${++idCounter}`;
      // Rows inserted without a state take it from `hidden`; a hidden row is never open.
      // (A closed row that is not yet hidden is still animating closed.)
      if (!detailRow.dataset.state || (detailRow.hidden && this.#isOpen(detailRow))) {
        detailRow.dataset.state = detailRow.hidden ? "closed" : "open";
      }

      const cell = detailRow.cells[0];
      if (cell && !cell.hasAttribute("colspan")) {
        const columns = this.#columnCount(detailRow);
        if (columns > 1) cell.colSpan = columns;
      }

      const row = this.#rowFor(detailRow);
      for (const toggle of row?.querySelectorAll(TOGGLE) ?? []) {
        if (!toggle.hasAttribute("aria-controls") && !toggle.hasAttribute("data-all")) {
          toggle.setAttribute("aria-controls", detailRow.id);
        }
      }
    }
    this.#syncToggles();
    this.#measure();
  }

  #columnCount(detailRow: HTMLTableRowElement) {
    const table = detailRow.closest("table");
    const reference = table?.tHead?.rows[0] ?? this.#rowFor(detailRow);
    let count = 0;
    for (const cell of reference?.cells ?? []) count += cell.colSpan;
    return count;
  }

  #syncToggles() {
    const rows = this.detailRows;
    for (const toggle of this.querySelectorAll<HTMLElement>(TOGGLE)) {
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
    const container = this.querySelector<HTMLElement>('[data-slot="table-container"]') ?? this;
    this.style.setProperty("--sa-table-row-detail-width", `${container.clientWidth}px`);

    const detailRow = this.detailRows[0];
    const row = detailRow && this.#rowFor(detailRow);
    const detailCell = detailRow?.cells[0];
    const firstCell = row?.cells[0];
    const lastCell = row?.cells[row.cells.length - 1];
    if (!detailCell || !firstCell || !lastCell) return;

    const detailStyle = getComputedStyle(detailCell);
    const alignedCell =
      this.dataset.inset === "aligned" && firstCell.querySelector(TOGGLE) ? row.cells[1] : null;
    const startCell = alignedCell ?? firstCell;
    const start =
      startCell.offsetLeft +
      parseFloat(getComputedStyle(startCell).paddingLeft) -
      parseFloat(detailStyle.paddingLeft);
    const end =
      parseFloat(getComputedStyle(lastCell).paddingRight) - parseFloat(detailStyle.paddingRight);

    this.style.setProperty("--sa-table-row-detail-inset-start", `${Math.max(start, 0)}px`);
    this.style.setProperty("--sa-table-row-detail-inset-end", `${Math.max(end, 0)}px`);
  }
}
