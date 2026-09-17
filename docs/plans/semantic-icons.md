# Icon encapsulation and semantic icons

Status: encapsulation and semantic icons implemented; accordion indicator follow-up implemented; Dashboard mappings excluded. Updated 2026-09-16. Repositories: workspace, stellar-admin, website, skills.

## Implemented

`IconOptions` owns a private case-insensitive dictionary and exposes `AddIcon`, `AddIconPack<TIconPack>`, `GetIconNames`, `TryGetIcon`, `RemoveIcon`, and `ClearIcons`. Builder registration delegates to these methods; rendering and the playground browser use the lookup methods. Individual additions reject duplicate names; later packs override matching names. Lucide remains seeded by default. Clearing permits replacing the entire default set. The website and handwritten consumer icon reference describe the methods.

## Semantic API

The public `SemanticIconRole` enum in Core defines roles that packs map to icon names alongside their definitions. The following 23 mappings cover existing built-in `IconTagHelper` usages. Role names describe the purpose; distinct purposes can share a glyph without sharing an override.

### Tag helper mappings

Scope: migrate only built-in icon usages that already render through `IconTagHelper`. Inline SVGs are excluded, including the accordion title indicator.

| Role | Default Lucide icon | Current call sites / meaning |
| --- | --- | --- |
| `DropdownIndicator` | `chevron-down` | `SelectTagHelper`; indicates the select can open. |
| `SubmenuIndicator` | `chevron-right` | `DropdownMenuSubTriggerTagHelper`; opens a nested menu. |
| `BreadcrumbSeparator` | `chevron-right` | `BreadcrumbSeparatorTagHelper`; separates hierarchy levels. |
| `BreadcrumbEllipsis` | `ellipsis` | `BreadcrumbEllipsisTagHelper`; omitted hierarchy levels. |
| `PaginationEllipsis` | `ellipsis` | `PaginationEllipsisTagHelper`; omitted pages. |
| `PaginationPrevious` | `chevron-left` | `PaginationPreviousTagHelper`, `DataGridPagerTagHelper`. |
| `PaginationNext` | `chevron-right` | `PaginationNextTagHelper`, `DataGridPagerTagHelper`. |
| `PaginationFirst` | `chevron-first` | `PaginationFirstTagHelper`. |
| `PaginationLast` | `chevron-last` | `PaginationLastTagHelper`. |
| `CarouselPrevious` | `chevron-left` | `CarouselPreviousTagHelper`; previous slide. |
| `CarouselNext` | `chevron-right` | `CarouselNextTagHelper`; next slide. |
| `Close` | `x` | `SheetTagHelper`; dismisses the sheet. |
| `Loading` | `loader-circle` | `SpinnerTagHelper`; glyph must work with continuous rotation. |
| `CheckboxSelected` | `check` | `InputTagHelper` checkbox indicator. |
| `RadioSelected` | `circle` | `InputTagHelper` radio indicator. |
| `MenuItemSelected` | `check` | `DropdownMenuCheckboxItemTagHelper`, `DropdownMenuRadioItemTagHelper`; both currently use a checkmark. |
| `ChoiceSelected` | `check` | `QuestionnaireChoiceTagHelper`; selected questionnaire choice. |
| `SortAscending` | `arrow-up` | `DataGridSortTagHelper`. |
| `SortDescending` | `arrow-down` | `DataGridSortTagHelper`. |
| `SortUnsorted` | `chevrons-up-down` | `DataGridSortTagHelper`; sortable column without an active direction. |
| `ToggleSidebar` | `panel-left` | `SidebarTriggerTagHelper`. |
| `ScrollToEnd` | `arrow-down` | `MessageScrollerButtonTagHelper`; CSS rotates the glyph for scrolling to the start. |
| `OtpSeparator` | `minus` | `InputOtpRenderer`; separates groups of code digits. |

Mapping decisions: keep pagination and carousel roles separate, but share pagination roles with the data grid pager. Keep breadcrumb and pagination ellipses separate. Share menu selection across checkbox/radio menu items while preserving the radio input's different glyph. Do not introduce a generic `Overflow` role until a built-in overflow action needs it.

Preserve current rotation and direction handling: carousel CSS adapts navigation glyphs for orientation/RTL, and the message scroller rotates its downward glyph for the start direction. Pack authors must supply glyphs compatible with those transformations. A separate `ScrollToStart` role would require changing that rendering/CSS contract and is not proposed here.

### Other icon usages found

`AlertTagHelper.Icon` and consumer-authored `<sa-icon name="...">` select explicit glyphs; retain their name-based behavior. The missing-icon fallback is renderer infrastructure, not a semantic role supplied by a pack.

Dashboard Razor views also contain built-in named icons: resource create (`plus`), search (`search`), edit (`square-pen`), delete (`trash-2`), and sidebar branding/account/menu entries (`compass`, `user`, `chevrons-up-down`, `badge-check`, `credit-card`, `bell`, `log-out`). These need a separate dashboard mapping review before claiming the entire product supports replacing Lucide. Configured resource/menu/empty-state icon names also need their defaults audited. They are not included in the initial tag helper role list.

A search of the TagHelpers client TypeScript found no icon-rendering code.

Inline SVGs, including `AccordionItemTitleTagHelper`, must remain unchanged in this migration. Their use may reflect deliberate rendering or styling requirements; do not assume a visually similar Lucide glyph is interchangeable. Handle each separately after investigating the original implementation and its history, with before/after visual verification of relevant states and themes before accepting a replacement. No accordion role is proposed in the initial mapping.

`IIconPack` exposes `GetSemanticIconMappings()` returning `IReadOnlyDictionary<SemanticIconRole, string>`. A default interface implementation returning an empty mapping preserves custom packs that only provide named icons. Lucide supplies the initial mappings. `AddIconPack<T>()` imports both definitions and mappings; later mappings replace earlier mappings for the same role. Packs used as complete replacements should cover the standard roles; additive packs may supply only a subset. Mapping targets are validated before importing a pack, against only the incoming pack’s icon names using case-insensitive matching. Missing targets throw `ArgumentException` without partially registering the pack.

`IconOptions` keeps the mapping private and exposes `MapSemanticIcon(SemanticIconRole role, string name)` for application overrides and `GetSemanticIconName(SemanticIconRole role)` returning `string?` for semantic lookup. Component callers resolve the role to a name and pass that name to the existing renderer, so a named icon override also affects its semantic uses. `ClearIcons()` clears both definitions and mappings; removing an icon removes mappings targeting it. Missing roles use the existing missing-icon rendering behavior without silently restoring Lucide. Role coverage is not enforced; partial additive packs are supported.

`IconTagHelper` remains entirely unaware of semantic roles: no role property, constructor overload, or role lookup inside it. Component tag helpers resolve roles through `IconOptions` before setting `Name`. Preserve child-content overrides already supported by components.

```csharp
var iconTagHelper = new IconTagHelper(_iconOptions)
{
    Name = _iconOptions.GetSemanticIconName(SemanticIconRole.PaginationEllipsis)
};
await iconTagHelper.ProcessAsync(context, iconOutput);
```

An unmapped role returns `null`, invoking the existing missing-icon fallback.

Application configuration:

```csharp
services.Configure<IconOptions>(icons =>
{
    icons.ClearIcons();
    icons.AddIconPack<MyIconPack>();
    icons.MapSemanticIcon(SemanticIconRole.DropdownIndicator, "my-caret-down");
});
```

A pack might map `DropdownIndicator` to `caret-down`, `PaginationEllipsis` to `dots-three`, and `Close` to `times`. Tag helpers never need to know those names.

## Remaining work

Inline SVGs remain separate work, case by case, with historical investigation and before/after visual verification. Dashboard view icons and configured icon defaults need their own mapping review before claiming all built-in product icons are pack-independent. No inline SVG, `IconTagHelper`, CSS, or client TypeScript was changed.

The DropdownMenu `_RadioGroup.cshtml` and `_RadioEvents.cshtml` samples use the explicit icon name `filter`, which currently renders the missing-icon fallback. These sample usages are unchanged and outside this migration; their semantic menu-selection indicators render normally.

## Verification

Encapsulation: the TagHelpers executable checks passed, including SVG rendering, missing-icon fallback, provider isolation, duplicate rejection, pack precedence, clearing defaults, name snapshots, and case-insensitive removal. Executing the checks built Core and TagHelpers. Used installed SDK 10.0.400 from the workspace because the child repository's pinned 10.0.100 is unavailable; NuGet vulnerability lookup emitted NU1900 warnings. Formatted the four touched C# files with cached CSharpier 1.3.0. Website lint, type checks, and production build passed.

Mapping review: inventoried direct `IconTagHelper` construction, shared dropdown/data-grid rendering methods, inline SVG, Dashboard Razor named icons, and TagHelpers client TypeScript. Documentation-only change; no runtime implementation or tests run.


Semantic implementation: tag helper executable checks passed after formatting, including complete Lucide role coverage, replacement without Lucide, partial and legacy packs, mapping precedence, named definition overrides, options isolation, missing-role rendering, case-insensitive mapping/removal, invalid mapping rejection without partial registration, application overrides targeting already registered icons, and child-content overrides. Existing field rendering checks also passed. CSharpier formatted all 29 touched/new C# files. Source comparison confirmed unchanged default glyph names for all 18 direct migrated icon constructions; shared dropdown/data-grid call sites were reviewed separately. `IconTagHelper` and the accordion source were verified byte-for-byte unchanged.

DocsSamples built and served on temporary port 5206. HTTP rendering checks covered Pagination, Breadcrumb, Carousel, DataGrid, DropdownMenu, Checkbox, Radio, Select, InputOtp, Spinner, Sheet, Sidebar, Questionnaire, MessageScroller, and Accordion. All returned successful pages containing SVG; only the two explicit `filter` usages noted above rendered fallback icons. No browser interaction or visual comparison was performed; inline SVG replacement is excluded. The temporary server was stopped after verification. Used installed SDK 10.0.400; builds reported the existing NuGet vulnerability endpoint warnings and DocsSamples nullable warnings. Website lint, type checking, and production build passed. Documentation and consumer reference now describe mappings and scope. Changes are uncommitted.

Naming review: renamed the enum to `SemanticIconRole`, pack mappings to `GetSemanticIconMappings()`, and application overrides to `MapSemanticIcon(role, name)`. `GetSemanticIconName(role)` is unchanged. Updated call sites, tests, website documentation, and consumer guidance; no old API names remain in the searched source and documentation. Tag helper tests and website lint, type checks, and production build passed after the rename.

Pack ownership: semantic mappings must target icons supplied by the same pack, even when a matching icon is already registered. Application-level `MapSemanticIcon` can still select any registered icon. Added a regression check for rejecting a mapping-only pack targeting an existing icon while preserving existing state.

Pack ownership verification: tag helper regression checks and website lint, type checks, and production build passed. CSharpier and whitespace checks passed.


## Accordion indicator follow-up (2026-09-16)

Authorized separately after the initial semantic icon commits. `AccordionItemTitleTagHelper` now resolves `SemanticIconRole.AccordionIndicator` through `IconOptions` and renders it through `IconTagHelper`; Lucide maps the role to `chevron-down`. Existing enum values are preserved. The wrapper and CSS are unchanged, retaining the 16px size, muted color, and 180-degree open-state rotation. IconTagHelper remains unaware of semantic roles.

History review: the inline chevron dates to the initial import; commit `a359876` moved its size and rotation classes into the existing wrapper-child CSS selector. No additional inline-SVG requirement was found in that history. Consumer docs describe the downward closed-state glyph expected from packs. Replacement-pack rendering is covered by the semantic icon checks with Lucide removed.

## Pack registration settings (2026-09-17)

Implemented `IconPackOptions` and callback overloads on `StellarAdminBuilder.AddIconPack<T>()` and `IconOptions.AddIconPack<T>()`, preserving the existing parameterless overloads. `Prefix` defaults to null and is prepended literally to registered names and imported semantic mapping targets; no unprefixed aliases are added. `ImportSemanticMappings` defaults to true; false skips reading, validating, and importing pack mappings. Existing case-insensitive lookup, removal, and full-name replacement behavior remains unchanged. Website and handwritten consumer icon documentation cover both settings.

Verification: TagHelpers executable checks passed, covering prefixed registration through DI, same-named icons under different prefixes, semantic rendering, case-insensitive lookup and removal, application mapping overrides, disabled imports, empty prefixes, and invalid mapping rejection without partial registration. This also built Core and TagHelpers. Used installed SDK 10.0.400 from the workspace rather than the child repo's unavailable pinned SDK 10.0.100; cached NuGet vulnerability lookup warnings remain. CSharpier formatted the four C# files. Website lint, type checks, and production build passed. Sandbox restrictions required rerunning validation outside the sandbox. No visual changes or browser checks; no remaining work for these settings. Changes remain uncommitted.

Commit-time verification: subsequent edits added `StellarAdminBuilder.ClearIcons()`, changed DocsSamples registration to the configuration callback, and moved mapping validation after icon registration. The latest TagHelpers executable run fails with `An invalid mapping must not partially register a pack.` The mapping getter is also now called when imports are disabled. These latest edits are preserved in the requested commit; restoring atomic validation and skipping the getter when imports are disabled remain follow-up work. The earlier passing verification describes the implementation before these edits. Website changes are intentionally excluded from commits at the user's request.


## Tabler mappings and comparison preview (2026-09-17)

Updated Tabler definitions to 3.46.0 and supplied all 24 semantic roles in both packs. The filled pack uses `inner-shadow-top` for Loading and `point` for OtpSeparator, as reviewed. DocsSamples now exposes `/Icon/Semantic` through the Semantic Icons sidebar entry, iterating enum roles and each pack's actual mappings for Lucide, Tabler Outline, Tabler Filled, and the Voyager sample. Preview registrations use prefixes with semantic imports disabled. Voyager now implements the required mapping method with an empty dictionary.

Verification: DocsSamples build passed with 10 existing nullable-property warnings; CSharpier and whitespace checks passed. Chromium checks at desktop/light and mobile/dark confirmed 24 rows, 72 icons, 24 unmapped cells, and three animated loading icons. Screenshots reviewed; mobile scrolling stays inside the table region. Website files were not changed. Changes remain uncommitted; no remaining work for the comparison preview.


## Lucide refresh and commit verification (2026-09-17)

Updated Lucide definitions from lucide-static 1.46.0 (1,838 icons). Updated sample references for removed names and replaced the older missing `filter` icon with `funnel` in both DropdownMenu radio samples. The DocsSamples audit verified 607 literal Lucide references across 151 files and all 24 semantic mappings; the custom Voyager and Tabler preview references also resolve. Core build and whitespace checks passed. At commit time, the TagHelpers executable passed core icon options and isolation checks, then failed the previously recorded assertion `An invalid mapping must not partially register a pack.` That behavior remains follow-up work; current user edits are preserved. All non-website changes are being committed at the user’s request, including the SDK update and workspace schema mapping. Website edits and docs sample regeneration remain with the user.
