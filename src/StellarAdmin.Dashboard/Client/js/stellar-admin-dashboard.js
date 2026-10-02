// Lookup editors: the server renders search results with htmx; selecting or clearing an item only
// updates the field's hidden value and display text.
document.addEventListener("click", (event) => {
  if (!(event.target instanceof Element)) {
    return;
  }

  const option = event.target.closest("[data-lookup-value]");
  if (option) {
    const sheet = option.closest("[data-lookup='sheet']");
    setLookupValue(sheet.dataset.lookupFor, option.dataset.lookupValue, option.textContent.trim());
    sheet.close();
    return;
  }

  const clear = event.target.closest("[data-lookup='clear']");
  if (clear) {
    setLookupValue(clear.dataset.lookupFor, "", "");
    document.getElementById(`${clear.dataset.lookupFor}-open`).focus();
  }
});

function setLookupValue(id, value, text) {
  const input = document.getElementById(id);
  document.getElementById(`${id}-display`).value = text;
  const clear = document.getElementById(`${id}-clear`);
  if (clear) {
    clear.hidden = value === "";
  }

  if (input.value !== value) {
    input.value = value;
    input.dispatchEvent(new Event("change", { bubbles: true }));
  }
}
