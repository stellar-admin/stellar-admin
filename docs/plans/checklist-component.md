# Checklist component

Status: extracted, awaiting review (2026-10-20).

Extract the home page's setup checklist (`Areas/StellarAdmin/Views/Home/_SetupChecklist.cshtml` and `_SetupStep.cshtml`) into an `sa-checklist` component in `StellarAdmin.UI`, with `sa-checklist-item` rows. Each item has a status (such as done, to do, warning or optional), a title and description, an action, and details that expand below it. The progress bar and the card around it stay separate, so pages compose them as they need. The home page then uses the component.

Read the current setup checklist and similar `sa-*` components first, then propose the tag helpers, statuses and API to Jerrie before implementing. Follow the component workflow for samples, website docs and consumer references.

## API decisions (2026-10-20)

Explored in `sandbox/html/steps.html` (one component for checklists and timelines), `sandbox/html/checklist-timeline.html` (the split) and `sandbox/html/checklist.html` (ten checklist scenarios with their Razor). The checklist and a timeline are separate components; a timeline is a later plan, and a horizontal stepper or wizard is out of scope for both.

- `sa-checklist`: the list, with a small size and `divider` (`ChecklistDivider { Inset, FullWidth, None }`, `Inset` by default: rules between rows start at the text column).
- `sa-checklist-item` and `sa-checklist-item-link` (the row as an anchor, as `sa-item-link`), with `status`, `title` and `description` shorthand attributes.
- `ChecklistStatus { Todo, InProgress, Done }`, `Todo` by default. A built-in status draws its icon and hidden status text; a done title keeps its colour.
- Child parts `sa-checklist-item-title` (with `muted`), `sa-checklist-item-description` and `sa-checklist-item-status`, named after the item as `sa-accordion-item-title` is. Giving an attribute and its own child part together throws. `sa-checklist-item-status` holds any content (an `sa-icon` for warning, failed or optional) and requires `label`, the hidden text a screen reader announces; it sets no status semantics.
- `sa-checklist-item-actions` for the trailing content (buttons, a chevron, a date and an avatar), named like `sa-item-actions`.
- No details or content part. Anything else in an item (an `sa-collapsible` toggled by an `sa-button`, a nested checklist) lays out below the text. No whole-row toggle, no checkbox ticking and no group part: groups are several checklists with ordinary headings.

Visuals approved: ring-and-dot in-progress icon, outline circle check for done, muted ring for to do, 20px icons, 3-unit row padding, accent colour on the in-progress icon only.

## Extraction (2026-10-20)

- Library: `TagHelpers/Checklist/` (`ChecklistTagHelper`, `ChecklistItemTagHelper`, `ChecklistItemLinkTagHelper`, the four part tag helpers, `ChecklistSize`, `ChecklistDivider`, `ChecklistStatus`) and `Client/css/components/checklist.css`. The built-in status icons are the new `SemanticIconRole` members `ChecklistTodo`, `ChecklistInProgress` and `ChecklistDone`, mapped in the three packs to `circle`, `circle-dot` and `circle-check`. The link row renders its `<a>` inside an `<li class="sa-checklist-row">`.
- Samples: `docs/DocsSamples/Pages/Checklist/`, the ten prototype scenarios plus a dividers demo. Website: `content/docs/ui/components/checklist.mdx` and the exported demos. Consumer reference: `skills/stellar-admin-ui/references/components/checklist.md`, with a hand-written composition section.
- The admin home page's setup checklist uses the component (`_SetupChecklist.cshtml`, `_SetupStep.cshtml`); `HomePageTests` select the checklist parts.

Verified on 2026-10-20: the UI library builds with no warnings; `StellarAdmin.UI.Tests` (663) and `StellarAdmin.IntegrationTests` (576) pass; the DocsSamples page has no unresolved `sa-*` tags and was checked in light and dark mode, at mobile width, and in the Ops and Soft themes; the admin home page was checked at desktop and mobile width; the website passes `pnpm lint`, `pnpm types:check` and `pnpm build`; `SkillsGenerator --check` reports no drift. Not run: `util/theme-check/contrast.mjs`.

Decided: keep `circle-dot` for in progress, although its dot is smaller than the prototype's.

Not yet done: Jerrie's review.
