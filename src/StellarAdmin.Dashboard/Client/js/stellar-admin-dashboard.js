// Lookup editors: the lookup picker loads into a sheet, and the server renders search results with htmx into its
// sa-command, which handles the keyboard; selecting or clearing an item only updates the field's hidden value and
// shows either the selection or the empty buttons. New loads the referenced resource's create form into a sheet, and
// a created item is selected like a search result.

// A level of the sheet stack. A button with data-sheet-open adds a level from the layout's template, above any that
// are open, and loads the URL it names into the level with htmx; a create form's lookups open further levels in
// turn. The sheet is modal, so only that opener's request, or a request from the content, can be in flight; closing
// the sheet cancels them and removes the level, so a late response never fills it. A failed load shows the sheet's
// error, and Retry repeats the opener's request.
class DashboardRemoteSheet extends HTMLElement {
  #opener = null;

  // Each level replaces the template's id with its own, so its close button and content have unique targets
  static open(opener) {
    const level = document.querySelectorAll("dashboard-remote-sheet").length + 1;
    const fragment = document.importNode(
      document.getElementById("dashboard-sheet-template").content,
      true,
    );
    const sheet = fragment.querySelector("dashboard-remote-sheet");
    for (const element of sheet.querySelectorAll("[id^='dashboard-sheet'], [commandfor]")) {
      for (const name of ["id", "commandfor"]) {
        const value = element.getAttribute(name);
        if (value?.startsWith("dashboard-sheet")) {
          element.setAttribute(name, value.replace("dashboard-sheet", `dashboard-sheet-${level}`));
        }
      }
    }

    document.body.append(sheet);
    sheet.#opener = opener;
    sheet.querySelector("dialog").showModal();
    sheet.#load();
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

        this.remove();
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

  get #content() {
    return this.querySelector("[data-sheet='content']");
  }

  #load() {
    htmx.ajax("GET", this.#opener.dataset.sheetOpen, {
      source: this.#opener,
      target: this.#content,
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
    DashboardRemoteSheet.open(opener);
  }
});

customElements.define("dashboard-remote-sheet", DashboardRemoteSheet);

// A lookup's picker in a sheet: a search and the results to choose from; for names the hidden input of
// the editor that results belong to. It only handles events from its own elements, so htmx events dispatched on
// the document for elements already removed from the page never reach it, and removing it cancels its requests.
class DashboardLookupPicker extends HTMLElement {
  constructor() {
    super();

    // Selecting a result closes the sheet, then passes the selection to the editor
    this.addEventListener("itemselect", (event) => {
      const item = event.target;
      if (item.matches("[data-lookup='more']")) {
        return;
      }

      const selection = item.querySelector("template[data-lookup='selection']");
      this.closest("dialog").close();
      this.#editor.dispatchEvent(
        new CustomEvent("lookup-select", {
          bubbles: true,
          detail: { value: event.detail.value, selection },
        }),
      );
    });

    // Searches send the editor's current value, so the server marks the selected result
    this.addEventListener("htmx:config:request", (event) => {
      event.detail.ctx.request.body.set("selected", this.#editor.value);
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
        this.querySelector("[data-lookup='results']").replaceChildren(this.#template("loading"));
        this.querySelector("[data-lookup='search']").dispatchEvent(new Event("lookup-retry"));
      }
    });
  }

  disconnectedCallback() {
    for (const element of this.querySelectorAll("[data-htmx-powered]")) {
      element.dispatchEvent(new Event("htmx:abort"));
    }
  }

  get #editor() {
    return document.getElementById(this.getAttribute("for"));
  }

  #template(name) {
    return this.querySelector(`template[data-lookup='${name}']`).content.cloneNode(true);
  }
}

customElements.define("dashboard-lookup-picker", DashboardLookupPicker);

// A resource's create form in a sheet, for a lookup editor's New button. It posts with htmx into the sheet, so a
// rejected form comes back with its errors and a created item comes back as dashboard-lookup-created. A post that
// fails shows the form's error and keeps what was typed.
class DashboardCreateSheet extends HTMLElement {
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

customElements.define("dashboard-create-sheet", DashboardCreateSheet);

// Replaces the create form once the item is created: it closes the sheet and passes the new item's key to the
// editor whose hidden input it names. Without a key, the sheet only closes.
class DashboardLookupCreated extends HTMLElement {
  connectedCallback() {
    const editor = document
      .getElementById(this.getAttribute("for"))
      ?.closest("dashboard-lookup-editor");
    const key = this.getAttribute("value");

    this.closest("dialog")?.close();
    if (editor && key) {
      editor.selectCreated(key);
    }
  }
}

customElements.define("dashboard-lookup-created", DashboardLookupCreated);

// A lookup editor: the picker sends a selection to its hidden input as lookup-select, and Clear empties it. It
// shows either the selection or the empty buttons, and moves focus to the button that is still shown.
class DashboardLookupEditor extends HTMLElement {
  constructor() {
    super();

    this.addEventListener("lookup-select", (event) => {
      this.#setValue(event.detail.value, event.detail.selection);
      this.querySelector("[data-lookup='selected'] [data-lookup='open']").focus();
    });

    this.addEventListener("click", (event) => {
      if (event.target.closest("[data-lookup='clear']")) {
        this.#setValue("", null);
        this.querySelector("[data-lookup='empty'] [data-lookup='open']").focus();
      }
    });
  }

  // The server renders the created item's display from the field's settings, and the value the form posts for it.
  // If that fails, the key stands in for the title, as it does for a value the items no longer have.
  async selectCreated(key) {
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
      new CustomEvent("lookup-select", {
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

customElements.define("dashboard-lookup-editor", DashboardLookupEditor);
