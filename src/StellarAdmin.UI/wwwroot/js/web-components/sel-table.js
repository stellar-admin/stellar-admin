import { TableEditing } from "./sel-table/editing.js";
import { TableRowDetails } from "./sel-table/row-details.js";
import { TableSelection } from "./sel-table/selection.js";

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
export class Table extends HTMLElement {
  /** @type {TableEditing | null} */
  #editing = null;
  /** @type {TableRowDetails | null} */
  #rowDetails = null;
  /** @type {TableSelection | null} */
  #selection = null;

  connectedCallback() {
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

  disconnectedCallback() {
    this.#selection?.disconnect();
    this.#selection = null;
    this.#rowDetails?.disconnect();
    this.#rowDetails = null;
    this.#editing?.disconnect();
    this.#editing = null;
  }

  /**
   * The values of the selected rows, including selected rows in edit mode.
   * @returns {string[]}
   */
  get selectedValues() {
    return this.#selection?.selectedValues ?? [];
  }

  /**
   * The detail rows this table controls (excluding those of nested tables).
   * @returns {HTMLTableRowElement[]}
   */
  get detailRows() {
    return this.#rowDetails?.detailRows ?? [];
  }

  /**
   * Expands a detail row.
   * @param {HTMLTableRowElement} detailRow
   */
  expand(detailRow) {
    this.#rowDetails?.expand(detailRow);
  }

  /**
   * Collapses a detail row.
   * @param {HTMLTableRowElement} detailRow
   */
  collapse(detailRow) {
    this.#rowDetails?.collapse(detailRow);
  }

  /**
   * Expands a collapsed detail row, or collapses an expanded one.
   * @param {HTMLTableRowElement} detailRow
   */
  toggle(detailRow) {
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

if (!customElements.get("sel-table")) customElements.define("sel-table", Table);
