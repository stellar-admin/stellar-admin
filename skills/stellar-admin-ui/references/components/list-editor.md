---
component: ListEditor
tags: [sa-list-editor]
generated: true
---

# ListEditor

Renders an editor for a list of text or number values, with a row per item.

<!-- structure:begin -->
## Composition and behavior

`<sa-list-editor>` edits an array or list of strings or numbers bound with `asp-for` (other element types throw; use a checkbox or toggle group for enum members). It renders one box with a row per item and Add as the last row. Rows post indexed names (`Highlights[0]`, `Highlights[1]`) that MVC binds to the collection, and a hidden marker makes removing every row bind an empty collection. Blank rows are dropped on submit. Rows render on the server; adding, removing, reordering, paste and the checks need `stellar-admin.js`.

`sortable` adds a drag handle and Alt+↑/↓; `numbered` adds row numbers. `min-items`/`max-items` default from `[MinLength]`, `[MaxLength]` or `[Length]`; with a maximum, Add shows "3 of 8" and disables when full. Item checks are attributes on the editor, applied to every item as browser checks: `pattern` + `pattern-message`, `max-length`, `min`/`max`/`step` for numbers (which align to the end; `align` changes it), `unique` (case-insensitive) and `casing`. Errors show under the row when focus leaves it and on submit, which they block. A ModelState error for `Name[2]` shows under row 3; one for `Name` under the field. The browser checks are not a server check: validate items on the server. `readonly="true"` shows the items in read-only inputs, without Add, remove or reordering, and posts them; `disabled="true"` fades the editor and posts nothing.
<!-- structure:end -->

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `add-text` | `string` | — | — |
| `align` | `ListEditorAlign` | `End` | `Start`, `End` |
| `casing` | `ListEditorCasing` | `None` | `None`, `Upper`, `Lower` |
| `disabled` | `bool` | `false` | `true`, `false` |
| `empty-text` | `string` | — | — |
| `max` | `double` | — | — |
| `max-items` | `int` | — | — |
| `max-length` | `int` | — | — |
| `min` | `double` | — | — |
| `min-items` | `int` | — | — |
| `numbered` | `bool` | `false` | `true`, `false` |
| `pattern` | `string` | — | — |
| `pattern-message` | `string` | — | — |
| `placeholder` | `string` | — | — |
| `prefix` | `string` | — | — |
| `readonly` | `bool` | `false` | `true`, `false` |
| `sortable` | `bool` | `false` | `true`, `false` |
| `step` | `double` | `1` | — |
| `suffix` | `string` | — | — |
| `texts` | `ListEditorTexts` | — | — |
| `unique` | `bool` | `false` | `true`, `false` |
| `class-names` | `ListEditorClassNames` | — | — |
| `column-span` | `string` | `1` | — |
| `column-span-lg` | `string` | — | — |
| `column-span-md` | `string` | — | — |
| `column-span-sm` | `string` | — | — |
| `column-start` | `string` | `auto` | — |
| `column-start-lg` | `string` | — | — |
| `column-start-md` | `string` | — | — |
| `column-start-sm` | `string` | — | — |
| `description` | `string` | — | — |
| `error` | `string` | — | — |
| `asp-for` | `ModelExpression` | — | — |
| `label` | `string` | — | — |
| `name` | `string` | — | — |
| `wrap-in-field` | `bool` | — | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

## Examples

*From `Pages/ListEditor/_Intro.cshtml`*

```razor
<sa-list-editor asp-for="Highlights" sortable="true" max-length="120"
                placeholder="e.g. Tasting at a family-run tasca" add-text="Add highlight"
                empty-text="No highlights yet" />
```

*From `Pages/ListEditor/_Numbered.cshtml`*

```razor
<sa-list-editor asp-for="Highlights" sortable="true" numbered="true" add-text="Add highlight" />
```

*From `Pages/ListEditor/_ItemChecks.cshtml`*

```razor
<sa-list-editor asp-for="ArrivalAirports" unique="true" casing="ListEditorCasing.Upper"
                pattern="[A-Z]{3}" pattern-message="Use a three-letter IATA code, like LIS."
                placeholder="LIS" add-text="Add airport" />
```

*From `Pages/ListEditor/_Validation.cshtml`*

```razor
<sa-list-editor asp-for="Highlights" add-text="Add highlight" />
```
