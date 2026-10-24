import { defineBasicExtension } from "prosekit/basic";
import { createEditor, defineUpdateHandler, union } from "prosekit/core";
import { defineImageUploadHandler } from "prosekit/extensions/image";
import { ListDOMSerializer } from "prosekit/extensions/list";
import { definePlaceholder } from "prosekit/extensions/placeholder";
import { defineReadonly } from "prosekit/extensions/readonly";
import {
  registerAutocompleteEmptyElement,
  registerAutocompleteItemElement,
  registerAutocompletePopupElement,
  registerAutocompletePositionerElement,
  registerAutocompleteRootElement,
} from "prosekit/web/autocomplete";
import {
  registerInlinePopoverPopupElement,
  registerInlinePopoverPositionerElement,
  registerInlinePopoverRootElement,
} from "prosekit/web/inline-popover";
import {
  registerMenuItemElement,
  registerMenuPopupElement,
  registerMenuPositionerElement,
} from "prosekit/web/menu";
import {
  registerResizableHandleElement,
  registerResizableRootElement,
} from "prosekit/web/resizable";
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

import { actions, blockTypes, defineTableKeymap, tableActions } from "./commands.js";
import { createUploader, defineImageView, rejectReason } from "./images.js";

/** @typedef {import("./commands.js").RichTextEditor} RichTextEditor */

/**
 * Rich text editor web component. Rendered by the `sa-rich-text-editor` tag helper, which also
 * renders the module script that loads this file, so it loads only on pages with an editor.
 *
 * It renders in light DOM and works on the server-rendered parts: the toolbar, the content area,
 * the link popover, the slash menu, the table handles and the selection bubble. Controls name what
 * they do in `data-action`; the element keeps toggles checked and buttons disabled to match the
 * selection. The value is HTML in the child textarea, which is what the form posts and what shows
 * until this script has loaded. Images upload to `data-upload-url` as they are added. A read-only
 * editor (`data-readonly`) has no toolbar; a disabled one (`data-disabled`) has its toolbar's
 * controls disabled.
 */
class SelRichTextEditor extends HTMLElement {
  /** @type {RichTextEditor | undefined} */
  #editor;
  /** @type {HTMLTextAreaElement | undefined} */
  #textarea;
  /** @type {HTMLInputElement | undefined} */
  #fileInput;
  /** @type {import("./images.js").UploadOptions | undefined} */
  #upload;

  connectedCallback() {
    if (this.#editor) return;

    const textarea = /** @type {HTMLTextAreaElement | null} */ (
      this.querySelector(":scope > textarea")
    );
    const content = /** @type {HTMLElement | null} */ (
      this.querySelector(":scope > [data-slot=rich-text-editor-content]")
    );
    if (!textarea || !content) return;
    this.#textarea = textarea;

    const uploadUrl = this.dataset.uploadUrl;
    if (uploadUrl) {
      this.#upload = {
        url: uploadUrl,
        accept: (this.dataset.accept ?? "image/*")
          .split(",")
          .map((type) => type.trim().toLowerCase()),
        maxFileSize: Number(this.dataset.maxFileSize) || 5 * 1024 * 1024,
        token: () => this.#antiforgeryToken(),
      };
    }

    const disabled = this.hasAttribute("data-disabled");
    const readonly = disabled || this.hasAttribute("data-readonly");
    const editor = createEditor({
      extension: this.#defineExtension(textarea.placeholder, readonly),
      defaultContent: toHtml(textarea.value),
    });
    this.#editor = editor;
    editor.mount(content);
    restoreImageSizes(editor, textarea.value);
    editor.use(defineUpdateHandler(() => this.#sync()));

    // A click on the field's label focuses the editor, as it would the textarea
    const labelId = content.getAttribute("aria-labelledby")?.split(" ")[0];
    const label = labelId ? document.getElementById(labelId) : null;
    if (!disabled) label?.addEventListener("click", () => editor.focus());

    if (!readonly) {
      this.#setUpToolbar();
      this.#setUpLinkPopover();
      this.#setUpSlashMenu();
      this.#setUpTableHandles();
      this.#setUpBubble();
      this.#showMacShortcuts();
    }

    this.#sync(true);

    if (disabled) {
      for (const control of /** @type {NodeListOf<HTMLButtonElement | HTMLInputElement>} */ (
        this.querySelectorAll("[data-slot=rich-text-editor-toolbar] :is(button, input)")
      )) {
        control.disabled = true;
      }
    }
  }

  /**
   * @param {string} placeholder
   * @param {boolean} readonly
   */
  #defineExtension(placeholder, readonly) {
    const upload = this.#upload;
    const extensions = [
      defineBasicExtension(),
      definePlaceholder({ placeholder }),
      defineImageView(),
      defineTableKeymap(),
    ];
    if (readonly) extensions.push(defineReadonly());
    if (upload) {
      extensions.push(
        defineImageUploadHandler({
          uploader: createUploader(upload),
          canPaste: ({ file }) => this.#canUpload(file),
          canDrop: ({ file }) => this.#canUpload(file),
          onError: ({ error, uploadTask }) => this.#uploadFailed(error, uploadTask.objectURL),
        }),
      );
    }
    return union(...extensions);
  }

  /** Opens the file picker and uploads the picked image at the cursor. */
  pickImage() {
    const upload = this.#upload;
    if (!upload) return;
    if (!this.#fileInput) {
      const input = document.createElement("input");
      input.type = "file";
      input.accept = upload.accept.join(",");
      input.hidden = true;
      input.addEventListener("change", () => {
        const file = input.files?.[0];
        input.value = "";
        if (!file || !this.#canUpload(file)) return;
        this.#editor?.commands.uploadImage({
          file,
          uploader: createUploader(upload),
          onError: ({ error, uploadTask }) => this.#uploadFailed(error, uploadTask.objectURL),
        });
        this.#editor?.focus();
      });
      this.#fileInput = input;
      this.append(input);
    }
    this.#fileInput.click();
  }

  /** Opens the link popover for the link at the cursor, or a new one for the selection. */
  openLinkPopover() {
    const popover = /** @type {HTMLElement | null} */ (
      this.querySelector("[data-slot=rich-text-editor-link]")
    );
    const editor = this.#editor;
    if (!popover || !editor) return;
    const trigger = /** @type {HTMLElement | null} */ (
      this.querySelector('[data-slot=rich-text-editor-toolbar] [data-action="link"]')
    );
    if (editor.marks.link.isActive()) editor.commands.expandLink();
    const input = /** @type {HTMLInputElement} */ (popover.querySelector("input"));
    const { $from } = editor.state.selection;
    input.value = $from.marks().find((mark) => mark.type.name === "link")?.attrs.href ?? "";
    /** @type {(options?: { source?: HTMLElement }) => void} */ (popover.showPopover)({
      source: trigger ?? undefined,
    });
    input.focus();
    input.select();
  }

  /** @param {File} file */
  #canUpload(file) {
    const reason = this.#upload ? rejectReason(file, this.#upload) : "Images can't be added here.";
    this.#showNote(reason);
    return reason === null;
  }

  /**
   * Removes the image whose upload failed and says why.
   *
   * @param {unknown} error
   * @param {string} objectURL
   */
  #uploadFailed(error, objectURL) {
    const view = this.#editor?.view;
    if (view && !view.isDestroyed) {
      const tr = view.state.tr;
      view.state.doc.descendants((node, pos) => {
        if (node.type.name === "image" && node.attrs.src === objectURL) {
          tr.delete(tr.mapping.map(pos), tr.mapping.map(pos + node.nodeSize));
        }
      });
      if (tr.docChanged) view.dispatch(tr.setMeta("addToHistory", false));
    }
    this.#showNote(error instanceof Error ? error.message : "The image couldn't be uploaded.");
  }

  /** @param {string | null} message */
  #showNote(message) {
    const note = /** @type {HTMLElement | null} */ (
      this.querySelector(":scope > [data-slot=rich-text-editor-note]")
    );
    if (!note) return;
    note.textContent = message ?? "";
    note.hidden = !message;
  }

  #antiforgeryToken() {
    const selector = 'input[name="__RequestVerificationToken"]';
    const form = this.#textarea?.form;
    if (form) return /** @type {HTMLInputElement | null} */ (form.querySelector(selector))?.value;
    for (let element = this.parentElement; element; element = element.parentElement) {
      const token = /** @type {HTMLInputElement | null} */ (element.querySelector(selector));
      if (token) return token.value;
    }
    return undefined;
  }

  // Toolbar and bubble controls keep the editor's focus and selection while they're clicked. A
  // toggle is a label around a checkbox: the click mustn't flip it, the editor owns its state.
  #setUpToolbar() {
    const editor = /** @type {RichTextEditor} */ (this.#editor);
    this.addEventListener("mousedown", (event) => {
      const target = /** @type {Element} */ (event.target);
      if (target.closest("[data-action], [data-slot=dropdown-menu-trigger]")) {
        event.preventDefault();
      }
    });
    this.addEventListener("click", (event) => {
      const control = /** @type {HTMLElement | null} */ (
        /** @type {Element} */ (event.target).closest("[data-action]")
      );
      if (!control || !this.contains(control)) return;
      event.preventDefault();
      const name = control.dataset.action ?? "";
      actions[name]?.run(editor, this);
      if (name !== "link" && name !== "image") editor.focus();
    });
    this.addEventListener("valuechange", (event) => {
      const value = /** @type {CustomEvent<{ value: string }>} */ (event).detail.value;
      const block = blockTypes[value];
      if (!block) return;
      block.run(editor);
      /** @type {HTMLElement | null} */ (
        /** @type {Element} */ (event.target).closest("[popover]")
      )?.hidePopover();
      editor.focus();
    });
  }

  // The popover sits inside the page's form, so it can't hold a form of its own: Enter applies.
  #setUpLinkPopover() {
    const popover = /** @type {HTMLElement | null} */ (
      this.querySelector("[data-slot=rich-text-editor-link]")
    );
    if (!popover) return;
    const input = /** @type {HTMLInputElement} */ (popover.querySelector("input"));
    /** @param {boolean} remove */
    const apply = (remove) => {
      const editor = /** @type {RichTextEditor} */ (this.#editor);
      const href = input.value.trim();
      if (remove || !href) editor.commands.removeLink();
      else editor.commands.addLink({ href });
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
      const button = /** @type {HTMLElement | null} */ (
        /** @type {Element} */ (event.target).closest("[data-link-action]")
      );
      if (button) apply(button.dataset.linkAction === "remove");
    });
  }

  // ProseKit's autocomplete watches for "/" and filters the items by their value. Each item names
  // a block type or an action, as the toolbar's controls do.
  #setUpSlashMenu() {
    const root =
      /** @type {(HTMLElement & { editor: RichTextEditor | null; regex: RegExp | null }) | null} */ (
        this.querySelector("prosekit-autocomplete-root")
      );
    if (!root) return;
    const editor = /** @type {RichTextEditor} */ (this.#editor);
    root.regex = /\/(|\S.*)$/u;
    root.editor = editor;
    for (const item of /** @type {NodeListOf<HTMLElement>} */ (
      root.querySelectorAll("[data-slash]")
    )) {
      item.addEventListener("select", () => {
        const name = item.dataset.slash ?? "";
        if (blockTypes[name]) blockTypes[name].run(editor);
        else actions[name]?.run(editor, this);
      });
    }
  }

  // ProseKit shows a grip on the hovered row and column, selects it on click and opens its menu, and
  // drags it to reorder.
  #setUpTableHandles() {
    const editor = /** @type {RichTextEditor} */ (this.#editor);
    for (const element of /** @type {NodeListOf<HTMLElement & { editor: RichTextEditor | null }>} */ (
      this.querySelectorAll("prosekit-table-handle-root, prosekit-table-handle-root *")
    )) {
      if (element.localName.startsWith("prosekit-table-handle-")) element.editor = editor;
    }
    for (const item of /** @type {NodeListOf<HTMLElement>} */ (
      this.querySelectorAll("[data-table-action]")
    )) {
      item.addEventListener("select", () => {
        tableActions[item.dataset.tableAction ?? ""]?.run(editor);
        editor.focus();
      });
    }
  }

  // ProseKit's inline popover positions the bubble over the selection
  #setUpBubble() {
    const root = /** @type {(HTMLElement & { editor: RichTextEditor | null }) | null} */ (
      this.querySelector("prosekit-inline-popover-root")
    );
    if (root) root.editor = /** @type {RichTextEditor} */ (this.#editor);
  }

  // The server writes Ctrl; ProseKit's Mod is Command on Apple platforms
  #showMacShortcuts() {
    if (!/Mac|iPhone|iPad/.test(navigator.platform)) return;
    for (const key of this.querySelectorAll("kbd[data-key=mod]")) {
      key.textContent = "⌘";
    }
  }

  /** @param {boolean} [initial] */
  #sync(initial = false) {
    const editor = this.#editor;
    const textarea = this.#textarea;
    if (!editor || !textarea) return;

    for (const control of /** @type {NodeListOf<HTMLElement>} */ (
      this.querySelectorAll("[data-action]")
    )) {
      const action = actions[control.dataset.action ?? ""];
      if (!action) continue;
      const input = /** @type {HTMLInputElement | null} */ (
        control.querySelector(":scope > input")
      );
      if (input && action.active) input.checked = action.active(editor);
      if (action.enabled) {
        const enabled = action.enabled(editor);
        if (input) input.disabled = !enabled;
        else /** @type {HTMLButtonElement} */ (control).disabled = !enabled;
      }
    }

    for (const item of /** @type {NodeListOf<HTMLElement>} */ (
      this.querySelectorAll("[data-table-action]")
    )) {
      const action = tableActions[item.dataset.tableAction ?? ""];
      if (action?.enabled) item.hidden = !action.enabled(editor);
      if (action?.active) item.dataset.state = action.active(editor) ? "checked" : "unchecked";
    }

    const block = Object.keys(blockTypes).find((key) => blockTypes[key].active(editor));
    const label = this.querySelector("[data-slot=rich-text-editor-block-label]");
    for (const item of /** @type {NodeListOf<HTMLElement>} */ (
      this.querySelectorAll("[data-slot=rich-text-editor-block-menu] [role=menuitemradio]")
    )) {
      const checked = item.dataset.value === block;
      item.setAttribute("aria-checked", String(checked));
      item.dataset.state = checked ? "checked" : "unchecked";
      if (checked && label) label.textContent = item.dataset.label ?? "";
    }
    if (!block && label) label.textContent = label.getAttribute("data-default") ?? "";

    // Images still uploading have temporary URLs; the form value gets the final ones once they're in
    if (initial) return;
    const html = this.#html();
    if (textarea.value === html) return;
    textarea.value = html;
    textarea.dispatchEvent(new Event("input", { bubbles: true }));
  }

  // Clean HTML for the form value: real ul and ol lists instead of ProseKit's flat lists, plain hr
  // elements, no sizes on images that weren't resized, and no value at all for an empty document.
  #html() {
    const editor = /** @type {RichTextEditor} */ (this.#editor);
    const doc = editor.state.doc;
    const first = doc.firstChild;
    if (doc.childCount === 1 && first?.type.name === "paragraph" && first.content.size === 0) {
      return "";
    }

    const container = document.createElement("div");
    container.append(ListDOMSerializer.fromSchema(editor.schema).serializeFragment(doc.content));
    for (const item of container.querySelectorAll(".prosemirror-flat-list")) {
      for (const name of item.getAttributeNames()) item.removeAttribute(name);
    }
    for (const rule of container.querySelectorAll(".prosekit-horizontal-rule")) {
      rule.replaceWith(...rule.childNodes);
    }
    for (const image of container.querySelectorAll("img")) {
      for (const name of ["width", "height"]) {
        if (!image.getAttribute(name)) image.removeAttribute(name);
      }
    }
    return container.innerHTML;
  }
}

/**
 * The textarea's value as HTML. A value without tags, such as text written before the field became
 * rich text, becomes paragraphs: one per blank-line-separated block, with line breaks kept.
 *
 * @param {string} value
 */
function toHtml(value) {
  if (!value.trim()) return "<p></p>";
  if (/<[a-z][\s\S]*>/i.test(value)) return value;
  const escape = (/** @type {string} */ text) =>
    text.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
  return value
    .split(/\r?\n\s*\r?\n/)
    .map((block) => `<p>${escape(block.trim()).replace(/\r?\n/g, "<br>")}</p>`)
    .join("");
}

/**
 * ProseKit reads an image's size from its rendered box, which a parsed value doesn't have, so the
 * sizes saved with resized images are put back from their attributes.
 *
 * @param {RichTextEditor} editor
 * @param {string} value
 */
function restoreImageSizes(editor, value) {
  const template = document.createElement("template");
  template.innerHTML = value;
  const sizes = [...template.content.querySelectorAll("img[src]")].map((image) => ({
    width: Number(image.getAttribute("width")) || null,
    height: Number(image.getAttribute("height")) || null,
  }));
  if (!sizes.some((size) => size.width || size.height)) return;

  const { state } = editor.view;
  const tr = state.tr;
  let index = 0;
  state.doc.descendants((node, pos) => {
    if (node.type.name !== "image") return;
    const size = sizes[index++];
    if (size && (size.width || size.height)) {
      tr.setNodeMarkup(pos, undefined, { ...node.attrs, ...size });
    }
  });
  if (tr.docChanged) editor.view.dispatch(tr.setMeta("addToHistory", false));
}

registerAutocompleteRootElement();
registerAutocompletePositionerElement();
registerAutocompletePopupElement();
registerAutocompleteItemElement();
registerAutocompleteEmptyElement();
registerInlinePopoverRootElement();
registerInlinePopoverPositionerElement();
registerInlinePopoverPopupElement();
registerMenuPositionerElement();
registerMenuPopupElement();
registerMenuItemElement();
registerResizableRootElement();
registerResizableHandleElement();
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

if (!customElements.get("sel-rich-text-editor")) {
  customElements.define("sel-rich-text-editor", SelRichTextEditor);
}
