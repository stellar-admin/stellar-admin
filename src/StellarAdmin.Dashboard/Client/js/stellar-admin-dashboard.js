// Lookup editors: the server renders search results with htmx; selecting or clearing an item only
// updates the field's hidden value and shows either the selection or the empty buttons.
document.addEventListener("click", (event) => {
  if (!(event.target instanceof Element)) {
    return;
  }

  const option = event.target.closest("[data-lookup-value]");
  if (option) {
    const sheet = option.closest("[data-lookup='sheet']");
    const id = sheet.dataset.lookupFor;
    setLookupValue(
      id,
      option.dataset.lookupValue,
      option.querySelector("template[data-lookup='selection']"),
    );
    sheet.close();
    document.getElementById(`${id}-change`).focus();
    return;
  }

  const open = event.target.closest("[data-lookup='open']");
  if (open) {
    // Each open reloads the results for the current search
    const sheet = document.getElementById(`${open.dataset.lookupFor}-lookup`);
    sheet.querySelector("[data-lookup='search']").dispatchEvent(new Event("lookup-open"));
    return;
  }

  const clear = event.target.closest("[data-lookup='clear']");
  if (clear) {
    setLookupValue(clear.dataset.lookupFor, "", null);
    document.getElementById(`${clear.dataset.lookupFor}-choose`).focus();
  }
});

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
