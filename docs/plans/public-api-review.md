# Public API review before the preview release

Status: proposed (2026-10-16). Nothing is implemented.

A read-only review of the public API of StellarAdmin.UI, StellarAdmin and StellarAdmin.EntityFrameworkCore before the first NuGet preview. It started from a reflection dump of the built assemblies (about 700 public types) and was checked against source. Every change below is cheap now and a breaking change after release.

This file is the only tracker. Sub-points go into the item they belong to, never into new items.

## How it works

- **Your call** (`D` items): real design decisions. We go through them one at a time. Each has one recommendation. Answer yes, no or another choice, and the outcome is recorded in the table.
- **Just do it** (`J` items): changes with an obvious fix. Approve the whole list at once, or name the ones to drop.
- **Settled**: decisions already made, kept for reference.

## Your call

| # | Question | Recommendation | Outcome |
| --- | --- | --- | --- |
| D1 | Seal the concrete tag helpers? | Yes. ASP.NET leaves its own tag helpers open, but opening later is free and sealing later breaks people. | |
| D2 | Support custom field inputs built on `FieldInputBaseTagHelper`? | Yes. Fix its awkward members and document it. | |
| D3 | Keep `ISidebarItemsProvider` public so apps can add their own sidebar items? | Yes. Tidy the models and document it. | |
| D4 | One scheme for link versions of components? | `sa-<component>-link` everywhere, with `sa-link-button`. | |
| D5 | What does an attribute like `columns` mean with no breakpoint suffix? | The smallest width, everywhere. | |
| D6 | `PositionArea`: rename, or split into `side` and `align`? | Rename it and document `Top` vs `TopCenter`. | |
| D7 | How to say "single or multiple"? | `multiple` as a `bool?`, like `<select multiple>`. | |
| D8 | Tabs `selected` and a group `value` on ToggleGroup? | Rename the Tabs attribute to `value`. Add `value` to ToggleGroup. | |
| D9 | How should EF `UseItems` take its types? | A set selector, so the types are inferred: `UseItems((AppDbContext db) => db.Customers, c => c.Id, c => c.Name)`. | |
| D10 | Make custom editors work like custom displays? | Yes. Make `FieldEditor` abstract, make the handler optional and put `TemplateName` on the editor. | |
| D11 | Which client contracts are stable? | Stable: the CSS layer order, `window.stellarAdmin.toast` and the `SA-Toasts` header. Not stable: `sel-*` element names and the checkbox field prefix. | |
| D12 | Replace the row details loader delegate with a class? | Yes. A small abstract `RowDetailsLoader` with `LoadAsync(services, key, cancellationToken)`. | |

## Just do it

| # | Change | Outcome |
| --- | --- | --- |
| J1 | Delete dead public types | Done (2026-10-16) |
| J2 | Make the two tag helper base classes abstract | Approved |
| J3 | Make the admin panel controllers and their query types internal | Approved, if feasible |
| J4 | Make view models sealed classes with internal constructors, and drop the public `ViewModels.Internal` namespace | Approved, if feasible |
| J5 | Tidy the protected hooks EF Core uses | |
| J6 | Seal builders, options types, view models and the view component | |
| J7 | Make tag helper plumbing internal or protected | |
| J8 | Make only `FieldEditorView` and `FormFieldProperties` the documented editor contract | |
| J9 | Make `StellarAdminOptions` internal and give `StellarAdminBuilder` an internal constructor | |
| J10 | Make the resource options read-only | |
| J11 | Add the missing `TagHelper` suffix to five classes | |
| J12 | Rename `sa-linkbutton` to `sa-link-button` | |
| J13 | Rename `InputGroupAddOn*` to `InputGroupAddon*` | |
| J14 | Give components their own enums where they borrow another's, and fix the Sidebar enum members | |
| J15 | Use logical `Start`/`End` names | |
| J16 | Move the toast API to `StellarAdmin.UI.Toasts` | |
| J17 | Use `min`/`max` on Progress | |
| J18 | Use `active` instead of `is-active`, and `Disabled` instead of `IsDisabled` | |
| J19 | Use one naming scheme for DataGrid sort and paging | |
| J20 | Bind `invalid` and `maxlength` | |
| J21 | Use `show-*` instead of `render-*` | |
| J22 | Rename `heading` on `sa-command-group` to `title` | |
| J23 | Give every bound property an explicit attribute name | |
| J24 | Make Slider and Progress numbers `double?` | |
| J25 | Make the remaining plain `bool` attributes `bool?` | |
| J26 | Make icon definitions immutable | |
| J27 | Freeze `IconOptions` after startup | |
| J28 | Align size and media enum member order | |
| J29 | Replace optional parameters with overloads | |
| J30 | Fix `Enable*` return types and write the rule down | |
| J31 | Make the core `LookupItemsBuilder` fluent, like the EF Core one | |
| J32 | Configure menu and forms options the same way | |
| J33 | Fix small builder naming issues | |
| J34 | Add PublicApiAnalyzers, and package validation after the preview | |
| J35 | Pack `THIRD-PARTY-NOTICES.txt` | |
| J36 | Rename the client events to one style | |
| J37 | Document the global MVC hooks | |
| J38 | Add `class-names` to the field helpers that lack it | |
| J39 | Small attribute polish | |
| J40 | Rewrite the public XML docs in plain language | Approved |

## Settled

- **No `InternalsVisibleTo` for EF Core.** Friend access was removed on purpose on 2026-09-27 (`4c93886`), so EF Core builds on the same public hooks any package can use. J5 tidies those hooks instead.
- **One namespace for the UI package.** Views need one `@using`. Only J16 moves anything.
- **One enum per component.** This is deliberate and stays.

## Details

Paths are relative to `src/`. "UI" means `StellarAdmin.UI/TagHelpers/`.

### D1. Seal the concrete tag helpers

Only 6 of the 319 public UI classes are sealed. Builders, options and view models are sealed under J6 either way. This question covers only the leaf tag helpers, such as `ButtonTagHelper`. Leaving them open lets apps subclass them to change defaults, as people do with ASP.NET's own tag helpers. It also makes every protected member of the base classes a promise.

### D2. Custom field inputs

`FieldInputBaseTagHelper` gives `sa-input`, `sa-select` and the others their label, description, error and grid placement. If it stays open and documented, an app can build its own input with all of that for free. If it closes, apps build fields by hand with `sa-field` and its parts.

If yes:
- Make the `LabelForId` setter private.
- Replace `ChoiceGroupTagHelper.RenderAsync(..., bool multiple, bool card, bool down, ...)` with something readable, or make it `private protected`.
- Add a short guide.

If no:
- Make the constructors `private protected`.
- Make `FieldLayout` and `FieldPart` internal.

### D3. Sidebar items provider

`ISidebarItemsProvider` is resolved as a list, so an app can register its own provider today and its items merge into the sidebar and command palette. This is undocumented. If yes: seal `SidebarItem` and its subclasses and turn them from records into plain classes, add a `CancellationToken` to the provider method, and document it. If no: make the provider and items internal, and the sidebar view can no longer be overridden.

### D4. Link versions of components

Today: `sa-linkbutton`, `sa-item-link`, `sa-command-link-item`, `sa-bubble-link-content`, `sa-attachment-link-trigger`, and `sa-dropdown-menu-item`, which becomes a link when given `href`. The other option is one tag per part that renders `<a>` when given `href` or `asp-*`.

### D5. Breakpoint attributes

On `sa-grid` (`UI/Grid/GridTagHelper.cs:15`), `sa-details` and the choice groups, `columns` applies from 40rem up, and `columns-base` is the smallest width. On fields, `sa-grid-item` and `sa-display-field`, `column-span` and `column-start` are the smallest width. The recommendation drops `columns-base`. Today's `columns` becomes `columns-md` or `columns-sm`, whichever names 40rem.

### D6. PositionArea

The name comes from CSS anchor positioning. It is used by Popover, Tooltip and DropdownMenu. `Top` and `TopCenter` differ only in that `Top` spans the anchor. A possible new name is `PopoverPosition`.

### D7. Single or multiple

Today: `sa-toggle-group` `type` with `ToggleGroupType`, `sa-questionnaire-item` `multiple` as a `bool`, and `sa-table` `row-detail-expand-mode` with `TableRowDetailExpandMode`.

### D8. Tabs and ToggleGroup values

On `sa-tabs`, `selected` is the selected panel's id. Everywhere else, `selected` is a `bool?` on an item. RadioGroup and SegmentedControl take the current value as `value` on the group. ToggleGroup has only `selected` on each item.

### D9. EF UseItems types

`UseItems<TContext,TEntity,TValue>` (`StellarAdmin.EntityFrameworkCore/Editors/ChoiceEditorExtensions.cs:17`, and the lookup extensions) cannot infer `TContext`. Callers must write `UseItems<AppDbContext, Customer, int>(...)`. Core's `LookupSheetEditor.UseItems<TSource,TEntity,TValue>` has the same problem. The skills docs need updating with the change.

### D10. Custom editors vs custom displays

Custom editors are documented in `skills/stellar-admin/references/editors.md`. Today `FieldEditor` is a concrete class with a public constructor, needs a handler, and has `TemplateName` on the handler. `FieldDisplay` is abstract, its handler is optional, and `TemplateName` is on the display.

### D11. Client contracts

Candidates:
- The `SA-Toasts` header and the `StellarAdmin.Toasts` TempData key.
- The `__sa_checkbox_group.` field prefix.
- `window.stellarAdmin`.
- The CSS layer order.
- The `sel-*` element names.

Anything marked stable gets documented. Anything else gets a note that it may change.

### D12. Row details loader

`RegisterRowDetails` and `AddPanel` take a `Func<IServiceProvider,string,CancellationToken,Task<object>>`. A class has a readable name and can gain members later without breaking anyone.

### J1. Dead public types

- `StellarAdmin/Areas/StellarAdmin/Views/_ViewImports.cshtml.cs` and `_ViewStart.cshtml.cs` are empty `PageModel` classes, left over from the Pro repository. The `.cshtml` files stay.
- `Areas/StellarAdmin/ViewModels/PagedListViewModel.cs` and `ViewModels/Internal/IPagedListViewModel.cs` are used only by each other.
- `Areas/StellarAdmin/ViewModels/IndexPageSelection.cs` has no references.

### J2. Abstract base classes

`UI/StellarAdminTagHelperBase.cs:11` and `UI/StellarAdminAnchorTagHelperBase.cs:12` are concrete, so Razor registers `<stellar-admin-tag-helper-base>` as an element. Apps can still derive from them.

### J3. Internal controllers

`ResourceController<TResource>`, `HomeController` and `LogoutController`, plus `ResourceIndexQuery` and `ResourceLookupQuery`. Register them through the internal `StellarAdminControllerFeatureProvider` and confirm with a routing test.

### J4. View models

- Six positional records (`ResourceIndexPagingViewModel` and five siblings) become sealed classes.
- Constructors become internal.
- `IResourceIndexPageViewModel` becomes an abstract class with an internal constructor in the main `ViewModels` namespace.

### J5. Hooks EF Core uses

| Hook | Change |
| --- | --- |
| `RegisterRowDetails`, `AddPanel` | Take the loader class from D12 |
| `ResourceRowDetailPanelsBuilder<T>` | Abstract, with a `private protected` constructor |
| `ResourceScopeOptions` | A plain class with `init` properties, not a positional record |
| `ResourceSearchOptions` | The same |
| `ResourceOptions<T>.KeySelector` | Internal setter. `UseKey` stays the way in. |
| `ResourceBuilderBase`, `ResourceIndexBuilderBase`, `LookupItems` | No change. Add docs under J40. |

### J6. Seal everything else

Builders, options types, view models, `SidebarViewComponent`, `StellarAdminUIBuilder`, `StellarAdminUIOptions`, `IconOptions` and the other non-tag-helper classes. Tag helpers are D1.

### J7. Tag helper plumbing

- `TryGetNamedSlot` becomes protected, because apps' own tag helpers read slots (documented in `templated-views.md`).
- `TryAddNamedSlot` becomes internal, because only `<sa-slot-content>` calls it.
- `MessageScrollerId`, `SidebarId` and `UniqueIdDiscriminator` become internal.
- Remove the protected `GetUserSpecifiedClass`, which duplicates the `GetUserSuppliedClass` extension.
- Seal `Init`, so a subclass can't skip the setup that slots and parent lookup need.
- `ResponsiveGrid`, `GridTiers` and `DataGridConsts` become internal. Move whatever StellarAdmin needs from them into StellarAdmin.

### J8. Editor contract

`editors.md` tells authors to cast `ViewData[ViewDataKeys.FormFieldProperties]`, even though `FieldEditorView<TEditor>` already does it. Document `FieldEditorView<TEditor>` instead. Make the other `ViewDataKeys` internal, except `FormFieldProperties`, `FieldDisplay` and `FieldDisplayData`. They connect the built-in form views and are not override points. Make `GridEditorHtmlHelperExtensions` internal.

### J9. StellarAdminOptions

Keep building the options up front. That is deliberate, see the comment in `StellarAdminServiceCollectionExtensions.cs`. Nothing outside the assembly reads them, so make the type internal. That also closes the public `Scripts` and `Stylesheets` lists that skip `AddScript`'s de-duplication. Seal `StellarAdminBuilder` and make its constructor internal, so only `AddStellarAdmin` creates one.

### J10. Resource options read-only

- `ResourceIndexOptions.Columns` is a mutable list whose items can't be constructed. `ResourceFormOptions.Items` is the same.
- `ResourceIndexOptions` has public setters, so `services.Configure<ResourceOptions<T>>(...)` is a second, undocumented way to configure a resource.
- `ResourceSidebarItemOptions` has a public constructor but only internal setters.
- Make `ResourceOptions<T>`, `ResourceIndexOptions`, `ResourceFormOptions` and `ResourceSidebarItemOptions` internal.
- `DataGridColumnOptions`, `FormFieldOptions` and `FormItemOptions` stay public because view models expose them. Make them read-only.

### J11. TagHelper suffix

`AlertAction`, `AvatarBadge`, `AvatarGroupCount` and `SidebarMenuBadge` get the suffix. `PaginationPreviousLinkTagHelper` becomes `PaginationPreviousTagHelper`.

### J12. sa-linkbutton

The only tag that isn't kebab-case (`UI/Button/LinkButtonTagHelper.cs:9`). If D4 changes the scheme, follow D4.

### J13. InputGroupAddon

`InputGroupAddonTagHelper`, property `Align`, enum `InputGroupAddonAlign` (from `InputGroupAddOnVariantAlignment`).

### J14. Own enums and Sidebar members

- Add `ButtonGroupSeparatorOrientation`, `DetailsOrientation` and `SidebarMenuSubButtonSize`.
- Change `SidebarMenuSubLinkSize` from `Small`/`Medium` to `Default`/`Small`.
- Rename `SidebarVariant.Sidebar` to `Default` and `SidebarCollapsible.Offcanvas` to `OffCanvas`.

### J15. Start and End

`InlineStart`/`BlockEnd` (input group add-on), `Leading`/`Trailing` (DataGrid row detail toggle) and `Left`/`Right` (`PositionArea`) become `Start`/`End`, plus `Top`/`Bottom` where meant.

### J16. Toast namespace

`Toast`, `ToastAction`, `ToastType`, `IToastNotifier` and the extensions are used from controllers, not views.

### J17. Progress bounds

`minimum`/`maximum` (`UI/Progress/ProgressTagHelper.cs:19,28`) become `min`/`max`.

### J18. Active and disabled

`is-active` on Pagination, TabLink and the Sidebar menu buttons becomes `active`. `IsDisabled` on `TabTagHelper` and `TabLinkTagHelper` becomes `Disabled`.

### J19. DataGrid sort and paging

`sort-by` becomes `sort-field`, matching columns. The route placeholders become `{sortField}`/`{sortDirection}` and `{page}`/`{pageSize}`, and `page-no` becomes `page`.

### J20. invalid and maxlength

On `sa-input-otp`, `aria-invalid` becomes `invalid` and `max-length` becomes `maxlength`.

### J21. show-* toggles

`render-error` and `render-field` become `show-error` and `show-field`.

### J22. Group caption

`heading` on `sa-command-group` (`UI/Command/CommandGroupTagHelper.cs:17`) becomes `title`.

### J23. Explicit attribute names

33 bound properties take their attribute name from the C# name, so renaming the property silently renames the attribute. They are on Carousel, CheckboxGroup, ChoiceGroup, FormSection, RadioGroup, SegmentedControl and the Sidebar menu helpers.

### J24. Numbers

Slider `Min`, `Max`, `Step`, `MinDistance` and mark `Value`, and Progress `Value`, `Min` and `Max` become `double?`, so `step="0.5"` works. `SliderEditor` follows.

### J25. Nullable booleans

- Questionnaire item: `Required`, `Multiple`.
- Questionnaire choice: `Disabled`.
- Table: `RowDetails`, `Selection`.
- DataGrid: `RowsOnly`.
- DataGrid column and display field: `TemplateRendersEmpty`.

### J26. Icon definitions

`IconDefinition` shares one mutable `List<SvgShape>` per icon across the process. It becomes a sealed class with `IReadOnlyDictionary` and `IReadOnlyList`. `IIconPack.GetIcons()` returns `IReadOnlyDictionary`.

### J27. IconOptions

- Lock the registry after startup.
- Make `AddIcon` and `AddIconPack` treat duplicates the same way.
- Validate at startup instead of on the first render.

### J28. Enum member order

- Put `ButtonSize`, `AttachmentSize`, `ItemSize` and `ImageSize` in one order with a `Default` member.
- Give `GroupGap` and `StackGap` a `Default` that is not `= 2`.
- Add `Default` to `AttachmentMediaVariant`.

### J29. Overloads instead of optional parameters

- EF `UseItems(..., configure = null)`.
- `EfCoreResourceScopesBuilder.Add(..., predicate = null)`.
- `SliderEditor.AddMark(..., label = null)`.
- `SidebarLinkTarget.Action(..., area = null)` and `Page(..., area = null)`.
- `ToastNotifierExtensions.Success/Info/Warning/Error(..., description = null)`.

### J30. Enable* return types

A lambda that supplies required data (panels, scopes, a search predicate) returns the sub-builder. A lambda that only configures returns the parent. `EnableScopes` changes to match, and the rule goes into `docs/conventions/options-builders.md`.

### J31. Core LookupItemsBuilder

`LookupItemsBuilder<TEntity>` returns `this`, gets an internal constructor and moves to `Builders`. The label parameter gets one name across choices and lookups.

### J32. Menu and forms options

`ConfigureMenu` takes a builder like `ConfigureForms`. Both are stored as `StellarAdminUIOptions.Menu` and `.Forms`.

### J33. Small builder naming issues

- `Base(...)`/`BaseFull()` vs the `Default` tier name in the convention: align them.
- `LookupSheetEditor.Editor(...)` becomes `Field(...)`.
- `MultiLookupFieldOptions.Layout` becomes nullable.
- `UserMenuBuilder.Items` gets `Add*` methods.
- `ResourcePagingBuilder.PageSizes` takes a list.
- `SearchOn` requires at least one field at compile time.

### J34. Guardrails

Add PublicApiAnalyzers once the other items are done. Set a package validation baseline after the first preview. Stop suppressing CS1591 (missing XML comment) in the packaged projects.

### J35. Third-party notices

The packages embed Lucide and Tabler icon data and shadcn-derived CSS, and their licences require the notice.

### J36. Client event names

`selection-change` (`sel-table`) vs `checkedchange`, `valuechange` and `itemselect`. Pick one style.

### J37. Global MVC hooks

Document that `AddStellarAdminUI` adds a model binder for form collections, a value provider and a global toast filter.

### J38. class-names

`ToggleTagHelper`, `SegmentedControlTagHelper` and the choice groups lack `class-names`.

### J39. Small attribute polish

- `sa-toggle-group` `spacing` becomes an enum like `StackGap`.
- The Slider `thumb-labels` list can't contain a comma: document it or accept a list.

### J40. Plain-language XML docs

The XML docs are the IntelliSense text and the API reference. Many only make sense to someone who knows the code. `RegisterRowDetails` says:

> Lets rows expand to show a partial view, with a model the loader returns for a resource key when the row expands, or with the row's resource when the loader is null.

Rewritten:

```csharp
/// <summary>
///     Shows a partial view when a row is expanded.
/// </summary>
/// <param name="partialName">The partial view to render.</param>
/// <param name="loader">
///     Loads the partial's model from the row's key when the row is expanded.
///     Pass null to use the row itself as the model.
/// </param>
```

Rules:
- A short summary that says what the member does.
- One idea per sentence.
- Each parameter in its own `<param>`.

Start with the hooks in J5 and the resource builders, then the tag helpers. Add the rules and the example to `docs/conventions/xml-documentation.md`.

## Order of work

1. Settle the D items.
2. Removals and visibility: J1–J10.
3. Renames and types: J11–J33, together with the outcomes of D4–D10 and D12.
4. Packaging and docs: J35–J40.
5. J34 last, so the analyzers record the final API.

Each step updates samples, DocsSamples, consumer skills and website docs that use a changed name.
