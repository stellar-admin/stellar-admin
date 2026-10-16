# Editors

An editor is the input a form field renders. Without `UseEditor`, the input is inferred from the property, so most fields need no editor configuration. Choose one with `UseEditor<TEditor>()` or `UseEditor<TEditor>(editor => { ... })`. Editors are in `StellarAdmin.Resources.Editors`.

## Contents

- [Which editor](#which-editor)
- [What is inferred](#what-is-inferred)
- [Editor settings](#editor-settings)
- [Choices](#choices)
- [The empty choice](#the-empty-choice)
- [Media on choices](#media-on-choices)
- [Columns in radio and checkbox groups](#columns-in-radio-and-checkbox-groups)
- [Overriding and custom editors](#overriding-and-custom-editors)

```csharp
using StellarAdmin.Resources.Editors;

fields.Add(booking => booking.Cabin).UseEditor<RadioGroupEditor>(radio => radio.Appearance = RadioGroupAppearance.Cards);
fields.Add(booking => booking.Travellers).UseEditor<SliderEditor>();          // range from [Range(1, 12)]
fields.Add(booking => booking.Total).UseEditor<TextInputEditor>(input => input.Prefix = "$");
fields.Add(booking => booking.Notes).UseEditor<TextareaEditor>(text => text.Rows = 6);
fields.Add(customer => customer.IsVip).UseEditor<ToggleEditor>();
```

## Which editor

| Property and intent | Editor |
| --- | --- |
| Short text, number, email, URL, password | `TextInputEditor` (inferred) |
| Long text | `TextareaEditor` (inferred for `[DataType(DataType.MultilineText)]`) |
| `DateOnly`, `DateTime`, `TimeOnly` | `DateInputEditor`, `DateTimeInputEditor`, `TimeInputEditor` (inferred) |
| `bool` as a checkbox / a switch | `CheckboxEditor` (inferred) / `ToggleEditor` |
| Enum or fixed list, compact | `SelectEditor` (inferred for enums) |
| Enum or fixed list, all options visible, with descriptions | `RadioGroupEditor`; `Appearance = RadioGroupAppearance.Cards` for selectable cards |
| Two to five short options in a row | `SegmentedControlEditor` |
| Several values from a short list (collection property) | `CheckboxGroupEditor` or `ToggleGroupEditor` |
| Whole number in a range | `SliderEditor` |
| Verification or PIN code | `OneTimeCodeEditor` |
| Foreign key with few related rows | `SelectEditor` + EF `UseItems` ([relationships](relationships.md)) |
| Foreign key with many related rows, searchable, create new | `LookupSheetEditor` ([relationships](relationships.md)) |
| Several keys from a long list (many-to-many) | `MultiLookupSheetEditor` ([relationships](relationships.md)) |

`ChoiceEditor` is the abstract base of the choice editors; don't pass it to `UseEditor`. There is no upload, rich text, colour or tag-input editor.

## What is inferred

| Property | Input |
| --- | --- |
| `string`, `Guid` | Text input; `[DataType]` or `[EmailAddress]`/`[Phone]`/`[Url]` pick email, tel, url or password |
| Numbers | Number input: `step="1"` for whole numbers, `step="any"` for `decimal`, `double`, `float` |
| `[DataType(DataType.MultilineText)]` | Text area |
| `DateOnly`, `DateTime`, `TimeOnly` | Date, date-time or time input |
| `DateTimeOffset` | Text input in round-trip format, keeping the offset |
| `bool` | Checkbox |
| `bool?` | Select with Yes, No and Not set |
| Non-flags enum | Select of its members, using `[Display(Name)]` |
| Flags enum | Text input; use `CheckboxGroupEditor` or `ToggleGroupEditor` on a collection instead |
| Foreign key (`int CustomerId`) | **Number input**: configure `SelectEditor` or `LookupSheetEditor` |

Settings left unset are inferred from the property (for example, `SliderEditor` takes `Min`/`Max` from `[Range]`, `OneTimeCodeEditor` takes `Length` from the property's maximum length); explicit settings win.

## Editor settings

| Editor | Settings |
| --- | --- |
| `TextInputEditor` | `Type` (`TextInputType.Text`, `Email`, `Tel`, `Url`, `Password`, `Number`), `Placeholder`, `Prefix`, `Suffix`, `Min`, `Max`, `Step` |
| `TextareaEditor` | `Placeholder`, `Rows` |
| `DateInputEditor` | `Min`, `Max`, `Step` (days) |
| `DateTimeInputEditor`, `TimeInputEditor` | `Min`, `Max`, `Step` (`TimeSpan`) |
| `CheckboxEditor`, `ToggleEditor` | None |
| `SelectEditor` | `UseItems(...)`, `EmptyChoice`, `EmptyChoiceText`; choice groups render as option groups |
| `RadioGroupEditor` | `UseItems(...)`, `EmptyChoice`, `EmptyChoiceText`, `Appearance` (`RadioGroupAppearance.Default`, `Cards`), `Columns(...)`, `Flow` (`RadioGroupFlow.Across`, `Down`) |
| `CheckboxGroupEditor` | `UseItems(...)`, `Appearance` (`CheckboxGroupAppearance.Default`, `Cards`), `Columns(...)`, `Flow` (`CheckboxGroupFlow.Across`, `Down`); collection properties |
| `SegmentedControlEditor` | `UseItems(...)`, `EmptyChoice`, `EmptyChoiceText` |
| `ToggleGroupEditor` | `UseItems(...)`, `EmptyChoice`, `EmptyChoiceText`, `Appearance` (`ToggleGroupAppearance.Chips`, `Joined`, `Buttons`), `CheckPlacement` (`ToggleGroupCheckPlacement.ReplaceMedia`, `Start`, `End`); several values on a collection property, one otherwise |
| `SliderEditor` | `Min`, `Max` (from `[Range]`, else 0–100), `Step`, `ShowValue`, `ValueFormat` (`"{0} km"`), `MarkInterval`, `MarkLabels` (`SliderMarkLabels.None`, `Ends`, `All`), `AddMark(value, label)` |
| `OneTimeCodeEditor` | `Length` (from max length, else 6) |
| `LookupSheetEditor`, `MultiLookupSheetEditor` | See [relationships](relationships.md#lookup-sheet-settings) |

Every editor also has `ClassNames` for extra CSS classes on its parts (`ClassNames.Media` on choice editors). `RadioGroupFlow`, `CheckboxGroupFlow` and `SliderMarkLabels` are in `StellarAdmin.UI`; the appearance enums are in `StellarAdmin.Resources.Editors`.

```csharp
fields.Add(booking => booking.Travellers).UseEditor<SliderEditor>(slider =>
{
    slider.MarkInterval = 1;
    slider.ValueFormat = "{0} travellers";
});
```

## Choices

Choice editors (`SelectEditor`, `RadioGroupEditor`, `CheckboxGroupEditor`, `SegmentedControlEditor`, `ToggleGroupEditor`) get their choices from:

1. `UseItems(...)` when called.
2. Otherwise, a non-flags enum's members, with `[Display(Name, Description, GroupName)]` for the text, description and group.
3. Otherwise, Yes and No for a Boolean.
4. A collection property uses its element type the same way. Any other type requires `UseItems`.

| `UseItems` overload | Use |
| --- | --- |
| `UseItems(IEnumerable<ChoiceItem>)` | A fixed list |
| `UseItems(IEnumerable<SelectListItem>)` | Existing select list items |
| `UseItems<TProvider>()` | A registered service implementing `IChoiceItemsProvider` (`GetItemsAsync(cancellationToken)`) |
| `UseItems((services, cancellationToken) => ...)` | An inline async loader returning `IReadOnlyList<ChoiceItem>` |
| `UseItems<TContext, TEntity, TValue>(value, title, items => ...)` | Rows of an EF Core entity set (namespace `StellarAdmin.EntityFrameworkCore`; [relationships](relationships.md)) |

For an enum, describe the choices on the members instead of listing them:

```csharp
public enum CabinClass
{
    [Display(Name = "Economy", Description = "Standard seat, one cabin bag.")]
    Economy,
    [Display(Name = "Business", Description = "Lie-flat seat and lounge access.")]
    Business,
}

fields.Add(booking => booking.Cabin).UseEditor<RadioGroupEditor>(cabin =>
{
    cabin.Appearance = RadioGroupAppearance.Cards;
    cabin.Columns(2);
});
```

For other types, list the choices:

```csharp
fields.Add(tour => tour.GuideCode).UseEditor<RadioGroupEditor>(guide =>
{
    guide.Appearance = RadioGroupAppearance.Cards;
    guide.UseItems([
        new ChoiceItem("ana", "Ana Ribeiro") { Description = "Lisbon walking tours.", Media = new ItemMedia.Avatar("/images/ana.jpg") },
        new ChoiceItem("lena", "Lena Fischer") { Media = new ItemMedia.Avatar(null) },
    ]);
});
```

A `ChoiceItem(Value, Text)` also takes `Description`, `Disabled`, `Group` (`new ChoiceGroup("Europe")`) and `Media`. Values are strings, formatted and bound in the current culture. Each editor shows what it supports: descriptions in radio and checkbox groups, groups only in selects, media everywhere except selects.

## The empty choice

The editor adds a first choice that clears the value, labelled `EmptyChoiceText` ("Not set").

- `EmptyChoice.Auto` (default): added for optional values (`int?`, nullable enums, `bool?`, `string?` without `[Required]`). `SelectEditor` also shows it while a required value is still unset.
- `EmptyChoice.Include` always adds it; `EmptyChoice.Omit` never does.
- Collections never get one.

## Media on choices

`Media` is an `ItemMedia`: `new ItemMedia.Icon("plane")`, `new ItemMedia.Avatar(url)` (initials when the URL is null), `new ItemMedia.Image(url)` or `new ItemMedia.Code("LIS")`. Media leads each card in the `Cards` appearances. `ClassNames.Media = "size-10"` resizes it.

## Columns in radio and checkbox groups

`Columns(2)` or `Columns(columns => columns.Small(2).Large(3))` arranges choices in columns, measured against the field's width. `Flow = RadioGroupFlow.Down` fills each column before the next.

## Overriding and custom editors

To change a built-in editor's markup everywhere, add `Areas/StellarAdmin/Views/Shared/EditorTemplates/Editors/{Name}.cshtml`, for example `Editors/TextInput.cshtml`.

For a new editor:

1. A settings class deriving from `FieldEditor` and implementing `IFieldEditor<MyEditorHandler>`.
2. A handler deriving from `FieldEditorHandler<MyEditor>` (or implementing `IFieldEditorHandler<MyEditor>`), taking the editor and any services in its constructor. `TemplateName` names the editor template; `PrepareAsync(FieldEditorContext context, CancellationToken cancellationToken)` can load request data.
3. A template at `Views/Shared/EditorTemplates/{TemplateName}.cshtml` (or under `Areas/StellarAdmin/Views/Shared/`). It reads the editor and prepared data from `ViewData[ViewDataKeys.FormFieldProperties]` (`FormFieldProperties.Editor`, `.EditorData`), and renders inputs with `asp-for` on the model.
4. `fields.Add(model => model.Property).UseEditor<MyEditor>(editor => { ... })`.

Write the template's markup with `sa-*` components (stellar-admin-ui skill). For the editor to work in an inline row's cell, add a second template under `GridEditorTemplates/` ([inline editing](inline-editing.md#editors-in-cells)).
