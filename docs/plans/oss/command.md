# Command

Status: **active** — Phase 3 committed (`67ee449`); Phase 4 complete, awaiting review. Last updated: 2026-09-29.

Port shadcn's Command component (`cmdk`-based command palette) into `StellarAdmin.TagHelpers`, following the [port-shadcn-component](../../../.agents/skills/port-shadcn-component/SKILL.md) workflow. Work proceeds in phases with a review checkpoint after each; approval of one phase does not authorize the next.

## Resuming

Read this file, then check the current code under the paths in [Source paths](#source-paths) before continuing. Update the phase table and the phase log when a phase finishes or stops partway.

| Phase | Scope | Status |
| --- | --- | --- |
| 1 | Research and API proposal | ✅ approved 2026-09-28 |
| 2 | Tag helpers, structural CSS, custom-theme coverage, static DocsSamples demo | ✅ approved and committed 2026-09-28 (`7daaadb`) |
| 3 | `sel-command` web component — client filter mode, keyboard, selection | ✅ approved and committed 2026-09-28 (`67ee449`) |
| 4 | Filter `None` mode, list mutation re-scan; htmx server-search demo in ComponentPlayground | ✅ implemented 2026-09-29, awaiting review |
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
- `Client` mode: fuzzy-score value + keywords, hide non-matches, sort items by score within their own container (groups keep their order), hide empty groups, hide separators while a query is active, toggle the empty state.
- Both modes: a `MutationObserver` (child-list changes anywhere in the component) re-scans items after any change made outside the component: new items get IDs, client mode filters them against the current search, the empty state is updated, and the active item is kept if still selectable, otherwise the first enabled visible item is activated.

### Upstream deviations

- ARIA/roles mirror `cmdk`'s output; other `cmdk-*` attributes are not emitted (only `cmdk-group-heading`, needed by theme CSS).
- Groups are not sorted. cmdk's `sort()` intends to reorder groups by their best item's score, but it looks groups up by `[cmdk-group][data-value="<React useId>"]` while the group's `data-value` is its heading text, so the lookup never matches and shadcn's groups keep their authored order. We match that observed behaviour (decided 2026-09-28 after Jerrie found groups jumping).

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

### Phase 3 implementation notes

- `Client/js/web-components/sel-command.ts` (registered in `stellar-admin-ui.ts`) plus `command-score.ts`, a typed port of cmdk's `command-score` fuzzy scorer. cmdk's MIT notice was added to `THIRD-PARTY-NOTICES.txt`.
- Matching text is `data-value` (else the item text without its shortcut) followed by `data-keywords`; the query is trimmed. Score 0 hides the item via `hidden` (Tailwind preflight's `[hidden]` rule is `!important`, so theme `display` rules cannot override it).
- Sorting reorders items only among the slots items occupy in their parent container, so groups, separators and the empty element never move (see [Upstream deviations](#upstream-deviations) for why groups stay put in shadcn too). The original child order of every touched container is recorded and restored when the query is cleared (cmdk leaves the sorted order in place).
- Groups are hidden only while a query is active and they have no visible item; separators are hidden while a query is active; the empty element is shown whenever no item is visible (in any mode, so a server-returned empty list shows it too).
- `data-selected="true"` is added and removed (the shipped themes key off its presence), and `aria-selected` toggles `true`/`false`. The first selectable item is selected on connect and after every filter.
- Keys: ↑/↓, Home/End, Ctrl+N/J (next), Ctrl+P/K (previous), Enter (`item.click()`). IME composition is ignored. cmdk's Alt+↑/↓ group jump and Meta+↑/↓ are not implemented.
- Scrolling adjusts only the list's `scrollTop` (never `scrollIntoView`, which would scroll the page); a group's first selectable item brings the whole group, heading included, into view. Pointer hover and click select without scrolling.
- `mousedown` on an item is prevented so focus stays in the input; items keep `tabindex="-1"` so link items stay out of the tab order.
- `itemselect` is dispatched from the item's click handler, so Enter and pointer clicks raise it exactly once; it is not cancelable.
- Addition beyond the approved spec: inside a `<dialog>`, the dialog's `close` event clears the query and resets the list, matching upstream where the dialog content unmounts on close.
- `None` mode skips scoring, hiding, sorting and separator/group toggling; the list mutation re-scan followed in Phase 4.

### Phase 4 implementation notes

- `sel-command` observes itself (`childList`, `subtree`) rather than only the list, so a developer who swaps the whole list element (`outerHTML`) is covered too. Attribute changes are not observed, so the component's own `hidden`/`data-selected`/`id` writes and htmx's settle classes never trigger a refresh. The component's own reordering is discarded with `takeRecords()` at the end of every filter pass.
- After an external mutation the active item is kept when it is still connected, visible and enabled (as cmdk does when items mount); otherwise the first selectable item is activated and scrolled into the list's view. An htmx `innerHTML` swap replaces every node, so the first result is activated.
- Remembered sort order survives mutations: containers that left the component are forgotten, removed children are dropped, and children added while a query was active are restored after the remembered ones when the query clears.
- Generated item IDs skip IDs already present in the document (for example a copy of rendered markup), so `aria-activedescendant` never points at the wrong element. Author-supplied IDs are never changed.
- The empty element lives inside the list, so a server fragment that replaces the list contents must render `<sa-command-empty>` itself; the component shows it whenever no item is present.
- No library C# changed. The input's `hx-*` attributes pass through `sa-command-input` as designed in Phase 2.
- Demo: `sandbox/ComponentPlayground/Pages/Demo/Command.cshtml` (+ `.cshtml.cs`, `_CommandResults.cshtml`), registered under Navigation in the playground layout. `filter="CommandFilter.None"`; the input uses `hx-get="?handler=Search" hx-trigger="input changed delay:200ms" hx-target="#trip-search-list" hx-sync="this:replace"`. The handler adds 250 ms of simulated latency and matches destinations and bookings with a case-insensitive `Contains`; an empty query returns four popular destinations. A small script shows the `itemselect` value. Building the playground regenerates the tracked `wwwroot/css/site.css` with the demo's utilities.

## Source paths

- Tag helpers: `src/StellarAdmin.TagHelpers/TagHelpers/Command/` (`CommandTagHelper`, `CommandInputTagHelper`, `CommandListTagHelper`, `CommandEmptyTagHelper`, `CommandGroupTagHelper`, `CommandItemTagHelper`, `CommandLinkItemTagHelper`, `CommandSeparatorTagHelper`, `CommandShortcutTagHelper`, `CommandDialogTagHelper`, `CommandFilter`, `CommandContext`, `CommandRenderingHelper`).
- Icon role: `src/StellarAdmin.Core/Icons/SemanticIconRole.cs` and the three `*IconPack.cs` files; reference table in `skills/stellar-admin-tag-helpers/references/icons.md`.
- Structural CSS: `src/StellarAdmin.TagHelpers/Client/css/components.css` (`.sa-command*`).
- Custom theme CSS: final block of each of `aurora`, `concourse`, `ice`, `ledger`, `meridian`, `observatory`, `parallax` `.css` under `Client/css/themes/`.
- Coverage: `util/theme-coverage/coverage.json` (`Command`).
- Samples: `docs/DocsSamples/Pages/Command/` (`_Intro`, `_Dialog`), registered under Navigation in `Pages/Shared/_NavigationLayout.cshtml`.
- Server-search demo: `sandbox/ComponentPlayground/Pages/Demo/Command.cshtml`, `Command.cshtml.cs`, `_CommandResults.cshtml`; nav entry in `Pages/Shared/_Layout.cshtml`.
- Web component: `src/StellarAdmin.TagHelpers/Client/js/web-components/sel-command.ts` and `command-score.ts`; registration in `Client/js/stellar-admin-ui.ts`.

## Phase log

- 2026-09-28 — Phase 1: read upstream Base UI and Radix sources, shipped theme CSS, sibling helpers and web components. API proposal approved with the decisions above. No product code changed.
- 2026-09-28 — Phase 2: tag helpers, Search icon role, structural CSS, seven custom-theme blocks, coverage entry and two static demos. Verified: TagHelpers build 0 warnings; `node util/theme-coverage/check.mjs` (57 components × 15 themes) and `check.test.mjs` (9 pass); `npm run build:css`; Core tests 67/67 and TagHelpers tests 136/136; DocsSamples on port 5206 rendered with no unresolved `sa-*` elements; headless-Chromium screenshots of the intro (static `data-selected` preview) in default, Vega dark, Ledger light/dark, Aurora, Concourse, Ice, Meridian dark, Observatory and Parallax, and the dialog in Ledger desktop and default mobile. Not yet done: interaction (Phase 3), website docs, skills examples, theme-spec notes.
- 2026-09-28 — Phase 3: `sel-command` web component and cmdk scorer port. Verified: `tsc --noEmit` clean, `oxfmt` on the new files, `npm run build:js`; DocsSamples on port 5206 driven over CDP with 52 passing checks (initial selection and `aria-activedescendant`, arrow/Home/End/Ctrl bindings, disabled skipping, loop on/off, fuzzy and keyword filtering, sorting of items and groups with order restore, empty state, separators, Enter/click `itemselect`, hover selection, focus retention, link navigation via Enter, dialog focus and reset on reopen, list-only scrolling); screenshots of the filtered and empty states (Observatory) and Vega dark. Server stopped. Not run: .NET builds/tests (no C# changed).
- 2026-09-28 — Phase 3 revision: Jerrie reported groups jumping (typing "t" moved Bookings above Destinations) and could not reproduce it in shadcn. Root cause: our port reordered groups, while cmdk's group reordering never takes effect (ID/value selector mismatch). Group sorting removed; items now sort in place within their slots. Re-verified over CDP: all checks pass, including new ones for the dialog "t" case (groups and separator keep their DOM order; Cape Town ranks first within Destinations). Server stopped.
- 2026-09-29 — Phase 4: list mutation re-scan in `sel-command`, collision-free generated item IDs, and the htmx server-search demo in ComponentPlayground. Verified: `tsc --noEmit` clean, `oxfmt`, `npm run build:js`; ComponentPlayground build 0 warnings/0 errors and CSharpier on `Command.cshtml.cs`; playground on port 5206 driven over CDP with 35 passing checks (hx attributes on the input, no unresolved `sa-*`, no client filtering before the response, server results with IDs and `aria-activedescendant`, first result activated, focus retained, stable `aria-controls`, keyboard into the second group, Enter `itemselect`, server-rendered empty state, overlapping requests settle on the last query via `hx-sync`, developer-appended item keeps the active item, removing the active item activates the first, client-mode copy filters added items against the current query and restores order with them appended); screenshots of results and empty state. Phase 3 suite re-run against DocsSamples on port 5206: 54 checks pass. DocsSamples build: 0 errors, 10 existing CS8618 warnings in unrelated sample models. Servers stopped.
