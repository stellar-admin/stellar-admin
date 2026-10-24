---
component: DictionaryEditor
tags: [sa-dictionary-editor]
generated: true
---

# DictionaryEditor

Renders an editor for a dictionary of keys and values, with a row per entry.

<!-- structure:begin -->
## Composition and behavior

`<sa-dictionary-editor>` edits a `Dictionary<TKey, TValue>` or `IDictionary<TKey, TValue>` bound with `asp-for`, with a string or enum key and a string, number or enum value (other types throw). Each row has the key and value under the `key-label` and `value-label` column heads. Rows post `Name[0].Key` and `Name[0].Value`, which MVC binds to the dictionary; a hidden marker makes removing every row bind an empty dictionary, and blank rows are dropped on submit. Needs `stellar-admin.js` for adding, removing, paste and the checks.

An enum key or value is a select of its members (display names from `[Display]`); `asp-key-items`/`asp-value-items` (`IEnumerable<SelectListItem>`) supply other choices. Keys are unique: keys used by other rows are disabled in a key select, and selectable keys cap `max-items`. The key and value take the list editor's item checks with a `key-` or `value-` prefix (`key-pattern`, `value-min`, `value-suffix`, …); prefix and suffix are for values only. Number values align to the end; `value-align` changes it. A row with only a key or only a value is invalid. A ModelState error for `Name[1].Value` shows under that row. `readonly` shows text (a select's item text) and posts nothing. How the app stores a dictionary (EF Core value converter or JSON column) is outside the editor.
<!-- structure:end -->

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `add-text` | `string` | — | — |
| `empty-text` | `string` | — | — |
| `key-casing` | `DictionaryEditorCasing` | `None` | `None`, `Upper`, `Lower` |
| `asp-key-items` | `IEnumerable<SelectListItem>` | — | — |
| `key-label` | `string` | — | — |
| `key-max-length` | `int` | — | — |
| `key-pattern` | `string` | — | — |
| `key-pattern-message` | `string` | — | — |
| `key-placeholder` | `string` | — | — |
| `max-items` | `int` | — | — |
| `min-items` | `int` | — | — |
| `numbered` | `bool` | `false` | `true`, `false` |
| `readonly` | `bool` | `false` | `true`, `false` |
| `texts` | `DictionaryEditorTexts` | — | — |
| `value-align` | `DictionaryEditorAlign` | `End` | `Start`, `End` |
| `value-casing` | `DictionaryEditorCasing` | `None` | `None`, `Upper`, `Lower` |
| `asp-value-items` | `IEnumerable<SelectListItem>` | — | — |
| `value-label` | `string` | — | — |
| `value-max` | `double` | — | — |
| `value-max-length` | `int` | — | — |
| `value-min` | `double` | — | — |
| `value-pattern` | `string` | — | — |
| `value-pattern-message` | `string` | — | — |
| `value-placeholder` | `string` | — | — |
| `value-prefix` | `string` | — | — |
| `value-step` | `double` | `1` | — |
| `value-suffix` | `string` | — | — |
| `class-names` | `DictionaryEditorClassNames` | — | — |
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

*From `Pages/DictionaryEditor/_Intro.cshtml`*

```razor
<sa-dictionary-editor asp-for="LocalNames" key-label="Language" key-pattern="[a-z]{2}"
                      key-pattern-message="Use a two-letter language code, like pt."
                      key-casing="DictionaryEditorCasing.Lower" key-placeholder="pt"
                      value-label="Local name" value-max-length="120" add-text="Add translation"
                      empty-text="No translations yet. Pages use the English name." />
```

*From `Pages/DictionaryEditor/_EnumKeys.cshtml`*

```razor
<sa-dictionary-editor asp-for="SeasonalPrices" key-label="Season" key-placeholder="Choose a season"
                      value-label="Price per traveller" value-min="0" value-step="0.01"
                      value-prefix="€" add-text="Add season" />
```

*From `Pages/DictionaryEditor/_Items.cshtml`*

```razor
<sa-dictionary-editor asp-for="GuidesByLanguage" asp-key-items="Index.Languages"
                      asp-value-items="Index.Guides" key-label="Language" key-placeholder="Choose a language"
                      value-label="Guide" value-placeholder="Choose a guide" add-text="Add language" />
```

*From `Pages/DictionaryEditor/_Validation.cshtml`*

```razor
<sa-dictionary-editor asp-for="BaggageAllowance" key-label="Cabin" value-label="Allowance"
                      value-suffix="kg" />
```
