# Lookup editor

Status: **active**. Phases 1 and 2 committed on branch `lookup-editor`; Phase 3 implemented, awaiting review. Last updated: 2026-10-02.

`SelectEditor` suits short lists. `LookupEditor` handles long ones: a read-only display input in an input group with a lookup button that opens a sheet with free-text search and paged results. The form posts a hidden value; the display text (and description) is resolved when the form loads. Single select only. Results are a single column rendered with `sa-item`. Work proceeds in phases with a review checkpoint after each; approval of one phase does not authorize the next.

| Phase | Scope | Status |
| --- | --- | --- |
| 1 | Editor handlers receive the field's current value | implemented, awaiting review |
| 2 | `LookupEditor`, lookup source abstractions, handler and template without search | implemented, awaiting review |
| 3 | Search endpoint, sheet results, selection and clearing | implemented, awaiting review |
| 4 | EF Core items, search projection and reference loading in the edit query | not started |
| 5 | Playground, gallery example and consumer reference | not started |

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

### Phase 5: playground and references

Switch a playground field to `LookupEditor`, add the gallery example and regenerate the consumer skills reference.

## Open decisions

- Labels for the lookup's fixed text ("No results found.", the minimum-length hint, "Load more") — hard-coded for now.
- Phase 4: SQLite translates `string.Contains` to case-sensitive `instr`; the EF Core source should search case-insensitively (the playground source uses `EF.Functions.Like`).

## Verification

Phase 1, 2026-10-02: Dashboard integration tests 254 passed (three new: the create page receives the factory value, the edit page the loaded value, and a rejected post the posted value; the custom editor fixture now echoes the context), EF Core integration tests 59 passed, Dashboard unit tests 37 passed. Touched C# files formatted with CSharpier. No browser check (no UI change).

Phase 2, 2026-10-02: Dashboard integration tests 263 passed at first; after removing the description and clear button, `LookupEditorTests` has five tests (selected item text, empty text, unknown value, sheet title and search, missing `UseItems`), all passing. Touched C# files formatted with CSharpier. For review, the playground's product edit form now uses `LookupEditor` for Category through a temporary hand-written `CategoryLookupSource` (Phase 4 replaces it with EF Core items; create keeps `SelectEditor`). The edit page for product 1 returned 200 with the selected category's text, description and sheet; a headless Chromium screenshot of that page shows a single-row field with the search button at the end.

Phase 3, 2026-10-02: Dashboard integration tests 274 passed; `LookupEditorTests` now has 20 (search input wiring, clear button for optional, empty, required and `AllowClear` fields, results with values, text and descriptions, term filtering, paging through the load-more URL, no results, the minimum-length hint, and 404 for unknown forms, fields and negative skip). The EF Core integration tests were not rerun. Touched C# files formatted with CSharpier and the script with oxfmt. In the playground (port 5206), headless Chromium over CDP on product 1's edit page: opening the sheet listed the ten categories with descriptions, typing "kit" narrowed to Kitchen & Dining, Enter in the search neither submitted nor closed anything, selecting set the hidden value to 5 and the display text and closed the sheet, and clear emptied both, hid the clear button and focused the lookup button. Screenshots showed the sheet list and the one-row field with clear and lookup buttons. The playground's temporary source was switched to `EF.Functions.Like` because SQLite `instr` made "kit" match nothing.
