/**
 * Row selection for a `sel-table` with `data-selection`. The checkbox inside `thead` is
 * the select-all; the checkboxes inside `tbody` marked `data-row-select` select their row.
 * Other checkboxes in the cells (a switch editor, say) are left alone.
 *
 * The behavior keeps three things in sync as checkboxes toggle:
 *   - the select-all checkbox's checked/indeterminate state
 *   - data-state="selected" on each selected row's `tr` (the theme styles it)
 *   - toggling every (non-disabled) row checkbox when select-all changes
 *
 * Selection state lives entirely in the checkboxes themselves, so it posts as
 * ordinary form data. Without JavaScript the row checkboxes still work and
 * still post; only select-all and the row highlight are lost.
 *
 * A bubbling `selection-change` custom event (detail: `{ values }`) fires on the table
 * on every user-driven change. It does NOT fire for the initial server-rendered state.
 *
 * Rows with a `data-key` are remembered by key: when a script or htmx replaces a row, the
 * row that takes its place with the same key gets its selection back, whatever the new
 * markup says. Rows with a key not seen before keep their rendered state.
 */
export class TableSelection {
  #host: HTMLElement;
  // The keyed rows present at the last sync, and whether each was selected.
  #remembered = new Map<string, { row: HTMLTableRowElement; selected: boolean }>();
  #mutationObserver = new MutationObserver((records) => {
    if (records.some((record) => record.type === "childList")) this.#scheduleSync();
  });
  #syncScheduled = false;

  constructor(host: HTMLElement) {
    this.#host = host;
  }

  connect() {
    this.#host.addEventListener("change", this.#onChange);
    this.#mutationObserver.observe(this.#host, { childList: true, subtree: true });

    // Reflect any server-rendered checked state (row highlights, select-all
    // checked/indeterminate) as soon as the component connects.
    this.#syncState();
  }

  disconnect() {
    this.#host.removeEventListener("change", this.#onChange);
    this.#mutationObserver.disconnect();
  }

  /** The values of the currently selected row checkboxes. */
  get selectedValues(): string[] {
    return this.#rowCheckboxes
      .filter((checkbox) => checkbox.checked)
      .map((checkbox) => checkbox.value);
  }

  get #selectAll(): HTMLInputElement | null {
    return (
      Array.from(
        this.#host.querySelectorAll<HTMLInputElement>('thead input[type="checkbox"]'),
      ).find((checkbox) => this.#owns(checkbox)) ?? null
    );
  }

  get #rowCheckboxes(): HTMLInputElement[] {
    return Array.from(
      this.#host.querySelectorAll<HTMLInputElement>(
        'tbody input[type="checkbox"][data-row-select]',
      ),
    ).filter((checkbox) => this.#owns(checkbox));
  }

  /**
   * Whether a checkbox belongs to this table's selection: not inside a nested table, and
   * not inside a row's details (`tr[data-slot="table-row-detail"]`) — details content is
   * the author's, so its checkboxes are not row selectors.
   */
  #owns(checkbox: HTMLInputElement) {
    if (checkbox.closest("sel-table") !== this.#host) return false;
    const detailRow = checkbox.closest('tr[data-slot="table-row-detail"]');
    return !detailRow || !this.#host.contains(detailRow);
  }

  #onChange = (event: Event) => {
    const target = event.target;
    if (
      !(target instanceof HTMLInputElement) ||
      target.type !== "checkbox" ||
      !this.#owns(target)
    ) {
      return;
    }

    const rowCheckboxes = this.#rowCheckboxes;
    if (target === this.#selectAll) {
      for (const checkbox of rowCheckboxes) {
        if (!checkbox.disabled) {
          checkbox.checked = target.checked;
        }
      }
    } else if (!rowCheckboxes.includes(target)) {
      return;
    }

    this.#syncState();
    this.#host.dispatchEvent(
      new CustomEvent("selection-change", {
        bubbles: true,
        detail: { values: this.selectedValues },
      }),
    );
  };

  #scheduleSync() {
    if (this.#syncScheduled) return;
    this.#syncScheduled = true;
    queueMicrotask(() => {
      this.#syncScheduled = false;
      this.#syncState();
    });
  }

  #syncState() {
    const rowCheckboxes = this.#rowCheckboxes;
    const remembered = new Map<string, { row: HTMLTableRowElement; selected: boolean }>();
    let selectedCount = 0;

    for (const checkbox of rowCheckboxes) {
      const row = checkbox.closest("tr");
      const key = row?.dataset.key;
      if (row && key !== undefined) {
        // A row that replaced the remembered row takes its selection.
        const previous = this.#remembered.get(key);
        if (previous && previous.row !== row) checkbox.checked = previous.selected;
        remembered.set(key, { row, selected: checkbox.checked });
      }
      if (checkbox.checked) {
        selectedCount++;
        row?.setAttribute("data-state", "selected");
      } else {
        row?.removeAttribute("data-state");
      }
    }

    this.#remembered = remembered;

    const selectAll = this.#selectAll;
    if (selectAll) {
      selectAll.checked = rowCheckboxes.length > 0 && selectedCount === rowCheckboxes.length;
      selectAll.indeterminate = selectedCount > 0 && selectedCount < rowCheckboxes.length;
    }
  }
}
