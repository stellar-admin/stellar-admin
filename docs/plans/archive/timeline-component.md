# Timeline component

Status: completed (2026-10-20).

An `sa-timeline` component in `StellarAdmin.UI` for a list of events (booking history, activity feeds, audit logs, itineraries), separate from the checklist, as decided in the [checklist plan](../checklist-component.md).

## API decisions (2026-10-20)

Explored in `sandbox/html/timeline.html` (eleven scenarios with their Razor). A survey of other libraries (Ant Design, MUI, Chakra, Primer, PrimeVue: no per-item status; Mantine, Nuxt UI: one active index) led to no status.

- `sa-timeline`: an `ol` with `size` (`TimelineSize { Default, Small }`), `time-placement` (`TimelineTimePlacement { End, Below, Start }`, `End` by default), `marker` (`TimelineMarker { Dot, Ring, Number }`, what an item with no media shows) and `media-style` (`TimelineMediaStyle { Disc, Plain, Ring }`, `Disc` by default).
- `sa-timeline-item` with `title`, `description` and `time` shorthand attributes, `rail` (`TimelineRail { Solid, Dashed, None }`, the rail leaving the item) and `current`: the current item, the items before it and the rails between them take the accent; with several current items the accent runs to the last. The current item gets `aria-current="step"`. No newest-first progress.
- Child parts `sa-timeline-item-title` (with `muted`), `-description`, `-time` (a `<time>` with a `DateTimeOffset` `datetime`), `-media` (an icon or avatar in place of the marker, with an optional `label` read before the title) and `-actions`. Giving an attribute and its own child part together throws. Anything else in an item lays out below the text; no group part.

Visuals approved: rail in the line colour at 2px, 5 units between items, sunken disc for media icons.

## Extraction (2026-10-20)

- Library: `TagHelpers/Timeline/` (`TimelineTagHelper`, `TimelineItemTagHelper`, the five part tag helpers and the five enums) and `Client/css/components/timeline.css`. An item without media renders an empty, `aria-hidden` `.sa-timeline-item-marker`; the stylesheet draws the dot, ring or number.
- Samples: `docs/DocsSamples/Pages/Timeline/`, the eleven prototype scenarios plus marker style, media style and time placement demos. Website: `content/docs/ui/components/timeline.mdx` and the exported demos. Consumer reference: `skills/stellar-admin-ui/references/components/timeline.md`, with a hand-written composition section.

Verified on 2026-10-20: the UI library builds with no warnings; `StellarAdmin.UI.Tests` (681) pass; the DocsSamples page has no unresolved `sa-*` tags and was checked in light and dark mode and at mobile width; the website passes `pnpm lint`, `pnpm types:check` and `pnpm build`; `SkillsGenerator --check` reports no drift. Jerrie reviewed the component and checked it visually on 2026-10-20. Also on 2026-10-20: `util/theme-check/contrast.mjs` on the DocsSamples timeline page passes (0 of 10,432 texts below contrast, across the base theme, the Ledger, Ops and Soft fixtures and 12 random themes in light and dark mode). For review, not failures: the media disc separates from a card at 1.08 in light mode (below the 1.1 guide, as approved), and the samples' `rounded-md` change list reads as a pill in Soft. `screenshots.mjs` captures of the page in each theme fixture looked right.
