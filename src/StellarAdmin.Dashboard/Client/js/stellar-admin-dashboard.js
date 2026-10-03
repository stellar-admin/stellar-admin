// Lookup editors: the server renders search results with htmx into an sa-command, which handles the
// keyboard; selecting or clearing an item only updates the field's hidden value and shows either the
// selection or the empty buttons.
document.addEventListener("itemselect", (event) => {
  const item = event.target;
  const sheet = item.closest("[data-lookup='sheet']");
  if (!sheet || item.matches("[data-lookup='more']")) {
    return;
  }

  const id = sheet.dataset.lookupFor;
  setLookupValue(id, event.detail.value, item.querySelector("template[data-lookup='selection']"));
  sheet.close();
  document.getElementById(`${id}-change`).focus();
});

document.addEventListener("click", (event) => {
  if (!(event.target instanceof Element)) {
    return;
  }

  const open = event.target.closest("[data-lookup='open']");
  if (open) {
    // Each open reloads the results for the current search
    const sheet = document.getElementById(`${open.dataset.lookupFor}-lookup`);
    sheet.querySelector("[data-lookup='search']").dispatchEvent(new Event("lookup-open"));
    return;
  }

  const retry = event.target.closest("[data-lookup='retry']");
  if (retry) {
    const sheet = retry.closest("[data-lookup='sheet']");
    sheet.querySelector("[data-lookup='search']").dispatchEvent(new Event("lookup-open"));
    return;
  }

  const clear = event.target.closest("[data-lookup='clear']");
  if (clear) {
    setLookupValue(clear.dataset.lookupFor, "", null);
    document.getElementById(`${clear.dataset.lookupFor}-choose`).focus();
  }
});

// Each request sends the current value, so the server marks the selected result
document.addEventListener("htmx:config:request", (event) => {
  const sheet = event.target.closest("[data-lookup='sheet']");
  if (sheet) {
    event.detail.ctx.request.body.set(
      "selected",
      document.getElementById(sheet.dataset.lookupFor).value,
    );
  }
});

// Opening the sheet shows loading rows; typing keeps the current results until the new ones arrive
document.addEventListener("htmx:before:request", (event) => {
  const sheet = event.target.closest("[data-lookup='sheet']");
  if (sheet && event.detail.ctx.sourceEvent?.type === "lookup-open") {
    showLookupTemplate(sheet, "loading");
  }
});

// htmx swaps error responses too, so a failed request swaps nothing and shows the error instead
document.addEventListener("htmx:response:error", (event) => showLookupError(event));
document.addEventListener("htmx:error", (event) => showLookupError(event));

function showLookupError(event) {
  const element = event.target;
  const sheet = element.closest("[data-lookup='sheet']");
  if (!sheet) {
    return;
  }

  event.detail.ctx.swap = "none";

  // A failed Load more ends the list with the error; a failed search replaces the results
  if (element.matches("[data-lookup='more']")) {
    element.replaceWith(
      sheet.querySelector("template[data-lookup='error']").content.cloneNode(true),
    );
  } else {
    showLookupTemplate(sheet, "error");
  }
}

function showLookupTemplate(sheet, name) {
  sheet
    .querySelector("[data-lookup='results']")
    .replaceChildren(
      sheet.querySelector(`template[data-lookup='${name}']`).content.cloneNode(true),
    );
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
