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
| D4 | One scheme for link versions of components? | `sa-<component>-link` everywhere, with `sa-link-button`. | `sa-<component>-link` everywhere, including `sa-button-link` and `sa-dropdown-menu-item-link`. Done (2026-10-17) |
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
- All 1378 tests pass. Tag helpers are D1.

### J7. Tag helper plumbing

Done:
- `TryGetNamedSlot` is `protected internal`, because apps' own tag helpers read slots and `<sa-slot-outlet>` reads its host's slots.
- `TryAddNamedSlot`, `MessageScrollerId` and `SidebarId` are internal. `DataGridConsts` is internal.
- The protected `GetUserSpecifiedClass` is removed. The item tag helpers use the `GetUserSuppliedClass` extension.
- `Init` is sealed and calls a new protected virtual `OnInit`. `SelectTagHelper`, `QuestionnaireItemTagHelper` and `QuestionnaireChoicesTagHelper` override `OnInit` instead.
- `ResponsiveGrid` and `GridTiers` stay public, because StellarAdmin uses almost all of them and has no friend access. They get docs under J40.
- `UniqueIdDiscriminator` stays public for now, because the DataGridSpike sandbox uses it from outside the assembly. Open question.
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

## Order of work

1. Settle the D items.
2. Removals and visibility: J1–J10.
3. Renames and types: J11–J33, together with the outcomes of D4–D10 and D12.
4. Packaging and docs: J35–J40.
5. J34 last, so the analyzers record the final API.

Each step updates samples, DocsSamples and consumer skills that use a changed name, and adds the website changes to the website follow-up.
