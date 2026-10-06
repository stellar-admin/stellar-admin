# Lookup editor

Status: **active**. Phases 1–5 committed on branch `lookup-editor`; Phases 6–9 implement the [display redesign](#display-redesign). Last updated: 2026-10-06.

`SelectEditor` suits short lists. `LookupEditor` handles long ones: a read-only display input in an input group with a lookup button that opens a sheet with free-text search and paged results. The form posts a hidden value; the display text (and description) is resolved when the form loads. Single select only. Results are a single column rendered with `sa-item`. Work proceeds in phases with a review checkpoint after each; approval of one phase does not authorize the next.

| Phase | Scope | Status |
| --- | --- | --- |
| 1 | Editor handlers receive the field's current value | committed |
| 2 | `LookupEditor`, lookup source abstractions, handler and template without search | committed |
| 3 | Search endpoint, sheet results, selection and clearing | committed |
| 4 | EF Core items, search projection and reference loading in the edit query | committed |
| 5 | Playground, gallery example and consumer reference | committed |
| 6 | Configuration structure and item media data, no visual change | committed |
| 7 | Editor appearance: Card and Input layouts, media, empty and read-only states, `EnableCreate` | committed |
| 8 | Sheet appearance: listbox results, media, selection check, keyboard, loading and error states | committed |
| 9 | Playground, gallery and consumer reference for the redesign | committed |

## Proposed API

Shared editor settings:

```csharp
public sealed class LookupEditor : FieldEditor, IFieldEditor<LookupEditorHandler>
{
    public string? SheetTitle { get; set; }         // defaults to the field label
    public string? SearchPlaceholder { get; set; }
    public string? EmptyText { get; set; }          // display text when nothing is selected
    public bool? AllowClear { get; set; }           // defaults to the property's nullability
    public int MinimumSearchLength { get; set; }    // 0 loads the first page when the sheet opens
    public int PageSize { get; set; } = 20;
}
```

EF Core:

```csharp
section.Add(order => order.CustomerId, field =>
    field.UseEditor<LookupEditor>(options =>
    {
        options.SheetTitle = "Select customer";
        options.UseItems<ApplicationDbContext, Customer, Guid>(
            c => c.Id,
            c => c.Name,
            items =>
            {
                items.SearchOn(c => c.Name, c => c.Email);
                items.DescribeWith(c => c.Email);
                items.OrderBy(c => c.Name);
                items.ReferenceFrom<Order>(o => o.Customer); // optional; inferred from the FK navigation
            });
    }));
```

Without EF Core, an entity source resolved from DI per request, with `Func` selectors:

```csharp
options.UseItems<CustomerLookupSource, Customer, Guid>(
    c => c.Id,
    c => c.Name,
    items => items.DescribeWith(c => c.Email));

public interface ILookupSource<TEntity, TValue>
{
    Task<LookupPage<TEntity>> SearchAsync(LookupQuery query, CancellationToken cancellationToken);
    Task<TEntity?> FindAsync(TValue value, CancellationToken cancellationToken);
}

public sealed record LookupQuery(string? Term, int Skip, int Take);
public sealed record LookupPage<TEntity>(IReadOnlyList<TEntity> Items, bool HasMore);
```

The EF Core integration is an `ILookupSource<TEntity, TValue>` built from `SearchOn` and `OrderBy`, so both paths share one handler.

## Reference loading

The selected item is always resolved from a full referenced entity, to which the same value, text and description selectors used by search results are applied.

- EF Core edit loads include the reference navigation (inferred from the field's foreign key, or set with `ReferenceFrom`), so the reference arrives in the same query.
- If the referenced entity's value does not match the field's current value (a failed post that changed the selection), it is ignored.
- Otherwise, and on create pages, with a custom edit loader, or with a foreign key that has no navigation, the handler calls `FindAsync` with the current value.
- Search results project the selectors in SQL; only the selected reference loads a full row.

## Phases

### Phase 1: current value for editor handlers

`IFieldEditorHandler.PrepareAsync(CancellationToken)` becomes `PrepareAsync(FieldEditorContext context, CancellationToken cancellationToken)`. `FieldEditorContext` carries the field name (its property path), the model being rendered and the field's current value read through its property path (null when a parent is null). The signature is replaced rather than overloaded: the Dashboard package is not published, and one method keeps custom handlers simple. The controller passes the factory model on create, the loaded model on edit and the bound model after a failed post.

The context holds the field name rather than `FormFieldOptions`, whose constructor is internal, so handler tests can build a context. Because the model is in the context, the Phase 4 handler may read an included navigation straight from the model instead of a request-scoped cache; decide in Phase 4.

Known edge: when binding fails to convert a posted value, the model keeps its previous value while `ModelState` holds the attempted one, so the context value reflects the model.

### Phase 2: editor without search

`LookupEditor`, `ILookupSource<TEntity, TValue>`, `LookupQuery`, `LookupPage<TEntity>`, the core `UseItems<TSource, TEntity, TValue>` and its items builder, `LookupEditorHandler` resolving the display item, and `Editors/Lookup.cshtml`: `sa-input-group` with a read-only display input, hidden value input, lookup button, optional clear button and an empty `sa-sheet`.

The description selector is `items.DescribeWith(...)`, a leaf behavior verb. The source is resolved from DI like `IChoiceItemsProvider`. A value the source cannot find displays the value itself; a value of another type than `TValue` fails rendering. The field is a one-row input group: the read-only input showing the item text and a lookup button. The description and clear button are not rendered (a block-end description add-on stacked the group into rows); The description belongs to the lookup list (Phase 3), not the editor; `AllowClear` stays, and the clear button returns with clearing in Phase 3. Read-only fields render no button or sheet. The lookup button opens the sheet with `command="show-modal"`, without script. Markup hooks use `data-lookup` because the tag helpers own `data-slot`.

### Phase 3: search and selection

A `ResourceController` lookup action (roughly `GET {resource}/lookup?field=&term=&skip=`) under the resource's authorization, returning a partial of `sa-item` rows and a load-more row. Debounced search and paging use the Dashboard's htmx; a small script applies a selection and clears it.

Implemented: `GET {resource}/lookup?form=create|edit&field=&term=&skip=` on the conventional route, so the resource's authorization metadata applies. `form` selects the create or edit fields, since the two forms may configure the field differently; the template takes it from the current action and the field's property path from a new `FormFieldProperties.FieldName`. An unknown form or field, a field that is read-only or not a `LookupEditor` with items, or a negative `skip` returns 404. The term is trimmed; a term shorter than `MinimumSearchLength` returns "Type at least N characters to search." without querying the source, and an empty first page returns "No results found." (both hard-coded English for now). Result values are formatted in the current culture, matching form binding.

The search input is named `term` but points its `form` attribute at no form, so it is neither posted with the page nor submits it on Enter. It requests the first page when the lookup button is clicked (`click from:#{id}-open`) and on debounced input. Each result is an `sa-item` with a stretched `button[data-lookup-value]` over the row (the item tag helper only renders `div` or `a`). Load more is a button that swaps itself for the next page. `wwwroot/stellar-admin-dashboard.js` (source `Client/js`, copied by the client build and registered after htmx) sets the hidden value and display text, closes the sheet, raises `change` on the hidden input, and toggles the clear button. The clear button sits before the lookup button, is shown when `AllowClear ?? !ModelMetadata.IsRequired`, and is `hidden` while nothing is selected. The current selection is not highlighted in the results.

### Phase 4: EF Core

The `extension(LookupEditor)` `UseItems` with expression selectors, `EfCoreLookupSource` (search, ordering, `Take + 1` paging, projection), navigation inference and `ReferenceFrom`, and the include in `FindEntityByKeyAsync`.

Implemented differently from the sketch above: the EF Core items are not an `ILookupSource<TEntity, TValue>`, because a source returns whole entities and receives no form model, so it could neither project results nor read the included reference. Instead `LookupItems` (with `LookupItem`, `LookupResult` and `LookupResults`) is now public, `LookupEditor` gains `UseItems(LookupItems)` and a public `Items` getter, and the EF Core package derives an internal `EfCoreLookupItems`. Friend-assembly access was removed earlier, so this public extension point is what lets the EF Core data source find lookup fields.

`UseItems<TContext, TEntity, TValue>(value, text, items => …)` takes expression selectors; the builder has `SearchOn`, `DescribeWith`, `OrderBy` and `ReferenceFrom<TModel>`, each returning the builder like the select items builder. Search defaults to the text selector and matches `selector.ToLower().Contains(term)` with the lowered term as a parameter, so it ignores case on any provider (SQLite LOWER folds only ASCII). Results order by `OrderBy`, or the text, then the value for stable paging, take `Take + 1` and project value, text and description in SQL. The edit load (`FindAsync`, used by the edit page and post) includes each lookup field's navigation: the `ReferenceFrom` navigation when the form model is that type, otherwise the single non-collection navigation of a single-property foreign key on the field's top-level property that targets `TEntity`. The handler reads that navigation from the model and uses it when its value matches the field's value; otherwise (create, a changed selection after a failed post, nested fields, custom edit models, no navigation) it queries the projection by value. An unknown value displays the value itself. The playground's temporary `CategoryLookupSource` is removed; its edit form uses the EF Core items.

### Phase 5: playground and references

Switch a playground field to `LookupEditor`, add the gallery example and regenerate the consumer skills reference.

Implemented: the product edit form's Category already used `LookupEditor` with EF Core items since Phase 4; product create keeps `SelectEditor`, so both remain visible. A "Lookup" gallery resource (`LookupGallery`, after Select) uses an in-memory `GalleryAirportLookupSource` registered by the gallery, searching city, code and country. Its scenarios cover optional, required and described items; title, descriptions, read-only and an unknown value; and `SheetTitle`, `SearchPlaceholder`, `EmptyText`, `AllowClear = false`, `MinimumSearchLength`, `PageSize` and `ClassNames.Control`. The consumer reference (`skills/stellar-admin-dashboard/references/setup.md`, hand-written) gains a `LookupEditor` row in the editor table, a paragraph with an `ILookupSource` example, and an EF Core paragraph covering `SearchOn`, `DescribeWith`, `OrderBy`, the navigation loaded with the entity and `ReferenceFrom`. The generated component references cover only FormPage and IndexPage, so there was nothing to regenerate.

### Phase 6: configuration structure

The API from [Configuration structure](#configuration-structure) without changing what renders. `LookupEditor` loses its scalar settings and gains `Editor(Action<LookupFieldOptions>)` (`EmptyText`, `AllowClear`) and `Sheet(Action<LookupSheetOptions>)` (`Title`, `SearchPlaceholder`, `PageSize`, `MinimumSearchLength`). `UseItems`' `text` parameter becomes `title`. Both items builders replace `DescribeWith` with `UseDescription` and gain `UseCode` and `UseAvatar`. `LookupItem` and `LookupResult` rename `Text` to `Title` and carry a `LookupMedia` (a `LookupMediaType` of `Code` or `Avatar`, and the code text or image URL), projected in SQL by EF Core. Tests, the playground, the gallery and the consumer reference follow the renames; the gallery's labels are updated but its scenarios are not extended until Phase 9.

### Phase 7: editor appearance

`editor.Layout` (`LookupEditorLayout`), `editor.ShowMedia` and `lookup.EnableCreate()`, and the Card, Input, empty and read-only states from [Editor appearance](#editor-appearance), with the script updating the media, title and description on selection. The layout default needs to know whether a description is configured, so `LookupItems` gains an abstract `HasDescription`. The selected item's media and text come from a shared `_LookupSelection` partial: the editor renders it for the current value, each search result carries it in a `<template>`, and selecting a result swaps its parts into the editor. Both open buttons (Change and Choose) fire a `lookup-open` event on the search input, replacing the `click from:` trigger that only allowed one button.

### Phase 8: sheet appearance

`sheet.ShowMedia` and the results from [Sheet appearance](#sheet-appearance): an `sa-command` with media rows and a trailing check on the selected value, arrow-key navigation from the search input, a Load more item, skeleton rows while loading, and the minimum-length, no-results and error states.

Implemented: the sheet holds an `sa-command` with `filter="CommandFilter.None"`. Its `sa-command-input` keeps the htmx attributes and gets `autofocus=""` so the keys work as soon as the sheet opens (the close button took focus before). The results are swapped into a container inside `sa-command-list`. The lookup action returns `sa-command-item` rows with the item's value, media (when `sheet.ShowMedia`), title, description and the selection template, and `checked` on the row matching a new `selected` query parameter. The script adds `selected` from the hidden input to every lookup request, and the Load more URL carries it. The script applies the selection on the command's `itemselect` event. Load more is a command item that htmx swaps for the next page, so Enter and click both load more. `LookupItems` gains an abstract `MediaType` so the sheet's skeleton template (shown when the sheet opens) matches the media and description. A failed request swaps nothing and shows an error template with Try again. Code chips and avatars come from a shared `_LookupMedia` partial that the editor's selection also uses.

### Phase 9: playground and references

Gallery scenarios for media, layouts, `ShowMedia` and `EnableCreate`, the playground's product category, and the consumer reference. The prototypes stay in `sandbox/html`.

Implemented: the gallery gains a "Media and layout" section. It has code and avatar items with and without a description, an explicit Input layout over described items, an explicit Card layout without a description, `editor.ShowMedia` and `sheet.ShowMedia` set to false, `EnableCreate` on an empty field, and a read-only field with a code. The gallery has no airport images, so its avatars show initials. With a code, the description is the country alone. The playground's product category drops `EmptyText = "Not specified"`, so an empty field reads "Choose Category", and adds `UseAvatar(category => null)` for initials, since `Category` has no image or code column. The consumer reference's `LookupEditor` row and paragraphs cover `Layout`, both `ShowMedia` settings, `EnableCreate`, `UseCode`, `UseAvatar` and the sheet's keyboard and selection mark, and its EF Core example adds `UseCode`. The gallery's empty fields still use the scenario labels in "Choose {field}".

## Display redesign

Settled 2026-10-03 after visual exploration in `sandbox/html/lookup-compact.html` (editor) and `sandbox/html/lookup-sheet.html` (sheet results), implemented in Phases 6–9.

### Configuration structure

`LookupEditor` settings split into four areas: the items (where they come from and what each contains), the editor in the form, the sheet (search and results), and top-level features. Item content is data shared by the editor and the sheet.

```csharp
field.UseEditor<LookupEditor>(lookup =>
{
    // Items: source and item content
    lookup.UseItems<ApplicationDbContext, Airport, string>(
        a => a.Code,                            // value
        a => a.Name,                            // title
        items =>
        {
            items.UseDescription(a => a.Country);
            items.UseCode(a => a.Code);         // or items.UseAvatar(a => a.LogoUrl)
            items.SearchOn(a => a.Name, a => a.Code);
            items.OrderBy(a => a.Name);
        });

    // Editor: the field in the form
    lookup.Editor(editor =>
    {
        editor.Layout = LookupEditorLayout.Card;    // Input | Card
        editor.ShowMedia = true;
        editor.EmptyText = "Choose airport";
        editor.AllowClear = true;
    });

    // Sheet: search and results
    lookup.Sheet(sheet =>
    {
        sheet.Title = "Departure airport";
        sheet.ShowMedia = true;
        sheet.SearchPlaceholder = "Search airports";
        sheet.PageSize = 20;
        sheet.MinimumSearchLength = 0;
    });

    // Top-level features
    lookup.EnableCreate();
});
```

- Value and title stay required positional arguments of `UseItems`; the `text` parameter is renamed `title`. Optional content goes in the items lambda, which is also the only place the entity type is known (`LookupEditor` is not generic).
- Item content uses `Use*`, matching `UseItems`, `UseKey` and `UseFactory`: `UseDescription` (replacing `DescribeWith` in both the core and EF Core builders), `UseCode` and `UseAvatar`. `UseCode` and `UseAvatar` set the item's media; the last call wins and neither means no media. An avatar without an image falls back to the title's initials.
- `Editor(...)` and `Sheet(...)` are bare-noun drill-downs into structure that always exists. `SheetTitle` (as `Title`), `SearchPlaceholder`, `PageSize` and `MinimumSearchLength` move from `LookupEditor` to the sheet; `EmptyText` and `AllowClear` move to the editor. Nothing has shipped, so the old properties are replaced, not kept.
- Media is shown per surface: `editor.ShowMedia` and `sheet.ShowMedia` (both default `true`) hide the configured media in the form or the results. There is no `ShowDescription`: calling `UseDescription` turns the description on, the sheet always shows it, and in the editor the layout decides (Card shows it, Input does not).
- `LookupEditorLayout` is a dedicated enum. `null` means `Card` when a description is configured, otherwise `Input`.
- `EnableCreate()` adds a "New" button beside "Choose {field}" when the field is empty. Initially the button does nothing; opening the referenced resource's create form in a sheet and selecting the new item is refined once the new design lands. There is no create button in the sheet for now.

### Editor appearance

| Element | Configured by |
| --- | --- |
| Label | The field's label, not a lookup setting |
| Media (code chip or avatar) | `items.UseCode` or `items.UseAvatar`, shown when `editor.ShowMedia` |
| Title | `UseItems` second argument |
| Description (card only) | `items.UseDescription` |
| Change button | Always shown when editable; localized text |
| Clear button | `editor.AllowClear`, null meaning only when the field is optional |
| Stored value (hidden) | `UseItems` first argument |

- **Card:** an extra-small `sa-item` with media, title and description, and Change and clear buttons.
- **Input:** an `sa-button-group`: the selection (media, title and a chevron) is an outline button that opens the sheet, followed by an outline clear button. Each segment has its own focus ring; the input group it replaced lit the whole shell whenever either button had focus.
- **Empty, editable:** two dashed buttons sized to their content, "Choose {field}" (`EmptyText`) with a search icon and, with `EnableCreate`, "New". Invalid fields use the destructive border and text.
- **Read-only, selected:** the layout's muted surface (`sa-item-variant-muted`, or a read-only `sa-input`) without buttons.
- **Read-only, empty:** the same muted surface showing "None" in muted text.

### Sheet appearance

- **Results:** an `sa-command` with `data-filter="none"` in its native styling. Each result is an `sa-command-item` with media (when `sheet.ShowMedia`), title and description. With a description the media matches the Card size (size-7 code chip, default avatar); without one it shrinks to the Input size. The command handles the keys (arrows, Home/End, Ctrl+N/P, Enter) while focus stays in the search input. The first row starts active, the pointer moves the active row, and the selected row shows the item's check.
- **Paging:** a "Load more" command item after the results, `PageSize` items per page. After loading, the first new result takes over as the active row and the list keeps its scroll position: when a change removes the active item, `sa-command` activates the first item added in the same change, and only scrolls if that item is out of view.
- **No match highlighting:** the server decides what matched, so results are not marked up.
- **Non-result states:** the minimum-length hint is a muted line with a search icon. "No results" and a failed search (with Try again) use `sa-empty` blocks. Opening the sheet shows skeleton rows shaped like the media and description, while typing keeps the current results until the new ones arrive.

### Deferred

- What `EnableCreate`'s button does: how it finds the referenced resource (from the entity type or `ReferenceFrom`), whether that resource's create action must be allowed, and how the create sheet returns the new item's value and title to the editor.

## Open decisions

- None. The lookup's fixed text now comes from `ResourceLabelOptions` (see Review fixes under Verification), and its wording is reviewed with the PR.

## Verification

Phase 1, 2026-10-02: Dashboard integration tests 254 passed (three new: the create page receives the factory value, the edit page the loaded value, and a rejected post the posted value; the custom editor fixture now echoes the context), EF Core integration tests 59 passed, Dashboard unit tests 37 passed. Touched C# files formatted with CSharpier. No browser check (no UI change).

Phase 2, 2026-10-02: Dashboard integration tests 263 passed at first; after removing the description and clear button, `LookupEditorTests` has five tests (selected item text, empty text, unknown value, sheet title and search, missing `UseItems`), all passing. Touched C# files formatted with CSharpier. For review, the playground's product edit form now uses `LookupEditor` for Category through a temporary hand-written `CategoryLookupSource` (Phase 4 replaces it with EF Core items; create keeps `SelectEditor`). The edit page for product 1 returned 200 with the selected category's text, description and sheet; a headless Chromium screenshot of that page shows a single-row field with the search button at the end.

Phase 3, 2026-10-02: Dashboard integration tests 274 passed; `LookupEditorTests` now has 20 (search input wiring, clear button for optional, empty, required and `AllowClear` fields, results with values, text and descriptions, term filtering, paging through the load-more URL, no results, the minimum-length hint, and 404 for unknown forms, fields and negative skip). The EF Core integration tests were not rerun. Touched C# files formatted with CSharpier and the script with oxfmt. In the playground (port 5206), headless Chromium over CDP on product 1's edit page: opening the sheet listed the ten categories with descriptions, typing "kit" narrowed to Kitchen & Dining, Enter in the search neither submitted nor closed anything, selecting set the hidden value to 5 and the display text and closed the sheet, and clear emptied both, hid the clear button and focused the lookup button. Screenshots showed the sheet list and the one-row field with clear and lookup buttons. The playground's temporary source was switched to `EF.Functions.Like` because SQLite `instr` made "kit" match nothing.

Phase 4, 2026-10-02: EF Core integration tests 66 passed, seven new in `ResourceLookupTests` (the edit page shows the selection from one joined query, with inference and with `ReferenceFrom`; a rejected edit with a changed selection and a rejected create show the posted selection; search ignores case; pages order by text with load more; `SearchOn`, `DescribeWith` and `OrderBy` replace the defaults). Dashboard integration tests 274 passed, Dashboard unit tests 37 passed. Touched C# files formatted with CSharpier. In the playground (port 5206) with EF Core items, the same headless Chromium check as Phase 3 passed (list of ten with descriptions, "kit" narrows to Kitchen & Dining, Enter does not submit, select sets value 5, clear empties), and the lookup action matched "KIT". No screenshots reviewed; the markup is unchanged from Phase 3.

Phase 5, 2026-10-02: the playground builds. On port 5206 the gallery's edit page returned 200 with each selection's text (the unknown value shows "XXX", `EmptyText` shows as the placeholder) and create showed every field empty; the lookup action paged five items with load more, returned the minimum-length hint for "s" and matched "tok" to both Tokyo airports with descriptions. Headless Chromium over CDP on the `PageSize 5` field: opening listed five, load more made ten, "tok" narrowed to two, Enter did not submit, selecting set HND and "Tokyo Haneda" and closed the sheet, and clear emptied both. A screenshot of the open sheet showed text, descriptions and load more. No tests were run (no library change).

Phase 6, 2026-10-03: Dashboard integration tests 274 passed, EF Core integration tests 66 passed, Dashboard unit tests 37 passed, with the existing lookup tests moved to `Editor(...)`, `Sheet(...)` and `UseDescription`; the playground builds. Touched C# files formatted with CSharpier. Media (`UseCode`, `UseAvatar`) reaches `LookupItem` and `LookupResult` but nothing renders it yet, so it has no tests; HTTP tests follow when Phases 7 and 8 render it. No browser check (markup unchanged apart from the renamed properties).

Phase 7, 2026-10-03: Dashboard integration tests 285 passed. `LookupEditorTests` covers the selected Card with title and description, empty state with Choose text ("Choose categoryId" from the label, or `EmptyText`), `EnableCreate`'s New button, the layout default for items with and without a description, explicit `Input` hiding the description, `UseCode`, `UseAvatar` with an image and with initials, `ShowMedia = false`, read-only selected and empty ("None"), the invalid empty buttons after a rejected post, and results carrying each item's display in a template. EF Core integration tests 66 passed (the display-text helper now reads the selection's title), Dashboard unit tests 37 passed, the playground builds. Touched C# files formatted with CSharpier, the script with oxfmt. In the playground (port 5206, Development) over CDP on product 1's edit page: Change opened the sheet with ten results, selecting Kitchen & Dining set value 5, swapped the card's title and description, closed the sheet and focused Change; clear showed the Choose button and focused it; Choose then selected another category. On the gallery's edit page an Input-layout field swapped to Mexico City on selection. Screenshots of the card (selected and empty) and the gallery's edit and create pages matched the prototype, with the parallax theme drawing a divider before the input group's Change button. The playground's product category and the gallery still use Phase 5 labels, so empty fields read "Choose string? · UseItems" or "Not specified". Phase 9 updates them.

Phase 7 revision, 2026-10-03: the Input layout became a button group (selection button and clear) after review found the input group's focus confusing; the prototype's "Button group · selection button" variant was chosen. Choose uses a search icon. Dashboard integration tests 285 passed, EF Core integration tests 66 passed. In the playground (port 5207, Development) over CDP on the gallery's edit page: tabbing ringed the selection button and then the clear button separately, selecting Mexico City swapped the title inside the button and kept focus on it, clear showed the Choose button with the search icon, and after a rejected save the selection button drew the destructive ring. No gallery field uses media in the Input layout, so the media padding was checked by injecting a code chip into the page.

Phase 8, 2026-10-03: Dashboard integration tests 289 passed. `LookupEditorTests` now covers results as command items with values, roles, titles and descriptions, the `checked` row from `selected`, `selected` carried in the Load more URL, `sheet.ShowMedia` showing and hiding the code, the skeleton template's media and description shape, the no-results block with the term, and the minimum-length hint. EF Core integration tests 66 passed with the result selectors updated, and Dashboard unit tests 37 passed. Touched C# files were formatted with CSharpier and the script with oxfmt. In the playground (port 5207, Development), headless Chromium over CDP:
- **Product 1's edit page:** opening showed 8 skeleton bars, then ten results with the current category checked and focus in the search input. Two ArrowDowns and Enter selected Desk Accessories (value 2), updated the card, closed the sheet and focused Change. Reopening checked the new value. "kit" narrowed the results to one, and adding "zzz" showed "No results found" with the term. A search pointed at an unknown field showed "Search failed" rather than the 404 page, and Try again searched again.
- **Gallery `PageSize 5` field:** End, then Enter on Load more, made 10 results, and again made 15 (DXB checked). As expected, the active row went back to the first result. "tok" narrowed to the two Tokyo airports, and ArrowDown and Enter selected NRT.
- **Gallery `MinimumSearchLength 2` field:** showed the hint, and "to" listed three results.

The screenshots of the open sheet and the error state look right. No console errors. The playground has no media scenarios until Phase 9, so media in the sheet was checked only through the HTTP tests.

Phase 9, 2026-10-03: the playground builds with no warnings, and the touched C# files were formatted with CSharpier. No library code changed, so the tests were not rerun. In the playground (port 5207, Development), headless Chromium over CDP:
- **Gallery edit page, "Media and layout":** screenshots show each scenario as configured: code and avatar cards, code and avatar buttons, Reykjavík as a button without its description, London as a card without a description, Mexico City without its code, Nairobi with its code, Choose plus New on the empty `EnableCreate` field, and Zurich muted with its code.
- **Sheets:** the code and avatar sheets show their media in every row, with the current value checked. The `sheet.ShowMedia = false` sheet has 20 rows and no media. ArrowDown and Enter in the code sheet changed Amsterdam to Athens and swapped the card's code, title and description.
- **Gallery create page:** every field shows its Choose button, and the read-only one shows None.
- **Product 1's edit page:** the category card shows the "St" initials avatar for Stationery.

No console errors. Observed but not changed: single-word titles get two-letter initials with a lowercase second letter ("Am", "Is") while multi-word titles get capitals ("HK", "LA"). Also, empty Card-layout fields have taller Choose buttons than Input-layout fields.

Load more fix, 2026-10-03: `sel-command.ts` changed as described under Paging. The TagHelpers script builds and the TypeScript check passes. No JavaScript tests exist for the component, and no .NET tests were run. In the playground (port 5207, Development), headless Chromium over CDP on the gallery's code field, 20 rows per page: End then Enter on Load more made 31 rows with NBO (row 21) active and visible, and the list stayed scrolled (scrollTop 121 before, 137 after). Clicking Load more with the mouse gave the same result. Typing a search still made the first result active, and there were no console errors.

Review fixes, 2026-10-03:
- **Avatar initials:** `AvatarTagHelper` follows Mantine. A single word gives its first two letters and several words give the first letters of the first two, all uppercase ("Amsterdam" → "AM", "Hong Kong" → "HK"). This affects every `sa-avatar` with a `name`. New `AvatarTagHelperTests` cover both rules, extra spaces, a one-letter name and explicit `initials`.
- **Lookup text:** `ResourceLabelOptions` and `ResourceLabelsBuilder` gain thirteen `Lookup…` callbacks taking a new public `LookupLabelContext` (`FieldLabel`, `MinimumSearchLength`, `Term`): change, choose, clear, create, error title and description, load more, minimum-length message, none, no-results title and description, retry and search label. The defaults are the previous wording, and `EmptyText`, `Title` and `SearchPlaceholder` still win per field. The editor template and `_LookupResults` inject the options, and the lookup action resolves the field label for the results. The consumer reference documents the callbacks.
- **Verification:** TagHelpers unit tests 221 passed, Dashboard integration tests 291 passed (two new tests override editor and no-results text through `ConfigureResourceLabels`, and the avatar initials expectation changed from "Ca" to "CA"), Dashboard unit tests 37 passed, EF Core integration tests 66 passed. Touched C# files were formatted with CSharpier. No browser check was run.

PR review changes, 2026-10-05:
- **Lookup query:** the lookup action binds its query string to a new public `ResourceLookupQuery` (`Form`, `Field`, `Term`, `Skip`, `Selected`), like `Index` binds `ResourceIndexQuery`. The query string and the Load more URL are unchanged. Dashboard integration tests 291 passed.
- **Boolean names:** the lookup editor template and `_LookupSelection` rename `card` and `invalid` to `isCard` and `isInvalid`. Dashboard integration tests 291 passed.
- **Nested labels:** `ResourceLabelOptions` and `ResourceLabelsBuilder` group their 26 callbacks by category: `labels.Index(...)`, `labels.Create(...)`, `labels.Edit(...)`, `labels.Delete(...)` and `labels.Lookup(...)` configure new `ResourceIndexLabelsBuilder`, `ResourceCreateLabelsBuilder`, `ResourceEditLabelsBuilder`, `ResourceDeleteLabelsBuilder` and `LookupLabelsBuilder`, and the options expose matching `Index`, `Create`, `Edit`, `Delete` and `Lookup` objects. The category prefix is dropped from each name (`IndexTitle` becomes `Index.Title`, `LookupChooseLabel` becomes `Lookup.ChooseLabel`). Defaults are unchanged. This is a breaking change, accepted because nothing is released. The controller, lookup views, playground, integration tests and consumer reference use the new shape. Dashboard integration tests 291 passed, Dashboard unit tests 37 passed, the playground builds, CSharpier formatted the touched C# files, and SkillsGenerator reports no drift.

Script structure, 2026-10-06:
- **htmx:** the Dashboard's `htmx.org` goes from `4.0.0-beta6` to `4.0.0`, and the ComponentPlayground's copy of `htmx.min.js` is replaced with the 4.0.0 file.
- **Shared sheet:** one block tracks the button that opened the sheet, from the dialog's `show-modal` command. Closing the sheet aborts that button's request and shows the loading state again. A failed load shows the sheet's error, and Retry repeats the opener's request with `htmx.ajax`. The sheet is modal, so only one opener's request can be in flight, and the stale-response tracking is gone.
- **`<dashboard-lookup-panel for="…">`:** replaces the `data-lookup="panel"` div in `_LookupSheet`. It closes the sheet and sends `lookup-select` to the editor when a result is chosen, sends `selected` with its searches, shows its error for a failed search or Load more, and runs Retry. Its listeners are on the element itself, so htmx events dispatched on the document for removed elements never reach it, and removing it aborts its requests. Load more uses `hx-sync="this:drop"`, so a double-click sends one request.
- **`<dashboard-lookup>`:** wraps the editor's controls inside `sa-field`. It shows a selection, clears the value and moves focus, finding its parts through `data-lookup` (`value`, `selected`, `empty`, `open`, `clear`) instead of element IDs. The clear buttons drop `data-lookup-for`. The element IDs stay.
- **Verification:**
  - **htmx 4.0.0 on the old script:** Dashboard integration tests 295 passed, and the browser script passed.
  - **After the refactor:** Dashboard integration tests 295 passed, with two assertions updated for `dashboard-lookup-panel` and `dashboard-lookup`. EF Core integration tests 66 passed. The script was formatted with oxfmt.
  - **Browser:** in the playground (port 5207, Development), headless Chromium over CDP on the gallery's create page:
    - The 19-step lookup script passed: open, select, focus, reopen, search error and Retry, close reset, clear, Load more and its failure, sheet error and Retry, double-clicked Load more. Closing during a slow load did not let the late response fill the sheet, and the next opener loaded its own panel.
    - A double-clicked Load more sent one request, and closing during a search cancelled it.
    - Select, plus clear where allowed, worked with the expected focus on all 21 editable lookups in both layouts.
  - **Console:** htmx 4 logs an `AbortError` for each cancelled request (beta6 did the same). There were no other console errors.
