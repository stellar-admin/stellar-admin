# OSS docs: content sweep

## Code audit — 2026-09-18

Current status: **completed**. Website routing-attributes include and component accessibility sections exist; inspected samples use the Voyager copy and corrected radio/input markup. The former source paths now resolve under `docs/DocsSamples`.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

**Index status:** completed. **Indexed:** 2026-09-05. Historical record; completion is recorded in the phase/progress notes below. Deferred items require a separately scoped task.

Status: **COMPLETE 2026-08-19** (code follow-ups in review notes §2.3) — decisions: English pagination labels, sentence case, accessibility on the flagged set.

Chunk #4 of the follow-up from the 2026-08-17 review of the free `StellarAdmin.TagHelpers`
docs (review notes: session artifact "oss-review-notes", §2.1 and §5 item 4). Bug sweep
(`oss-bug-sweep.md`) and new pages (`oss-docs-new-pages.md`) are done. Scope: copy and
structure of the *existing* component pages and their demo partials. Out of scope: new
examples for undocumented features (#5), code changes to tag helpers (#6), feature
decisions (#7).

Paths: `docs/` = `website/content/docs/tag-helpers/components/`, `Pages/` =
`stellar-admin-pro/docs/DocsSamples/Pages/`, `TH/` = `stellar-admin/src/StellarAdmin.TagHelpers/`.

## Ground rules

- Demo partial edits are **copy-only** (strings, names, placeholder values, `[Display]`
  names). Markup, attributes and the *shape* of every demo stay as they are, so what the
  example demonstrates (long text overflow, three list items, disabled option, ...) is
  preserved. The one exception is Phase 5 (accessible names on icon-only demo buttons),
  called out there.
- All copy follows the Voyager Travel theme (travel agency; destinations, bookings,
  travellers, itineraries, flights, hotels; staff names like the existing Ibn Battuta /
  Marco Polo explorers). "..." not "...".
- Every partial change is followed by a generator run and a spot-check of the regenerated
  `_include`/demo output; every `.mdx` change is checked in the running website (port 3000).
- Stop-and-review after each phase; commit on request.

## Phase 1 — Voyager Travel re-theme of shadcn-copy partials

Partials flagged in the review (verify the list against the current sources first; some
may already have been re-themed):

| Component | Partials | Off-theme copy |
|---|---|---|
| Card | `_Default`, `_Small`, `_Login`, `_MeetingNotes`, `_WithImage` | shadcn login card, meeting notes, generic placeholders |
| Separator | `_Horizontal`, `_Vertical`, `_InList` | "StellarAdmin.UI" title + blurb, Blog/Docs/Source |
| Dialog | `_Intro`, `_ReturnFormValue`, `_ScrollableContent`, ... | "Edit profile", "delete your account from our servers", Lorem ipsum |
| Sheet | `_Intro`, `_ReturnFormValue`, sides | "Edit profile" |
| Alert | `_Basic`, `_Icons`, `_Shorthand`, `_Actions` | shadcn alert copy |
| Progress | file names | shadcn file names |
| Spinner | `_InEmpty` | shadcn empty state |
| Popover | `_ButtonGroup` | Copilot |
| Empty | all five | shadcn empty states |
| Item | nearly all; `_HeaderAndFooter` "React Native" | shadcn item copy |
| Avatar | initials "DU" (DuneUI leftover); "Battutta" typo (also `Pages/Js/Dialog/_ManualResult`) | |
| Badge | `_LongText` | |
| ButtonGroup | Copilot, GPU Size, `$ € £`, "Button / Another Button" | |
| InputGroup | `https://x.com/shadcn`, `@company.com`, `console.log` | |
| Kbd | "Voice Enabled" | |
| Checkbox | group "Show these items on the desktop" | |
| Field | Finder/iCloud, ChatGPT notifications, "123 Main St", HR departments, "super-dooper error" | |
| Switch | "Email notifications" | |
| Select | `_Disabled` (Apple/Banana) | |
| Pagination | `_CustomContent` (Afrikaans labels with no explanation) | see decision below |
| Collapsible | `@@ibnbattuta` handle | keep the handle (it's the Ibn Battuta theme); only re-theme the repo names inside |

Also: any `.mdx` prose that quotes the old copy ("Click 'Edit profile'...") is updated in
step; the "Battutta" typo is fixed everywhere (`grep -rn Battutta`).

Deliverable: edited partials, regenerated `website/public/demo` + `_include`, screenshot
spot-check of ~6 demos (Card login, Empty, Item, ButtonGroup, Field, Pagination custom).

## Phase 2 — Prose gaps and copy nits on existing pages

- One sentence of prose under every `###` example subsection that is currently heading +
  `<Demo>` only, and under every empty `## Usage` (review list: Card, Separator, Group,
  Stack, Button icon examples, Badge, Empty, Kbd, Item, Skeleton, Spinner, Progress, Select,
  Textarea, ButtonGroup, Breadcrumb, Collapsible, Pagination, Tabs...). Each sentence says
  what the example shows and, where useful, which attribute drives it — no filler.
- `radio.mdx`: add the missing `## Examples` heading (examples currently sit under Usage).
- Copy nits from the review: `alert.mdx` "Destructive" prose is the custom-CSS text;
  `skeleton.mdx` "shimmering" (CSS is `animate-pulse`); `popover.mdx` JS-API prose pasted
  from Tooltip; `js-dialog.mdx` heading `dialog(element)` vs table
  `dialog(selector, options?)`; `js-alert-dialog.mdx` `confirm` signature (takes `data?`);
  `tooltip.mdx` "Limitations" duplicates the supported list; `input.mdx` "rules ...
  applies"; `field.mdx:59` "any if the various"; `textarea.mdx:22` missing full stop;
  `input-otp.mdx` "single hidden `<input>`" (it's transparent/overlaid and focusable).
- Slider / Input OTP / Toggle Group pages say "add the `disabled` attribute" but those bind
  `bool?`, so bare `disabled` is a Razor compile error: change to `disabled="true"` and say
  why. (Making the binding consistent is a #6 item; the docs must match today's code.)
- Unicode ellipsis in `dropdown-menu.mdx:90` and `Pages/DropdownMenu/_ClickEvents.cshtml:45`
  (`TH/DropdownMenuItemTagHelper.cs:11` and `readme.md:105` are code/infra → #2/#6).

## Phase 3 — Heading case

Normalise `###` example headings to **sentence case** across all component pages
("Custom separator", "Icons only", "Active item", "URL", "With select", "Icon on the left",
"Manual validation"). Sentence case is what the newer pages and Jerrie's own edits use
("Theme stylesheets", "Expanded by default"). Acronyms keep their case (URL, OTP, JS).
`## Usage` / `## Examples` / `## API Reference` stay as they are (page template).

Mechanical; one pass with a script over `docs/*.mdx`, then a diff review.

Also in this phase (added 2026-08-18 at Jerrie's request): a **semicolon sweep** of the prose on
all `tag-helpers/**/*.mdx` pages (not code, tables or CSS). Clauses joined with a semicolon become
separate sentences, or are joined with "but"/"and" where the second clause depends on the first
(e.g. "It has no attributes of its own but forwards any standard global HTML attributes ..."). The
Phase 2 prose was already de-semicoloned.

## Phase 4 — Shared routing attributes include

**Status: DONE 2026-08-18.** `components/_shared/routing-attributes.mdx` holds the table (incl.
`asp-all-route-data`, verified in `StellarAdminAnchorTagHelperBase`) and is included from
breadcrumb, pagination, tabs, link-button, item, dropdown-menu and sidebar (the last two had
verbatim copies the plan missed); each page keeps its own intro sentence.
Renders verified headless on all five pages; `_shared/...` is not a routable page (404).

The routing `<TypeTable>` (asp-action / controller / area / page / page-handler / route /
route-* / protocol / host / fragment) is repeated verbatim in `breadcrumb.mdx`,
`pagination.mdx` (under `<sa-pagination-link>`), `tabs.mdx`, `link-button.mdx`, `item.mdx`.
Create one hand-authored partial and `<include>` it from the five pages.

Location: `docs/_shared/routing-attributes.mdx` — **not** `_include/`, which the generator
wipes on every run (`Generator.cs:151-160`). `source.config.ts` only matches
`*/*.mdx` and `*/*/*.mdx`, so a `_shared/` folder at the same depth as `_include/` is
excluded from pages automatically.

Content: the union of the five copies plus `asp-all-route-data` (supported by the framework
anchor tag helper we delegate to; verify against `TH/StellarAdminAnchorTagHelperBase.cs`
before listing it). One-line intro sentence per page stays page-specific.

## Phase 5 — Accessibility sections

Add an `## Accessibility` section (between Examples and API Reference, matching shadcn's
page order) to the pages where it carries information:

- Overlays: Dialog, Alert Dialog, Sheet, Tooltip, Popover, Dropdown Menu — what the tag
  helpers wire (`aria-labelledby`/`aria-describedby` from title/description, `role`,
  focus management via native `<dialog>`, `aria-expanded` on triggers, Escape/arrow-key
  behaviour from `sel-dropdown-menu`) and what the author must supply (a title, an
  accessible name for icon-only triggers).
- Navigation: Tabs, Pagination, Breadcrumb, Sidebar — `aria-current`, `aria-label` on the
  nav landmark, `sr-only` text on icon-only prev/next links, sidebar trigger name.
- Forms: Field, Checkbox, Radio, Switch, Slider, Input OTP, Select — label association
  (now real after bug-sweep #6/#8), `aria-invalid` + `aria-describedby` for errors and
  descriptions, keyboard behaviour of slider/OTP.
- Button / Link Button / Toggle / Button Group: icon-only buttons need `aria-label` or
  `sr-only` text; toggle `aria-pressed`.

Every claim is verified against the tag helper / web-component source before it is written;
where the component does *not* do something (e.g. icons are not `aria-hidden`, Dialog has no
close button), the section says what the author should do today, and the code-side gap goes
to the #6 list rather than being papered over.

**Markup exception:** demos with icon-only buttons and no accessible name (Button/LinkButton
`_IconOnly`, ButtonGroup, Kbd `_Tooltip`, Empty, InputGroup `_Intro`/`_Textarea`,
Pagination prev/next) get an `aria-label` or `<span class="sr-only">` so the docs don't
demonstrate the anti-pattern the new sections warn about. That's an attribute/child added,
not a shape change; listed here so it's explicit.

## Decisions needed (Jerrie)

1. **Pagination `_CustomContent`** — the Afrikaans "Vorige/Volgende" labels demonstrate
   custom link content. Proposal: keep the point (custom text) but use English travel copy
   ("Earlier departures" / "Later departures") — or keep the Afrikaans and add one sentence
   of prose explaining it's a localisation example. Default if you don't say: English.
2. **Heading case** — sentence case as proposed, or Title Case?
3. **Accessibility scope** — the page set above, or every component page (a short section
   on Badge/Skeleton/Separator would mostly say "decorative, nothing to do")? Default:
   the set above.

## Phases (tracking)

1. [x] Re-theme partials + regenerate + spot-check (2026-08-18). 68 DocsSamples files + `field.mdx`/`switch.mdx`
   model snippets + `card.mdx` heading; 340 demos regenerated; 11 demos screenshot-checked.
   Deviations from copy-only, all in DocsSamples/docs only: model property renames so
   `asp-for` names match the new copy (`Show*`/`SyncMapWithItinerary`, `Enable{Booking,Flight}*`,
   `InsuranceCover`, `TravelStyle`) with `DocsStatic`/`Index` initialisers updated; matching
   `id`/`for`/`name`/`value` renames on the explicit Field checkbox/radio demos; two icon
   names changed to fit the copy (`file-braces-corner`->`file-text` in InputGroup textarea,
   `circle-dashed`->`corner-down-left` in Kbd). Left as is (self-descriptive, not shadcn copy):
   Card header/footer-with-border, Item `_Components`/`_Sizes`/`_Variants`, ButtonGroup `_Sizes`,
   Alert `_Destructive`.
2. [x] Prose gaps + copy nits + `disabled="true"` + ellipses (2026-08-18). 102 bare sections (27 pages)
   got one sentence each; `radio.mdx` gained `## Examples`; all listed nits fixed (alert Destructive,
   skeleton pulse, popover `source`, `dialog(selector, options?)`, alertDialog `confirm(data?)`,
   tooltip limitation -> link, input/field/textarea typos, OTP overlay wording); Slider/OTP prose
   says `disabled="true"` and the Slider/OTP/Toggle Group item/Dropdown item TypeTables say `bool?`;
   ellipsis fixed in `dropdown-menu.mdx` and `DropdownMenu/_ClickEvents.cshtml` (demo regenerated;
   svg-attribute-order churn in the other demo files reverted).
3. [x] Heading case + semicolon sweep (2026-08-18). 83 `###` headings on 33 pages moved to sentence
   case (acronyms and "Tag Helpers" kept; "In Empty"/"In List"/"In InputGroup" -> "In an empty
   state"/"In a list"/"In an input group"; no in-page anchors affected). All ~50 prose semicolons on
   `tag-helpers/**/*.mdx` (incl. TypeTable descriptions) split into sentences or joined with
   "but"/"and"; the "no attributes of its own; it forwards ..." pattern is now "... of its own but
   forwards ...". Zero prose semicolons remain outside code.
   Follow-up (Jerrie, during proofread): the per-tag "no attributes of its own / forwards any ..."
   boilerplate was repetitive (68 + ~79 occurrences on 42 pages). Added a **Conventions** page
   (`docs/conventions.mdx`, Getting Started after Installation, linked from Installation) covering
   attribute pass-through, custom classes (`sa-*` + `data-slot` kept, user classes appended),
   model binding, links/routing (enum-valued and `bool?` attribute sections dropped: plain Razor behaviour, not StellarAdmin's); then
   removed the boilerplate from all API Reference sections (156 paragraphs, 46 pages), keeping only
   the specific exceptions (Table `class` vs container, Accordion `<details>` attributes, Alert Dialog
   `closedby` default, Sidebar wrapper `id`, Icon `<svg>` attribute precedence).
4. [x] Shared routing include (2026-08-18). `components/_shared/routing-attributes.mdx` (+ `asp-all-route-data`)
   included from breadcrumb/pagination/tabs/link-button/item/dropdown-menu/sidebar; verified headless.
5. [x] Accessibility sections + icon-only demo names (2026-08-19). `## Accessibility` on 22 pages
   (overlays 6, nav 4, forms 7, buttons 5), all claims audited against source; 72 icon-only demo
   buttons in 18 DocsSamples partials got `aria-label`, demos/snippets regenerated. Code gaps logged
   in review notes §2.3; Pagination hidden labels + Slider thumb naming deferred to a code follow-up.
