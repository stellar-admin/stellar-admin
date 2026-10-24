// Lookup editors: the lookup picker loads into a sheet, and the server renders search results with htmx into its
// sa-command, which handles the keyboard; selecting or clearing an item only updates the field's hidden values and
// shows either the selection or the empty buttons. New loads the referenced resource's create form into a sheet, and
// a created item is selected like a search result.

// A level of the sheet stack. A button with data-sheet-open adds a level from the layout's template, above any that
// are open, and loads the URL it names into the level with htmx; a create form's lookups open further levels in
// turn. The sheet is modal, so only that opener's request, or a request from the content, can be in flight; closing
// the sheet cancels them and removes the level once it has slid out, so a late response never fills it. A failed
// load shows the sheet's error, and Retry repeats the opener's request. Covered levels recede behind the top one:
// Esc closes only the top level, and a click on a covered level's visible edge closes the levels above it.
class AdminRemoteSheet extends HTMLElement {
  static #count = 0;
  #opener = null;
  #url = null;

  // Each level replaces the template's id with its own, so its close button and content have unique targets
  static open(opener) {
    const level = ++AdminRemoteSheet.#count;
    const fragment = document.importNode(
      document.getElementById("admin-sheet-template").content,
      true,
    );
    const sheet = fragment.querySelector("admin-remote-sheet");
    for (const element of sheet.querySelectorAll("[id^='admin-sheet'], [commandfor]")) {
      for (const name of ["id", "commandfor"]) {
        const value = element.getAttribute(name);
        if (value?.startsWith("admin-sheet")) {
          element.setAttribute(name, value.replace("admin-sheet", `admin-sheet-${level}`));
        }
      }
    }

    document.body.append(sheet);
    sheet.#opener = opener;
    sheet.#url = dependentSheetUrl(opener);
    sheet.querySelector("dialog").showModal();
    AdminRemoteSheet.#restack();
    sheet.#load();
  }

  // The open levels' sheets, bottom first
  static get levels() {
    return [...document.querySelectorAll("admin-remote-sheet > sel-dialog > dialog")].filter(
      (dialog) => dialog.open,
    );
  }

  // Marks the covered levels and how far each recedes, for the stylesheet
  static #restack() {
    const levels = AdminRemoteSheet.levels;
    levels.forEach((dialog, index) => {
      const above = levels.length - 1 - index;
      dialog.toggleAttribute("data-under", above > 0);
      dialog.style.setProperty("--sa-sheet-shift", Math.min(above, 3));
      dialog.removeAttribute("data-hover");
      dialog.removeAttribute("data-pointing");
    });
  }

  // The covered level whose visible edge is under a click on the top level's backdrop, nearest first
  static coveredAt(event) {
    const levels = AdminRemoteSheet.levels;
    const top = levels.at(-1);
    if (event.target !== top || matchMedia("(width < 40rem)").matches) {
      return -1;
    }

    if (event.clientX >= top.getBoundingClientRect().left) {
      return -1;
    }

    return levels.findLastIndex((dialog, index) => {
      const box = dialog.getBoundingClientRect();
      return (
        index < levels.length - 1 &&
        event.clientX >= box.left &&
        event.clientX <= box.right &&
        event.clientY >= box.top &&
        event.clientY <= box.bottom
      );
    });
  }

  constructor() {
    super();

    // The dialog's close event doesn't bubble, so it is caught on the way down
    this.addEventListener(
      "close",
      () => {
        for (const element of [
          this.#opener,
          ...this.#content.querySelectorAll("[data-htmx-powered]"),
        ]) {
          if (element) {
            htmx.trigger(element, "htmx:abort");
          }
        }

        AdminRemoteSheet.#restack();
        this.#removeAfterExit();
      },
      { capture: true },
    );

    this.addEventListener("click", (event) => {
      if (event.target.closest("[data-sheet='retry']")) {
        this.#show("loading");
        this.#load();
      } else if (event.target.closest("[data-sheet='close']")) {
        this.querySelector("dialog").close();
      }
    });
  }

  // The opener is outside the sheet, so its request's errors only reach the document
  connectedCallback() {
    for (const name of ["htmx:response:error", "htmx:error"]) {
      document.addEventListener(name, this.#onError);
    }
  }

  disconnectedCallback() {
    for (const name of ["htmx:response:error", "htmx:error"]) {
      document.removeEventListener(name, this.#onError);
    }
  }

  #onError = (event) => {
    const ctx = event.detail.ctx;
    if (ctx.sourceElement !== this.#opener || event.detail.error?.name === "AbortError") {
      return;
    }

    ctx.swap = "none";
    this.#show("error");
  };

  async #removeAfterExit() {
    const dialog = this.querySelector("dialog");
    await Promise.allSettled(dialog.getAnimations().map((animation) => animation.finished));
    this.remove();
  }

  get #content() {
    return this.querySelector("[data-sheet='content']");
  }

  // The opener can sit where it inherits htmx attributes, such as a row of the index grid, so the load sets its own
  // swap and drops any inherited selection and history
  #load() {
    htmx.ajax("GET", this.#url, {
      source: this.#opener,
      target: this.#content,
      swap: "innerHTML",
      select: undefined,
      selectOOB: undefined,
      push: undefined,
      replace: undefined,
    });
  }

  #show(name) {
    this.#content.replaceChildren(
      this.querySelector(`template[data-sheet='${name}']`).content.cloneNode(true),
    );
  }
}

document.addEventListener("click", (event) => {
  const opener = event.target.closest("[data-sheet-open]");
  if (opener) {
    AdminRemoteSheet.open(opener);
    return;
  }

  const covered = AdminRemoteSheet.coveredAt(event);
  if (covered >= 0) {
    for (const dialog of AdminRemoteSheet.levels.slice(covered + 1).reverse()) {
      dialog.close();
    }
  }
});

// Ctrl, or Cmd, in the shortcuts the server renders shows as ⌘ on a Mac
const mac = /mac|iphone|ipad/i.test(navigator.userAgentData?.platform ?? navigator.platform);

function showModKeys(root) {
  if (!mac) {
    return;
  }

  for (const key of root.querySelectorAll("[data-mod-key]")) {
    key.textContent = "⌘";
  }
}

showModKeys(document);

// Create and edit pages: Ctrl+Enter or Cmd+Enter saves, and Ctrl+Shift+Enter or Cmd+Shift+Enter saves and adds
// another. The add another button comes before the submit button, so a plain Enter in a field, which would press the
// first, presses the submit button instead. After adding another, the form's first field has focus
document.addEventListener("keydown", (event) => {
  const form = event.target.closest?.("form[data-resource-form]");
  if (!form || event.key !== "Enter" || event.defaultPrevented || event.isComposing) {
    return;
  }

  const another = form.querySelector("[data-form-add-another]");
  const modified = event.ctrlKey || event.metaKey;
  const button =
    modified && event.shiftKey
      ? another
      : modified ||
          (another &&
            !event.shiftKey &&
            !event.altKey &&
            event.target.matches(
              "input:not([type='checkbox'], [type='radio'], [type='button'], [type='submit'], [type='reset'], [type='file'])",
            ))
        ? form.querySelector("[data-form-submit]")
        : null;
  if (button) {
    event.preventDefault();
    form.requestSubmit(button);
  }
});

document
  .querySelector("form[data-focus-first-field='true']")
  ?.querySelector(
    "input:not([type='hidden'], [disabled]), select:not([disabled]), textarea:not([disabled]), [data-lookup='open']:not([hidden] *)",
  )
  ?.focus();

// Index grid inline rows. One row is edited at a time: Edit or Add cancels the row being edited first. In the row,
// Enter in a text field or Ctrl+Enter saves and Escape cancels, focusing the row's Edit button or the Add button again.
// A row's message, which follows it with the errors no cell shows, goes when the row is cancelled or a save's response
// replaces the row, which brings the message back if the save is rejected again. So does the count of rows added
// with add another, which a new row's response brings back.
function removeRowMessage(row) {
  let next = row?.nextElementSibling;
  while (next?.matches(".sa-row-message, .sa-add-another-status")) {
    const following = next.nextElementSibling;
    next.remove();
    next = following;
  }
}

// The grid's own Add button while it shows, or else the page header's
function addButton() {
  return (
    document.querySelector("#index-page-grid .sa-table-add-row [data-row-add]") ??
    document.querySelector("[data-row-add]")
  );
}

// The first field of a row in edit mode, or its first field with an error
function focusRow(row, { invalid = false, select = false } = {}) {
  const field =
    (invalid ? row.querySelector("[aria-invalid='true']:not([hidden] *)") : null) ??
    row.querySelector(
      "input:not([type='hidden']), select, textarea, [data-lookup='open']:not([hidden] *)",
    );
  field?.focus({ preventScroll: true });
  if (select && field instanceof HTMLInputElement) {
    field.select();
  }
}

// While a row is edited or added and the grid dims the other rows for that operation (data-inline-dim lists "edit"
// and "create"), the other rows, the Add buttons, the scopes, the search, the sort links and the pager step back and
// can't be used until the row is saved or cancelled. The edited row's message and details stay usable
function unlockInlineRow() {
  for (const locked of document.querySelectorAll("[data-inline-locked]")) {
    locked.removeAttribute("data-inline-locked");
    locked.inert = false;
  }
}

function lockInlineRow() {
  unlockInlineRow();
  const grid = document.getElementById("index-page-grid");
  const editing = grid?.querySelector(".sa-table-body > tr[data-editing]");
  const dim = (grid?.dataset.inlineDim ?? "").split(" ");
  if (!editing || !dim.includes(editing.dataset.key === "new" ? "create" : "edit")) {
    return;
  }

  // The edited row's message and details follow it, up to the next row or the add row
  const kept = [editing];
  let next = editing.nextElementSibling;
  while (next && !next.matches("[data-key], .sa-table-add-row")) {
    kept.push(next);
    next = next.nextElementSibling;
  }

  // Rows added with add another stay next to the new row, and usable
  const locked = [
    ...[...editing.parentElement.children].filter(
      (row) => !kept.includes(row) && !row.matches("[data-empty], [data-added]"),
    ),
    ...document.querySelectorAll(
      "[data-row-add]:not(#index-page-grid *), #index-page-scopes, #index-page-search, " +
        "#index-page-grid [data-slot='data-grid-sort-link'], #index-page-grid [data-slot='card-footer']",
    ),
  ];
  for (const element of locked) {
    element.setAttribute("data-inline-locked", "");
    element.inert = true;
  }
}

// Add another keeps a new row open after each save, with the rows it added next to it. Saving the new row, or
// cancelling it, ends the run: the grid reloads once, unless saved rows stay where they were added. Edit or Add, which
// open another row straight away, leave the added rows in place
function endAddAnother({ reload = true } = {}) {
  const grid = document.getElementById("index-page-grid");
  const added = [...(grid?.querySelectorAll(".sa-table-body > tr[data-added]") ?? [])];
  if (added.length === 0) {
    return;
  }

  for (const row of added) {
    row.removeAttribute("data-added");
  }

  if (!reload || grid.dataset.addAnother !== "reload") {
    return;
  }

  const keys = added.map((row) => row.dataset.key);
  const container = document.getElementById("index-page-data-grid");
  Promise.resolve(
    htmx.ajax("GET", location.href, {
      source: container,
      target: container,
      swap: "outerHTML",
      select: "#index-page-data-grid",
      selectOOB: undefined,
      push: undefined,
      replace: undefined,
    }),
  )
    .then(() => {
      const body = document.querySelector("#index-page-grid .sa-table-body");
      for (const key of keys) {
        const row = body?.querySelector(`:scope > tr[data-key="${CSS.escape(key)}"]`);
        if (row) {
          flashRow(row, { focus: false });
        }
      }
      addButton()?.focus({ preventScroll: true });
    })
    .catch(() => {});
}

let switchingRow = false;

document.addEventListener(
  "click",
  (event) => {
    if (event.target.closest("[data-row-edit], [data-row-add]")) {
      switchingRow = true;
      document.querySelector("#index-page-grid tr[data-editing] [data-row-cancel]")?.click();
      switchingRow = false;
    }

    // Cancel ends the edit; sel-table restores the row, and stops the click
    const cancelled = event.target
      .closest("#index-page-grid tr[data-editing] [data-row-cancel]")
      ?.closest("tr");
    if (cancelled) {
      removeRowMessage(cancelled);
      unlockInlineRow();
      if (cancelled.dataset.key === "new") {
        endAddAnother({ reload: !switchingRow });
      }
    }
  },
  { capture: true },
);

document.addEventListener("keydown", (event) => {
  const row = event.target.closest?.("#index-page-grid tr[data-editing]");
  if (!row || event.defaultPrevented || event.isComposing) {
    return;
  }

  // Ctrl+Shift+Enter or Cmd+Shift+Enter saves a new row and adds another; Ctrl+Enter or Cmd+Enter saves from any field
  const modified = event.ctrlKey || event.metaKey;
  if (event.key === "Enter" && modified && event.shiftKey) {
    const another = row.querySelector("[data-row-save-another]");
    if (another) {
      event.preventDefault();
      another.click();
    }
  } else if (
    event.key === "Enter" &&
    (modified ||
      event.target.matches(
        "input:not([type='checkbox'], [type='radio'], [type='button'], [type='submit'], [type='reset'])",
      ))
  ) {
    event.preventDefault();
    row.querySelector("[data-row-save]")?.click();
  } else if (event.key === "Escape") {
    event.preventDefault();
    const key = row.dataset.key;
    const body = row.parentElement;
    row.querySelector("[data-row-cancel]")?.click();
    const display = key === "new" ? null : body.querySelector(`tr[data-key="${CSS.escape(key)}"]`);
    (display ? display.querySelector("[data-row-edit]") : addButton())?.focus();
  }
});

function flashRow(row, { focus = true } = {}) {
  row.setAttribute("data-saved", "");
  setTimeout(() => row.removeAttribute("data-saved"), 1400);
  if (focus) {
    row.querySelector("[data-row-edit]")?.focus({ preventScroll: true });
  }
}

// The grid's footer, with its record count, as the page's address shows it
function reloadGridFooter() {
  const selector = "#index-page-grid [data-slot='card-footer']";
  const footer = document.querySelector(selector);
  if (footer) {
    htmx.ajax("GET", location.href, {
      source: footer,
      target: footer,
      swap: "outerHTML",
      select: selector,
      selectOOB: undefined,
      push: undefined,
      replace: undefined,
    });
  }
}

// A response swaps out the row it came from, and htmx sends the swap's end for the detached row's button to the
// document: the work after the swap is set up before it, by request, with the rows around the one replaced to find
// what came in its place
const rowSwaps = new WeakMap();

document.addEventListener("htmx:after:swap", (event) => {
  const ctx = event.detail?.ctx;
  showModKeys(document);
  lockInlineRow();
  rowSwaps.get(ctx)?.();
  rowSwaps.delete(ctx);
});

document.addEventListener("htmx:before:swap", (event) => {
  const source = event.target;
  if (!(source instanceof Element)) {
    return;
  }

  const save = source.closest("[data-row-save]");
  const another = source.closest("[data-row-save-another]");
  const row = source.closest("tr");
  if (save || another) {
    removeRowMessage(row);
  }

  const kind = another
    ? "another"
    : save
      ? "save"
      : source.matches("[data-row-edit]")
        ? "edit"
        : source.matches("[data-row-add]")
          ? "add"
          : null;
  if (!kind) {
    return;
  }

  const body = row?.parentElement;
  const previous = row?.previousElementSibling;
  const ctx = event.detail.ctx;
  rowSwaps.set(ctx, () => {
    const grid = document.getElementById("index-page-grid");
    const editing = grid?.querySelector("tr[data-editing]");
    if (kind === "another") {
      // The saved row joins the rows added in the run, and the new row takes the focus, or its first field with an
      // error when the save was rejected. The pager's count may have changed when the rows stay in place
      const key = ctx.response?.headers.get("SA-Saved-Row");
      const saved = key
        ? grid?.querySelector(`.sa-table-body > tr[data-key="${CSS.escape(key)}"]`)
        : null;
      if (saved) {
        saved.setAttribute("data-added", "");
        flashRow(saved, { focus: false });
        lockInlineRow();
        if (grid.dataset.addAnother === "keep") {
          reloadGridFooter();
        }
      }
      if (editing) {
        editing.scrollIntoView({ block: "nearest", behavior: "smooth" });
        focusRow(editing, { invalid: true });
      }
    } else if (kind === "add" && editing) {
      editing.scrollIntoView({ block: "nearest", behavior: "smooth" });
      focusRow(editing);
    } else if (kind === "edit" && editing) {
      focusRow(editing, { select: true });
    } else if (kind === "save" && editing) {
      focusRow(editing, { invalid: true });
    } else if (kind === "save" && ctx.response?.headers.has("HX-Retarget")) {
      // A save that reloaded the grid names the saved row when the page shows it. Without it, focus goes to the Add
      // button after adding, or else to the first row
      const key = ctx.response.headers.get("SA-Saved-Row");
      const display = key
        ? grid?.querySelector(`.sa-table-body > tr[data-key="${CSS.escape(key)}"]`)
        : null;
      if (display) {
        flashRow(display);
      } else {
        (row?.dataset.key === "new"
          ? addButton()
          : grid?.querySelector(".sa-table-body [data-row-edit]")
        )?.focus();
      }
    } else if (kind === "save") {
      // Saving the new row ends a run of add another; its rows stay where they were added
      endAddAnother({ reload: false });

      // A saved row takes the place of the editing row, after the row that was before it. The pager's count may have
      // changed, so the footer is reloaded
      const saved = previous?.isConnected ? previous.nextElementSibling : body?.firstElementChild;
      const display = saved?.matches("tr[data-key]")
        ? saved
        : grid?.querySelector(`tr[data-key="${CSS.escape(row?.dataset.key ?? "")}"]`);
      if (display) {
        flashRow(display);
      }
      reloadGridFooter();
    }
  });
});

// An editing row saves with htmx rather than by submitting a form, so it waits here for its images to finish uploading,
// as a form does: until then the row's value is still the old image. Cancelling the row still works
document.addEventListener("htmx:config:request", (event) => {
  if (event.detail?.ctx?.request?.method?.toUpperCase() === "GET") return;
  const row = event.target instanceof Element ? event.target.closest("tr") : null;
  const uploading = [...(row?.querySelectorAll("sel-image-upload") ?? [])].some(
    (upload) => upload.uploading,
  );
  if (!uploading) return;

  event.preventDefault();
  window.stellarAdmin?.toast?.warning("Wait for the upload to finish, then save again.");
});

// Toasts the server queued for an htmx request, such as a row added in the index grid
document.addEventListener("htmx:after:request", (event) => {
  const headers = event.detail?.ctx?.response?.headers;
  if (headers) {
    window.stellarAdmin?.toast?.fromResponse(headers);
  }
});

document.addEventListener("mousemove", (event) => {
  const levels = AdminRemoteSheet.levels;
  const covered = AdminRemoteSheet.coveredAt(event);
  levels.forEach((dialog, index) => dialog.toggleAttribute("data-hover", index === covered));
  levels.at(-1)?.toggleAttribute("data-pointing", covered >= 0);
});

// Esc closes the top level only, unless a popover in it is open: browsers can group modal dialogs and close them all
// on one Esc, so the stack handles the key instead of the dialog's cancel
document.addEventListener("keydown", (event) => {
  const top = AdminRemoteSheet.levels.at(-1);
  if (
    event.key !== "Escape" ||
    event.defaultPrevented ||
    !top?.contains(event.target) ||
    top.querySelector(":popover-open")
  ) {
    return;
  }

  event.preventDefault();
  top.close();
});

customElements.define("admin-remote-sheet", AdminRemoteSheet);

// A lookup's picker in a sheet: a search and the results to choose from; for names the editor that results belong to,
// its hidden input for a single-select lookup. It only handles events from its own elements, so htmx events dispatched
// on the document for elements already removed from the page never reach it, and removing it cancels its requests.
// A multi-select picker toggles items in its editor and stays open: All | Selected switches to searching only the
// selected items, where an unchecked item leaves the list, and Done or Ctrl+Enter (⌘↵ on a Mac) closes the sheet.
// New, or Alt+N (⌥N), opens the create form as the next level, and the created item comes back checked at the top of
// the results, with the search kept.
class AdminLookupPicker extends HTMLElement {
  static #mac = /mac|iphone|ipad/i.test(navigator.userAgentData?.platform ?? navigator.platform);
  #enter = false;

  constructor() {
    super();

    // Selecting a result closes the sheet, then passes the selection to the editor
    this.addEventListener("itemselect", (event) => {
      const item = event.target;
      if (item.matches("[data-lookup='more']")) {
        return;
      }

      if (this.#multiple) {
        this.#toggle(item);
        return;
      }

      const selection = item.querySelector("template[data-lookup='selection']");
      this.closest("dialog").close();
      this.#editor.dispatchEvent(
        new CustomEvent("lookupselect", {
          bubbles: true,
          detail: { value: event.detail.value, selection },
        }),
      );
    });

    // Searches send the editor's current values, so the server marks the selected results
    this.addEventListener("htmx:config:request", (event) => {
      const body = event.detail.ctx.request.body;
      body.delete("selected");
      for (const value of this.#selected) {
        body.append("selected", value);
      }

      if (this.#selectedOnly) {
        body.set("selectedOnly", "true");
      }
    });

    // A failed search shows the error in the results, and a failed Load more in its place; cancelled requests show nothing
    for (const name of ["htmx:response:error", "htmx:error"]) {
      this.addEventListener(name, (event) => {
        if (event.detail.error?.name === "AbortError") {
          return;
        }

        event.detail.ctx.swap = "none";
        const error = this.#template("error");
        if (event.target.matches("[data-lookup='more']")) {
          event.target.replaceWith(error);
        } else {
          this.querySelector("[data-lookup='results']").replaceChildren(error);
        }
      });
    }

    // Retry shows the loading rows again and repeats the search; typing keeps the current results until the new ones arrive
    this.addEventListener("click", (event) => {
      if (event.target.closest("[data-lookup='retry']")) {
        this.#search();
      } else if (event.target.closest("[data-lookup='clear-all']")) {
        this.#clearAll();
      } else if (event.target.closest("[data-lookup='done']")) {
        this.closest("dialog").close();
      }
    });

    this.addEventListener("change", (event) => {
      if (event.target.closest("[data-lookup='view']")) {
        this.querySelector("[data-lookup='clear-all']").hidden = !this.#selectedOnly;
        this.#update();
        this.#search();
      }
    });

    // Ctrl+Enter closes before the command list sees the Enter; a plain Enter marks the toggle as made from the
    // keyboard, so the search text is selected for the next search. Alt+N is matched by its key's position, since ⌥N
    // types a character on a Mac.
    this.addEventListener(
      "keydown",
      (event) => {
        if (!this.#multiple || event.isComposing) {
          return;
        }

        const create = this.querySelector("[data-sheet-open][data-lookup='create']");
        if (event.altKey && event.code === "KeyN" && !event.ctrlKey && !event.metaKey && create) {
          event.preventDefault();
          event.stopPropagation();
          create.click();
          return;
        }

        if (event.key !== "Enter") {
          return;
        }

        if (AdminLookupPicker.#mac ? event.metaKey : event.ctrlKey) {
          event.preventDefault();
          event.stopPropagation();
          this.closest("dialog").close();
        } else {
          this.#enter = true;
          setTimeout(() => (this.#enter = false));
        }
      },
      { capture: true },
    );
  }

  // A multi-select editor sends one change when the sheet closes, if its values changed. A lookup whose items depend
  // on other fields names their values under the title
  connectedCallback() {
    showDependentFilter(this, this.#editor);
    if (!this.#multiple) {
      return;
    }

    const editor = this.#editor;
    editor.beginEdit();
    editor.addEventListener("lookupcreated", this.#onCreated);
    this.closest("dialog")?.addEventListener(
      "close",
      () => {
        editor.removeEventListener("lookupcreated", this.#onCreated);
        editor.endEdit();
      },
      { once: true },
    );

    if (AdminLookupPicker.#mac) {
      this.querySelector("[data-lookup='done-key'] kbd").textContent = "⌘";
      this.querySelector("[data-lookup='done']").setAttribute("aria-keyshortcuts", "Meta+Enter");
      const newKey = this.querySelector("[data-lookup='new-key'] kbd");
      if (newKey) {
        newKey.textContent = "⌥";
      }
    }

    this.#update();
  }

  disconnectedCallback() {
    for (const element of this.querySelectorAll("[data-htmx-powered]")) {
      element.dispatchEvent(new Event("htmx:abort"));
    }
  }

  get #editor() {
    return document.getElementById(this.getAttribute("for"));
  }

  // The server renders the created item's row, checked, as the Selected view does, along with the value the form
  // posts for it. If that fails, the key is selected without a row.
  #onCreated = async (event) => {
    const key = event.detail.key;
    const search = this.querySelector("[data-lookup='search']");
    const url = new URL(search.getAttribute("hx-get"), document.baseURI);
    url.searchParams.set("selectedOnly", "true");
    url.searchParams.append("selected", key);

    let row = null;
    try {
      const response = await fetch(url);
      if (response.ok) {
        const parsed = document.createElement("template");
        parsed.innerHTML = await response.text();
        row = parsed.content.querySelector("[data-lookup='item']");
      }
    } catch {
      // The key is selected below
    }

    const value = row?.dataset.value ?? key;
    if (!this.#selected.includes(value)) {
      this.#editor.toggle(value);
    }

    if (row) {
      for (const element of this.#results.querySelectorAll(
        `[data-lookup='message'], [data-lookup='item'][data-value="${CSS.escape(value)}"]`,
      )) {
        element.remove();
      }

      this.#results.prepend(row);
    }

    this.#update();
    search.focus();
    search.select();
  };

  get #multiple() {
    return this.hasAttribute("multiple");
  }

  get #selected() {
    return this.#multiple ? this.#editor.values : [this.#editor.value];
  }

  get #selectedOnly() {
    return this.querySelector("[data-lookup='view'] input[value='selected']")?.checked ?? false;
  }

  get #results() {
    return this.querySelector("[data-lookup='results']");
  }

  #toggle(item) {
    const checked = this.#editor.toggle(item.dataset.value);
    if (checked) {
      item.setAttribute("data-checked", "true");
    } else {
      item.removeAttribute("data-checked");
    }

    if (!checked && this.#selectedOnly) {
      item.remove();
      if (!this.#results.querySelector("[data-lookup='item']")) {
        this.#search();
      }
    }

    this.#update();
    if (this.#enter) {
      this.querySelector("[data-lookup='search']").select();
    }
  }

  // No undo: the Selected view empties, and searches again to show that nothing is selected
  #clearAll() {
    this.#editor.clearAll();
    for (const item of this.#results.querySelectorAll("[data-lookup='item']")) {
      item.removeAttribute("data-checked");
    }

    this.#update();
    if (this.#selectedOnly) {
      this.#search();
    }

    this.querySelector("[data-lookup='search']").focus();
  }

  #search() {
    this.#results.replaceChildren(this.#template("loading"));
    this.querySelector("[data-lookup='search']").dispatchEvent(new Event("lookupretry"));
  }

  // The count, and the Selected view and Clear all are only enabled with a selection, though the Selected view stays
  // reachable while it shows
  #update() {
    const count = this.#selected.length;
    this.querySelector("[data-lookup='count']").textContent = count;
    this.querySelector("[data-lookup='view'] input[value='selected']").disabled =
      count === 0 && !this.#selectedOnly;
    this.querySelector("[data-lookup='clear-all']").disabled = count === 0;
  }

  #template(name) {
    return this.querySelector(`template[data-lookup='${name}']`).content.cloneNode(true);
  }
}

customElements.define("admin-lookup-picker", AdminLookupPicker);

// A resource's create form in a sheet, for a lookup editor's New button. It posts with htmx into the sheet, so a
// rejected form comes back with its errors and a created item comes back as admin-lookup-created. A post that
// fails shows the form's error and keeps what was typed.
class AdminCreateSheet extends HTMLElement {
  constructor() {
    super();

    this.addEventListener("htmx:config:request", () => {
      this.querySelector("[data-create-sheet='error']").hidden = true;
    });

    for (const name of ["htmx:response:error", "htmx:error"]) {
      this.addEventListener(name, (event) => {
        if (event.detail.error?.name === "AbortError") {
          return;
        }

        event.detail.ctx.swap = "none";
        this.querySelector("[data-create-sheet='error']").hidden = false;
      });
    }
  }

  // The first field with an error has focus, or else the first field. After a swap, htmx puts focus back on the
  // element with the previously focused id, so this waits until it has.
  connectedCallback() {
    queueMicrotask(() => {
      const field =
        this.querySelector("[aria-invalid='true'], .input-validation-error") ??
        this.querySelector(
          "input:not([type='hidden']), select, textarea, [data-lookup='open']:not([hidden] *)",
        );
      field?.focus();
    });
  }
}

customElements.define("admin-create-sheet", AdminCreateSheet);

// Replaces the create form once the item is created: it closes its level and sends lookupcreated with the new
// item's key from the hidden input it names, in the level below. The editor that owns the input decides what to do
// with it; a single-select editor selects it in place of its value. Without a key, the level only closes.
class AdminLookupCreated extends HTMLElement {
  connectedCallback() {
    const input = document.getElementById(this.getAttribute("for"));
    const key = this.getAttribute("value");

    this.closest("dialog")?.close();
    if (key) {
      input?.dispatchEvent(new CustomEvent("lookupcreated", { bubbles: true, detail: { key } }));
    }
  }
}

customElements.define("admin-lookup-created", AdminLookupCreated);

// A lookup editor: the picker sends a selection to its hidden input as lookupselect, a create form sends a created
// item's key as lookupcreated, and Clear empties it. It shows either the selection or the empty buttons, and moves
// focus to the button that is still shown.
class AdminLookupSheetEditor extends HTMLElement {
  constructor() {
    super();

    this.addEventListener("lookupselect", (event) => {
      this.#setValue(event.detail.value, event.detail.selection);
      this.querySelector("[data-lookup='selected'] [data-lookup='open']").focus();
    });

    this.addEventListener("lookupcreated", (event) => this.#selectCreated(event.detail.key));

    this.addEventListener("click", (event) => {
      if (event.target.closest("[data-lookup='clear']")) {
        this.#setValue("", null);
        this.querySelector("[data-lookup='empty'] [data-lookup='open']").focus();
      }
    });
  }

  // The server renders the created item's display from the field's settings, and the value the form posts for it.
  // If that fails, the key stands in for the title, as it does for a value the items no longer have.
  async #selectCreated(key) {
    const url = new URL(this.getAttribute("selection-url"), document.baseURI);
    url.searchParams.set("value", key);

    let selection = null;
    try {
      const response = await fetch(url);
      if (response.ok) {
        const parsed = document.createElement("template");
        parsed.innerHTML = await response.text();
        selection = parsed.content.querySelector("template[data-lookup='selection']");
      }
    } catch {
      // The fallback below shows the key
    }

    this.dispatchEvent(
      new CustomEvent("lookupselect", {
        detail: {
          value: selection?.dataset.value ?? key,
          selection: selection ?? this.#keySelection(key),
        },
      }),
    );
  }

  #keySelection(key) {
    const selected = this.querySelector("[data-lookup='selected']");
    const media = selected.querySelector("[data-lookup='media']").cloneNode(false);
    const text = selected.querySelector("[data-lookup='text']").cloneNode(true);
    media.hidden = true;
    text.querySelector("[data-slot='item-description']")?.remove();
    text.querySelector("[data-lookup='title']").textContent = key;

    const template = document.createElement("template");
    template.content.append(media, text);
    return template;
  }

  #setValue(value, template) {
    const input = this.querySelector("[data-lookup='value']");
    const selected = this.querySelector("[data-lookup='selected']");
    if (template) {
      for (const part of [...template.content.cloneNode(true).children]) {
        selected.querySelector(`[data-lookup='${part.dataset.lookup}']`).replaceWith(part);
      }
    }

    selected.hidden = value === "";
    this.querySelector("[data-lookup='empty']").hidden = value !== "";

    if (input.value !== value) {
      input.value = value;
      input.dispatchEvent(new Event("change", { bubbles: true }));
    }
  }
}

customElements.define("admin-lookup-sheet-editor", AdminLookupSheetEditor);

// A multi-select lookup editor: a hidden input per selected value, and the items that display them. The picker toggles
// values and clears them all, and the server renders the items again from the values, the latest request winning.
// Remove takes an item and its value out, and Clear takes them all. It shows either the selection or the empty
// buttons, and moves focus to a button that is still shown. It fires change on itself when Remove or Clear changes
// the value, and once when the sheet closes if the value changed while it was open.
class AdminMultiLookupSheetEditor extends HTMLElement {
  #initial = null;
  #request = null;

  constructor() {
    super();

    this.addEventListener("click", (event) => {
      const remove = event.target.closest("[data-lookup='remove']");
      if (remove) {
        this.#remove(remove.closest("[data-lookup='item']"));
      } else if (event.target.closest("[data-lookup='clear']")) {
        this.#clear();
      }
    });
  }

  get values() {
    return this.#values.map((input) => input.value);
  }

  beginEdit() {
    this.#initial = this.values.join("\n");
  }

  clearAll() {
    for (const input of this.#values) {
      input.remove();
    }

    this.#show();
    this.#render();
  }

  endEdit() {
    const changed = this.#initial !== null && this.#initial !== this.values.join("\n");
    this.#initial = null;
    this.#openButton()?.focus();
    if (changed) {
      this.dispatchEvent(new Event("change", { bubbles: true }));
    }
  }

  // Adds a value at the end, or removes it, and returns whether it is selected
  toggle(value) {
    const input = this.#values.find((element) => element.value === value);
    if (input) {
      input.remove();
    } else {
      const added = document.createElement("input");
      added.type = "hidden";
      added.name = this.getAttribute("name");
      added.value = value;
      this.querySelector("[data-lookup='values']").append(added);
    }

    this.#show();
    this.#render();
    return !input;
  }

  get #items() {
    return [...this.querySelectorAll("[data-lookup='item']")];
  }

  get #values() {
    return [...this.querySelectorAll("[data-lookup='values'] input")];
  }

  // Focus moves to the next item's Remove, or the previous one's after the last item, or else the add button
  #remove(item) {
    const items = this.#items;
    const index = items.indexOf(item);
    const next = items[index + 1] ?? items[index - 1];

    this.#values.find((input) => input.value === item.dataset.value)?.remove();
    item.remove();
    this.#changed();
    (next?.querySelector("[data-lookup='remove']") ?? this.#openButton())?.focus();
  }

  #clear() {
    for (const element of [...this.#values, ...this.#items]) {
      element.remove();
    }

    this.#changed();
    this.#openButton()?.focus();
  }

  // A rendering still on its way has older values, so it is replaced
  #changed() {
    this.#show();
    if (this.#request) {
      this.#render();
    }

    this.dispatchEvent(new Event("change", { bubbles: true }));
  }

  #openButton() {
    return this.querySelector("[data-lookup='open']:not([hidden] *)");
  }

  // Removed items leave straight away; if the rendering fails, added items show when the form is next rendered
  async #render() {
    const values = this.values;
    for (const item of this.#items) {
      if (!values.includes(item.dataset.value)) {
        item.remove();
      }
    }

    this.#request?.abort();
    const request = new AbortController();
    this.#request = request;

    const url = new URL(this.getAttribute("selection-url"), document.baseURI);
    for (const value of values) {
      url.searchParams.append("selected", value);
    }

    try {
      const response = await fetch(url, { signal: request.signal });
      if (!response.ok) {
        return;
      }

      const html = await response.text();
      if (this.#request !== request) {
        return;
      }

      const container = this.querySelector("[data-lookup='items']");
      const focused = container.contains(document.activeElement);
      container.innerHTML = html;
      if (focused) {
        this.#openButton()?.focus();
      }
    } catch {
      // A cancelled or failed rendering keeps the items shown
    } finally {
      if (this.#request === request) {
        this.#request = null;
      }
    }
  }

  #show() {
    const empty = this.#values.length === 0;
    this.querySelector("[data-lookup='selected']").hidden = empty;
    this.querySelector("[data-lookup='empty']").hidden = !empty;
  }
}

customElements.define("admin-multi-lookup-sheet-editor", AdminMultiLookupSheetEditor);

// Fields whose items depend on other fields. admin-dependent-field names its field, the inputs and labels of the fields it
// depends on, and the URL that renders it again. When one of those fields changes, the fields that depend on it,
// directly or through other dependents, keep their controls under a skeleton while the server renders them again with
// their values cleared, and each replaces its element. The request sends only the values of the fields they depend on;
// a newer change of the same field cancels it, and a failed one leaves the fields as they were.
class AdminDependentField extends HTMLElement {
  get parents() {
    try {
      return JSON.parse(this.getAttribute("parents") ?? "[]");
    } catch {
      return [];
    }
  }
}

customElements.define("admin-dependent-field", AdminDependentField);

const dependentRefreshes = new WeakMap();

// The fields of the form, or of the grid row, that an element is in
function dependentScope(element) {
  return element.closest("tr[data-editing], form") ?? document;
}

// The values an input name posts in a scope, as the form would post them
function postedValues(scope, name) {
  const values = [];
  for (const input of scope.querySelectorAll(`[name="${CSS.escape(name)}"]`)) {
    // A control in a disabled fieldset doesn't post either
    if (
      input.matches(":disabled") ||
      !("value" in input) ||
      input.matches("button, [type='button'], [type='submit']")
    ) {
      continue;
    }

    if (input instanceof HTMLSelectElement) {
      values.push(...[...input.selectedOptions].map((option) => option.value));
    } else if (!(input.type === "checkbox" || input.type === "radio") || input.checked) {
      values.push(input.value);
    }
  }

  return values;
}

// The values of the fields a field depends on, as Model.Field, which the server binds to the form's model
function appendParentValues(scope, fields, append) {
  const seen = new Set();
  for (const field of fields) {
    for (const parent of field.parents) {
      if (!seen.has(parent.name)) {
        seen.add(parent.name);
        for (const value of postedValues(scope, parent.name)) {
          append(`Model.${parent.field}`, value);
        }
      }
    }
  }
}

// A lookup's sheet loads its items with the values of the fields they depend on
function dependentSheetUrl(opener) {
  const url = opener.dataset.sheetOpen;
  const field = opener.closest("admin-dependent-field");
  if (!field) {
    return url;
  }

  const resolved = new URL(url, document.baseURI);
  appendParentValues(dependentScope(field), [field], (name, value) =>
    resolved.searchParams.append(name, value),
  );
  return resolved.pathname + resolved.search;
}

// The text a field shows for its value: a select's chosen option, a lookup's selection, or the labels of the checked
// choices
function displayedValue(scope, name) {
  const titles = [];
  for (const input of scope.querySelectorAll(`[name="${CSS.escape(name)}"]`)) {
    if (input.matches(":disabled")) {
      continue;
    }

    if (input instanceof HTMLSelectElement) {
      titles.push(
        ...[...input.selectedOptions]
          .filter((option) => option.value)
          .map((option) => option.text.trim()),
      );
    } else if (input.matches("[data-lookup='value']")) {
      const title = input
        .closest("admin-lookup-sheet-editor")
        ?.querySelector("[data-lookup='selected'] [data-lookup='title']");
      if (input.value && title) {
        titles.push(title.textContent.trim());
      }
    } else if (input.matches("admin-multi-lookup-sheet-editor")) {
      const items = [...input.querySelectorAll("[data-lookup='items'] [data-lookup='item']")];
      const shown = items.length > 0 ? items : [...input.querySelectorAll("[data-lookup='items']")];
      titles.push(
        ...shown.map((item) =>
          (item.querySelector("[data-lookup='title']") ?? item).textContent
            .trim()
            .replace(/\s+/g, " "),
        ),
      );
    } else if (input.type === "checkbox" || input.type === "radio") {
      if (input.checked && input.value) {
        titles.push(
          (input.labels?.[0] ?? input.closest("label"))?.textContent.trim() ?? input.value,
        );
      }
    } else if (input.type !== "hidden" && input.value) {
      titles.push(input.value);
    }
  }

  return titles.filter(Boolean).join(", ");
}

// Under a lookup sheet's title: "Filtered by Country: Japan, Star rating: 4"
function showDependentFilter(picker, editor) {
  const field = editor?.closest("admin-dependent-field");
  const description = picker
    .closest("[data-sheet='content']")
    ?.querySelector("[data-lookup='filter']");
  if (!field || !description) {
    return;
  }

  const scope = dependentScope(field);
  const values = field.parents
    .map((parent) => [parent.label, displayedValue(scope, parent.name)])
    .filter(([, value]) => value)
    .map(([label, value]) => `${label}: ${value}`);
  if (values.length > 0) {
    description.textContent = `${description.dataset.prefix} ${values.join(", ")}`;
    description.hidden = false;
  }
}

async function refreshDependents(scope, name) {
  const fields = [...scope.querySelectorAll("admin-dependent-field")];
  const direct = fields.filter((field) => field.parents.some((parent) => parent.name === name));
  if (direct.length === 0) {
    return;
  }

  // The fields that depend on the changed field, and on those in turn
  const names = new Set([name]);
  const affected = new Set();
  let grew = true;
  while (grew) {
    grew = false;
    for (const field of fields) {
      if (!affected.has(field) && field.parents.some((parent) => names.has(parent.name))) {
        affected.add(field);
        names.add(field.getAttribute("name"));
        grew = true;
      }
    }
  }

  let requests = dependentRefreshes.get(scope);
  if (!requests) {
    requests = new Map();
    dependentRefreshes.set(scope, requests);
  }

  requests.get(name)?.abort();
  const request = new AbortController();
  requests.set(name, request);
  for (const field of affected) {
    field.setAttribute("data-dependent", "refreshing");
  }

  const body = new FormData();
  body.set("changed", direct[0].parents.find((parent) => parent.name === name).field);
  const token =
    (scope === document ? document : scope).querySelector(
      "input[name='__RequestVerificationToken']",
    ) ?? document.querySelector("input[name='__RequestVerificationToken']");
  if (token) {
    body.set(token.name, token.value);
  }
  appendParentValues(scope, fields, (key, value) => body.append(key, value));

  try {
    const response = await fetch(direct[0].getAttribute("refresh-url"), {
      method: "POST",
      body,
      signal: request.signal,
    });
    if (!response.ok) {
      throw new Error(`The fields couldn't be refreshed (${response.status}).`);
    }

    const parsed = document.createElement("template");
    parsed.innerHTML = await response.text();
    for (const replacement of parsed.content.querySelectorAll("admin-dependent-field")) {
      const current = [...affected].find(
        (field) => field.getAttribute("name") === replacement.getAttribute("name"),
      );
      if (current?.isConnected) {
        current.replaceWith(replacement);
        window.htmx?.process?.(replacement);
      }
    }
  } catch (error) {
    if (error.name === "AbortError") {
      return;
    }

    window.stellarAdmin?.toast?.warning(
      "The fields that depend on this one couldn't be updated. Try again.",
    );
  } finally {
    if (requests.get(name) === request) {
      requests.delete(name);
      for (const field of affected) {
        field.removeAttribute("data-dependent");
      }
    }
  }
}

// A change of a field that others depend on, whether a native input or an editor that names its field, such as the
// multi-select lookup
document.addEventListener("change", (event) => {
  const target = event.target;
  const name = target instanceof Element ? (target.getAttribute("name") ?? "") : "";
  if (name) {
    refreshDependents(dependentScope(target), name);
  }
});
