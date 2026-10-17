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
| D1 | Seal the concrete tag helpers? | Yes. ASP.NET leaves its own tag helpers open, but opening later is free and sealing later breaks people. | Yes, seal them. Done (2026-10-17) |
| D2 | Support custom field inputs built on `FieldInputBaseTagHelper`? | Yes. Fix its awkward members and document it. | Yes, keep it open, fix the members and add a guide. Done (2026-10-17) |
| D3 | Keep `ISidebarItemsProvider` public so apps can add their own sidebar items? | Yes. Tidy the models and document it. | Yes, keep it public, tidy the models and document it. Done (2026-10-17) |
| D4 | One scheme for link versions of components? | `sa-<component>-link` everywhere, with `sa-link-button`. | `sa-<component>-link` everywhere, including `sa-button-link` and `sa-dropdown-menu-item-link`. Done (2026-10-17) |
| D5 | What does an attribute like `columns` mean with no breakpoint suffix? | The smallest width, everywhere. | The smallest width, and `columns-base` goes. Done (2026-10-17) |
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
| J2 | Make the two tag helper base classes abstract | Done (2026-10-16) |
| J3 | Make the admin panel controllers and `ResourceLookupQuery` internal | Done (2026-10-16) |
| J4 | Make view models sealed classes with internal constructors, and drop the public `ViewModels.Internal` namespace | Done (2026-10-16) |
| J5 | Tidy the protected hooks EF Core uses | |
| J6 | Seal builders, options types, view models and the view component | Done (2026-10-17) |
| J7 | Make tag helper plumbing internal or protected || Done (2026-10-17) |
| J8 | Make only `FieldEditorView` and `FormFieldProperties` the documented editor contract || Done (2026-10-17) |
| J9 | Make `StellarAdminOptions` internal and give `StellarAdminBuilder` an internal constructor | Done (2026-10-17) |
| J10 | Make the resource options read-only to everyone, not internal | On hold with J5 |
| J11 | Add the missing `TagHelper` suffix to five classes | Done (2026-10-17) |
| J12 | Rename `sa-linkbutton` to `sa-link-button` | Done as `sa-button-link` under D4 (2026-10-17) |
| J13 | Rename `InputGroupAddOn*` to `InputGroupAddon*` | Done (2026-10-17) |
| J14 | Give components their own enums where they borrow another's, and fix the Sidebar enum members | Done (2026-10-17) |
| J15 | Use logical `Start`/`End` names | Done (2026-10-17) |
| J16 | Move the toast API to `StellarAdmin.UI.Toasts` | Done (2026-10-17) |
| J17 | Use `min`/`max` on Progress | Done (2026-10-17) |
| J18 | Use `active` instead of `is-active`, and `Disabled` instead of `IsDisabled` | Done (2026-10-17) |
| J19 | Use one naming scheme for DataGrid sort and paging | Done (2026-10-17) |
| J20 | Use `maxlength` on Input OTP and `aria-invalid` on Chip Group | Done (2026-10-17) |
| J21 | Use `show-error` and `wrap-in-field` instead of `render-*` | Done (2026-10-17) |
| J22 | Rename `heading` on `sa-command-group` to `title` | Done (2026-10-17) |
| J23 | Give every bound property an explicit attribute name | Done (2026-10-17) |
| J24 | Make Slider and Progress numbers `double?` | Done (2026-10-17) |
| J25 | Make the remaining plain `bool` attributes `bool?` | Done (2026-10-17) |
| J26 | Make icon definitions immutable | Done (2026-10-17) |
| J27 | Freeze `IconOptions` after startup | Done (2026-10-17) |
| J28 | Align size and media enum member order | Done (2026-10-17) |
| J29 | Replace optional parameters with overloads | Done (2026-10-17) |
| J30 | Fix `Enable*` return types and write the rule down | Done (2026-10-17) |
| J31 | Make the core `LookupItemsBuilder` fluent, like the EF Core one | Done (2026-10-17) |
| J32 | Configure menu and forms options the same way | On hold (menu options to be removed) |
| J33 | Fix small builder naming issues | Done (2026-10-17) |
| J34 | Add PublicApiAnalyzers, and package validation after the preview | |
| J35 | Pack `THIRD-PARTY-NOTICES.txt` | Done (2026-10-17) |
| J36 | Rename the client events to one style | Done (2026-10-17) |
| J37 | Document the global MVC hooks | Done (2026-10-17) |
| J38 | Add `class-names` to the field helpers that lack it | Done (2026-10-17) |
| J39 | Small attribute polish | Done (2026-10-17) |
| J40 | Rewrite the public XML docs in plain language | Done (2026-10-17) |
| J41 | Let the choice editors take every field part class | Done (2026-10-17) |

## Settled

- **No `InternalsVisibleTo` for EF Core.** Friend access was removed on purpose on 2026-09-27 (`4c93886`), so EF Core builds on the same public hooks any package can use. J5 tidies those hooks instead.
- **One namespace for the UI package.** Views need one `@using`. Only J16 moves anything.
- **One enum per component.** This is deliberate and stays.

## Details

Paths are relative to `src/`. "UI" means `StellarAdmin.UI/TagHelpers/`.

### D1. Seal the concrete tag helpers

Only 6 of the 319 public UI classes are sealed. Builders, options and view models are sealed under J6 either way. This question covers only the leaf tag helpers, such as `ButtonTagHelper`. Leaving them open lets apps subclass them to change defaults, as people do with ASP.NET's own tag helpers. It also makes every protected member of the base classes a promise.

Done: all 284 concrete public tag helpers in StellarAdmin.UI and StellarAdmin are sealed. Every remaining open tag helper is an abstract base class.

### D2. Custom field inputs

`FieldInputBaseTagHelper` gives `sa-input`, `sa-select` and the others their label, description, error and grid placement. If it stays open and documented, an app can build its own input with all of that for free. If it closes, apps build fields by hand with `sa-field` and its parts.

If yes:
- Make the `LabelForId` setter private.
- Replace `ChoiceGroupTagHelper.RenderAsync(..., bool multiple, bool card, bool down, ...)` with something readable, or make it `private protected`.
- Add a short guide.

If no:
- Make the constructors `private protected`.
- Make `FieldLayout` and `FieldPart` internal.

Done:
- The `LabelForId` setter is `private protected`. A custom input that needs its label to target another element sets the host's `id`.
- `ApplyFieldAttributes` is `protected` and documented, so a custom input can add the `aria-describedby` and `aria-invalid` that the built-in inputs get. It was `private protected`.
- `ChoiceGroupTagHelper`, `ChoiceGroupItemTagHelper` and `ChoiceGroupItemMediaTagHelper` have `private protected` constructors and render methods, so only the checkbox and radio group tag helpers derive from them.
- The guide is "Your own field inputs" in the UI skill's `references/forms.md`. Its color input example is a test in `FieldInputBaseTagHelperTests.Custom.cs`.

### D3. Sidebar items provider

`ISidebarItemsProvider` is resolved as a list, so an app can register its own provider today and its items merge into the sidebar and command palette. This is undocumented. If yes: seal `SidebarItem` and its subclasses and turn them from records into plain classes, add a `CancellationToken` to the provider method, and document it. If no: make the provider and items internal, and the sidebar view can no longer be overridden.

Done:
- `SidebarItem`, `SidebarGroupItem`, `SidebarLinkItemBase`, `SidebarLinkItem`, `SidebarActionLinkItem` and `SidebarPageLinkItem` are plain classes. The concrete ones are sealed, and the abstract ones have `private protected` constructors.
- Constructors check their strings. `SidebarGroupItem.Items` is an `IReadOnlyList<SidebarLinkItemBase>`. `Icon`, `OpenInNewTab` and `Order` stay `init`.
- `ISidebarItemsProvider.GetItemsAsync(HttpContext, CancellationToken)` returns `Task<IReadOnlyList<SidebarItem>>`, like `IChoiceItemsProvider`. The sidebar passes `RequestAborted`.
- The admin panel skill's `references/sidebar.md` has an "Items from code" section.

### D4. Link versions of components

Decided: the link version of a part is `sa-<part>-link`, with no exceptions.

Done:

- `sa-linkbutton` became `sa-button-link`, `sa-attachment-link-trigger` became `sa-attachment-trigger-link`, `sa-bubble-link-content` became `sa-bubble-content-link`, and `sa-command-link-item` became `sa-command-item-link`.
- The tag helper classes and files were renamed to match: `ButtonLinkTagHelper`, `AttachmentTriggerLinkTagHelper`, `BubbleContentLinkTagHelper` and `CommandItemLinkTagHelper`.
- Dashboard views, DocsSamples, ComponentPlayground, the port-shadcn-component skill and the consumer skills were updated, and the skill references regenerated.

- `sa-dropdown-menu-item` split in two. `sa-dropdown-menu-item` now renders `<button type="button" role="menuitem">`, and `type="submit"` submits its form without script. `sa-dropdown-menu-item-link` renders `<a role="menuitem">` and takes `href` or the `asp-*` routing attributes. Both share `close-on-click`, `disabled`, `inset` and `variant`.
- The user menu's links use `sa-dropdown-menu-item-link`, and its log out item is a submit item, so the click handler in `stellar-admin-panel.js` is gone.
- No reason for the earlier `<div>` was recorded. The sidebar branding plan reused the existing item and submitted its form from script.
- Verification: unit tests for both tag helpers, the user menu integration tests, and `util/dropdown-menu/check.mjs`, a browser check on a ComponentPlayground fixture with trusted input. All 18 browser checks passed in headless Chromium, including Enter and Space running an item once, a submit item submitting once, disabled items, submenus and matching styles for button and link items in the four playground themes. Not checked in Firefox or Safari.

### D5. Breakpoint attributes

On `sa-grid` (`UI/Grid/GridTagHelper.cs:15`), `sa-details` and the choice groups, `columns` applies from 40rem up, and `columns-base` is the smallest width. On fields, `sa-grid-item` and `sa-display-field`, `column-span` and `column-start` are the smallest width. The recommendation drops `columns-base`. Today's `columns` becomes `columns-md` or `columns-sm`, whichever names 40rem.

Done:
- `columns` on `sa-grid`, `sa-details`, `sa-radio-group` and `sa-checkbox-group` sets the smallest width, and wider widths inherit it. `columns-base` is gone, and `columns` can now be combined with `columns-md`.
- The admin panel views, DocsSamples and the UI skill's grid, details, choice group and forms references use `columns-md` where they meant 40rem.
- `sandbox/html/grid.html` is an old prototype and still shows `columns-base`.
- The build is clean apart from older warnings, the skills have no drift, and all 1435 tests pass (2026-10-17). Two conflict tests went with the old rule.

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

Done. `ResourceController<TResource>`, `HomeController`, `LogoutController` and `ResourceLookupQuery` are `internal sealed` and undocumented. `AddStellarAdmin` registers Home and Logout through `AddController`, as resources already were. `ResourceIndexQuery` stays public, because index view overrides read it through `Model.Query` and model binding needs its setters. All 1378 tests pass, including the home and logout integration tests.

### J4. View models

Done:
- The five index records (`ResourceIndexPagingViewModel`, `ResourceIndexScopeViewModel`, `ResourceIndexSearchViewModel`, `ResourceIndexSortViewModel`, `ResourceIndexDeleteDialogViewModel`) are sealed classes with internal constructors. A positional record always has a public constructor, so adding a property would have broken callers.
- The unused `ResourceIndexEmptyViewModel` is deleted.
- `ResourceIndexPageViewModel<TResource>`, `ResourceIndexInlineRowsViewModel`, `ResourceFormPageViewModel` (now sealed) and `ResourceViewPageViewModel` have internal constructors.
- `IResourceIndexPageViewModel` stays an interface, because it is the `@model` for a shared index view overridden for every resource. It moved to `StellarAdmin.Areas.StellarAdmin.ViewModels` and lost `[EditorBrowsable(Never)]`. `ViewModels.Internal` now holds only internal types.

Rule for records: keep records for types consumers create and pass in. Use sealed classes with internal constructors for public types only the library creates. `SidebarBrandModel` and `SidebarUserMenuModel` were in the second group and are now sealed classes with internal constructors too (2026-10-17).

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

Done:
- Sealed `StellarAdminUIBuilder`, `StellarAdminUIOptions`, `StellarAdminUIMenuOptions`, `IconOptions`, `IconPackOptions`, the three icon packs, `StellarAdminBuilder`, `StellarAdminOptions`, `ResourceIndexOptions`, `SidebarViewComponent` and the five leaf editor class-name types.
- `ResourceFormOptions` is abstract.
- `ResourceFieldsBuilder<T>` and `ResourceViewFieldsBuilder<T>` are abstract with internal constructors. The create, edit and view builders use the new internal `ResourceFormFieldsBuilder<T>` and `ResourceViewPageFieldsBuilder<T>`.
- Left open because types derive from them: `FieldEditor` (J8), `EditorClassNames`, `ChoiceEditorClassNames`, `ResourceRowDetailPanelsBuilder<T>` and `ResourceSearchOptions` (both J5).
- All 1378 tests pass. After D1–D3 and the `UniqueIdDiscriminator` change, all 1437 tests pass (2026-10-17). Tag helpers are D1.

### J7. Tag helper plumbing

Done:
- `TryGetNamedSlot` is `protected internal`, because apps' own tag helpers read slots and `<sa-slot-outlet>` reads its host's slots.
- `TryAddNamedSlot`, `MessageScrollerId` and `SidebarId` are internal. `DataGridConsts` is internal.
- The protected `GetUserSpecifiedClass` is removed. The item tag helpers use the `GetUserSuppliedClass` extension.
- `Init` is sealed and calls a new protected virtual `OnInit`. `SelectTagHelper`, `QuestionnaireItemTagHelper` and `QuestionnaireChoicesTagHelper` override `OnInit` instead.
- `ResponsiveGrid` and `GridTiers` stay public, because StellarAdmin uses almost all of them and has no friend access. They get docs under J40.
- `UniqueIdDiscriminator` is internal. The DataGridSpike sandbox no longer sets it.
- All 1378 tests pass.

### J8. Editor contract

Done:
- `editors.md` teaches `FieldEditorView<TEditor>` instead of casting `ViewData[ViewDataKeys.FormFieldProperties]`. The integration test's `ProductNameEditor.cshtml` inherits it.
- All `ViewDataKeys` except `FieldDisplay` and `FieldDisplayData` are internal. `FormFieldProperties` is internal too, because no consumer outside this repository reads it (Jerrie, 2026-10-17).
- `GridEditorHtmlHelperExtensions` stays public, because an override of `_IndexDataGrid.cshtml` renders inline editors with `GridEditorAsync`.
- All 1378 tests pass.

### J9. StellarAdminOptions

Done:
- `StellarAdminOptions` is internal, including `Scripts` and `Stylesheets`, so scripts and stylesheets go through `AddScript` and `AddStylesheet` and their de-duplication. The options are still built up front, as the comment in `StellarAdminServiceCollectionExtensions.cs` explains.
- The `StellarAdminBuilder` constructor is internal, so only `AddStellarAdmin` creates one. J6 already sealed the builder.
- All 1378 tests pass.

### J10. Resource options read-only

Keep `ResourceOptions<T>`, `ResourceIndexOptions` and `ResourceFormOptions` public, because EF Core reads them and there is no `InternalsVisibleTo` (Jerrie, 2026-10-17). Make them read-only to everyone instead. `Columns` and `Items` become `IReadOnlyList`, and the public setters on `ResourceIndexOptions`, `ResourceFormOptions` and `KeySelector` become internal. EF Core writes `Index.Scopes`, `Index.Search`, `Scopes.Items` and `Scopes.DefaultScope` today, so it needs public builder hooks for those first. That is J5's work, so J10 is on hold with J5. `DataGridColumnOptions`, `FormFieldOptions`, `FormItemOptions` and `FormDisplayFieldOptions` already have only internal setters.

### J11. TagHelper suffix

Done:
- `AlertAction`, `AvatarBadge`, `AvatarGroupCount` and `SidebarMenuBadge` are now `…TagHelper`, and their files are renamed to match.
- `PaginationPreviousLinkTagHelper` is now `PaginationPreviousTagHelper`, matching its file and `PaginationNextTagHelper`.
- The element names don't change, so markup is unaffected.
- All 1378 tests pass.

### J12. sa-linkbutton

Done: renamed to `sa-button-link` rather than `sa-link-button`, following D4.

### J13. InputGroupAddon

`InputGroupAddonTagHelper`, property `Align`, enum `InputGroupAddonAlign` (from `InputGroupAddOnVariantAlignment`).

Done (2026-10-17):

- Renamed the tag helper, the enum and the property `Alignment` to `Align`, which matches Group, Stack, Message and Bubble. The markup attribute was already `align`, so only the enum name changes in markup.
- The enum members are unchanged and wait for J15.
- Verified with the full test run (1385 passed) and a skills drift check.

### J14. Own enums and Sidebar members

- Add `ButtonGroupSeparatorOrientation`, `DetailsOrientation` and `SidebarMenuSubButtonSize`.
- Change `SidebarMenuSubLinkSize` from `Small`/`Medium` to `Default`/`Small`.
- Rename `SidebarVariant.Sidebar` to `Default` and `SidebarCollapsible.Offcanvas` to `OffCanvas`.

Done (2026-10-17):

- Added `ButtonGroupSeparatorOrientation`, `DetailsOrientation`, `DisplayFieldOrientation` and `SidebarMenuSubButtonSize`. `DisplayFieldOrientation` wasn't in the original list, but `sa-display-field` also borrowed `FieldOrientation`.
- The StellarAdmin details builder and options now use `DetailsOrientation`, and the display field builder and options use `DisplayFieldOrientation`. View sections keep `FieldOrientation` because they also lay out editor fields, and `_FormItems.cshtml` converts it for the display fields in the section.
- `SidebarMenuSubLinkSize` and `SidebarMenuSubButtonSize` are `Default`/`Small`, and `SidebarVariant.Default` and `SidebarCollapsible.OffCanvas` are renamed. Rendered attributes are unchanged.
- Verified with the full test run (1385 passed) and a skills drift check. Not checked in a browser.

### J15. Start and End

`InlineStart`/`BlockEnd` (input group add-on), `Leading`/`Trailing` (DataGrid row detail toggle) and `Left`/`Right` (`PositionArea`) become `Start`/`End`, plus `Top`/`Bottom` where meant.

Done (2026-10-17):

- `InputGroupAddonAlign` is now `Start`, `End`, `Top` and `Bottom`. The `data-align` values and the `sa-input-group-addon-align-*` classes follow, for example `start` and `top` instead of `inline-start` and `block-start`.
- `DataGridRowDetailToggle` is now `Start`, `End` and `None`.
- `PositionArea`, `SheetSide` and `SidebarSide` keep physical `Left`/`Right` names by decision. They name a screen position, their CSS is physical and the library has no right-to-left support. `Start`/`End` is used where the meaning is order in the layout.
- Verified with the full test run (1385 passed), a client CSS build and a skills drift check. Not checked in a browser.

### J16. Toast namespace

`Toast`, `ToastAction`, `ToastType`, `IToastNotifier` and the extensions are used from controllers, not views.

Done (2026-10-17):

- All 8 files under `Toasts/`, including the internal notifier, queue and result filter, are now in `StellarAdmin.UI.Toasts`.
- Callers add `using StellarAdmin.UI.Toasts;`. The toaster skill reference says so.
- Verified with the full test run (1385 passed) and a skills drift check.

### J17. Progress bounds

`minimum`/`maximum` (`UI/Progress/ProgressTagHelper.cs:19,28`) become `min`/`max`.

Done (2026-10-17):

- The attributes are now `min`/`max` and the properties `Min`/`Max`, matching Slider. The validation messages say Min and Max.
- The ComponentPlayground Progress demo and the progress skill reference were updated.
- Verified with the full test run (1385 passed) and a skills drift check.

### J18. Active and disabled

`is-active` on Pagination, TabLink and the Sidebar menu buttons becomes `active`. `IsDisabled` on `TabTagHelper` and `TabLinkTagHelper` becomes `Disabled`.

Done (2026-10-17):

- `PaginationLink`, `TabLink`, `SidebarMenuButton` and `SidebarMenuSubButton` now use `active`/`Active`. The Sidebar buttons got an explicit `[HtmlAttributeName("active")]`.
- `Tab` and `TabLink` use `Disabled`. Internal callers (the pagination shortcuts and the DataGrid pager) were updated.
- Markup in the admin index page, DocsSamples and the ComponentPlayground now uses `active=`. The skills were regenerated, and the conventions reference's kebab-case example now uses `MinDistance`.
- Verified with the full test run (1385 passed) and a skills drift check.

### J19. DataGrid sort and paging

`sort-by` becomes `sort-field`, matching columns. The route placeholders become `{sortField}`/`{sortDirection}` and `{page}`/`{pageSize}`, and `page-no` becomes `page`.

Done (2026-10-17):

- `sa-data-grid-sort` takes `sort-field` (`SortField`), and its placeholders are `{sortField}` and `{sortDirection}`.
- `sa-data-grid-pager` takes `page`, backed by `CurrentPage` because the anchor base already has `Page` for `asp-page`. Its placeholders are `{page}` and `{pageSize}`.
- The XML docs, error messages, admin index grid, DocsSamples DataGrid intro, DataGridSpike pages and data-grid skill reference were updated. The admin's own query keys (`sortBy`, `page`) and the DocsSamples query keys are unchanged, and Razor Pages reserves `page` as a route value, so examples keep `asp-route-pageNo="{page}"`.
- Verified with the full test run (1385 passed) and a skills drift check.

### J20. invalid and maxlength

On `sa-input-otp`, `aria-invalid` becomes `invalid` and `max-length` becomes `maxlength`.

Done (2026-10-17), with a changed decision:

- `sa-input-otp` takes `maxlength`, the native attribute name. The property stays `MaxLength`.
- `sa-input-otp` keeps `aria-invalid`. It is the standard ARIA state, and `sa-input`, `sa-textarea`, `sa-select`, `sa-checkbox` and `sa-radio` already take it unchanged.
- `sa-chip-group` was the odd one out, so its `invalid` (`Invalid`) became `aria-invalid` (`AriaInvalid`).
- Markup in DocsSamples, the ComponentPlayground and the OneTimeCode and MultiLookupSheet editor templates was updated, and the skills were regenerated.
- Verified with the full test run (1385 passed) and a skills drift check.

### J21. show-* toggles

`render-error` and `render-field` become `show-error` and `show-field`.

Done (2026-10-17), with a changed name:

- `sa-questionnaire-item` and `sa-questionnaire-input` take `show-error` (`ShowError`).
- The field inputs take `wrap-in-field` (`WrapInField`) instead of `show-field`, because `show-field="false"` reads as hiding the input when it only skips the field wrapper.
- Internal callers (InputGroup input and textarea, ChoiceGroup), the grid editor templates, DocsSamples, ComponentPlayground, tests and skills were updated.
- Verified with the full test run (1385 passed) and a skills drift check.

### J22. Group caption

`heading` on `sa-command-group` (`UI/Command/CommandGroupTagHelper.cs:17`) becomes `title`.

Done (2026-10-17):

- `sa-command-group` takes `title` (`Title`), matching `sa-command-dialog`. The rendered markup, including the `command-group-heading` data-slot that themes style, is unchanged.
- The command palette, DocsSamples, ComponentPlayground, tests and the command skill reference were updated.
- Verified with the full test run (1385 passed) and a skills drift check.

### J23. Explicit attribute names

33 bound properties take their attribute name from the C# name, so renaming the property silently renames the attribute. They are on Carousel, CheckboxGroup, ChoiceGroup, FormSection, RadioGroup, SegmentedControl and the Sidebar menu helpers.

Done (2026-10-17):

- 32 properties now declare `[HtmlAttributeName]` with the name Razor derived before, so markup is unchanged. Besides the components above, the list included `FieldInputBaseTagHelper.Name` and `InputGroupAddonTagHelper.Align`.
- `TagHelperAttributeNamesTests` fails when a public tag helper in StellarAdmin.UI has a bound property without `HtmlAttributeName` or `HtmlAttributeNotBound`. The three StellarAdmin tag helpers already name their only property and aren't covered by the test.
- No website change is needed.
- Verified with the full test run (1386 passed) and a skills drift check.

### J24. Numbers

Slider `Min`, `Max`, `Step`, `MinDistance` and mark `Value`, and Progress `Value`, `Min` and `Max` become `double?`, so `step="0.5"` works. `SliderEditor` follows.

Done (2026-10-17):

- `sa-slider` `min`/`max`/`step`/`min-distance`, `sa-slider-marks` `interval`, `sa-slider-mark` `value` and `sa-progress` `min`/`max`/`value` are doubles. `sa-slider-value` `index` stays an int.
- Bound and authored slider values are read as doubles, so a `decimal` or `double` model is no longer truncated. A step that isn't positive throws.
- Values are formatted with up to as many decimals as `min` and `step` have, on the server and in `sel-slider`. Generated marks and client snapping round away floating-point noise, so a 0.1 step gives 0.3, not 0.30000000000000004.
- `SliderEditor` `Min`, `Max`, `Step`, `MarkInterval` and `AddMark` take doubles, and fractional `[Range]` bounds now apply. Its docs say numeric instead of whole-number.
- Verified with the full test run (1390 passed), the client build and a skills drift check. Client snapping wasn't checked in a browser.

### J25. Nullable booleans

- Questionnaire item: `Required`, `Multiple`.
- Questionnaire choice: `Disabled`.
- Table: `RowDetails`, `Selection`.
- DataGrid: `RowsOnly`.
- DataGrid column and display field: `TemplateRendersEmpty`.

Done (2026-10-17):

- All eight are `bool?` and resolve to `false` in `ProcessAsync`, so markup and behaviour are unchanged. No other bound property in the library was a plain `bool`.
- The Questionnaire item and choice properties gained their "Defaults to false" remarks, and the choice's `Disabled` summary now says it cannot be selected.
- No website change is needed.
- Verified with the full test run (1390 passed) and a skills drift check.

### J26. Icon definitions

`IconDefinition` shares one mutable `List<SvgShape>` per icon across the process. It becomes a sealed class with `IReadOnlyDictionary` and `IReadOnlyList`. `IIconPack.GetIcons()` returns `IReadOnlyDictionary`.

Done (2026-10-17):

- `IconDefinition` and `SvgShape` are sealed classes. Their constructors take `IReadOnlyDictionary`/`IReadOnlyList` and keep read-only copies, so changing the inputs afterwards doesn't change the icon. `SvgShape.Attributes` is `IReadOnlyDictionary` instead of `IImmutableDictionary`, so callers pass a plain `Dictionary`.
- `IIconPack.GetIcons()` returns `IReadOnlyDictionary<string, IconDefinition>`.
- The icon generator emits `SvgShape[]` and passes the pack's shared `<svg>` attributes instead of an immutable copy per icon.
- The Voyager pack, the test packs and the hand-written icons skill reference were updated.
- Verified with the full test run (1394 passed) and a skills drift check.

### J27. IconOptions

- Lock the registry after startup.
- Make `AddIcon` and `AddIconPack` treat duplicates the same way.
- Validate at startup instead of on the first render.

Done (2026-10-17):

- `AddStellarAdminUI()` registers a `PostConfigure` that freezes `IconOptions` into frozen dictionaries after every `Configure` call. `AddIcon`, `AddIconPack`, `ClearIcons`, `MapSemanticIcon` and `RemoveIcon` then throw `InvalidOperationException`.
- `AddIcon` replaces a same-named icon, like `AddIconPack`, so the last registration wins.
- `ValidateOnStart()` resolves the options when the host starts, so an invalid pack or mapping stops startup instead of the first render.
- The icons skill reference was updated.
- Verified with the full test run (1400 passed), a skills drift check and DocsSamples starting and rendering the Icon page.

### J28. Enum member order

- Put `ButtonSize`, `AttachmentSize`, `ItemSize` and `ImageSize` in one order with a `Default` member.
- Give `GroupGap` and `StackGap` a `Default` that is not `= 2`.
- Add `Default` to `AttachmentMediaVariant`.

Done (2026-10-17):

- One rule: `Default` comes first, then the other sizes from smallest to largest.
- `AttachmentSize` and `ItemSize` are now Default, ExtraSmall, Small. `GroupGap` and `StackGap` are now Default, ExtraSmall, Small, Large, ExtraLarge. `ButtonSize` already followed the rule.
- `ImageSize` keeps no `Default`, because its default depends on context (Small for a thumbnail, Medium for a figure).
- `AttachmentMediaVariant` is unchanged, because `Icon` already is the default and is 0.
- Verified with the full test run (1400 passed) and a skills drift check after regenerating the references.

### J29. Overloads instead of optional parameters

- EF `UseItems(..., configure = null)`.
- `EfCoreResourceScopesBuilder.Add(..., predicate = null)`.
- `SliderEditor.AddMark(..., label = null)`.
- `SidebarLinkTarget.Action(..., area = null)` and `Page(..., area = null)`.
- `ToastNotifierExtensions.Success/Info/Warning/Error(..., description = null)`.

Done (2026-10-17):

- Each method is now two overloads, one without the last parameter and one where it is required and non-nullable.
- The longer overloads throw `ArgumentNullException` for a null `configure`, `predicate`, `label` or `description`, and `ArgumentException` for a null or blank `area`.
- `ResourceController` built the inline-row created toast with a description that can be null, so it now adds the `Toast` directly.
- Tests cover the null arguments and both overloads of `AddMark` and the toast shortcuts.
- Verified with the full test run (1421 passed) and a skills drift check. The website needs no change.

### J30. Enable* return types

A lambda that supplies required data (panels, scopes, a search predicate) returns the sub-builder. A lambda that only configures returns the parent. `EnableScopes` changes to match, and the rule goes into `docs/conventions/options-builders.md`.

Done (2026-10-17):

- No code changed. Every `Enable*` method already returns the feature's builder without a `configure` callback and the parent builder with one, including `EnableScopes`.
- The registration rule in `docs/conventions/options-builders.md` now covers `Enable*` as well as `Add*`, and says a lambda that supplies required data, such as `EnableRowDetails(panels)`, is not a `configure` callback.

### J31. Core LookupItemsBuilder

`LookupItemsBuilder<TEntity>` returns `this`, gets an internal constructor and moves to `Builders`. The label parameter gets one name across choices and lookups.

Done (2026-10-17):

- `LookupItemsBuilder<TEntity>` moved to `StellarAdmin.Resources.Builders`, has an internal constructor, and its `Use*` methods return the builder. Its tests moved to match.
- The label is called `title` everywhere. The EF choice `UseItems` parameter `text` became `title`, and `ChoiceItem.Text` became `ChoiceItem.Title`, with the views, internals and XML docs updated.
- The editors and relationships skill references were updated.
- Verified with the full test run (1422 passed) and a skills drift check. The website doesn't mention these APIs.

### J32. Menu and forms options

`ConfigureMenu` takes a builder like `ConfigureForms`. Both are stored as `StellarAdminUIOptions.Menu` and `.Forms`.

On hold (2026-10-17): Jerrie plans to remove the menu options (`ConfigureMenu`, `StellarAdminUIMenuOptions` and `StellarAdminUIOptions.Menu`) now that StellarAdmin has its own stylesheets instead of following shadcn. Come back to this once that's decided, since only the forms half may remain.

### J33. Small builder naming issues

- `Base(...)`/`BaseFull()` vs the `Default` tier name in the convention: align them.
- `LookupSheetEditor.Editor(...)` becomes `Field(...)`.
- `MultiLookupFieldOptions.Layout` becomes nullable.
- `UserMenuBuilder.Items` gets `Add*` methods.
- `ResourcePagingBuilder.PageSizes` takes a list.
- `SearchOn` requires at least one field at compile time.

Done (2026-10-17):

- `LookupSheetEditor.Editor(...)` and `MultiLookupSheetEditor.Editor(...)` are now `Field(...)`, matching `Sheet(...)` and the `LookupFieldOptions` types. The relationships skill reference was updated.
- The conventions doc now names the smallest form grid tier `Base`, matching `Base(n)` and `BaseFull()`.
- `MultiLookupFieldOptions.Layout` stays non-nullable, because the multi lookup has no automatic layout for null to stand for.
- `ResourcePagingBuilder.PageSizes` takes an `IReadOnlyList<int>`.
- `SearchOn` takes a required first selector and `params` for more, so it needs at least one at compile time.
- `UserMenuBuilder.Items` is gone. `menu.Add(item)` appends and `menu.Clear()` empties the list, both returning the builder. Log out is no longer an item: it always shows after the items, and `menu.Logout(...)` configures it with `Path`, `Icon` and `Show` next to `RedirectPath`. The four `UserMenuItem.Logout(...)` factories are removed, and `SidebarUserMenuModel.Logout` carries the item to the view.
- `LogoutBuilder.AuthenticationSchemes` is replaced by `AddAuthenticationScheme(scheme)`.
- The sidebar skill reference and `SKILL.md` were updated. `sandbox/html/sidebar-branding.html` is an old prototype and was left as it was.
- Verified with the full test run (1428 passed) and a skills drift check. The website doesn't mention these APIs.

### J34. Guardrails

Add PublicApiAnalyzers once the other items are done. Set a package validation baseline after the first preview. Stop suppressing CS1591 (missing XML comment) in the packaged projects.

### J35. Third-party notices

The packages embed Lucide and Tabler icon data and shadcn-derived CSS, and their licences require the notice.

Done (2026-10-17):

- `src/Directory.Build.props` packs `THIRD-PARTY-NOTICES.txt` at the package root, so all three packages carry it.
- shadcn/ui and tw-animate-css were removed from the file. The components follow shadcn's structure but use none of its code, and nothing uses tw-animate-css any more.
- Tailwind stays because `stellar-admin-panel.css` is built with the Tailwind CLI. cmdk stays because `command-score.ts` is a port of its scorer.
- Lit and interestfor were added because `stellar-admin.js` bundles them. htmx was added as a courtesy, since its 0BSD licence asks for no notice.
- Verified with `dotnet pack`: each of the three packages contains the file. The test suite wasn't run, because no code changed.

### J36. Client event names

`selection-change` (`sel-table`) vs `checkedchange`, `valuechange` and `itemselect`. Pick one style.

Done (2026-10-17):

- Every client event is now one lowercase word, like the browser's own events.
- Public: `selection-change` → `rowselectionchange`, which avoids the browser's `selectionchange` on `document`. `tab-show` → `tabshow`, `tab-hide` → `tabhide`, `row-detail-expand` → `rowdetailexpand` and `row-detail-collapse` → `rowdetailcollapse`. `itemselect`, `checkedchange` and `valuechange` were already in this style.
- Internal to StellarAdmin: `lookup-select` → `lookupselect`, `lookup-created` → `lookupcreated` and `lookup-retry` → `lookupretry`. The `admin-lookup-created` element keeps its name.
- The `row-detail-expand-mode` attribute is unchanged.
- Updated the XML docs, the `_IndexDataGrid` and `_LookupSheet` triggers, the sandbox, DocsSamples, the tests and the regenerated skill references.
- Verified with the full test run (1428 passed) and a skills drift check.

### J37. Global MVC hooks

Document that `AddStellarAdminUI` adds a model binder for form collections, a value provider and a global toast filter.

Done (2026-10-17):

- `AddStellarAdminUI()` has remarks saying it changes the MVC options for the whole app. They list the checkbox group model binder, the value provider that reads the form on every form post, and the global toast result filter.
- The `configure` overload and `AddStellarAdmin()` point to those remarks.
- Verified with the full test run (1428 passed) and a skills drift check.

### J38. class-names

`ToggleTagHelper`, `SegmentedControlTagHelper` and the choice groups lack `class-names`.

Done (2026-10-17):

- `sa-toggle` takes `ToggleClassNames`: the field parts plus `Control`.
- `sa-segmented-control` takes `SegmentedControlClassNames`: the field parts plus `Control` and `Item`, which adds to every option.
- `sa-checkbox-group` and `sa-radio-group` share `ChoiceGroupClassNames` on `ChoiceGroupTagHelper`: the field parts plus `Control` and `Item`. `Item` applies to child items and `asp-items` options alike.
- `Control` adds to the same element as `class`.
- Added a `class-names` test for each helper. Verified with the full test run (1431 passed) and a skills drift check.

### J39. Small attribute polish

- `sa-toggle-group` `spacing` becomes an enum like `StackGap`.
- The Slider `thumb-labels` list can't contain a comma: document it or accept a list.

Done (2026-10-17):

- `sa-toggle-group` `spacing` takes the new `ToggleGroupSpacing` enum: `Default` (0.5rem, was `2`), `None` (joined, was `0`), `Small` (0.25rem, was `1`) and `Large` (1rem, was `4`). The rendered `data-spacing` and `--gap` values are unchanged.
- Updated the admin toggle group editor, the DocsSamples and the regenerated skill reference.
- `thumb-labels` stays a comma-separated string, and its XML docs now say a label can't contain a comma.
- Added a spacing test. Verified with the full test run (1435 passed) and a skills drift check.

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

Progress (2026-10-17):

- Installed the [csharp-docs](../../.agents/skills/csharp-docs/SKILL.md) skill from github/awesome-copilot in `.agents/skills/`, with a `.claude/skills/` link.
- Jerrie approved the rules after a trial on four classes. Follow the skill's wording, but leave internal members undocumented, add `<value>` only when it adds something, put defaults in `<value>`, put behaviour notes in `<remarks>`, document only the exceptions the member throws itself, and skip examples.
- `docs/conventions/xml-documentation.md` now holds these rules and the `RegisterRowDetails` example.
- The trial classes already follow them: `QuestionnaireChoiceTagHelper`, `IconDefinition`, `IIconPack` and `StellarAdminUIExtensions`.
- The SkillsGenerator reads an attribute's default from "The default is …" in `<value>`. It only falls back to `<remarks>` when they mention a default. This also stopped it misreading "Must be `null` if …" (the anchor routing attributes) and the `Name` cref on `name` as defaults, which changed 25 skill reference tables.
- Checked: the build passes, all 1435 tests pass and the skills drift check is clean.
- Pilot done: the resource builders and the J5 hooks (`ResourceOptions`, `ResourceScopeOptions`, `ResourceSearchOptions`, `LookupItems`), 44 files. One agent rewrote each batch and a second checked it against the code. The check fixed wrong claims, such as the inline row field rules and the `UseAvatar` empty-string case.
- Jerrie approved the pilot's wording choices. Write-only properties say "Sets…". Enum properties get a `<value>` that starts "One of the enumeration values that specifies…" and gives the default. Method defaults go in `<remarks>`. Delegate defaults say "The default returns…".
- Checked after the pilot: the diff touches only `///` lines, the build passes with no doc warnings, all 1435 tests pass and the skills drift check is clean.

Done (2026-10-17):

- The rest of the public API follows the convention: StellarAdmin, StellarAdmin.EntityFrameworkCore, the StellarAdmin.UI root and every tag helper folder, 608 files. Eight folder batches each had one agent rewrite the docs and a second check them against the code, which made 46 corrections.
- The SkillsGenerator reads "The default is" across wrapped lines, so about 50 tag attributes now show their default in the consumer skill tables. Defaults that depend on context, such as the image `aspect-ratio` and `size`, are given in words and show "—".
- Checked: the diff touches only `///` lines, the build passes with no doc warnings, all 1435 tests pass and the skills drift check is clean.

Follow-ups the agents noticed in the code, not fixed:

- `TempDataKeys.ErrorMessage` is never used.
- `QuestionnaireItemTagHelper.Required` only writes `data-required`, and no CSS or JS reads it.
- A non-numeric `groups` value on `sa-input-otp` throws a `FormatException` from `int.Parse`, not an `ArgumentException`.
- A disabled `sa-slider` still posts its hidden inputs.
- `SidebarMenuButton` and `SidebarMenuLink` write a valueless `data-active`, while `SidebarMenuSubButton` writes `data-active="true"`.
- The pagination first and last tag helpers use the `sa-pagination-previous` and `sa-pagination-next` classes.
- `DataGridColumnTagHelper` uses `string.Format` without an explicit culture.

### J41. Choice editor class names

The checkbox group, segmented control and toggle group editors in the admin panel pass only `ClassNames.Control`, through `class`. `EditorClassNamesMapper` throws when any other part is set. The single-control editors pass every part through `class-names`, and the radio editor writes its own markup with every part plus `Option.*`.

Approved (Jerrie, 2026-10-17):

- The three editors pass `ChoiceGroupClassNames`, `SegmentedControlClassNames` and `ToggleGroupClassNames` through `class-names`, so `Root`, `Label`, `Content`, `Description`, `Error` and `Control` all work. The Control-only check goes.
- Per-choice classes stay out of scope. Only the radio editor has `Option.*`, and matching it needs new `EditorClassNames` properties.
- Do it after J39.

Done (2026-10-17):

- `EditorClassNamesMapper` maps every field part for the checkbox group, segmented control and toggle group editors, and their templates pass the result through `class-names`. The Control-only check is gone.
- The toggle group editor keeps its appearance classes on `class`.
- The integration test that expected a checkbox group `Label` to fail now checks that `Label` and `Root` style the field. Verified with the full test run (1435 passed) and a skills drift check.

## Website follow-up

The website is updated in one pass after the remaining renames, or before the NuGet preview if that comes first (Jerrie, 2026-10-17). The pass works in the `../website-rename` worktree on `stellaradmin-rename`. Each item that changes markup or public names adds a line here when it is done.

The pass:

1. Update the hand-written MDX for every item below.
2. Run `dotnet run --project docs/DocsSamplesGenerator` with `STELLARADMIN_WEBSITE_DIR=../website-rename` to regenerate the demos and snippets.
3. Run `pnpm examples:export` in the website for the inline examples.
4. Search the website for the old names, then run its lint, type check and build.

Changes the website still needs:

- [ ] D4: `sa-linkbutton` → `sa-button-link`, `sa-attachment-link-trigger` → `sa-attachment-trigger-link`, `sa-bubble-link-content` → `sa-bubble-content-link`, `sa-command-link-item` → `sa-command-item-link`.
- [ ] D4: link items in dropdown menus use `sa-dropdown-menu-item-link`. `sa-dropdown-menu-item` is a button and takes `type="submit"`.
- [ ] J13: `InputGroupAddOn*` → `InputGroupAddon*`, the enum is `InputGroupAddonAlign` and the property is `Align`.
- [ ] J14: `ButtonGroupSeparatorOrientation`, `DetailsOrientation`, `DisplayFieldOrientation` and `SidebarMenuSubButtonSize` replace borrowed enums. `SidebarMenuSubLinkSize` is `Default`/`Small`, `SidebarVariant.Sidebar` → `Default`, `SidebarCollapsible.Offcanvas` → `OffCanvas`.
- [ ] J15: `InputGroupAddonAlign` members are `Start`/`End`/`Top`/`Bottom`. `DataGridRowDetailToggle` members are `Start`/`End`/`None`.
- [ ] J16: the toast API needs `using StellarAdmin.UI.Toasts;`.
- [ ] J17: `sa-progress` takes `min`/`max`.
- [ ] J18: `is-active` → `active` on Pagination, TabLink and the Sidebar menu buttons. `IsDisabled` → `Disabled` on Tab and TabLink.
- [ ] J19: `sa-data-grid-sort` takes `sort-field` with `{sortField}`/`{sortDirection}`. `sa-data-grid-pager` takes `page` with `{page}`/`{pageSize}`.
- [ ] J20: `sa-input-otp` takes `maxlength`. `sa-chip-group` takes `aria-invalid` instead of `invalid`.
- [ ] J21: `render-error` → `show-error` on the Questionnaire item and input. `render-field` → `wrap-in-field` on the field inputs.
- [ ] J22: `sa-command-group` takes `title` instead of `heading`.
- [ ] J24: `slider.mdx` says values are formatted without decimals. They now take up to as many decimals as `min` and `step` have.
- [ ] J26: `icons.mdx` uses `IReadOnlyDictionary<string, IconDefinition> GetIcons()`, `IReadOnlyList<SvgShape>` and `IReadOnlyDictionary<string, string>` for shape attributes, and drops the `.ToImmutableDictionary()` calls.
- [ ] J27: `icons.mdx` says duplicate `AddIcon` names throw. A same-named icon now replaces the existing one, icons are frozen once configured, and invalid packs fail at startup.
- [ ] J28: `item.mdx`, `stack.mdx`, `group.mdx` and `attachment.mdx` list `ItemSize`, `StackGap`, `GroupGap` and `AttachmentSize` members in the old order. The new order is Default first, then smallest to largest.
- [ ] J35: the website's licence notices in the docs list shadcn/ui and tw-animate-css. Match them to `THIRD-PARTY-NOTICES.txt`: Tailwind, Lucide, Tabler, cmdk, Lit, interestfor and htmx.
- [ ] J36: events are renamed: `selection-change` → `rowselectionchange`, `tab-show` → `tabshow`, `tab-hide` → `tabhide`, `row-detail-expand` → `rowdetailexpand`, `row-detail-collapse` → `rowdetailcollapse`. Affects `tabs.mdx`, `table.mdx` and their includes.
- [ ] J37: the installation page should say that `AddStellarAdminUI()` (and so `AddStellarAdmin()`) adds a checkbox group model binder, a value provider and a global toast result filter to the MVC options for the whole app.
- [ ] J38: `sa-toggle`, `sa-segmented-control`, `sa-checkbox-group` and `sa-radio-group` take `class-names` (`ToggleClassNames`, `SegmentedControlClassNames` and `ChoiceGroupClassNames`). Add them wherever the docs list each component's attributes.
- [ ] J39: `sa-toggle-group` `spacing` takes `ToggleGroupSpacing` (`Default`, `None`, `Small`, `Large`) instead of a number. `spacing="0"` becomes `spacing="ToggleGroupSpacing.None"`. The slider docs should say a thumb label can't contain a comma.
- [ ] J41: the checkbox group, segmented control and toggle group editors accept every `ClassNames` field part, not just `Control`. Remove any Control-only note from the editor docs.
- [ ] D2: add the "Your own field inputs" guide from the UI skill's `references/forms.md` to the forms docs.
- [ ] D3: document `ISidebarItemsProvider` from the "Items from code" section of the admin panel skill's `references/sidebar.md`.
- [ ] D5: `columns` on `sa-grid`, `sa-details`, `sa-radio-group` and `sa-checkbox-group` now sets the smallest width, and `columns-base` is gone. Change `columns="N"` to `columns-md="N"` and `columns-base` to `columns` in `grid.mdx`, `details.mdx`, `form-section.mdx`, the choice group pages and their API tables.
- [ ] J40: the public XML docs were rewritten and many skill reference defaults changed. Regenerate the API reference and the attribute tables from the new docs.

## Order of work

1. Settle the D items.
2. Removals and visibility: J1–J10.
3. Renames and types: J11–J33, together with the outcomes of D4–D10 and D12.
4. Packaging and docs: J35–J40.
5. J34 last, so the analyzers record the final API.

Each step updates samples, DocsSamples and consumer skills that use a changed name, and adds the website changes to the website follow-up.
