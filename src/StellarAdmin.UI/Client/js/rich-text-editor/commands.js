import { defineKeymap, Priority, withPriority } from "prosekit/core";
import { TextSelection } from "prosekit/pm/state";
import {
  addRowAfter,
  columnIsHeader,
  goToNextCell,
  isInTable,
  rowIsHeader,
  selectedRect,
  toggleHeader,
} from "prosemirror-tables";

/** @typedef {import("prosekit/core").Editor<import("prosekit/basic").BasicExtension>} RichTextEditor */

/**
 * What a toolbar control, bubble control or slash menu item does. `active` checks a toggle,
 * `enabled` disables a button when the command can't run.
 *
 * @typedef {object} Action
 * @property {(editor: RichTextEditor, host: { pickImage(): void; openLinkPopover(): void }) => void} run
 * @property {(editor: RichTextEditor) => boolean} [active]
 * @property {(editor: RichTextEditor) => boolean} [enabled]
 */

/** @type {Record<string, Action>} */
export const actions = {
  undo: { run: (e) => e.commands.undo(), enabled: (e) => e.commands.undo.canExec() },
  redo: { run: (e) => e.commands.redo(), enabled: (e) => e.commands.redo.canExec() },
  bold: { run: (e) => e.commands.toggleBold(), active: (e) => e.marks.bold.isActive() },
  italic: { run: (e) => e.commands.toggleItalic(), active: (e) => e.marks.italic.isActive() },
  underline: {
    run: (e) => e.commands.toggleUnderline(),
    active: (e) => e.marks.underline.isActive(),
  },
  strike: { run: (e) => e.commands.toggleStrike(), active: (e) => e.marks.strike.isActive() },
  code: { run: (e) => e.commands.toggleCode(), active: (e) => e.marks.code.isActive() },
  "bullet-list": {
    run: (e) => e.commands.toggleList({ kind: "bullet" }),
    active: (e) => e.nodes.list.isActive({ kind: "bullet" }),
  },
  "ordered-list": {
    run: (e) => e.commands.toggleList({ kind: "ordered" }),
    active: (e) => e.nodes.list.isActive({ kind: "ordered" }),
  },
  blockquote: {
    run: (e) => e.commands.toggleBlockquote(),
    active: (e) => e.nodes.blockquote.isActive(),
  },
  "code-block": {
    run: (e) => e.commands.toggleCodeBlock(),
    active: (e) => e.nodes.codeBlock.isActive(),
  },
  "horizontal-rule": { run: (e) => insertHorizontalRule(e) },
  table: { run: (e) => insertTable(e), enabled: (e) => !e.nodes.table.isActive() },
  image: { run: (_, host) => host.pickImage() },
  link: { run: (_, host) => host.openLinkPopover(), active: (e) => e.marks.link.isActive() },
};

/**
 * The block types of the toolbar's block menu and the slash menu.
 *
 * @type {Record<string, { run(editor: RichTextEditor): void; active(editor: RichTextEditor): boolean }>}
 */
export const blockTypes = {
  paragraph: { run: (e) => e.commands.setParagraph(), active: (e) => e.nodes.paragraph.isActive() },
  h1: {
    run: (e) => e.commands.setHeading({ level: 1 }),
    active: (e) => e.nodes.heading.isActive({ level: 1 }),
  },
  h2: {
    run: (e) => e.commands.setHeading({ level: 2 }),
    active: (e) => e.nodes.heading.isActive({ level: 2 }),
  },
  h3: {
    run: (e) => e.commands.setHeading({ level: 3 }),
    active: (e) => e.nodes.heading.isActive({ level: 3 }),
  },
};

/**
 * The table handle menus' items.
 *
 * @type {Record<string, { run(editor: RichTextEditor): void; enabled?(editor: RichTextEditor): boolean; active?(editor: RichTextEditor): boolean }>}
 */
export const tableActions = {
  "column-before": { run: (e) => e.commands.addTableColumnBefore() },
  "column-after": { run: (e) => e.commands.addTableColumnAfter() },
  "row-above": { run: (e) => e.commands.addTableRowAbove() },
  "row-below": { run: (e) => e.commands.addTableRowBelow() },
  "header-row": {
    run: (e) => e.exec(toggleHeader("row")),
    active: (e) => headerActive(e, "row"),
  },
  "header-column": {
    run: (e) => e.exec(toggleHeader("column")),
    active: (e) => headerActive(e, "column"),
  },
  merge: {
    run: (e) => e.commands.mergeTableCells(),
    enabled: (e) => e.commands.mergeTableCells.canExec(),
  },
  split: {
    run: (e) => e.commands.splitTableCell(),
    enabled: (e) => e.commands.splitTableCell.canExec(),
  },
  clear: { run: (e) => e.commands.deleteCellSelection() },
  "delete-column": { run: (e) => e.commands.deleteTableColumn() },
  "delete-row": { run: (e) => e.commands.deleteTableRow() },
  "delete-table": { run: (e) => e.commands.deleteTable() },
};

/**
 * @param {RichTextEditor} e
 * @param {"row" | "column"} role
 */
function headerActive(e, role) {
  if (!isInTable(e.state)) return false;
  const rect = selectedRect(e.state);
  return role === "row"
    ? rowIsHeader(rect.map, rect.table, rect.top)
    : columnIsHeader(rect.map, rect.table, rect.left);
}

/**
 * Inserts a three by three table with a header row and puts the cursor in its first cell.
 *
 * @param {RichTextEditor} e
 */
function insertTable(e) {
  const from = e.state.selection.from;
  e.commands.insertTable({ row: 3, col: 3, header: true });
  let tablePos = -1;
  e.state.doc.nodesBetween(Math.max(0, from - 2), e.state.doc.content.size, (node, pos) => {
    if (tablePos >= 0) return false;
    if (node.type.name === "table") tablePos = pos;
  });
  if (tablePos >= 0) {
    const { state } = e.view;
    e.view.dispatch(state.tr.setSelection(TextSelection.near(state.doc.resolve(tablePos + 1))));
  }
}

/**
 * Inserts a divider. An empty block the cursor is in becomes the divider instead of staying above
 * it, as the Markdown shortcut `---` does.
 *
 * @param {RichTextEditor} e
 */
function insertHorizontalRule(e) {
  const { $from, empty } = e.state.selection;
  const block = $from.parent;
  if (empty && block.isTextblock && block.content.size === 0 && $from.depth > 0) {
    const start = $from.before();
    const tr = e.state.tr.delete(start, $from.after());
    e.view.dispatch(tr.setSelection(TextSelection.near(tr.doc.resolve(start))));
  }
  e.commands.insertHorizontalRule();
}

/** Tab moves to the next cell, and adds a row from the last one; Shift+Tab moves back. */
export function defineTableKeymap() {
  /** @type {import("prosekit/pm/state").Command} */
  const nextCellOrNewRow = (state, dispatch, view) => {
    if (!isInTable(state)) return false;
    if (goToNextCell(1)(state, dispatch, view)) return true;
    if (!dispatch || !view) return true;
    addRowAfter(view.state, view.dispatch);
    return goToNextCell(1)(view.state, view.dispatch, view);
  };

  return withPriority(
    defineKeymap({ Tab: nextCellOrNewRow, "Shift-Tab": goToNextCell(-1) }),
    Priority.high,
  );
}
