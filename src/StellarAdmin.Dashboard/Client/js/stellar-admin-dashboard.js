// Lookup editors: the search panel loads into the shared sheet, and the server renders search results
// with htmx into its sa-command, which handles the keyboard; selecting or clearing an item only updates
// the field's hidden value and shows either the selection or the empty buttons. htmx dispatches its
// events on the document for elements already removed from the page, so handlers check for an element.

// Selecting a result in the shared sheet closes it, then passes the selection to the editor named by the panel
document.addEventListener("itemselect", (event) => {
  const item = event.target;
  const panel = item.closest("[data-lookup='panel']");
  if (!panel || item.matches("[data-lookup='more']")) {
    return;
  }

  const editor = document.getElementById(panel.dataset.lookupFor);
  const selection = item.querySelector("template[data-lookup='selection']");
  document.getElementById("dashboard-sheet").close();
  editor.dispatchEvent(
    new CustomEvent("lookup-select", {
      bubbles: true,
      detail: { value: event.detail.value, selection },
    }),
  );
});

// The editor shows a selection made in the shared sheet
document.addEventListener("lookup-select", (event) => {
  const id = event.target.id;
  setLookupValue(id, event.detail.value, event.detail.selection);
  document.getElementById(`${id}-change`).focus();
});

// The shared sheet's search panel sends the current value of the editor it belongs to, so the server marks the selected result
document.addEventListener("htmx:config:request", (event) => {
  const panel = event.target instanceof Element && event.target.closest("[data-lookup='panel']");
  if (panel) {
    event.detail.ctx.request.body.set(
      "selected",
      document.getElementById(panel.dataset.lookupFor).value,
    );
  }
});

// The request loading the shared sheet's content; only its response may fill the sheet, so a slower
// response for an earlier opener, or one arriving after the sheet closed, is dropped
let sheetRequest = null;

document.addEventListener("htmx:before:request", (event) => {
  const ctx = event.detail.ctx;
  if (ctx.target?.id === "dashboard-sheet-content") {
    sheetRequest?.request.abort();
    sheetRequest = ctx;
    showSheetTemplate("dashboard-sheet-loading");
  }
});

document.addEventListener("htmx:before:swap", (event) => {
  const ctx = event.detail.ctx;
  if (ctx.target?.id === "dashboard-sheet-content" && ctx !== sheetRequest) {
    event.preventDefault();
  }
});

document.addEventListener("htmx:finally:request", (event) => {
  if (event.detail.ctx === sheetRequest) {
    sheetRequest = null;
  }
});

// Closing the shared sheet cancels its requests and shows the loading state again, so the next opener starts fresh
document.addEventListener(
  "close",
  (event) => {
    if (event.target.id !== "dashboard-sheet") {
      return;
    }

    sheetRequest?.request.abort();
    sheetRequest = null;
    const content = document.getElementById("dashboard-sheet-content");
    for (const element of content.querySelectorAll("[data-htmx-powered]")) {
      element.dispatchEvent(new Event("htmx:abort"));
    }

    showSheetTemplate("dashboard-sheet-loading");
  },
  true,
);

// A failed load shows the sheet's error, and Retry repeats the same request; a failed search or Load
// more in the search panel shows the panel's error instead. Cancelled requests show nothing.
let failedSheetRequest = null;

document.addEventListener("htmx:response:error", (event) => showSheetError(event));
document.addEventListener("htmx:error", (event) => showSheetError(event));

function showSheetError(event) {
  const ctx = event.detail.ctx;
  if (event.detail.error?.name === "AbortError") {
    return;
  }

  if (ctx === sheetRequest) {
    ctx.swap = "none";
    failedSheetRequest = ctx;
    showSheetTemplate("dashboard-sheet-error");
    return;
  }

  const element = event.target;
  const panel = element instanceof Element && element.closest("[data-lookup='panel']");
  if (!panel) {
    return;
  }

  ctx.swap = "none";
  const error = panel.querySelector("template[data-lookup='error']").content.cloneNode(true);
  if (element.matches("[data-lookup='more']")) {
    element.replaceWith(error);
  } else {
    panel.querySelector("[data-lookup='results']").replaceChildren(error);
  }
}

document.addEventListener("click", (event) => {
  if (!(event.target instanceof Element)) {
    return;
  }

  if (event.target.closest("[data-sheet='retry']")) {
    const { method, action } = failedSheetRequest.request;
    htmx.ajax(method, action, {
      source: failedSheetRequest.sourceElement,
      target: "#dashboard-sheet-content",
    });
    return;
  }

  const retry = event.target.closest("[data-lookup='retry']");
  if (retry) {
    retry
      .closest("[data-lookup='panel']")
      .querySelector("[data-lookup='search']")
      .dispatchEvent(new Event("lookup-retry"));
    return;
  }

  const clear = event.target.closest("[data-lookup='clear']");
  if (clear) {
    setLookupValue(clear.dataset.lookupFor, "", null);
    document.getElementById(`${clear.dataset.lookupFor}-choose`).focus();
  }
});

// Retrying a failed search shows the loading rows again; typing keeps the current results until the new ones arrive
document.addEventListener("htmx:before:request", (event) => {
  const panel = event.target instanceof Element && event.target.closest("[data-lookup='panel']");
  if (panel && event.detail.ctx.sourceEvent?.type === "lookup-retry") {
    panel
      .querySelector("[data-lookup='results']")
      .replaceChildren(
        panel.querySelector("template[data-lookup='loading']").content.cloneNode(true),
      );
  }
});

function showSheetTemplate(id) {
  document
    .getElementById("dashboard-sheet-content")
    .replaceChildren(document.getElementById(id).content.cloneNode(true));
}

function setLookupValue(id, value, template) {
  const input = document.getElementById(id);
  const selected = document.getElementById(`${id}-selected`);
  if (template) {
    for (const part of [...template.content.cloneNode(true).children]) {
      selected.querySelector(`[data-lookup='${part.dataset.lookup}']`).replaceWith(part);
    }
  }

  selected.hidden = value === "";
  document.getElementById(`${id}-empty`).hidden = value !== "";

  if (input.value !== value) {
    input.value = value;
    input.dispatchEvent(new Event("change", { bubbles: true }));
  }
}
