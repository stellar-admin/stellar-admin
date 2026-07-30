import { LitElement } from "lit";
import { customElement } from "lit/decorators.js";

/**
 * Table row-selection web component. Wrap it around a table whose header row
 * contains a select-all checkbox and whose body rows each contain a selection
 * checkbox — no marker attributes needed: the checkbox inside `thead` is the
 * select-all, the checkboxes inside `tbody` select their row.
 *
 * The component keeps three things in sync as checkboxes toggle:
 *   - the select-all checkbox's checked/indeterminate state
 *   - data-state="selected" on each selected row's `tr` (the theme styles it)
 *   - toggling every (non-disabled) row checkbox when select-all changes
 *
 * Selection state lives entirely in the checkboxes themselves, so it posts as
 * ordinary form data. Without JavaScript the row checkboxes still work and
 * still post; only select-all and the row highlight are lost.
 *
 * Scripts read the current selection from the `selectedValues` property, and a
 * bubbling `selection-change` custom event (detail: `{ values }`) fires on every
 * user-driven change. It does NOT fire for the initial server-rendered state —
 * initialize from `selectedValues` (after `customElements.whenDefined`) instead.
 */
@customElement("sel-table-selection")
export class TableSelection extends LitElement {
  override createRenderRoot() {
    return this;
  }

  override connectedCallback() {
    super.connectedCallback();
    this.addEventListener("change", this.#onChange);

    // Reflect any server-rendered checked state (row highlights, select-all
    // checked/indeterminate) as soon as the component connects.
    this.#syncState();
  }

  override disconnectedCallback() {
    super.disconnectedCallback();
    this.removeEventListener("change", this.#onChange);
  }

  get #selectAll(): HTMLInputElement | null {
    return this.querySelector<HTMLInputElement>('thead input[type="checkbox"]');
  }

  get #rowCheckboxes(): HTMLInputElement[] {
    return Array.from(this.querySelectorAll<HTMLInputElement>('tbody input[type="checkbox"]'));
  }

  /** The values of the currently selected row checkboxes. */
  get selectedValues(): string[] {
    return this.#rowCheckboxes
      .filter((checkbox) => checkbox.checked)
      .map((checkbox) => checkbox.value);
  }

  #onChange = (event: Event) => {
    const target = event.target;
    if (!(target instanceof HTMLInputElement) || target.type !== "checkbox") {
      return;
    }

    if (target === this.#selectAll) {
      for (const checkbox of this.#rowCheckboxes) {
        if (!checkbox.disabled) {
          checkbox.checked = target.checked;
        }
      }
    }

    this.#syncState();
    this.dispatchEvent(
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
