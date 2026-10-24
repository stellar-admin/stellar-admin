# Read-only look across the UI library

Status: proposed (2026-10-24).

Give read-only fields one recognisable look across the UI library's components, so a read-only field is never mistaken for an editable one. Jerrie isn't happy that a read-only input shows nothing to say it is read-only.

What the admin panel renders today when a field is `IsReadOnly`:

- Text input, input group and text area: `readonly`, styled exactly like an editable field.
- Select, checkbox, checkbox group, segmented control and icon picker: `disabled`, since they have no read-only state, so they look faded and don't post.
- Multi-select lookup: its items on the muted item fill, or "None" in a plain input when there are none.
- List and dictionary editors ([array-dictionary-editors](archive/array-dictionary-editors.md)): the disabled select's fill (`--sa-color-field-disabled`) for now, chosen to fit in until this plan settles the look.

Read the components' current states first, then prototype the options with the prototype-component skill and propose a look to Jerrie before changing components. Consider how read-only differs from disabled, whether read-only values still post, and whether controls without a native read-only state need one.
