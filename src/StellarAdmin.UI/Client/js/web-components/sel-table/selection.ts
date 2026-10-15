/**
 * Row selection for a `sel-table` with `data-selection`. The checkbox inside `thead` is
 * the select-all; the checkboxes inside `tbody` select their row. When any row checkbox
 * is marked `data-row-select`, only the marked ones select rows, so other checkboxes in
 * the cells (a switch editor, say) are left alone.
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
 */
export class TableSelection {
  #host: HTMLElement;

  constructor(host: HTMLElement) {
    this.#host = host;
  }

  connect() {
    this.#host.addEventListener("change", this.#onChange);

    // Reflect any server-rendered checked state (row highlights, select-all
    // checked/indeterminate) as soon as the component connects.
    this.#syncState();
  }

  disconnect() {
    this.#host.removeEventListener("change", this.#onChange);
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
    const checkboxes = Array.from(
      this.#host.querySelectorAll<HTMLInputElement>('tbody input[type="checkbox"]'),
    ).filter((checkbox) => this.#owns(checkbox));
    const marked = checkboxes.filter((checkbox) => checkbox.hasAttribute("data-row-select"));
    return marked.length > 0 ? marked : checkboxes;
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

  #syncState() {
    const rowCheckboxes = this.#rowCheckboxes;
    let selectedCount = 0;

    for (const checkbox of rowCheckboxes) {
      const row = checkbox.closest("tr");
      if (checkbox.checked) {
        selectedCount++;
        row?.setAttribute("data-state", "selected");
      } else {
        row?.removeAttribute("data-state");
      }
    }

    const selectAll = this.#selectAll;
    if (selectAll) {
      selectAll.checked = rowCheckboxes.length > 0 && selectedCount === rowCheckboxes.length;
      selectAll.indeterminate = selectedCount > 0 && selectedCount < rowCheckboxes.length;
    }
  }
}
