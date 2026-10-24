# Array and dictionary editors

Status: in progress. Approved 2026-10-24 with the five choices below; stage 1 (StellarAdmin.UI) built 2026-10-24 and awaiting Jerrie's review; stage 2 not started. Prototype in `sandbox/html/array-dictionary-editors.html`.

## Summary

Editors for a list of simple values (`List<string>`, `int[]`) and for a dictionary of simple values keyed by a string or an enum (`Dictionary<string, string>`, `Dictionary<Season, decimal>`). Both are one box of rows, like a property editor: a row per item, Add as the last row, remove on hover or focus, optional drag reordering and row numbers for lists, and errors under the row they belong to.

StellarAdmin.UI gets the `sa-list-editor` and `sa-dictionary-editor` tag helpers and the `sel-collection-editor` web component. The admin panel gets `UseListEditor` and `UseDictionaryEditor`, picks them by default for fields of those types, checks the items again on save, and shows the values with `BadgeDisplay`.

## Decisions

Settled with Jerrie during the prototype (2026-10-23 and 2026-10-24):

- One unified box for both editors. Chips (a tag input) are a different control and not part of this plan.
- Add is the last row. Row actions show on hover and focus (always on touch screens). The drag handle always shows when the list can be reordered.
- An empty editor says "No … yet", and the text can be changed. The count ("3 of 8") shows only when there is a maximum. No keyboard hints are shown.
- Reordering and row numbers are options for lists. Dictionaries are never reordered.
- Lists hold simple values only: text and numbers, with no selects. A unique list of enum members is a checkbox group or toggle group, which exist already.
- A dictionary key is text or a select; a value is text, a number or a select. A select's items come from an enum's members or from supplied items, the same sources as the select editor (`UseItems`, `UseResource<T>()`). A key can be used once, so keys other rows use are disabled, and with a select key the maximum is the number of keys.
- Prefix and suffix (such as € or kg) on a list's items and a dictionary's values, not on keys.
- An item's error shows under its row; an error about the whole collection (too few items) shows under the field.
- Item settings sit directly on the list editor (`list.Pattern`), and on `Key(...)` and `Value(...)` for a dictionary.
- Drag and drop is our own, with pointer events and no library. It has the Sortable feel: the row lifts and follows the pointer, a slot shows where it lands, the other rows slide, and Escape cancels. Alt+↑/↓ moves the focused row.
- No `EachItem` validation attribute. Apps that use the tag helpers without the admin panel get the browser checks; the server checks are theirs, as for any other input.
- Not for many-to-many fields: those hold related records, not simple values. The stub's idea of using this editor for them is dropped.
- Read-only for now uses the disabled select's fill with no add row, remove or handle. The look across all components is [read-only-fields](read-only-fields.md).

Approved by Jerrie on 2026-10-24, with the plan:

- Lists hold text and numbers only in the first version, without dates or Guids.
- Each tag helper has its own casing enum.
- Inline grid row editing is out of scope: the field shows its display in an editing row.
- `BadgeDisplay` becomes the default display for these fields, with a new `Limit`.
- The default texts become label options.

## StellarAdmin.UI

### Tag helpers

```html
<sa-list-editor asp-for="Highlights" min-items="1" max-items="8" sortable="true" numbered="true"
                max-length="120" placeholder="e.g. Tasting at a family-run tasca"
                add-text="Add highlight" empty-text="No highlights yet" />

<sa-list-editor asp-for="ArrivalAirports" max-items="6" unique="true" casing="ListEditorCasing.Upper"
                pattern="^[A-Z]{3}$" pattern-message="Use a three-letter IATA code, like LIS." />

<sa-list-editor asp-for="DepartureDays" min="1" max="31" unique="true" />

<sa-dictionary-editor asp-for="LocalNames" empty-text="No translations yet. Pages use the English name."
                      key-label="Language" key-pattern="^[a-z]{2}$" key-pattern-message="Use a two-letter language code, like pt."
                      key-casing="DictionaryEditorCasing.Lower" value-label="Local name" value-max-length="120" />

<sa-dictionary-editor asp-for="SeasonalPrices" key-label="Season" value-label="Price per traveller" value-prefix="€" />

<sa-dictionary-editor asp-for="GuidesByLanguage" asp-key-items="Model.Languages" asp-value-items="Model.Guides" />
```

`sa-list-editor`:

| Attribute | Meaning |
| --- | --- |
| `asp-for` | The collection. Its element type sets the input: text, or a number with the type's `step`. |
| `min-items`, `max-items` | The number of items. Default from `[MinLength]`, `[MaxLength]` or `[Length]` on the property. |
| `sortable`, `numbered` | Drag and Alt+↑/↓ reordering; row numbers in the gutter. Both off by default. |
| `unique` | No duplicates (case-insensitive for text). |
| `pattern`, `pattern-message` | A regular expression each item must match, and the message when it doesn't. |
| `min`, `max`, `step` | A number item's range and step. |
| `max-length` | A text item's length. |
| `casing` | `ListEditorCasing.None`, `Upper` or `Lower`, applied as the item is entered. |
| `prefix`, `suffix` | Text beside each item, such as € or kg. |
| `align` | `ListEditorAlign.Start` or `End`. Numbers default to `End`, so their digits line up; text to `Start`. |
| `placeholder`, `add-text`, `empty-text` | The texts. |
| `readonly` | Values only, without add, remove or reordering. |

`sa-dictionary-editor` has `asp-for`, `min-items`, `max-items`, `numbered`, `add-text`, `empty-text` and `readonly`, and each item attribute twice, with a `key-` or `value-` prefix (`key-label`, `key-pattern`, `value-min`, `value-suffix`, …). `asp-key-items` and `asp-value-items` take the same items as `sa-select`'s `asp-items`; an enum key or value fills its select with the enum's members without them. Keys are always unique. Prefix, suffix and `value-align` (`DictionaryEditorAlign`) are for values only; keys always align to the start.

Each sibling tag helper has its own casing and alignment enums (`ListEditorCasing`, `DictionaryEditorCasing`, `ListEditorAlign`, `DictionaryEditorAlign`), following the component enum convention.

### Rendering, binding and the web component

- The tag helper renders the rows on the server, with indexed names (`Highlights[0]`, `LocalNames[0].Key` and `LocalNames[0].Value`), which MVC binds without a custom binder. A `<template>` holds the empty row that Add clones.
- A hidden presence marker posts with the field, so removing every item binds an empty collection. Reuse or generalise the checkbox group's marker and binder (`CheckboxGroupModelBinderProvider`).
- Pattern, length and range go on each input as the browser's `pattern`, `maxlength`, `min`, `max` and `step`; the messages go in data attributes.
- `sel-collection-editor` adds, removes and moves rows, renumbers the names after each change, drops blank rows on submit, and handles the keyboard (Enter adds a row or moves from key to value, Backspace or Escape on an empty row removes it, ↑/↓ move between rows, Alt+↑/↓ reorder) and paste (one item per line; `key=value` or tab-separated pairs for a dictionary).
- It checks a row when focus leaves it, again as the row is fixed, and every row on submit: the browser's validity for each input, plus duplicates and the number of items. It shows a row's error under the row and the count error under the field, and blocks the submit while any remain.
- An error the server returns for `Highlights[2]` or `LocalNames[1].Key` renders under that row, from ModelState.
- Drag and drop as in the prototype, with pointer events, no library, and no animation under `prefers-reduced-motion`.
- CSS in `@layer sa.components` from the theme tokens, as in the prototype, including the read-only state.

## Admin panel

### API

```csharp
fields.Add(t => t.Highlights).UseListEditor(list =>
{
    list.MinItems = 1;              // otherwise from [MinLength], [MaxLength] or [Length]
    list.MaxItems = 8;
    list.Sortable = true;
    list.Numbered = true;
    list.MaxLength = 120;
    list.Placeholder = "e.g. Tasting at a family-run tasca";
    list.AddText = "Add highlight";
    list.EmptyText = "No highlights yet";
});

fields.Add(t => t.ArrivalAirports).UseListEditor(list =>
{
    list.MaxItems = 6;
    list.Unique = true;
    list.Casing = ListEditorCasing.Upper;
    list.Pattern = "^[A-Z]{3}$";
    list.PatternMessage = "Use a three-letter IATA code, like LIS.";
});

fields.Add(t => t.DepartureDays).UseListEditor(list =>
{
    list.Min = 1;
    list.Max = 31;
    list.Unique = true;
});

fields.Add(t => t.LocalNames).UseDictionaryEditor(dict =>
{
    dict.EmptyText = "No translations yet. Pages use the English name.";
    dict.Key(key =>
    {
        key.Label = "Language";
        key.Casing = DictionaryEditorCasing.Lower;
        key.Pattern = "^[a-z]{2}$";
        key.PatternMessage = "Use a two-letter language code, like pt.";
    });
    dict.Value(value => value.Label = "Local name");
});

fields.Add(t => t.SeasonalPrices).UseDictionaryEditor(dict =>
{
    dict.Key(key => key.Label = "Season");                  // an enum key is a select of its members
    dict.Value(value =>
    {
        value.Label = "Price per traveller";
        value.Prefix = "€";
    });
});

fields.Add(t => t.GuidesByLanguage).UseDictionaryEditor(dict =>
{
    dict.Key(key => key.UseItems(Languages));
    dict.Value(value => value.UseResource<Guide>());        // the same sources as UseSelectEditor
});

fields.Add(t => t.BaggageAllowance).UseDictionaryEditor(dict =>
{
    dict.Key(key => key.Label = "Cabin");
    dict.Value(value =>
    {
        value.Label = "Allowance";
        value.Min = 0;
        value.Max = 50;
        value.Suffix = "kg";
    });
});
```

`ListEditor` has the list attributes above as properties. `DictionaryEditor` has `MinItems`, `MaxItems`, `Numbered`, `AddText` and `EmptyText`, and `Key(...)` and `Value(...)`, which drill into a key and a value with `Label`, `Placeholder`, `Pattern`, `PatternMessage`, `MaxLength`, `Min`, `Max`, `Step`, `Casing` and the item sources (`UseItems`, `UseResource<T>()`); `Prefix` and `Suffix` on the value only. The dictionary editor is typed by the form, like the select editor, so the sources infer their value types.

The item sources live on `ChoiceEditor` today. Moving them into a part the select editor and a dictionary's key and value share is the largest internal change, including the handler that loads the items for the template and the items that depend on other fields (`DependsOn`), which a dictionary doesn't need at first.

### Defaults and display

- A field whose type is a collection of strings or numbers gets the list editor without configuration, and a `Dictionary<string or enum, string, number or enum>` the dictionary editor. Arrays, `List<T>`, `IList<T>`, `ICollection<T>`, `IEnumerable<T>` and the read-only interfaces count; EF Core primitive collections are covered by the same rule.
- `BadgeDisplay` becomes the default display for those fields: a badge per item, or `key: value` per entry, with a new `Limit` that shows the first items and a "+N" badge with the rest in its title. Long badges truncate at the end.
- The texts the editors show by default ("Add item", "No items yet", "None", "3 of 8" and the default error messages) are label options, so apps can translate them like the lookup labels.

### Validation on save

- The editor's rules (count, unique, pattern, length, range, key and value present) run again on the server after binding and add errors to ModelState under the item's key (`Entity.Highlights[2]`, `Entity.LocalNames[1].Key`), or the field's key for the count. A value that doesn't parse already gets an indexed key from model binding.
- `ResourceController.AddValidationErrors` sends a handler's `ResourceValidationError` whose `FieldName` is `Highlights[2]` or `LocalNames[1].Key` to that key when the field is on the form, instead of the summary.

## Out of scope

- Lists of records or complex values, and many-to-many fields.
- Inline row editing in the index grid. A list or dictionary field shows its display in an editing row.
- Chips or tag input.
- How an app stores a dictionary (EF Core needs a value converter or a JSON column); the editor only edits the property.
- The read-only look across components ([read-only-fields](read-only-fields.md)).

## Stages

Each stage stops for Jerrie's review.

1. **StellarAdmin.UI.** Tag helpers, casing enums, web component, CSS and the empty-collection binder; TUnit tests for rendering and binding; Chromium checks of add, remove, drag, keyboard, paste, errors, read-only and phone width across the theme fixtures; DocsSamples examples (Voyager Travel), website docs and the consumer reference.
2. **Admin panel.** `ListEditor`, `DictionaryEditor`, the shared item sources, default editors, server checks with indexed errors, `AddValidationErrors` routing, the `BadgeDisplay` default and `Limit`, and the label options; tests; an AdminPlayground sample; the consumer skill reference.

## Stage 1 as built

- `ListEditorTagHelper` and `DictionaryEditorTagHelper` (`TagHelpers/ListEditor/`, `TagHelpers/DictionaryEditor/`) derive from the field input base, with `ListEditorCasing`, `DictionaryEditorCasing`, `ListEditorClassNames` and `DictionaryEditorClassNames`. Both require `asp-for`; `name` overrides the posted name. The shared, internal rendering is in `TagHelpers/CollectionEditor/`: `CollectionEditorRenderer` composes `InputTagHelper`, `SelectTagHelper`, `IconTagHelper` and the button helper; `CollectionEditorRows` reads the rows from ModelState after a post (only entries with a bound value count, so an error added for an item doesn't hide the model's rows) or from the model; `CollectionEditorTexts` holds the texts the script needs, rendered as `data-texts` JSON.
- The root element is `<sel-collection-editor>` with the `sa-collection-editor` class, `role="group"` labelled by the field label, and `data-kind`, `data-name`, `data-numbered`, `data-sortable`, `data-unique`, `data-readonly`, `data-min-items` and `data-max-items`. The grid columns come from those attributes in CSS, not from an inline style.
- The empty-collection binder is the checkbox group's, generalised rather than renamed (internal names and the `__sa_checkbox_group.` marker prefix are unchanged): a submission with indexed names (`Name[0]`) counts as values, and it now also binds dictionaries with a string or enum key and a string, number or enum value. It is now registered before MVC's dictionary binder as well as the array and collection binders.
- New semantic icons, appended to `SemanticIconRole`: `CollectionEditorAdd` (plus), `CollectionEditorRemove` (x) and `CollectionEditorHandle` (grip-vertical), mapped in the Lucide and Tabler outline packs.
- `sel-collection-editor` is form-associated: the number of items is the element's own validity (`ElementInternals`), and duplicates and a dictionary row's missing key or value are each input's custom validity, so the browser's own submit check blocks the form. The editor cancels the browser's message, shows the error under the row or field, and focuses the first invalid row. Rows left blank are removed by a capturing `submit` listener, before htmx or other listeners read the form. A form with `novalidate` isn't blocked.
- Errors show when focus leaves a control (its own check) or the row (the rest of the row), on submit, and live while a row shows an error. Errors that a focus change shows wait for the pointer's release, so that a row doesn't move under a click.
- Pattern, length and range messages are the browser's own, unless `pattern-message` is set. The other texts are English in `CollectionEditorTexts`, like the library's other built-in texts; stage 2's label options will need a way to pass translations in.
- After review: Escape in a blank row removes it, like Backspace, and focuses the row before it. It is handled (default prevented, propagation stopped) only then, so Escape in a row with a value still reaches the page, such as a dialog the editor is in. `align` and `value-align` were added; a number's cell and column head get `data-align="end"`, and the number inputs in the rows have no spin buttons (Chrome keeps their room even when they're hidden, which pushed an end-aligned number away from its suffix).
- While dragging, the editor has `data-reordering`, for the grabbing cursor. A `:root` attribute was tried and dropped: the website's inline-example export rejects document-level selectors.
- DocsSamples pages `ListEditor` and `DictionaryEditor`, website pages `list-editor.mdx` and `dictionary-editor.mdx` (with navigation and the JavaScript page's element table), the exported demos, the generated consumer references with a hand-written structure section, and the UI skill's forms and JavaScript guides.

## Verification

Stage 1, 2026-10-24:

- `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1` after a Release build: 1918 passed, 0 failed. The UI unit tests include 16 new rendering tests (23 cases) for the two tag helpers and 3 new binder tests (indexed list, empty nested dictionary, indexed dictionary). The admin panel (666) and EF Core (154) integration suites pass with the binder's new position.
- `npm run check`, `npm run build` and `npm run fmt:check` in `src/StellarAdmin.UI/Client`.
- Chromium over CDP against DocsSamples on port 5206: 30 list editor checks (server errors, Add, Enter, Backspace, ↑/↓, Alt+↓, remove and renumbering, labels, drag with ghost and slot, Escape cancel, handle keys, casing, duplicate and pattern errors and their timing, paste, the count error blocking a submit and clearing, blank rows dropped from the posted form, an invalid row blocking the submit and taking focus) and 18 dictionary editor checks (used keys disabled, names, key casing, Enter to the value, missing value, duplicate key, paste of pairs, enum keys capping the count, enum display names, the browser's range message, a server value error, read-only, a missing value blocking the submit, the posted form). All passed.
- Screenshots of every example in light and dark mode, at 375px (no horizontal scroll) and in the ledger, ops and soft theme fixtures.
- `dotnet run --project util/SkillsGenerator -- --check` (no drift) and `npm run check --prefix build/skill-validation`.
- Website: `pnpm lint`, `pnpm types:check` and `pnpm build` pass, and the exported `list-editor-intro.html` demo works on its own. `pnpm examples:export` fails on `body:has(.sa-environment-banner)`, which the environment indicator added on 2026-10-21 and which this stage didn't touch. Its `assets.json` was rewritten before the failure and points at the new bundle.

After review, 2026-10-24: the UI unit tests (767, with 3 new alignment tests, 5 cases); `npm run check` and `npm run build`; `npm run fmt:check`; Chromium over CDP against DocsSamples on 5206, 11 checks: Enter then Escape removes the added row in a list inside a modal dialog that stays open, focus returns to the last row, Escape in a filled row closes the dialog and leaves the rows, Escape on the only blank row focuses Add, the dictionary's Enter and Escape, and the computed end alignment of number values, their column head and read-only text, with the key at the start. Screenshots of the list and dictionary numbers.

Not checked: dragging with touch input, and a real page round trip through MVC binding in a browser (binding is covered by the binder tests).
