// Prototype: <sel-rich-text-editor>, a ProseKit editor driven by StellarAdmin toolbar markup.
//
// The element enhances a server-rendered textarea (the form value, and the no-JS fallback). The
// toolbar is plain sa-* markup; each control names the editor action it runs with data-action, and
// the element keeps toggles' checked state and buttons' disabled state in sync with the selection.

import {
  createEditor,
  defineKeymap,
  defineNodeView,
  defineUpdateHandler,
  Priority,
  union,
  withPriority,
  type Editor,
} from "prosekit/core";
import { defineBasicExtension } from "prosekit/basic";
import { definePlaceholder } from "prosekit/extensions/placeholder";
import { defineImageUploadHandler, type ImageAttrs } from "prosekit/extensions/image";
import { UploadTask, type Uploader } from "prosekit/extensions/file";
import { ListDOMSerializer } from "prosekit/extensions/list";
import type { Node as PmNode } from "prosekit/pm/model";
import type { Command } from "prosekit/pm/state";
import type { EditorView } from "prosekit/pm/view";
import { TextSelection } from "prosekit/pm/state";
import { addRowAfter, columnIsHeader, goToNextCell, isInTable, rowIsHeader, selectedRect, toggleHeader } from "prosemirror-tables";
import { registerInlinePopoverPopupElement, registerInlinePopoverPositionerElement, registerInlinePopoverRootElement } from "prosekit/web/inline-popover";
import {
  registerAutocompleteEmptyElement,
  registerAutocompleteItemElement,
  registerAutocompletePopupElement,
  registerAutocompletePositionerElement,
  registerAutocompleteRootElement,
} from "prosekit/web/autocomplete";
import { registerResizableHandleElement, registerResizableRootElement } from "prosekit/web/resizable";
import { registerMenuItemElement, registerMenuPopupElement, registerMenuPositionerElement } from "prosekit/web/menu";
import {
  registerTableHandleColumnMenuRootElement,
  registerTableHandleColumnMenuTriggerElement,
  registerTableHandleColumnPopupElement,
  registerTableHandleColumnPositionerElement,
  registerTableHandleDragPreviewElement,
  registerTableHandleDropIndicatorElement,
  registerTableHandleRootElement,
  registerTableHandleRowMenuRootElement,
  registerTableHandleRowMenuTriggerElement,
  registerTableHandleRowPopupElement,
  registerTableHandleRowPositionerElement,
} from "prosekit/web/table-handle";

registerInlinePopoverRootElement();
registerInlinePopoverPositionerElement();
registerInlinePopoverPopupElement();
registerResizableRootElement();
registerAutocompleteRootElement();
registerAutocompletePositionerElement();
registerAutocompletePopupElement();
registerAutocompleteItemElement();
registerAutocompleteEmptyElement();
registerResizableHandleElement();
registerMenuPositionerElement();
registerMenuPopupElement();
registerMenuItemElement();
registerTableHandleRootElement();
registerTableHandleDragPreviewElement();
registerTableHandleDropIndicatorElement();
registerTableHandleColumnPositionerElement();
registerTableHandleColumnPopupElement();
registerTableHandleColumnMenuRootElement();
registerTableHandleColumnMenuTriggerElement();
registerTableHandleRowPositionerElement();
registerTableHandleRowPopupElement();
registerTableHandleRowMenuRootElement();
registerTableHandleRowMenuTriggerElement();

function defineExtension(placeholder: string, uploader: Uploader<string>) {
  return union(
    defineBasicExtension(),
    definePlaceholder({ placeholder }),
    defineImageUploadHandler({ uploader }),
    defineImageView(),
    defineTableKeymap(),
  );
}

// Tab and Shift+Tab move between cells; Tab in the last cell adds a row, as in a spreadsheet.
const nextCellOrNewRow: Command = (state, dispatch, view) => {
  if (!isInTable(state)) return false;
  if (goToNextCell(1)(state, dispatch, view)) return true;
  if (!dispatch || !view) return true;
  addRowAfter(view.state, view.dispatch);
  return goToNextCell(1)(view.state, view.dispatch, view);
};

function defineTableKeymap() {
  return withPriority(
    defineKeymap({ Tab: nextCellOrNewRow, "Shift-Tab": goToNextCell(-1) }),
    Priority.high,
  );
}

type RichTextEditor = Editor<ReturnType<typeof defineExtension>>;

// Toolbar actions: what runs, whether it is "on", and whether it can run.
interface Action {
  run(editor: RichTextEditor, host: SelRichTextEditor): void;
  active?(editor: RichTextEditor): boolean;
  enabled?(editor: RichTextEditor): boolean;
}

const actions: Record<string, Action> = {
  undo: { run: (e) => e.commands.undo(), enabled: (e) => e.commands.undo.canExec() },
  redo: { run: (e) => e.commands.redo(), enabled: (e) => e.commands.redo.canExec() },
  bold: { run: (e) => e.commands.toggleBold(), active: (e) => e.marks.bold.isActive() },
  italic: { run: (e) => e.commands.toggleItalic(), active: (e) => e.marks.italic.isActive() },
  underline: { run: (e) => e.commands.toggleUnderline(), active: (e) => e.marks.underline.isActive() },
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
  blockquote: { run: (e) => e.commands.toggleBlockquote(), active: (e) => e.nodes.blockquote.isActive() },
  "code-block": { run: (e) => e.commands.toggleCodeBlock(), active: (e) => e.nodes.codeBlock.isActive() },
  "horizontal-rule": { run: (e) => e.commands.insertHorizontalRule() },
  table: {
    run: (e) => insertTable(e),
    enabled: (e) => !e.nodes.table.isActive(),
  },
  image: { run: (_, host) => host.pickImage() },
  link: { run: (_, host) => host.openLinkPopover(), active: (e) => e.marks.link.isActive() },
  unlink: { run: (e) => e.commands.expandLink() && e.commands.removeLink(), enabled: (e) => e.marks.link.isActive() },
};

// Table handle menu items. The handle selects its row or column first, so these act on that selection.
const headerActive = (e: RichTextEditor, role: "row" | "column") => {
  if (!isInTable(e.state)) return false;
  const rect = selectedRect(e.state);
  return role === "row" ? rowIsHeader(rect.map, rect.table, rect.top) : columnIsHeader(rect.map, rect.table, rect.left);
};

// Insert a table and put the cursor in its first cell, ready to type.
function insertTable(e: RichTextEditor) {
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

const tableActions: Record<string, { run(e: RichTextEditor): void; enabled?(e: RichTextEditor): boolean; active?(e: RichTextEditor): boolean }> = {
  "column-before": { run: (e) => e.commands.addTableColumnBefore() },
  "column-after": { run: (e) => e.commands.addTableColumnAfter() },
  "row-above": { run: (e) => e.commands.addTableRowAbove() },
  "row-below": { run: (e) => e.commands.addTableRowBelow() },
  "header-row": { run: (e) => e.exec(toggleHeader("row")), active: (e) => headerActive(e, "row") },
  "header-column": { run: (e) => e.exec(toggleHeader("column")), active: (e) => headerActive(e, "column") },
  merge: { run: (e) => e.commands.mergeTableCells(), enabled: (e) => e.commands.mergeTableCells.canExec() },
  split: { run: (e) => e.commands.splitTableCell(), enabled: (e) => e.commands.splitTableCell.canExec() },
  clear: { run: (e) => e.commands.deleteCellSelection() },
  "delete-column": { run: (e) => e.commands.deleteTableColumn() },
  "delete-row": { run: (e) => e.commands.deleteTableRow() },
  "delete-table": { run: (e) => e.commands.deleteTable() },
};

// Block type, shown as the dropdown's radio group value.
const blockTypes: Record<string, { run(e: RichTextEditor): void; active(e: RichTextEditor): boolean }> = {
  paragraph: { run: (e) => e.commands.setParagraph(), active: (e) => e.nodes.paragraph.isActive() },
  h1: { run: (e) => e.commands.setHeading({ level: 1 }), active: (e) => e.nodes.heading.isActive({ level: 1 }) },
  h2: { run: (e) => e.commands.setHeading({ level: 2 }), active: (e) => e.nodes.heading.isActive({ level: 2 }) },
  h3: { run: (e) => e.commands.setHeading({ level: 3 }), active: (e) => e.nodes.heading.isActive({ level: 3 }) },
};

// Prototype uploader: stands in for a POST to the ImageEditor upload endpoint. It reports progress and
// resolves with a data URL, as the server would resolve with the stored image's URL.
const simulatedUploader: Uploader<string> = ({ file, onProgress }) =>
  new Promise((resolve, reject) => {
    if (!file.type.startsWith("image/")) {
      reject(new Error("Only images can be uploaded."));
      return;
    }
    let loaded = 0;
    const timer = setInterval(() => {
      loaded = Math.min(file.size, loaded + file.size / 12);
      onProgress({ loaded, total: file.size });
      if (loaded >= file.size) {
        clearInterval(timer);
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result as string);
        reader.onerror = () => reject(reader.error);
        reader.readAsDataURL(file);
      }
    }, 120);
  });

function realUploader(url: string): Uploader<string> {
  return ({ file, onProgress }) =>
    new Promise((resolve, reject) => {
      const body = new FormData();
      body.append("file", file);
      const request = new XMLHttpRequest();
      request.open("POST", url);
      request.upload.addEventListener("progress", (e) => onProgress({ loaded: e.loaded, total: e.total }));
      request.addEventListener("load", () =>
        request.status < 300 ? resolve(JSON.parse(request.responseText).url) : reject(new Error(request.statusText)),
      );
      request.addEventListener("error", () => reject(new Error("Upload failed.")));
      request.send(body);
    });
}

// Image node view: resizable handles and an upload progress overlay, all in plain DOM.
function defineImageView() {
  return defineNodeView({
    name: "image",
    constructor: (node: PmNode, view: EditorView, getPos: () => number | undefined) => {
      const root = document.createElement("prosekit-resizable-root") as HTMLElement & {
        width: number | null;
        height: number | null;
        aspectRatio: number | null;
      };
      root.className = "sa-rich-text-editor-image";
      root.dataset.slot = "rich-text-editor-image";

      const img = document.createElement("img");
      img.draggable = false;
      const handle = document.createElement("prosekit-resizable-handle");
      handle.setAttribute("position", "bottom-right");
      handle.className = "sa-rich-text-editor-image-handle";
      const progress = document.createElement("div");
      progress.className = "sa-rich-text-editor-image-progress";
      progress.hidden = true;
      root.append(img, handle, progress);

      let unsubscribe: VoidFunction | undefined;
      const render = (attrs: ImageAttrs) => {
        if (img.getAttribute("src") !== attrs.src) {
          img.src = attrs.src ?? "";
          unsubscribe?.();
          const task = attrs.src ? UploadTask.get<string>(attrs.src) : undefined;
          progress.hidden = !task;
          if (task) {
            progress.textContent = "Uploading…";
            unsubscribe = task.subscribeProgress(({ loaded, total }) => {
              progress.textContent = `Uploading ${Math.round((loaded / total) * 100)}%`;
            });
            task.finished.finally(() => (progress.hidden = true));
          }
        }
        root.width = attrs.width ?? null;
        root.height = attrs.height ?? null;
      };
      render(node.attrs as ImageAttrs);
      img.addEventListener("load", () => {
        if (!root.aspectRatio && img.naturalWidth) root.aspectRatio = img.naturalWidth / img.naturalHeight;
      });

      root.addEventListener("resizeEnd", (event) => {
        const { width, height } = (event as CustomEvent<{ width: number; height: number }>).detail;
        const pos = getPos();
        if (pos == null) return;
        view.dispatch(view.state.tr.setNodeMarkup(pos, undefined, { ...node.attrs, width, height }));
      });

      return {
        dom: root,
        update: (updated: PmNode) => {
          if (updated.type !== node.type) return false;
          node = updated;
          render(updated.attrs as ImageAttrs);
          return true;
        },
        selectNode: () => root.setAttribute("data-selected", ""),
        deselectNode: () => root.removeAttribute("data-selected"),
        ignoreMutation: () => true,
        stopEvent: (event: Event) => (event.target as Element)?.closest?.("prosekit-resizable-handle") != null,
        destroy: () => unsubscribe?.(),
      };
    },
  });
}

class SelRichTextEditor extends HTMLElement {
  #editor?: RichTextEditor;
  #textarea?: HTMLTextAreaElement;
  #fileInput?: HTMLInputElement;

  connectedCallback() {
    if (this.#editor) return;

    const textarea = this.querySelector<HTMLTextAreaElement>(":scope > textarea");
    const content = this.querySelector<HTMLElement>("[data-slot=rich-text-editor-content]");
    if (!textarea || !content) return;
    this.#textarea = textarea;

    const uploadUrl = this.getAttribute("upload-url");
    const extension = defineExtension(
      textarea.placeholder || "",
      uploadUrl ? realUploader(uploadUrl) : simulatedUploader,
    );
    const editor = createEditor({ extension, defaultContent: textarea.value || "<p></p>" });
    this.#editor = editor;
    editor.use(defineUpdateHandler(() => this.#sync()));
    editor.mount(content);

    // Toolbar: keep focus (and the selection) in the editor while clicking controls.
    this.addEventListener("mousedown", (event) => {
      if ((event.target as Element).closest("[data-action] , [data-slot=dropdown-menu-trigger]")) {
        event.preventDefault();
      }
    });
    this.addEventListener("click", (event) => {
      const control = (event.target as Element).closest<HTMLElement>("[data-action]");
      if (!control || !this.contains(control)) return;
      // A toggle is a label around a checkbox: stop the label from flipping it, the editor owns the state.
      event.preventDefault();
      actions[control.dataset.action!]?.run(editor, this);
      if (control.dataset.action !== "link" && control.dataset.action !== "image") editor.focus();
    });
    this.addEventListener("valuechange", (event) => {
      const value = (event as CustomEvent<{ value: string }>).detail.value;
      if (!blockTypes[value]) return;
      blockTypes[value].run(editor);
      (event.target as Element).closest<HTMLElement>("[popover]")?.hidePopover();
      editor.focus();
    });

    this.#setUpLinkPopover();
    this.#setUpBubble();
    this.#setUpToggleTooltips();
    this.#setUpSlashMenu();
    this.#setUpTableHandles();
    this.#sync();
  }

  pickImage() {
    if (!this.#fileInput) {
      this.#fileInput = Object.assign(document.createElement("input"), { type: "file", accept: "image/*", hidden: true });
      this.#fileInput.addEventListener("change", () => {
        const file = this.#fileInput!.files?.[0];
        this.#fileInput!.value = "";
        if (!file) return;
        const uploadUrl = this.getAttribute("upload-url");
        this.#editor!.commands.uploadImage({ file, uploader: uploadUrl ? realUploader(uploadUrl) : simulatedUploader });
        this.#editor!.focus();
      });
      this.append(this.#fileInput);
    }
    this.#fileInput.click();
  }

  openLinkPopover() {
    const popover = this.querySelector<HTMLElement>("[data-slot=rich-text-editor-link]");
    const trigger = this.querySelector<HTMLElement>('[data-action="link"]');
    if (!popover) return;
    const editor = this.#editor!;
    if (editor.marks.link.isActive()) editor.commands.expandLink();
    const input = popover.querySelector<HTMLInputElement>("input")!;
    input.value = this.#currentLinkHref() ?? "";
    (popover.showPopover as (options?: { source?: HTMLElement }) => void)({ source: trigger ?? undefined });
    input.focus();
    input.select();
  }

  #currentLinkHref(): string | undefined {
    const { $from } = this.#editor!.state.selection;
    return $from.marks().find((m) => m.type.name === "link")?.attrs.href;
  }

  // The popover sits inside the page's form, so it can't hold a form of its own: Enter and the buttons apply.
  #setUpLinkPopover() {
    const popover = this.querySelector<HTMLElement>("[data-slot=rich-text-editor-link]");
    if (!popover) return;
    const input = popover.querySelector("input")!;
    const apply = (remove: boolean) => {
      const editor = this.#editor!;
      if (remove || !input.value.trim()) editor.commands.removeLink();
      else editor.commands.addLink({ href: input.value.trim() });
      popover.hidePopover();
      editor.focus();
    };
    input.addEventListener("keydown", (event) => {
      if (event.key === "Enter") {
        event.preventDefault();
        apply(false);
      }
    });
    popover.addEventListener("click", (event) => {
      const button = (event.target as Element).closest<HTMLElement>("[data-link-action]");
      if (button) apply(button.dataset.linkAction === "remove");
    });
  }

  // The slash menu: ProseKit's autocomplete watches for "/" and filters the items by their value; each item
  // names a block type or toolbar action, as the toolbar controls do.
  #setUpSlashMenu() {
    const root = this.querySelector<HTMLElement & { editor: RichTextEditor | null; regex: RegExp | null }>(
      "prosekit-autocomplete-root",
    );
    if (!root) return;
    root.regex = /\/(|\S.*)$/u;
    root.editor = this.#editor!;
    for (const item of root.querySelectorAll<HTMLElement>("[data-slash]")) {
      item.addEventListener("select", () => {
        const editor = this.#editor!;
        const name = item.dataset.slash!;
        if (blockTypes[name]) blockTypes[name].run(editor);
        else actions[name]?.run(editor, this);
      });
    }
  }

  // Table handles: ProseKit shows a grip on the hovered row and column, selects it on click and opens its
  // menu, and drags it to reorder. The menus are drawn as dropdown menus.
  #setUpTableHandles() {
    const editor = this.#editor!;
    for (const element of this.querySelectorAll<HTMLElement & { editor: RichTextEditor | null }>(
      "prosekit-table-handle-root, prosekit-table-handle-root *",
    )) {
      if (element.localName.startsWith("prosekit-table-handle-")) element.editor = editor;
    }
    for (const item of this.querySelectorAll<HTMLElement>("[data-table-action]")) {
      item.addEventListener("select", () => {
        tableActions[item.dataset.tableAction!]?.run(editor);
        editor.focus();
      });
    }
  }

  // Native interestfor only works on buttons and links, so a toggle (a label around a checkbox) gets no
  // tooltip from the browser. Open the tooltips of the labels with interestfor here, with the same delays.
  #setUpToggleTooltips() {
    let timer: ReturnType<typeof setTimeout> | undefined;
    const tooltipOf = (label: HTMLElement) => document.getElementById(label.getAttribute("interestfor")!);
    const show = (label: HTMLElement) => {
      clearTimeout(timer);
      timer = setTimeout(() => {
        const tip = tooltipOf(label);
        if (tip && !tip.matches(":popover-open")) {
          (tip.showPopover as (options?: { source?: HTMLElement }) => void)({ source: label });
        }
      }, 500);
    };
    const hide = (label: HTMLElement) => {
      clearTimeout(timer);
      const tip = tooltipOf(label);
      if (tip?.matches(":popover-open")) tip.hidePopover();
    };
    for (const label of this.querySelectorAll<HTMLElement>("label[interestfor]")) {
      label.addEventListener("pointerenter", () => show(label));
      label.addEventListener("pointerleave", () => hide(label));
      label.addEventListener("focusin", () => label.matches(":has(:focus-visible)") && show(label));
      label.addEventListener("focusout", () => hide(label));
      label.addEventListener("click", () => hide(label));
    }
  }

  // Clean HTML for the form value: real ul/ol lists instead of ProseKit's flat-list markup, plain hr
  // elements, no wrapper.
  #html(): string {
    const editor = this.#editor!;
    const container = document.createElement("div");
    container.append(ListDOMSerializer.fromSchema(editor.schema).serializeFragment(editor.state.doc.content));
    for (const item of container.querySelectorAll(".prosemirror-flat-list")) {
      for (const name of item.getAttributeNames()) item.removeAttribute(name);
    }
    for (const rule of container.querySelectorAll(".prosekit-horizontal-rule")) rule.replaceWith(...rule.childNodes);
    return container.innerHTML;
  }

  // The selection bubble: ProseKit's inline popover positions it; the controls inside are sa-* toggles.
  #setUpBubble() {
    const root = this.querySelector<HTMLElement & { editor: RichTextEditor | null }>("prosekit-inline-popover-root");
    if (root) root.editor = this.#editor!;
  }

  #sync() {
    const editor = this.#editor!;
    for (const control of this.querySelectorAll<HTMLElement>("[data-action]")) {
      const action = actions[control.dataset.action!];
      if (!action) continue;
      const input = control.querySelector<HTMLInputElement>(":scope > input");
      if (input && action.active) input.checked = action.active(editor);
      if (action.enabled) {
        const enabled = action.enabled(editor);
        if (input) input.disabled = !enabled;
        else (control as HTMLButtonElement).disabled = !enabled;
      }
    }

    for (const item of this.querySelectorAll<HTMLElement>("[data-table-action]")) {
      const action = tableActions[item.dataset.tableAction!];
      if (action?.enabled) item.hidden = !action.enabled(editor);
      if (action?.active) item.dataset.state = action.active(editor) ? "checked" : "unchecked";
    }

    const block = Object.keys(blockTypes).find((key) => blockTypes[key].active(editor));
    const label = this.querySelector<HTMLElement>("[data-slot=rich-text-editor-block-label]");
    for (const item of this.querySelectorAll<HTMLElement>("[data-slot=dropdown-menu-radio-item]")) {
      const checked = item.dataset.value === block;
      item.setAttribute("aria-checked", String(checked));
      item.dataset.state = checked ? "checked" : "unchecked";
      if (checked && label) label.textContent = item.textContent!.trim();
    }
    if (!block && label) label.textContent = "Text";

    // Images still uploading have blob: sources; the form value waits for the final URLs.
    const html = this.#html();
    this.#textarea!.value = html;
    this.dispatchEvent(new CustomEvent("contentchange", { detail: { html }, bubbles: true }));
  }
}

customElements.define("sel-rich-text-editor", SelRichTextEditor);
