# Command

Status: **active** — Phase 2 complete, awaiting review; Phase 3 not started. Last updated: 2026-09-28.

Port shadcn's Command component (`cmdk`-based command palette) into `StellarAdmin.TagHelpers`, following the [port-shadcn-component](../../../.agents/skills/port-shadcn-component/SKILL.md) workflow. Work proceeds in phases with a review checkpoint after each; approval of one phase does not authorize the next.

## Resuming

Read this file, then check the current code under the paths in [Source paths](#source-paths) before continuing. Update the phase table and the phase log when a phase finishes or stops partway.

| Phase | Scope | Status |
| --- | --- | --- |
| 1 | Research and API proposal | ✅ approved 2026-09-28 |
| 2 | Tag helpers, structural CSS, custom-theme coverage, static DocsSamples demo | ✅ implemented 2026-09-28, awaiting review |
| 3 | `sel-command` web component — client filter mode, keyboard, selection | ☐ |
| 4 | Filter `None` mode, list mutation re-scan; htmx server-search demo in ComponentPlayground | ☐ |
| 5 | Remaining demos, website docs, skills reference, tests, handover | ☐ |

Dashboard integration is a separate task after Phase 5.

## Approved design

### Decisions

- **Filter modes:** `Client` (default) and `None`. In `None` the component never shows or hides items; the developer supplies them (for example an htmx swap of the list contents). Mixed mode (client filtering plus server fetch) is out of scope.
- **The library has no htmx dependency.** Server search works through attribute pass-through on the input, a stable swap target (the list), and a list `MutationObserver`.
- **Dialog opening is the developer's job.** `sa-command-dialog` wraps `sa-dialog`, so the existing mechanisms apply: an invoker button (`commandfor` + `command="show-modal"`), `dialog.showModal()`, or `window.stellarAdmin.dialog(...)`. No `open-shortcut` attribute and no new JS wrapper; a ⌘K listener appears only in a demo.
- **htmx demo lives in `sandbox/ComponentPlayground`**, which already loads htmx. DocsSamples does not get htmx.
- **No loading component** (upstream shadcn does not wrap `Command.Loading`). Loading state is left to `aria-busy` / htmx's `htmx-request` class.

### Tags

| Tag | Renders | Notes |
| --- | --- | --- |
| `sa-command` | `<sel-command>` › `div[data-slot=command].sa-command` | `filter` (`CommandFilter`: `Client` default, `None`), optional `label` (visually hidden label), `loop`. |
| `sa-command-input` | `div[data-slot=command-input-wrapper]` › input group › `input[role=combobox]` + search icon addon | Unmatched attributes (`hx-*`, `name`, `placeholder`, …) land on the `<input>`. Input/list IDs and `aria-controls`/`aria-expanded`/`aria-autocomplete=list` rendered server-side via an internal `CommandContext`. `autocomplete=off`, `spellcheck=false`. |
| `sa-command-list` | `div[data-slot=command-list][role=listbox]` | The htmx swap target in `None` mode. |
| `sa-command-empty` | `div[data-slot=command-empty][role=presentation]`, hidden | Web component shows it when no item is visible. |
| `sa-command-group` | `div[data-slot=command-group][role=presentation]` › `div[cmdk-group-heading]` + `div[role=group]` | `heading`; group labelled by heading via `aria-labelledby`. `cmdk-group-heading` is required by the shipped theme selector `**:[[cmdk-group-heading]]`. |
| `sa-command-item` | `div[data-slot=command-item][role=option]` | `value` (defaults to text content), `keywords`, `disabled` (`data-disabled="true"`, `aria-disabled`), `checked` (`data-checked="true"`). Renders upstream's check indicator. |
| `sa-command-link-item` | `a[data-slot=command-item][role=option]` | Anchor variant; full `asp-*` routing via `StellarAdminAnchorTagHelperBase`. |
| `sa-command-separator` | `div[data-slot=command-separator][role=separator]` | |
| `sa-command-shortcut` | `span[data-slot=command-shortcut]` | |
| `sa-command-dialog` | `sa-dialog` composition with class `sa-command-dialog` | `title` / `description` visually hidden (upstream defaults "Command Palette" / "Search for a command to run..."), `show-close-button` default off. |

### `sel-command` behaviour

- Focus stays in the input. ↑/↓, Home/End, Ctrl+N/P/J/K move the active item via `aria-activedescendant` + `data-selected`; `loop` wraps. Pointer hover sets the active item. Disabled items are skipped.
- Item IDs are assigned client-side when missing, so server-rendered fragments need none.
- Enter or click calls `item.click()` then dispatches a bubbling `itemselect` `CustomEvent` with `{ value }` from the item. Links navigate; `hx-*` on items fire unaided.
- `Client` mode: fuzzy-score value + keywords, hide non-matches, sort by score, sort groups by best score, hide empty groups, hide separators while a query is active, toggle the empty state.
- Both modes: `MutationObserver` on the list re-scans items after any change and activates the first enabled visible item.

### Upstream deviations

- ARIA/roles mirror `cmdk`'s output; other `cmdk-*` attributes are not emitted (only `cmdk-group-heading`, needed by theme CSS).
- Group sorting by best score is kept in `Client` mode; may be dropped if DOM reordering proves problematic (record here if so).

## Phase 1 findings

- Upstream source: `apps/v4/registry/bases/base/ui/command.tsx`. Structural (non-`cn-*`) classes: root `flex size-full flex-col overflow-hidden`; dialog `top-1/3 translate-y-0 overflow-hidden p-0`; input `outline-hidden disabled:cursor-not-allowed disabled:opacity-50`; list `overflow-x-hidden overflow-y-auto`; item `group/command-item data-[disabled=true]:pointer-events-none data-[disabled=true]:opacity-50 [&_svg]:pointer-events-none [&_svg]:shrink-0`; item indicator `ml-auto opacity-0 group-has-data-[slot=command-shortcut]/command-item:hidden group-data-[checked=true]/command-item:opacity-100`.
- All eight generated shadcn themes already ship `.sa-command`, `-dialog`, `-input-wrapper`, `-input-group`, `-input-icon`, `-input`, `-list`, `-empty`, `-group`, `-separator`, `-item`, `-shortcut`. Do not re-run ThemeGenerator.
- The seven custom themes (Aurora, Concourse, Ice, Ledger, Meridian, Observatory, Parallax) have no command rules; each needs review and `util/theme-coverage/coverage.json` entries in Phase 2.
- Reusable pieces: `InputGroup`, `Dialog` (`sel-dialog`), `Kbd`, `Icon`; `sel-dropdown-menu.ts` has keyboard-navigation patterns. Existing event naming is lowercase (`checkedchange`, `valuechange`).

### Phase 2 implementation notes

- `sa-command` renders the `<sel-command>` element itself (as `sa-carousel` does) rather than wrapping an inner div. It emits `data-filter` and `data-loop`; IDs default to `sa-command-{uniqueId}` with `-input`, `-list`, `-label` suffixes published through `CommandContext`.
- Heading IDs use `GetUniqueId`, so groups rendered repeatedly from one source tag need a `UniqueIdDiscriminator` or explicit markup if heading IDs must stay unique.
- New public semantic icon role `SemanticIconRole.Search` (all three packs map it to `search`). Per Jerrie, new roles are appended at the end of the enum with implicit values rather than kept alphabetical with pinned numbers; `AccordionIndicator` was moved to the end too, removing its explicit `= 23`. All existing values are unchanged (`AccordionIndicator` 23, `Search` 24). The item check indicator reuses `MenuItemSelected`.
- Item attributes: `data-value`, `data-keywords` (free text), `data-disabled="true"`, `data-checked="true"`, `aria-selected="false"`, `tabindex="-1"`. Phase 3 must toggle `data-selected`/`aria-selected`.
- `sa-command-dialog` renders its own `<dialog>` (same shell as `sa-dialog`) with a visually hidden header and optional close button using the theme's `.sa-dialog-close` rule.
- Deviation: upstream positions the dialog with `top-1/3 translate-y-0` over a translate-centred dialog. Our native dialog is centred by `inset-0 m-auto`, so `.sa-command-dialog.sa-dialog-content` adds `top-1/3 bottom-auto mt-0`; `translate-y-0` is kept though inert.
- Custom themes: each of the seven gets one appended `@layer components.theme` block composing that theme's menu surface, rows, labels, separators and shortcuts with a flat input-group field (no permanent focus ring, since focus stays in the field). `data-selected` replaces hover/focus feedback. Aurora, Meridian, Observatory and Parallax add their menu keyboard-focus bar (`inset 2px 0 0 var(--primary)`) to the selected row because their hover fill alone is nearly invisible in dark mode; Ice keeps its accent bar. Record these inferences in the theme specifications during Phase 5.
- Demo layout: `size-full` stretches the root in flex-column parents with a definite height (as upstream); the intro demo adds `h-auto`.

## Source paths

- Tag helpers: `src/StellarAdmin.TagHelpers/TagHelpers/Command/` (`CommandTagHelper`, `CommandInputTagHelper`, `CommandListTagHelper`, `CommandEmptyTagHelper`, `CommandGroupTagHelper`, `CommandItemTagHelper`, `CommandLinkItemTagHelper`, `CommandSeparatorTagHelper`, `CommandShortcutTagHelper`, `CommandDialogTagHelper`, `CommandFilter`, `CommandContext`, `CommandRenderingHelper`).
- Icon role: `src/StellarAdmin.Core/Icons/SemanticIconRole.cs` and the three `*IconPack.cs` files; reference table in `skills/stellar-admin-tag-helpers/references/icons.md`.
- Structural CSS: `src/StellarAdmin.TagHelpers/Client/css/components.css` (`.sa-command*`).
- Custom theme CSS: final block of each of `aurora`, `concourse`, `ice`, `ledger`, `meridian`, `observatory`, `parallax` `.css` under `Client/css/themes/`.
- Coverage: `util/theme-coverage/coverage.json` (`Command`).
- Samples: `docs/DocsSamples/Pages/Command/` (`_Intro`, `_Dialog`), registered under Navigation in `Pages/Shared/_NavigationLayout.cshtml`.

## Phase log

- 2026-09-28 — Phase 1: read upstream Base UI and Radix sources, shipped theme CSS, sibling helpers and web components. API proposal approved with the decisions above. No product code changed.
- 2026-09-28 — Phase 2: tag helpers, Search icon role, structural CSS, seven custom-theme blocks, coverage entry and two static demos. Verified: TagHelpers build 0 warnings; `node util/theme-coverage/check.mjs` (57 components × 15 themes) and `check.test.mjs` (9 pass); `npm run build:css`; Core tests 67/67 and TagHelpers tests 136/136; DocsSamples on port 5206 rendered with no unresolved `sa-*` elements; headless-Chromium screenshots of the intro (static `data-selected` preview) in default, Vega dark, Ledger light/dark, Aurora, Concourse, Ice, Meridian dark, Observatory and Parallax, and the dialog in Ledger desktop and default mobile. Not yet done: interaction (Phase 3), website docs, skills examples, theme-spec notes.
