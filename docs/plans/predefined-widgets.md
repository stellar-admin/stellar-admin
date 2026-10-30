# Predefined widgets

Status: proposed (2026-10-30). Moved out of the [widgets](widgets.md) plan, where it was phase 4.

Ship ready-made widgets, so a developer can add a common widget without writing its partial view. They're built on the existing widget mechanism (a handler and a partial, see [widgets](widgets.md)): the developer still writes a handler that loads the data, and StellarAdmin supplies the partial and the model type it renders.

Candidates, in rough order:

- **Stats.** One or more stats in a card: a label and a value, optionally a change against an earlier period (up or down, good or bad), a short note, and a sparkline or a goal. Several stats can share one divided card. The handler returns a model StellarAdmin defines. The AdminPlayground's Price summary (`_ProductPriceSummaryWidget`) is a hand-written example of this.
- **Records list.** A short list of a resource's records, such as recent sign-ups, using the resource's title and reference information, with links to their view pages. Could follow [configured-row-details](configured-row-details.md).
- **Charts and the activity heatmap.** Wait for a chart component in StellarAdmin.UI; the prototypes draw them with inline SVG, which isn't a component.

A callout doesn't need a predefined widget: a widget without a handler that renders `<sa-alert>` is a few lines.

Starting points: the prototypes in `sandbox/html/widgets-*.html` (the gallery shows every widget kind and state, and the index page's variants A and C show stats), the candidate CSS in `sandbox/html/widgets/widgets.css`, and `src/StellarAdmin/Widgets/`.

To be filled out before implementing: the API for adding a predefined widget (for example `widgets.AddStats<THandler>(name)`), the model types, and whether the stats card belongs in StellarAdmin.UI as a component.
