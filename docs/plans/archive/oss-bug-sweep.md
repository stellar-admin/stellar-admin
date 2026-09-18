# OSS tag helpers bug sweep

## Code audit — 2026-09-18

Current status: **completed**. The inspected anchor-routing guard, scoped parent context, radio ID suffixes, input tag modes, dropdown state CSS and corrected sample markup retain the fixes. This is source inspection, not a rerun of every historical browser assertion.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

**Index status:** completed. **Indexed:** 2026-09-05. Historical record; completion is recorded in the phase/progress notes below. Deferred items require a separately scoped task.

Status: **COMPLETE 2026-08-18** (Phases 1-3).

First chunk of follow-up from the 2026-08-17 review of the free `StellarAdmin.TagHelpers`
docs, demo samples and code (review notes: session artifact "oss-review-notes"; the
remaining chunks — docs infrastructure, new docs pages, content sweep, example coverage,
convention cleanup, feature decisions, a rendering test project — are separate plans).
Scope here is strictly the verified/reported *bugs*: things that change rendered output
or break behaviour for consumers. Convention items (dictionaries→extensions, ordering,
naming, redundant child-content fetches) are deliberately left out.

Paths: `TH/` = `stellar-admin/src/StellarAdmin.TagHelpers/TagHelpers/`,
`Pages/` = `stellar-admin-pro/docs/DocsSamples/Pages/`.

## Phase 1 — mechanical library fixes (DONE 2026-08-18)

- [x] `output.Attributes.Add("class"/"data-slot", ...)` → `SetAttribute` in 17 helpers
      (Dialog/AlertDialog/Sheet header, footer, title, description, media; Progress
      label/value; Avatar badge/group; Separator; Select wrapper). Root cause of duplicate
      `class` attributes (browser kept the first — every `sa-*` class lost when the author
      supplied a class) and duplicate `data-slot` on every composed separator.
- [x] `SidebarTagHelper` captures the author's `class` before setting the wrapper's own
      `sa-sidebar group peer`; container no longer inherits the wrapper classes.
- [x] `</input>` end tags gone: Input/Switch/Toggle/ToggleGroupItem construct their inner
      `TagHelperOutput("input")` with `TagMode.StartTagOnly` (as InputOtp already did).
- [x] Avatar: `&nbsp;` semicolon, `RemoveEmptyEntries | TrimEntries` (was `TrimEntries`
      twice), dead `GetFontSizeClass` removed, `AvatarGroupCount` keeps the author's class.
- [x] Dropdown checkbox/radio indicators and sub-trigger chevron no longer depend on the
      unsafelisted `hidden` / `ml-auto` literals (only worked in apps with their own
      Tailwind build). Structural CSS instead: `.sa-dropdown-menu-item-indicator` gets
      `in-data-[state=unchecked]:hidden` (server + `sel-dropdown-menu` already maintain
      `data-state`), `.sa-dropdown-menu-sub-trigger` gets `[&>svg:last-child]:ml-auto`;
      the C# literals and the JS `classList.toggle("hidden")` are removed.
- [x] Checkbox indicator icon uses `text-primary-foreground` instead of the hard-coded
      `text-neutral-100 dark:text-black`.

Verified: `dotnet build` + `npm run build` green; bundle rules inspected in
`stellar-admin.nova.css`; rendered DocsSamples pages show zero duplicate `class`/`data-slot`
attributes and zero `</input>`; headless-chromium computed styles confirm unchecked
indicators are `display:none` and the tick colour equals `--primary-foreground`.

## Phase 2 — behavioural fixes (DONE 2026-08-18)

- [x] `StellarAdminTagHelperBase.Init` pushed onto a shared `Stack` and never popped;
      Razor's per-scope copy-on-write `context.Items` scopes *keys*, not the mutable
      stack object inside, so a `<sa-slot-content>` after a sibling tag helper attached
      to the wrong host. Now `Init` reads `Items[ParentKey]` as its parent and sets the
      key to itself (slots stay transparent) — the scoping does the popping.
      `Capture/RestoreParentTagHelperStack` removed (pro `DataGridTagHelper` no longer
      needs them; per-scope copies keep repeated child passes stable). Verified with a
      scratch app (sibling/nested/repeater/slot cases) and DocsSamples DataGrid,
      SlotContent, SlotOutlet, TemplatedTagHelper pages. (2026-08-18)
- [x] Model-bound radios shared one `id` (`PartialModel_BedType` ×3). `InputTagHelper` now
      suffixes the value (`TagBuilder.CreateSanitizedId("{id}_{value}")`) unless the author
      supplied an id, and points the auto label at it via `LabelForId`. (2026-08-18)
- [x] `SelectTagHelper` wrapper `<div>` now clears the host attributes after they are copied
      to the `<select>`; only its own `data-slot`/`data-size`/`class` remain. (2026-08-18)
- [x] Implicit-field labels (`label="..."` without `asp-for`): `FieldInputBaseTagHelper` mints
      `id="sa-{GetUniqueId(context)}"` on the host when it will render the label and no id
      was supplied, and the auto label gets `for` = the host id (generated or authored) via
      a new protected `LabelForId`. Input/Switch/Toggle/Select/Textarea carry the id on the
      control; InputOtp moves it onto its real `<input>`; Slider/ToggleGroup keep it on the
      host (label association there needs `aria-labelledby`, out of scope). (2026-08-18)
- [x] `LinkButtonTagHelper` / `LinkItemTagHelper` (and every other anchor helper) delegated
      to the framework anchor helper unconditionally, so no `href`/`asp-*` linked to the
      current page. `StellarAdminAnchorTagHelperBase` now owns the delegation
      (`HasRouteTarget` + `ApplyRouteAttributesAsync`, replacing the block copy-pasted in 8
      helpers) and skips it without a routing target (`asp-fragment/host/protocol` alone
      are modifiers, as `DropdownMenuItemTagHelper` already defined). (2026-08-18)
- [x] Tabs: the inner `tabs-list` div inherited the outer's whole class string (`sa-tabs
      group/tabs` + author class); author class now on the host only. `IsActiveRoute()`
      also compares `asp-route-*` values against the request's route values / query, so
      links differing only by a route value no longer all mark active (a link with fewer
      route values than the request still matches — `is-active` remains the override).
      (2026-08-18)
- [x] `sel-collapsible` rendered into shadow DOM and cached invokers once via `setTimeout`;
      now light DOM (no `render()`), invokers resolved by query on every state sync so
      late-added triggers get `aria-expanded` too. (2026-08-18)
- [x] `sel-sidebar` clobbered `collapsible="none"`; `#syncState` now returns early when the
      sidebar has no `data-collapsible-config`. (2026-08-18)
- [x] `dialog()` resolved `{confirmed: true}` for any close whose `returnValue` wasn't
      `"cancel"`. Decision (Jerrie): an empty `returnValue` is a dismissal — only a non-empty
      value other than `"cancel"` confirms. `alertDialog()` unaffected in practice
      (`sa-alert-dialog-action` defaults `value="confirm"`). js-dialog docs list the new
      condition. (2026-08-18)
- [x] Sheet close button now a `<button type="button">` rendered via
      `ButtonRenderingHelper.RenderAttributes` with an `sr-only` "Close". (2026-08-18)
- [x] `sa-input-group-button` sets `type="button"` unless the author supplied a type.
      (2026-08-18)
- [x] Radio demos (`Pages/Radio/*`) reused `id="bed-*"` and `name="bed-preference"` across
      demos, so labels toggled radios in other demos and all demos formed one group. Each
      demo now has its own name (and id prefix for explicit-label demos; implicit-label
      demos rely on generated ids). `Pages/Field/_Radio.cshtml` radios gained a shared
      `name`. (2026-08-18)

## Phase 3 — demo / docs sample fixes (DONE 2026-08-18)

- [x] `Pages/Card/_Default.cshtml` `size="default"` compiled to C# `default` (null) —
      attribute dropped (the demo shows the default size). (2026-08-18)
- [x] `Pages/Field/_Implicit.cshtml` `Enumerable.Range(2024, 2029)` → `Range(2024, 6)`.
      (2026-08-18)
- [x] `defaultValue="..."` (React leftover, not a property) → `value` in Dialog/Sheet/Js
      dialog demos (5 files, 10 inputs). (2026-08-18)
- [x] `Pages/Table/_Select.cshtml` leaked its `@{ var ... }` block into the exported
      snippet — a column-0 `<!-- code begin -->` now follows the block. (2026-08-18)
- [x] `Pages/Collapsible/_Intro.cshtml` `@@ibnbattuta` — **left as is**: the exported
      snippets are ```` ```razor ```` and hold Razor (`@foreach`, enum values), so `@@` is
      what a reader must paste into a `.cshtml`; unescaping would make the snippet fail
      to compile. Same holds for the other `@@` uses (InputGroup, Popover, Sidebar).
      (2026-08-18)
- [x] Regenerated the exported demos (`DocsSamplesGenerator`): 336 partials, new
      fingerprinted JS bundle, theme CSS and `_include` snippets reflect Phases 1-3.
      (2026-08-18)
- [x] (Jerrie's request) New Collapsible "Expanded by default" example
      (`Pages/Collapsible/_Expanded.cshtml`, generator entry, docs section) showing a
      collapsible without `hidden` rendering open, with the trigger's `aria-expanded`
      already `true`. (2026-08-18)
