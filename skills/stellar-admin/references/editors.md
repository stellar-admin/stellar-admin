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
- [Images](#images)
- [Rich text](#rich-text)
- [Icons](#icons)
- [Overriding and custom editors](#overriding-and-custom-editors)

```csharp
using StellarAdmin.Resources.Editors;

fields.Add(booking => booking.Cabin).UseEditor<RadioGroupEditor>(radio => radio.Appearance = RadioGroupAppearance.Cards);
fields.Add(booking => booking.Travellers).UseEditor<SliderEditor>();          // range from [Range(1, 12)]
fields.Add(booking => booking.Total).UseEditor<TextInputEditor>(input => input.Prefix = "$");
fields.Add(booking => booking.Notes).UseEditor<TextareaEditor>(text => text.Rows = 6);
fields.Add(customer => customer.IsVip).UseEditor<ToggleEditor>();
```

To set a title or other field settings as well, choose the editor inside the field callback. `fields.Add(selector, configure)` returns the fields builder, so `UseEditor` can't be chained after it:

```csharp
fields.Add(booking => booking.Notes, field =>
{
    field.Title = "Special requests";
    field.UseEditor<TextareaEditor>(text => text.Rows = 6);
});
```

## Which editor

| Property and intent | Editor |
| --- | --- |
| Short text, number, email, URL, password | `TextInputEditor` (inferred) |
| Long text | `TextareaEditor` (inferred for `[DataType(DataType.MultilineText)]`) |
| Formatted text stored as HTML: headings, lists, links, tables, images | `RichTextEditor` (inferred for `[DataType(DataType.Html)]`; [rich text](#rich-text)) |
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
| Image stored as a file name, key or URL | `ImageEditor` ([images](#images)) |
| Name of an icon (an amenity's or a category's icon) | `IconEditor` ([icons](#icons)) |

`ChoiceEditor` is the abstract base of the choice editors; don't pass it to `UseEditor`. There is no general file upload, colour or tag-input editor.

A `string` without `[DataType]` gets a single-line text input, however long its values are. Descriptions, bios, notes and other long text, often a property with no `MaxLength` or a large one, need `TextareaEditor`, or `RichTextEditor` when the values are HTML. The data shows which: seed data or stored values that contain tags, or app views that render the property with `Html.Raw`. Use `RichTextEditor` only for HTML; plain text loses its line breaks there, because HTML ignores them.

## What is inferred

| Property | Input |
| --- | --- |
| `string`, `Guid` | Text input; `[DataType]` or `[EmailAddress]`/`[Phone]`/`[Url]` pick email, tel, url or password |
| Numbers | Number input: `step="1"` for whole numbers, `step="any"` for `decimal`, `double`, `float` |
| `[DataType(DataType.MultilineText)]` | Text area |
| `[DataType(DataType.Html)]` | Rich text editor |
| `DateOnly`, `DateTime`, `TimeOnly` | Date, date-time or time input |
| `DateTimeOffset` | Text input in round-trip format, keeping the offset |
| `bool` | Checkbox |
| `bool?` | Select with Yes, No and Not set |
| Non-flags enum | Select of its members, labelled with `[Display(Name)]`, else the humanized member name (`NorthAmerica` shows as "North America") |
| Flags enum | Text input; use `CheckboxGroupEditor` or `ToggleGroupEditor` on a collection instead |
| Foreign key (`int CustomerId`) | **Number input**: configure `SelectEditor` or `LookupSheetEditor` |

Settings left unset are inferred from the property (for example, `SliderEditor` takes `Min`/`Max` from `[Range]`, `OneTimeCodeEditor` takes `Length` from the property's maximum length); explicit settings win.

## Editor settings

| Editor | Settings |
| --- | --- |
| `TextInputEditor` | `Type` (`TextInputType.Text`, `Email`, `Tel`, `Url`, `Password`, `Number`), `Placeholder`, `Prefix`, `Suffix`, `Min`, `Max`, `Step` |
| `TextareaEditor` | `Placeholder`, `Rows` |
| `RichTextEditor` | `Placeholder`, `Rows` (lines before it grows, default 8), `StoreImage(...)` (turns on images), `MaxFileSize` (bytes, default 5 MB), `AcceptedTypes` (default JPEG, PNG, WebP, GIF, AVIF); see [rich text](#rich-text) |
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
| `ImageEditor` | `StoreImage(...)` (required), `ResolveUrl(...)`, `Layout` (`ImageEditorLayout.Dropzone`, `Compact`, `Attachment`), `MaxFileSize` (bytes, default 5 MB), `AcceptedTypes` (default JPEG, PNG, WebP, GIF, AVIF); see [images](#images) |
| `IconEditor` | `Icons` (the names offered; default every registered icon), `Layout` (`IconEditorLayout.Auto`, `Picker`, `Inline`); see [icons](#icons) |

Every editor also has `ClassNames` for extra CSS classes on its parts (`ClassNames.Media` on choice editors, `ClassNames.Image` and `ClassNames.Note` on `ImageEditor`). `RadioGroupFlow`, `CheckboxGroupFlow` and `SliderMarkLabels` are in `StellarAdmin.UI`; the appearance enums are in `StellarAdmin.Resources.Editors`.

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

A `ChoiceItem(Value, Title)` also takes `Description`, `Disabled`, `Group` (`new ChoiceGroup("Europe")`) and `Media`. Values are strings, formatted and bound in the current culture. Each editor shows what it supports: descriptions in radio and checkbox groups, groups only in selects, media everywhere except selects.

## The empty choice

The editor adds a first choice that clears the value, labelled `EmptyChoiceText` ("Not set").

- `EmptyChoice.Auto` (default): added for optional values (`int?`, nullable enums, `bool?`, `string?` without `[Required]`). `SelectEditor` also shows it while a required value is still unset.
- `EmptyChoice.Include` always adds it; `EmptyChoice.Omit` never does.
- Collections never get one.

## Media on choices

`Media` is an `ItemMedia`: `new ItemMedia.Icon("plane")`, `new ItemMedia.Avatar(url)` (initials when the URL is null), `new ItemMedia.Image(url)` or `new ItemMedia.Code("LIS")`. Media leads each card in the `Cards` appearances. `ClassNames.Media = "size-10"` resizes it.

## Columns in radio and checkbox groups

`Columns(2)` or `Columns(columns => columns.Small(2).Large(3))` arranges choices in columns, measured against the field's width. `Flow = RadioGroupFlow.Down` fills each column before the next.

## Images

`ImageEditor` edits a string property that stores a reference to an image: a file name, a storage key or a URL. A picked or dropped file uploads straight away, with progress, to an endpoint the panel adds to the resource; it checks the file and calls `StoreImage`, which saves it wherever the app keeps images and returns the value to store. The form then saves only that value. `ResolveUrl` turns a stored value into the image's URL for the preview; without it the value is the URL. Show the same field with `ImageDisplay` in columns and on the view page, with the same resolver.

```csharp
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Editors;

fields.Add(destination => destination.Photo).UseEditor<ImageEditor>(image =>
{
    image.ResolveUrl(value => $"https://cdn.voyager.example/photos/{value}");
    image.StoreImage((file, services, ct) => services.GetRequiredService<IPhotoStore>().SaveAsync(file, ct)); // returns the value to store
    image.Layout = ImageEditorLayout.Compact;
    image.MaxFileSize = 10 * 1024 * 1024;
});

columns.Add(destination => destination.Photo, column => column.UseDisplay<ImageDisplay>(image => image.ResolveUrl(value => $"https://cdn.voyager.example/photos/{value}")));
```

- `StoreImage` is required: an `ImageEditor` field without it is left out of its form, and a configuration problem is logged. It receives the `IFormFile`, the request's services and a cancellation token, and must return a non-empty value.
- The upload endpoint uses the resource's authorization and antiforgery validation, and raises the request body limit to `MaxFileSize` for that request only. It checks the size and the type again: JPEG, PNG, GIF, WebP and AVIF are recognised by their content, so a renamed file is rejected; other `AcceptedTypes` go by the type the browser declares. SVG is not accepted by default.
- `ResolveUrl` has a synchronous overload and `ResolveUrl(async (value, services, ct) => ...)`. In the grid it runs once per row, so compute the URL; don't query for it.
- Replace and remove take effect when the form saves, and can be undone until then. StellarAdmin never deletes images: replaced, removed and abandoned uploads stay where `StoreImage` put them, so clean up files no record references in the app.
- In an inline row the editor is a thumbnail button with a remove badge, whatever its `Layout`; the row's Save waits for a running upload.

## Rich text

`RichTextEditor` edits a string property that stores HTML, with a toolbar (undo, text style, bold, italic, underline, strikethrough, inline code, lists, quote, code block, link, table, divider), a bubble over selected text, a `/` menu for blocks, Markdown shortcuts (`##`, `-`, `1.`, `>`) and table handles for rows and columns. `[DataType(DataType.Html)]` on the property picks it without `UseEditor`, and picks `HtmlDisplay` for its columns and view page ([columns](columns.md#built-in-displays-in-cells)).

```csharp
using StellarAdmin.Resources.Editors;

public class Guide
{
    [DataType(DataType.Html)]
    public string? Bio { get; set; }
}

fields.Add(guide => guide.Bio).UseEditor<RichTextEditor>(editor =>
{
    editor.Placeholder = "Tell travellers about this guide";
    editor.StoreImage(async (file, services, ct) =>
        await services.GetRequiredService<IPhotoStore>().SaveAsync(file, ct)); // returns the image's URL
});
```

- The form saves the HTML after sanitizing it, and `HtmlDisplay` sanitizes it again when it shows it, with `IRichTextSanitizer` (in `StellarAdmin.Resources`). The default keeps the editor's markup (headings, paragraphs, lists, quotes, code, tables, links, images, line breaks), allows http, https and mailto links and relative URLs, and removes everything else: scripts, event handlers, styles, classes and other elements. To change it, register your own: `builder.Services.AddSingleton<IRichTextSanitizer, MySanitizer>()`, which covers saving and showing. A field with another editor, such as `TextareaEditor`, isn't sanitized.
- Images are off until `StoreImage` is set. Then staff paste, drop or insert images, which upload straight away through the same endpoint as `ImageEditor`, with the same checks of `MaxFileSize` and `AcceptedTypes`. `StoreImage` returns the URL the text shows the image from, not a value for a resolver. Such a field with no `AcceptedTypes` or a `MaxFileSize` that isn't positive uses the editor's defaults, and a configuration problem is logged. StellarAdmin never deletes images, including ones removed from the text.
- A value without tags, such as text written before the field became rich text, opens as paragraphs, one per blank-line-separated block.
- The editor's script is a separate file, about 120 KB compressed, which the editor loads itself, only on pages that show it.
- There is no inline row template: don't make a rich text field editable in an inline row.

## Icons

`IconEditor` edits a string property that stores the name of a registered icon, so staff choose an icon instead of typing its name. Without `Icons` it offers every registered icon: a select-style button opens a popover that searches icon names and keywords, and loads the icons from an endpoint the panel adds, once, on first open. With `Icons` it offers only those, rendered with the page. Show the same field with `IconDisplay` in columns and on the view page.

```csharp
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Editors;

fields.Add(amenity => amenity.Icon).UseEditor<IconEditor>(icon =>
{
    icon.Icons = ["wifi", "waves-ladder", "coffee", "square-parking", "dumbbell"];
});
fields.Add(category => category.Icon).UseEditor<IconEditor>(); // every registered icon

columns.Add(amenity => amenity.Icon, column => column.UseDisplay<IconDisplay>());
```

- `Layout = IconEditorLayout.Auto` (the default) shows a list of up to 24 icons as tiles, one click each, and the searchable picker otherwise. `Picker` always uses the picker; `Inline` always uses tiles and requires `Icons`.
- An empty `Icons` offers every registered icon, names that aren't registered are dropped, and `Inline` without `Icons` uses `Auto`; each is logged as a configuration problem.
- A newly chosen value must be a registered icon, and one of `Icons` when set; otherwise the field shows "Choose an icon from the list." A stored value that isn't offered still shows and saves unchanged, so records with old names stay editable.
- The icons endpoint uses the panel's authorization, is cached with a version in its URL and serves Brotli or gzip itself. It sends every registered icon, so register only the packs the app uses.
- In an inline row the editor is a small button showing the icon, whatever its `Layout`, which opens the picker.

## Overriding and custom editors

To change a built-in editor's markup everywhere, add `Areas/StellarAdmin/Views/Shared/EditorTemplates/Editors/{Name}.cshtml`, for example `Editors/TextInput.cshtml`.

For a new editor:

1. A settings class deriving from `FieldEditor` that names its template with `public override string TemplateName => "MyEditor";`.
2. A template at `Views/Shared/EditorTemplates/{TemplateName}.cshtml` (or under `Areas/StellarAdmin/Views/Shared/`). It inherits `FieldEditorView<MyEditor>` (`@inherits StellarAdmin.Areas.StellarAdmin.FieldEditorView<MyEditor>`), reads the typed settings from `Editor`, and renders inputs with `asp-for` on the model. `Title`, `Description`, `IsReadOnly` and `CellErrorId` are also available.
3. `fields.Add(model => model.Property).UseEditor<MyEditor>(editor => { ... })`.

```csharp
public sealed class ColorEditor : FieldEditor
{
    public override string TemplateName => "Color";
    public bool AllowAlpha { get; set; }
}
```

An editor that needs request data also implements `IFieldEditor<THandler>`. The handler derives from `FieldEditorHandler<TEditor>` (or implements `IFieldEditorHandler<TEditor>`), takes the editor and any services in its constructor, and returns data from `PrepareAsync(FieldEditorContext context, CancellationToken cancellationToken)`; the template reads it as `EditorData`.

Write the template's markup with `sa-*` components (stellar-admin-ui skill). For the editor to work in an inline row's cell, add a second template under `GridEditorTemplates/` ([inline editing](inline-editing.md#editors-in-cells)).
