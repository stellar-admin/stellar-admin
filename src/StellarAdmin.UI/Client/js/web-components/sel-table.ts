import { LitElement } from "lit";
import { customElement } from "lit/decorators.js";
import { TableEditing } from "./sel-table/editing";
import { TableRowDetails } from "./sel-table/row-details";
import { TableSelection } from "./sel-table/selection";

/**
 * Table web component. Wrap it around a table to add the behaviors its data attributes
 * switch on, read when the element connects:
 *   - data-selection: row selection with a select-all checkbox (see `TableSelection`)
 *   - data-row-details: rows that expand to show details (see `TableRowDetails`)
 *   - data-editing: rows that are edited in place (see `TableEditing`)
 *
 * Each behavior listens on this element only, so rows that htmx (or any script) adds,
 * replaces or removes need no re-initialization. Scripts read the selection from
 * `selectedValues` and drive the details with `expandAll()`, `collapseAll()`,
 * `expand()`, `collapse()` and `toggle()`; wait for `customElements.whenDefined("sel-table")`
 * before reading the initial state.
 */
@customElement("sel-table")
export class Table extends LitElement {
  #editing: TableEditing | null = null;
  #rowDetails: TableRowDetails | null = null;
  #selection: TableSelection | null = null;

  override createRenderRoot() {
    return this;
  }

  override connectedCallback() {
    super.connectedCallback();
    if (this.hasAttribute("data-selection")) {
      this.#selection = new TableSelection(this);
      this.#selection.connect();
    }
    if (this.hasAttribute("data-row-details")) {
      this.#rowDetails = new TableRowDetails(this);
      this.#rowDetails.connect();
    }
    if (this.hasAttribute("data-editing")) {
      this.#editing = new TableEditing(this);
      this.#editing.connect();
    }
  }

  override disconnectedCallback() {
    super.disconnectedCallback();
    this.#selection?.disconnect();
    this.#selection = null;
    this.#rowDetails?.disconnect();
    this.#rowDetails = null;
    this.#editing?.disconnect();
    this.#editing = null;
  }

  /** The values of the selected rows, including selected rows in edit mode. */
  get selectedValues(): string[] {
    return this.#selection?.selectedValues ?? [];
  }

  /** The detail rows this table controls (excluding those of nested tables). */
  get detailRows(): HTMLTableRowElement[] {
    return this.#rowDetails?.detailRows ?? [];
  }

  /** Expands a detail row. */
  expand(detailRow: HTMLTableRowElement) {
    this.#rowDetails?.expand(detailRow);
  }

  /** Collapses a detail row. */
  collapse(detailRow: HTMLTableRowElement) {
    this.#rowDetails?.collapse(detailRow);
  }

  /** Expands a collapsed detail row, or collapses an expanded one. */
  toggle(detailRow: HTMLTableRowElement) {
    this.#rowDetails?.toggle(detailRow);
  }

  /** Expands every detail row. In single mode only the first row is expanded. */
  expandAll() {
    this.#rowDetails?.expandAll();
  }

  /** Collapses every detail row. */
  collapseAll() {
    this.#rowDetails?.collapseAll();
  }
}
