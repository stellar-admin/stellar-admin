const reduceMotion = matchMedia("(prefers-reduced-motion: reduce)");
const SLIDE = { duration: 150, easing: "cubic-bezier(0.2, 0, 0, 1)" };
const DRAG_THRESHOLD = 4;
const SCROLL_EDGE = 48;
let nextErrorId = 0;

/** @typedef {HTMLInputElement | HTMLSelectElement} Control */

/**
 * @typedef {object} Drag
 * @property {HTMLElement} handle
 * @property {HTMLLIElement} row
 * @property {number} pointerId
 * @property {number} startY
 * @property {boolean} started
 * @property {HTMLElement} [ghost]
 * @property {number} [border]
 * @property {number} [offsetY]
 * @property {HTMLLIElement[]} [order]
 */

/**
 * The list and dictionary editors' rows: adds, removes and reorders them, renumbers their names
 * so that MVC binds them as a collection, and checks them as they are edited and on submit.
 */
export class CollectionEditor extends HTMLElement {
  static formAssociated = true;

  /** @type {ElementInternals} */
  #internals;
  /** @type {Record<string, string>} */
  #texts = {};
  /** The controls whose errors show: those that lost focus, and every control after a submit */
  #shown = new WeakSet();
  #countShown = false;
  /** @type {Set<string>} */
  #disabledKeys = new Set();
  /** @type {Drag | null} */
  #drag = null;
  /** Errors to show once the pointer is released, so that a row doesn't move under a click */
  /** @type {(() => void) | null} */
  #deferred = null;

  constructor() {
    super();
    this.#internals = this.attachInternals();
  }

  connectedCallback() {
    if (this.hasAttribute("data-readonly")) {
      return;
    }

    this.#texts = JSON.parse(this.dataset.texts || "{}");
    this.#disabledKeys = new Set(
      [
        ...(this.#template()?.content.querySelectorAll('[data-part="key"] option:disabled') ?? []),
      ].map((option) => /** @type {HTMLOptionElement} */ (option).value),
    );

    this.addEventListener("command", this.#onCommand);
    this.addEventListener("input", this.#onInput);
    this.addEventListener("change", this.#onChange);
    this.addEventListener("focusout", this.#onFocusOut);
    this.addEventListener("keydown", this.#onKeyDown);
    this.addEventListener("paste", this.#onPaste);
    this.addEventListener("pointerdown", this.#onPointerDown);
    this.addEventListener("invalid", this.#onInvalid, true);

    this.#update();
  }

  disconnectedCallback() {
    this.removeEventListener("command", this.#onCommand);
    this.removeEventListener("input", this.#onInput);
    this.removeEventListener("change", this.#onChange);
    this.removeEventListener("focusout", this.#onFocusOut);
    this.removeEventListener("keydown", this.#onKeyDown);
    this.removeEventListener("paste", this.#onPaste);
    this.removeEventListener("pointerdown", this.#onPointerDown);
    this.removeEventListener("invalid", this.#onInvalid, true);

    if (this.#drag) {
      this.#endDrag(true);
    }
  }

  /**
   * Drops the rows left blank, so that they don't post. Called as the form is submitted.
   */
  dropBlankRows() {
    const blank = this.#rows().filter((row) => this.#isBlank(row));
    if (blank.length === 0) {
      return;
    }

    blank.forEach((row) => row.remove());
    this.#update();
  }

  get #dictionary() {
    return this.dataset.kind === "dictionary";
  }

  get #maxItems() {
    return this.dataset.maxItems ? Number(this.dataset.maxItems) : Infinity;
  }

  get #list() {
    return /** @type {HTMLOListElement} */ (
      this.querySelector(":scope > .sa-collection-editor-items")
    );
  }

  /** @returns {HTMLLIElement[]} */
  #rows() {
    return /** @type {HTMLLIElement[]} */ ([...this.#list.children]);
  }

  /** @returns {HTMLTemplateElement | null} */
  #template() {
    return this.querySelector(":scope > template");
  }

  /** @param {Element} row @returns {Control[]} */
  #controls(row) {
    return [...row.querySelectorAll("[data-part]")].filter(
      (element) => element instanceof HTMLInputElement || element instanceof HTMLSelectElement,
    );
  }

  /** @param {Element} row @param {string} part */
  #control(row, part) {
    return this.#controls(row).find((control) => control.dataset.part === part);
  }

  /** @param {Element} row */
  #isBlank(row) {
    return this.#controls(row).every((control) => control.value.trim() === "");
  }

  /** @param {EventTarget | null} target */
  #rowOf(target) {
    const row = target instanceof Element ? target.closest("li") : null;
    return row?.parentElement === this.#list ? /** @type {HTMLLIElement} */ (row) : null;
  }

  // ----------------------------------------------------------------------------------------------
  // Rows
  // ----------------------------------------------------------------------------------------------

  /**
   * Adds a blank row, at the end or after a row.
   * @param {HTMLLIElement | null} [after]
   * @returns {HTMLLIElement | null}
   */
  #add(after = null) {
    const template = this.#template();
    if (!template || this.#rows().length >= this.#maxItems) {
      return null;
    }

    const row = /** @type {HTMLLIElement} */ (template.content.firstElementChild?.cloneNode(true));
    if (after) {
      after.after(row);
    } else {
      this.#list.append(row);
    }

    return row;
  }

  /** @param {HTMLLIElement} row */
  #remove(row) {
    const rows = this.#rows();
    const index = rows.indexOf(row);
    const part =
      /** @type {HTMLElement | null} */ (document.activeElement)?.dataset?.part ?? "value";
    row.remove();
    this.#update();

    const next = rows[index + 1] ?? rows[index - 1];
    if (next) {
      this.#focus(next, part);
    } else {
      this.#addButton()?.focus();
    }
  }

  // Removes a blank row from the keyboard, moving to the row before it
  /** @param {HTMLLIElement} row */
  #discard(row) {
    const previous = /** @type {HTMLLIElement | null} */ (row.previousElementSibling);
    const next = /** @type {HTMLLIElement | null} */ (row.nextElementSibling);
    row.remove();
    this.#update();
    if (previous ?? next) {
      this.#focus(/** @type {HTMLLIElement} */ (previous ?? next), "value");
    } else {
      this.#addButton()?.focus();
    }
  }

  /**
   * Moves a row up or down by one, sliding the rows between.
   * @param {HTMLLIElement} row
   * @param {-1 | 1} delta
   */
  #move(row, delta) {
    const sibling = delta < 0 ? row.previousElementSibling : row.nextElementSibling;
    if (!sibling) {
      return false;
    }

    const before = this.#measure();
    if (delta < 0) {
      sibling.before(row);
    } else {
      sibling.after(row);
    }

    this.#update();
    this.#slide(before);
    return true;
  }

  /**
   * @param {HTMLLIElement} row
   * @param {string} [part]
   * @param {boolean} [end]
   */
  #focus(row, part, end = true) {
    const control = (part && this.#control(row, part)) || this.#controls(row)[0];
    if (!control) {
      return;
    }

    control.focus();
    if (end && control instanceof HTMLInputElement && control.type === "text") {
      control.setSelectionRange(control.value.length, control.value.length);
    }
  }

  #addButton() {
    return /** @type {HTMLButtonElement | null} */ (
      this.querySelector(":scope > .sa-collection-editor-add")
    );
  }

  /**
   * Brings everything that depends on the rows up to date: the names MVC binds, the numbers and
   * labels, the empty message, the count, the keys other rows use, and the checks.
   */
  #update() {
    const rows = this.#rows();
    const name = this.dataset.name ?? "";
    rows.forEach((row, index) => {
      const n = String(index + 1);
      for (const control of this.#controls(row)) {
        const part = control.dataset.part ?? "value";
        control.name = this.#dictionary
          ? `${name}[${index}].${part === "key" ? "Key" : "Value"}`
          : `${name}[${index}]`;
        control.setAttribute("aria-label", this.#text(part, { n }));
      }

      const number = row.querySelector(".sa-collection-editor-number");
      if (number) {
        number.textContent = n;
      }

      row
        .querySelector(".sa-collection-editor-handle")
        ?.setAttribute("aria-label", this.#text("move", { n }));
      row
        .querySelector(".sa-collection-editor-remove")
        ?.setAttribute("aria-label", this.#text("remove", { n }));
    });

    const empty = rows.length === 0;
    this.#list.hidden = empty;
    this.querySelector(":scope > .sa-collection-editor-head")?.toggleAttribute("hidden", empty);
    this.querySelector(":scope > .sa-collection-editor-empty")?.toggleAttribute("hidden", !empty);

    const add = this.#addButton();
    if (add) {
      const full = rows.length >= this.#maxItems;
      add.disabled = full;
      if (full && this.#texts.full) {
        add.title = this.#texts.full;
      } else {
        add.removeAttribute("title");
      }

      const count = add.querySelector(".sa-collection-editor-count");
      if (count) {
        count.textContent = this.#text("count", { count: String(rows.length) });
        count.toggleAttribute("hidden", empty);
      }
    }

    this.#disableUsedKeys(rows);
    this.#validate(rows);
    rows.forEach((row) => this.#show(row));
    this.#showCount();
  }

  // A key can be used once, so a key select disables the keys other rows use
  /** @param {HTMLLIElement[]} rows */
  #disableUsedKeys(rows) {
    const selects = rows
      .map((row) => this.#control(row, "key"))
      .filter((control) => control instanceof HTMLSelectElement);
    if (selects.length === 0) {
      return;
    }

    const used = selects.map((select) => select.value);
    selects.forEach((select, index) => {
      for (const option of select.options) {
        option.disabled =
          option.value !== "" &&
          (this.#disabledKeys.has(option.value) ||
            used.some((value, other) => other !== index && value === option.value));
      }
    });
  }

  // ----------------------------------------------------------------------------------------------
  // Checks: each control's own validity (pattern, length, range), a dictionary row's missing key
  // or value, duplicates and the number of items. They block the submit through the controls'
  // validity and the editor's, and show only once a control has lost focus or the form was
  // submitted.
  // ----------------------------------------------------------------------------------------------

  /** @param {HTMLLIElement[]} rows */
  #validate(rows) {
    const filled = rows.filter((row) => !this.#isBlank(row));
    for (const row of rows) {
      for (const control of this.#controls(row)) {
        control.setCustomValidity("");
      }
    }

    if (this.#dictionary) {
      for (const row of filled) {
        for (const part of ["key", "value"]) {
          const control = this.#control(row, part);
          if (control && control.validity.valid && control.value.trim() === "") {
            control.setCustomValidity(this.#text(`${part}Required`));
          }
        }
      }
    }

    if (this.#dictionary || this.hasAttribute("data-unique")) {
      const part = this.#dictionary ? "key" : "value";
      const seen = new Set();
      for (const row of filled) {
        const control = this.#control(row, part);
        const value = control ? this.#comparable(control) : "";
        if (!control || value === "") {
          continue;
        }

        if (seen.has(value) && control.validity.valid) {
          control.setCustomValidity(this.#text("duplicate", { value: this.#display(control) }));
        }

        seen.add(value);
      }
    }

    const min = Number(this.dataset.minItems ?? 0);
    const add = this.#addButton();
    if (filled.length < min && this.#texts.minItems) {
      this.#internals.setValidity({ rangeUnderflow: true }, this.#texts.minItems, add ?? undefined);
    } else {
      this.#internals.setValidity({});
    }
  }

  /** @param {Control} control */
  #comparable(control) {
    const value = control.value.trim();
    if (control instanceof HTMLInputElement && control.type === "number") {
      return value === "" ? "" : String(Number(value));
    }

    return control instanceof HTMLSelectElement ? value : value.toLocaleLowerCase();
  }

  /** @param {Control} control */
  #display(control) {
    return control instanceof HTMLSelectElement
      ? (control.selectedOptions[0]?.text ?? control.value)
      : control.value.trim();
  }

  /** @param {Control} control */
  #message(control) {
    return control.validity.patternMismatch && control.dataset.patternMessage
      ? control.dataset.patternMessage
      : control.validationMessage;
  }

  /**
   * Shows the first error of a row's shown controls under the row. A row none of whose controls
   * show keeps what the server rendered.
   * @param {HTMLLIElement} row
   */
  #show(row) {
    const controls = this.#controls(row);
    if (!controls.some((control) => this.#shown.has(control))) {
      return;
    }

    const failing = controls.filter(
      (control) => this.#shown.has(control) && !control.validity.valid,
    );
    let error = /** @type {HTMLElement | null} */ (
      row.querySelector(".sa-collection-editor-item-error")
    );
    if (failing.length === 0) {
      error?.remove();
      row.removeAttribute("data-invalid");
      for (const control of controls) {
        control.removeAttribute("aria-invalid");
        control.removeAttribute("aria-describedby");
      }

      return;
    }

    if (!error) {
      error = document.createElement("p");
      error.className = "sa-collection-editor-item-error";
      error.dataset.slot = "collection-editor-item-error";
      error.id = `${this.id}-e${++nextErrorId}-error`;
      row.append(error);
    }

    error.textContent = this.#message(failing[0]);
    row.setAttribute("data-invalid", "true");
    for (const control of controls) {
      if (failing.includes(control)) {
        control.setAttribute("aria-invalid", "true");
        control.setAttribute("aria-describedby", error.id);
      } else {
        control.removeAttribute("aria-invalid");
        control.removeAttribute("aria-describedby");
      }
    }
  }

  // The number of items, under the field: shown after a submit, until it is met
  #showCount() {
    if (!this.#countShown) {
      return;
    }

    const element = this.#fieldError();
    const message = this.#internals.validity.valid ? "" : this.#internals.validationMessage;
    if (message) {
      this.setAttribute("aria-invalid", "true");
    } else {
      this.removeAttribute("aria-invalid");
    }

    this.closest('[data-slot="field"]')?.toggleAttribute("data-invalid", message !== "");
    if (element) {
      element.textContent = message;
      element.classList.toggle("field-validation-error", message !== "");
      element.classList.toggle("field-validation-valid", message === "");
    }

    if (!message) {
      this.#countShown = false;
    }
  }

  #fieldError() {
    const ids = (this.getAttribute("aria-describedby") ?? "").split(/\s+/).filter(Boolean);
    return (
      ids
        .map((id) => document.getElementById(id))
        .find((element) => element?.dataset.slot === "field-error") ?? null
    );
  }

  /**
   * Marks a row's controls as shown and shows its error.
   * @param {HTMLLIElement} row
   * @param {Control[]} [controls]
   */
  #reveal(row, controls = this.#controls(row)) {
    controls.forEach((control) => this.#shown.add(control));
    this.#show(row);
  }

  // A submit that fails a check shows every row's error, and the count, in place of the browser's
  // message, and focuses the first invalid row
  /** @param {Event} event */
  #onInvalid = (event) => {
    const row = this.#rowOf(event.target);
    if (row) {
      event.preventDefault();
      this.#reveal(row);
      claimFocus(/** @type {HTMLElement} */ (event.target));
      return;
    }

    if (event.target === this && this.#fieldError()) {
      event.preventDefault();
      this.#countShown = true;
      this.#showCount();
      claimFocus(this.#addButton() ?? this);
    }
  };

  // ----------------------------------------------------------------------------------------------
  // Events
  // ----------------------------------------------------------------------------------------------

  /** @param {Event} event */
  #onCommand = (event) => {
    const { command, source } = /** @type {Event & { command: string; source: Element | null }} */ (
      event
    );
    if (command === "--add") {
      const row = this.#add();
      if (row) {
        this.#update();
        this.#focus(row);
      }
    } else if (command === "--remove") {
      const row = this.#rowOf(source);
      if (row) {
        this.#remove(row);
      }
    }
  };

  // A row that shows an error is checked again as it is typed, so that the error goes as soon as
  // it is fixed. Otherwise its errors wait until it loses focus.
  /** @param {Event} event */
  #onInput = (event) => {
    const control = /** @type {Control} */ (event.target);
    const row = this.#rowOf(control);
    if (!row || control instanceof HTMLSelectElement) {
      return;
    }

    if (!row.hasAttribute("data-invalid")) {
      this.#controls(row).forEach((part) => this.#shown.delete(part));
    }

    applyCasing(/** @type {HTMLInputElement} */ (control));
    this.#update();
  };

  /** @param {Event} event */
  #onChange = (event) => {
    const control = event.target;
    const row = this.#rowOf(control);
    if (!row || !(control instanceof HTMLSelectElement)) {
      return;
    }

    this.#shown.add(control);
    this.#update();
  };

  // Leaving a control shows its own error; leaving the row also asks for the rest of the row
  /** @param {FocusEvent} event */
  #onFocusOut = (event) => {
    const row = this.#rowOf(event.target);
    if (!row || !(event.target instanceof HTMLElement) || !event.target.dataset.part) {
      return;
    }

    const leaving = this.#rowOf(event.relatedTarget) !== row;
    const controls = leaving ? this.#controls(row) : [/** @type {Control} */ (event.target)];
    const reveal = () => {
      if (row.isConnected) {
        this.#validate(this.#rows());
        this.#reveal(row, controls);
      }
    };

    if (this.#deferred) {
      this.#deferred = reveal;
    } else {
      reveal();
    }
  };

  /** @param {KeyboardEvent} event */
  #onKeyDown = (event) => {
    const target = /** @type {HTMLElement} */ (event.target);
    const row = this.#rowOf(target);
    if (!row) {
      return;
    }

    if (target.classList.contains("sa-collection-editor-handle")) {
      if (event.key === "ArrowUp" || event.key === "ArrowDown") {
        event.preventDefault();
        if (this.#move(row, event.key === "ArrowUp" ? -1 : 1)) {
          target.focus();
        }
      }

      return;
    }

    const part = target.dataset.part;
    if (!part) {
      return;
    }

    const select = target instanceof HTMLSelectElement;
    const input = /** @type {HTMLInputElement} */ (target);
    if (event.altKey && (event.key === "ArrowUp" || event.key === "ArrowDown")) {
      if (this.hasAttribute("data-sortable")) {
        event.preventDefault();
        if (this.#move(row, event.key === "ArrowUp" ? -1 : 1)) {
          target.focus();
        }
      }
    } else if (event.key === "Enter" && !select) {
      // Enter moves from a key to its value, to the next row, or adds a row after the last
      event.preventDefault();
      if (this.#dictionary && part === "key") {
        this.#focus(row, "value");
        return;
      }

      const next = /** @type {HTMLLIElement | null} */ (row.nextElementSibling);
      if (next) {
        this.#focus(next, this.#dictionary ? "key" : "value");
        return;
      }

      if (this.#isBlank(row)) {
        return;
      }

      this.#reveal(row);
      const added = this.#add(row);
      if (added) {
        this.#update();
        this.#focus(added);
      }
    } else if (event.key === "Backspace" && !select && this.#isBlank(row)) {
      // Backspace in a blank row removes it
      if (input.type === "number" || input.selectionStart === 0) {
        event.preventDefault();
        this.#discard(row);
      }
    } else if (event.key === "Escape" && this.#isBlank(row)) {
      // Escape in a blank row cancels adding it. Only then: otherwise Escape is left to the page,
      // such as a dialog the editor is in, which the handled key doesn't close.
      event.preventDefault();
      event.stopPropagation();
      this.#discard(row);
    } else if ((event.key === "ArrowUp" || event.key === "ArrowDown") && input.type === "text") {
      // Up and down move between rows, keeping the column
      const sibling = event.key === "ArrowUp" ? row.previousElementSibling : row.nextElementSibling;
      if (sibling) {
        event.preventDefault();
        this.#focus(/** @type {HTMLLIElement} */ (sibling), part);
      }
    }
  };

  // Several lines become several rows; for a dictionary, key=value, "key: value" or tab-separated
  // lines become entries
  /** @param {ClipboardEvent} event */
  #onPaste = (event) => {
    const control = /** @type {HTMLElement} */ (event.target);
    const row = this.#rowOf(control);
    if (!row || !control.dataset.part) {
      return;
    }

    const lines = (event.clipboardData?.getData("text") ?? "")
      .split(/\r?\n/)
      .map((line) => line.trim())
      .filter(Boolean);
    const pairs = this.#dictionary && lines.some((line) => /\t|=|:\s/.test(line));
    if (lines.length < 2 && !pairs) {
      return;
    }

    event.preventDefault();

    // A blank row takes the first line; the rest go after it
    let after = /** @type {HTMLLIElement | null} */ (row);
    let target = this.#isBlank(row) ? row : null;
    let last = null;
    for (const line of lines) {
      const added = target ?? this.#add(after);
      target = null;
      if (!added) {
        break;
      }

      if (this.#dictionary) {
        const match = line.match(/^(.*?)(?:\t|\s*=\s*|:\s+)(.*)$/);
        this.#fill(added, "key", match ? match[1] : line);
        this.#fill(added, "value", match ? match[2] : "");
      } else {
        this.#fill(added, "value", line);
      }

      after = added;
      last = added;
    }

    this.#update();
    for (const pasted of this.#rows().filter((r) => !this.#isBlank(r))) {
      this.#reveal(pasted);
    }

    if (last) {
      this.#focus(last, "value");
    }
  };

  /**
   * Sets a pasted value. A select takes the item whose value or text matches.
   * @param {HTMLLIElement} row
   * @param {string} part
   * @param {string} text
   */
  #fill(row, part, text) {
    const control = this.#control(row, part);
    const value = text.trim();
    if (control instanceof HTMLSelectElement) {
      const lower = value.toLocaleLowerCase();
      const option = [...control.options].find(
        (option) =>
          option.value !== "" &&
          (option.value.toLocaleLowerCase() === lower || option.text.toLocaleLowerCase() === lower),
      );
      control.value = option?.value ?? "";
    } else if (control) {
      control.value = value;
      applyCasing(control);
    }
  }

  // ----------------------------------------------------------------------------------------------
  // Dragging by the handle, with pointer events so it also works for touch. The row lifts into a
  // copy that follows the pointer, its place stays open as a slot, and the other rows slide out of
  // the way. Escape cancels.
  // ----------------------------------------------------------------------------------------------

  /** @param {PointerEvent} event */
  #onPointerDown = (event) => {
    // A press in the editor moves focus before its click: errors that focus change shows wait for
    // the release, so that the rows don't move under the pointer
    this.#deferred = () => {};
    document.addEventListener(
      "pointerup",
      () => {
        setTimeout(() => {
          const reveal = this.#deferred;
          this.#deferred = null;
          reveal?.();
        });
      },
      { once: true },
    );

    const handle = /** @type {HTMLElement | null} */ (
      event.target instanceof Element ? event.target.closest(".sa-collection-editor-handle") : null
    );
    const row = this.#rowOf(handle);
    if (!handle || !row || event.button !== 0) {
      return;
    }

    this.#drag = {
      handle,
      row,
      pointerId: event.pointerId,
      startY: event.clientY,
      started: false,
    };
    handle.setPointerCapture(event.pointerId);
    document.addEventListener("pointermove", this.#onPointerMove);
    document.addEventListener("pointerup", this.#onPointerUp);
    document.addEventListener("pointercancel", this.#onPointerCancel);
    document.addEventListener("keydown", this.#onDragKeyDown, true);
  };

  /** @param {PointerEvent} event */
  #onPointerMove = (event) => {
    const drag = this.#drag;
    if (!drag || event.pointerId !== drag.pointerId) {
      return;
    }

    if (!drag.started) {
      if (Math.abs(event.clientY - drag.startY) < DRAG_THRESHOLD) {
        return;
      }

      this.#startDrag(drag);
    }

    if (drag.ghost) {
      drag.ghost.style.top = `${event.clientY - (drag.offsetY ?? 0)}px`;
    }

    // The slot moves before the first row whose middle is below the pointer
    const rows = this.#rows().filter((row) => row !== drag.row);
    const next =
      rows.find((row) => {
        const box = row.getBoundingClientRect();
        return event.clientY < box.top + box.height / 2;
      }) ?? null;
    if (next !== drag.row.nextElementSibling) {
      const before = this.#measure(drag.row);
      this.#list.insertBefore(drag.row, next);
      this.#slide(before, drag.row);
    }

    // Near the window's edge, the page scrolls
    if (event.clientY < SCROLL_EDGE) {
      scrollBy(0, -12);
    } else if (event.clientY > innerHeight - SCROLL_EDGE) {
      scrollBy(0, 12);
    }
  };

  /** @param {PointerEvent} event */
  #onPointerUp = (event) => {
    if (this.#drag && event.pointerId === this.#drag.pointerId) {
      this.#endDrag(false);
    }
  };

  /** @param {PointerEvent} event */
  #onPointerCancel = (event) => {
    if (this.#drag && event.pointerId === this.#drag.pointerId) {
      this.#endDrag(true);
    }
  };

  /** @param {KeyboardEvent} event */
  #onDragKeyDown = (event) => {
    if (this.#drag?.started && event.key === "Escape") {
      event.preventDefault();
      event.stopPropagation();
      this.#endDrag(true);
    }
  };

  /** @param {Drag} drag */
  #startDrag(drag) {
    const box = drag.row.getBoundingClientRect();
    const border = parseFloat(getComputedStyle(this).borderLeftWidth) || 0;

    // The copy sits in a box of its own, with the editor's columns so that its cells line up
    const ghost = document.createElement("div");
    ghost.className = "sa-collection-editor sa-collection-editor-ghost";
    ghost.setAttribute("aria-hidden", "true");
    for (const name of ["data-kind", "data-numbered", "data-sortable"]) {
      const value = this.getAttribute(name);
      if (value !== null) {
        ghost.setAttribute(name, value);
      }
    }

    ghost.style.left = `${box.left - border}px`;
    ghost.style.top = `${box.top - border}px`;
    ghost.style.width = `${box.width + 2 * border}px`;

    // A copy keeps the markup's values, not what was typed since
    const copy = /** @type {HTMLLIElement} */ (drag.row.cloneNode(true));
    copy.removeAttribute("data-invalid");
    copy.querySelector(".sa-collection-editor-item-error")?.remove();
    const controls = this.#controls(drag.row);
    this.#controls(copy).forEach((control, index) => {
      control.value = controls[index].value;
      control.removeAttribute("name");
      control.tabIndex = -1;
    });

    const list = document.createElement("ol");
    list.className = "sa-collection-editor-items";
    list.append(copy);
    ghost.append(list);
    document.body.append(ghost);

    drag.row.setAttribute("data-dragging", "");
    this.setAttribute("data-reordering", "");
    Object.assign(drag, {
      ghost,
      border,
      offsetY: drag.startY - box.top + border,
      order: this.#rows(),
      started: true,
    });
  }

  /** @param {boolean} cancel */
  #endDrag(cancel) {
    const drag = this.#drag;
    this.#drag = null;
    document.removeEventListener("pointermove", this.#onPointerMove);
    document.removeEventListener("pointerup", this.#onPointerUp);
    document.removeEventListener("pointercancel", this.#onPointerCancel);
    document.removeEventListener("keydown", this.#onDragKeyDown, true);
    this.removeAttribute("data-reordering");
    if (!drag?.ghost) {
      return;
    }

    if (cancel && drag.order) {
      const before = this.#measure(drag.row);
      this.#list.append(...drag.order);
      this.#slide(before, drag.row);
    }

    const ghost = drag.ghost;
    const finish = () => {
      ghost.remove();
      drag.row.removeAttribute("data-dragging");
      this.#update();
      drag.handle.focus({ preventScroll: true });
    };

    // The copy settles into the slot
    if (reduceMotion.matches || !this.isConnected) {
      finish();
      return;
    }

    const top = `${drag.row.getBoundingClientRect().top - (drag.border ?? 0)}px`;
    ghost.animate([{ top: ghost.style.top }, { top }], SLIDE).onfinish = finish;
    ghost.style.top = top;
  }

  // Rows slide to their new places: measure each row, change the order, then animate each from its
  // old place to its new one
  /** @param {HTMLLIElement} [skip] */
  #measure(skip) {
    return new Map(
      this.#rows()
        .filter((row) => row !== skip)
        .map((row) => [row, row.getBoundingClientRect().top]),
    );
  }

  /**
   * @param {Map<HTMLLIElement, number>} before
   * @param {HTMLLIElement} [skip]
   */
  #slide(before, skip) {
    if (reduceMotion.matches) {
      return;
    }

    for (const row of this.#rows()) {
      const from = before.get(row);
      const offset =
        row === skip || from === undefined ? 0 : from - row.getBoundingClientRect().top;
      if (offset) {
        row.animate([{ transform: `translateY(${offset}px)` }, { transform: "none" }], SLIDE);
      }
    }
  }

  /**
   * @param {string} key
   * @param {Record<string, string>} [values]
   */
  #text(key, values = {}) {
    return Object.entries(values).reduce(
      (text, [name, value]) => text.replaceAll(`{${name}}`, value),
      this.#texts[key] ?? "",
    );
  }
}

/** @param {HTMLInputElement} input */
function applyCasing(input) {
  const casing = input.dataset.casing;
  if (!casing) {
    return;
  }

  const value =
    casing === "upper" ? input.value.toLocaleUpperCase() : input.value.toLocaleLowerCase();
  if (value !== input.value) {
    const { selectionStart, selectionEnd } = input;
    input.value = value;
    input.setSelectionRange(selectionStart, selectionEnd);
  }
}

// Focuses the first invalid control a submit reports in the editors, once every invalid event of
// the submit has fired (a microtask would run after each one), unless an earlier control in the
// form is invalid and has the browser's message
/** @type {HTMLElement[]} */
let claims = [];

/** @param {HTMLElement} element */
function claimFocus(element) {
  claims.push(element);
  if (claims.length > 1) {
    return;
  }

  setTimeout(() => {
    const [first] = claims;
    claims = [];
    const form = first.closest("form");
    const earlier = form
      ? [...form.querySelectorAll(":invalid")].find(
          (element) =>
            !element.closest("sel-collection-editor") &&
            element.compareDocumentPosition(first) & Node.DOCUMENT_POSITION_FOLLOWING,
        )
      : null;
    if (!earlier) {
      first.focus();
    }
  });
}

// Rows left blank don't post. A capturing listener runs before the form's own submit listeners,
// such as htmx's.
document.addEventListener(
  "submit",
  (event) => {
    if (event.target instanceof HTMLFormElement) {
      for (const editor of event.target.querySelectorAll("sel-collection-editor")) {
        /** @type {CollectionEditor} */ (editor).dropBlankRows?.();
      }
    }
  },
  true,
);

if (!customElements.get("sel-collection-editor"))
  customElements.define("sel-collection-editor", CollectionEditor);
