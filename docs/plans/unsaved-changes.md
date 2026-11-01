# Unsaved changes

Status: proposed (2026-11-01).

Show that a create or edit form has changes that aren't saved yet, beside its Save button. Split out of [page layout](archive/page-layout.md)'s view, create and edit pass (Jerrie, 2026-11-01), which moves the form's actions into the page header but leaves the indicator out. An earlier version of the POC `sandbox/html/record-pages.html` showed it as an amber dot and "Unsaved changes" before the header's actions, the dot alone on a phone.

Proposed detection, from the review on 2026-11-01:

- The admin panel JS takes a snapshot of the form's values with `new FormData(form)` when the page loads, and on every `input` and `change` event in the form compares the current values with it, showing or hiding the indicator. Changing a value back clears it. `FormData` includes form-associated custom elements and controls outside the form that name it with `form=`.
- On create, the snapshot is the empty or default form. After Save and add another, the values `KeepForNext` carries over count as unchanged.
- A save the server rejects renders the form again with the posted values, which a snapshot would take as unchanged. The server marks that form as changed (for example `data-dirty` when `ModelState` is invalid).
- The comparison runs only on an event. Native inputs, the lookup and multi-lookup sheets, the slider, image upload, the icon picker and the rich text editor fire one. The list and dictionary editors (`sel-collection-editor`) don't when rows are added, removed or reordered, and would need to dispatch `change` (StellarAdmin.UI).
- Dependent fields that clear when their parent changes are covered by the parent's event.
- Open: a warning when leaving the page with unsaved changes (`beforeunload`).

To be filled out before implementing.
