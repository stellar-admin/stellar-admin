const EDITING_ROW = "tr[data-editing]";
const CANCEL = "[data-row-cancel]";
const LOCKED = "data-columns-locked";

/**
 * Inline editing for a `sel-table` with `data-editing`. A row in edit mode is a
 * `tr[data-editing]` with the same `data-key` as the row it shows; htmx (or any script)
 * puts it in the place of that row, and puts a display row back after a save.
 *
 * The behavior:
 *   - locks the column widths while any row is in edit mode, so editors cannot move the
 *     columns, and releases them when no row is. Widths are tracked while no row is in edit
 *     mode, since an editing row is in the page before the behavior sees it.
 *   - keeps the display row an editing row replaced, and puts a copy of it back when a
 *     `[data-row-cancel]` element in the editing row is clicked, without the click's own
 *     action (an htmx request, say); htmx, when on the page, processes the copy. A new row (`data-key="new"`) is removed instead. Only
 *     a row that was rendered in edit mode lets the click through, so it can ask the server
 *     for its display row.
 */
interface Widths {
  table: number;
  heads: number[];
  // Whether the table fills its container, so its columns lock as shares of its width.
  fills: boolean;
}

export class TableEditing {
  #host: HTMLElement;
  // The display rows that editing rows replaced, by key.
  #displayRows = new Map<string, HTMLTableRowElement>();
  #mutationObserver = new MutationObserver((records) => this.#onMutation(records));
  #resizeObserver = new ResizeObserver(() => this.#track());
  #observed: Element[] = [];
  // The header widths and the table width while no row was in edit mode.
  #widths: Widths | null = null;
  // The header cells' own style attributes, put back when the columns are released.
  #headStyles: (string | null)[] = [];

  constructor(host: HTMLElement) {
    this.#host = host;
  }

  connect() {
    this.#host.addEventListener("click", this.#onClick, true);
    this.#mutationObserver.observe(this.#host, { childList: true, subtree: true });
    this.#observeHeads();
    this.#track();
    this.#sync();
  }

  disconnect() {
    this.#host.removeEventListener("click", this.#onClick, true);
    this.#mutationObserver.disconnect();
    this.#resizeObserver.disconnect();
    this.#observed = [];
  }

  get #table(): HTMLTableElement | null {
    return (
      Array.from(this.#host.querySelectorAll("table")).find((table) => this.#owns(table)) ?? null
    );
  }

  get #heads(): HTMLTableCellElement[] {
    return Array.from(this.#table?.tHead?.rows[0]?.cells ?? []);
  }

  get #editingRows(): HTMLTableRowElement[] {
    return Array.from(this.#host.querySelectorAll<HTMLTableRowElement>(EDITING_ROW)).filter((row) =>
      this.#owns(row),
    );
  }

  #owns(element: Element) {
    return element.closest("sel-table") === this.#host;
  }

  /** Observes the table and its header cells, again when they have been replaced. */
  #observeHeads() {
    const table = this.#table;
    const observed = table ? [table, ...this.#heads] : [];
    if (
      observed.length === this.#observed.length &&
      observed.every((element, index) => element === this.#observed[index])
    ) {
      return;
    }
    this.#resizeObserver.disconnect();
    for (const element of observed) this.#resizeObserver.observe(element);
    this.#observed = observed;
  }

  /** Records the column widths, unless they are locked. */
  #track() {
    const table = this.#table;
    if (!table || table.hasAttribute(LOCKED)) return;
    this.#widths = this.#measure(table);
  }

  #measure(table: HTMLTableElement): Widths {
    const width = table.getBoundingClientRect().width;
    return {
      table: width,
      heads: this.#heads.map((head) => head.getBoundingClientRect().width),
      fills: Math.abs(width - (table.parentElement?.clientWidth ?? 0)) < 1,
    };
  }

  #onMutation(records: MutationRecord[]) {
    // Display rows that editing rows took the place of, by key.
    const removed = new Map<string, HTMLTableRowElement>();
    for (const record of records) {
      for (const node of record.removedNodes) {
        if (
          node instanceof HTMLTableRowElement &&
          node.dataset.key !== undefined &&
          !node.matches(EDITING_ROW) &&
          !node.isConnected
        ) {
          removed.set(node.dataset.key, node);
        }
      }
    }
    for (const row of this.#editingRows) {
      const displayRow = removed.get(row.dataset.key ?? "");
      if (displayRow) this.#displayRows.set(row.dataset.key!, displayRow);
    }

    this.#observeHeads();
    this.#sync();
  }

  /** Locks or releases the columns, and forgets display rows whose editing row has gone. */
  #sync() {
    const editingRows = this.#editingRows;
    const keys = new Set(editingRows.map((row) => row.dataset.key));
    for (const key of this.#displayRows.keys()) {
      if (!keys.has(key)) this.#displayRows.delete(key);
    }

    if (editingRows.length > 0) this.#lock();
    else this.#release();
  }

  #lock() {
    const table = this.#table;
    const heads = this.#heads;
    if (!table || heads.length === 0 || table.hasAttribute(LOCKED)) return;

    // A table rendered with rows in edit mode has no earlier widths: it keeps its current ones.
    const widths =
      this.#widths?.heads.length === heads.length ? this.#widths : this.#measure(table);
    this.#headStyles = heads.map((head) => head.getAttribute("style"));
    // A table that fills its container keeps doing so, its columns keeping their shares when the
    // container resizes; a wider table keeps its width.
    for (const [index, head] of heads.entries()) {
      head.style.width = widths.fills
        ? `${(widths.heads[index] / widths.table) * 100}%`
        : `${widths.heads[index]}px`;
    }
    if (!widths.fills) table.style.width = `${widths.table}px`;
    table.style.tableLayout = "fixed";
    table.setAttribute(LOCKED, "");
  }

  #release() {
    const table = this.#table;
    if (!table?.hasAttribute(LOCKED)) return;

    for (const [index, head] of this.#heads.entries()) {
      const style = this.#headStyles[index];
      if (style == null) head.removeAttribute("style");
      else head.setAttribute("style", style);
    }
    this.#headStyles = [];
    table.style.width = "";
    table.style.tableLayout = "";
    if (table.getAttribute("style") === "") table.removeAttribute("style");
    table.removeAttribute(LOCKED);
  }

  #onClick = (event: MouseEvent) => {
    const target = event.target;
    if (!(target instanceof Element)) return;
    const cancel = target.closest(CANCEL);
    const row = cancel?.closest<HTMLTableRowElement>(EDITING_ROW);
    if (!row || !this.#owns(row)) return;

    const key = row.dataset.key ?? "";
    const displayRow = this.#displayRows.get(key);
    if (!displayRow && key !== "new") return;

    // Handled here: the cancel button's own action (a request for the display row) is not needed.
    event.preventDefault();
    event.stopPropagation();
    if (displayRow) {
      this.#displayRows.delete(key);
      // A copy of the kept row: htmx removed its handlers from the row it swapped out but may still
      // mark the row as set up, and a copy carries no such state, so htmx sets it up again.
      const restored = displayRow.cloneNode(true) as HTMLTableRowElement;
      row.replaceWith(restored);
      (window as { htmx?: { process?: (element: Element) => void } }).htmx?.process?.(restored);
    } else {
      row.remove();
    }
  };
}
