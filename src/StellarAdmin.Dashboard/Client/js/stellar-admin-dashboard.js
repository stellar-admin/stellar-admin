// Lookup editors: the lookup picker loads into the shared sheet, and the server renders search results
// with htmx into its sa-command, which handles the keyboard; selecting or clearing an item only updates
// the field's hidden value and shows either the selection or the empty buttons.

// The shared sheet: the button that opened it loads its content with htmx. The sheet is modal, so only that
// opener's request can be in flight; closing the sheet cancels it and shows the loading state again, so a
// late response never fills the sheet for the next opener. A failed load shows the sheet's error, and Retry
// repeats the opener's request.
class DashboardRemoteSheet extends HTMLElement {
  #opener = null;

  constructor() {
    super();

    // The dialog's command and close events don't bubble, so they are caught on the way down
    this.addEventListener(
      "command",
      (event) => {
        if (event.command === "show-modal") {
          this.#opener = event.source;
        }
      },
      { capture: true },
    );

    this.addEventListener(
      "close",
      () => {
        if (this.#opener) {
          htmx.trigger(this.#opener, "htmx:abort");
        }

        this.#show("loading");
      },
      { capture: true },
    );

    this.addEventListener("click", (event) => {
      if (event.target.closest("[data-sheet='retry']")) {
        this.#show("loading");
        htmx.ajax("GET", this.#opener.getAttribute("hx-get"), {
          source: this.#opener,
          target: "#dashboard-sheet-content",
        });
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

  #show(name) {
    this.querySelector("#dashboard-sheet-content").replaceChildren(
      this.querySelector(`template[data-sheet='${name}']`).content.cloneNode(true),
    );
  }
}

customElements.define("dashboard-remote-sheet", DashboardRemoteSheet);

// A lookup's picker in the shared sheet: a search and the results to choose from; for names the hidden input of
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
