# Grid

Status: in progress (2026-10-12). Prototype, tag helpers, DocsSamples, website and skills done; Dashboard outstanding.

Give forms one public responsive grid, `sa-grid`, that works like `sa-details`: the same column and span attributes, the same breakpoints, the same CSS and the same rule for where a span goes. The Dashboard form grid moves onto it, and `sa-form-row` is removed.

## What exists

- `sa-form-row` (`TagHelpers/FormRow`, `form-row.css`): equal columns once the row is 40rem wide, stacked below; no counts or spans.
- `sa-details` (`TagHelpers/Details`, `details.css`): `columns`, `columns-sm/-md/-lg` on the list and `column-span*` on `sa-display-field`, resolved by `ResponsiveGrid`/`GridTiers`, with its own copy of the tier rules (container `details`).
- Dashboard form grid (`_FormItems.cshtml`, `_FormSection.cshtml`, `_FormGroup.cshtml`, `client.css`): private `.sa-form-grid*` markup and its own copy of the tier rules (container `form-grid`); each item sits in a cell that is a `field-group` container. Configured with `Columns` and `ColumnSpan` builders.
- Dashboard choice columns (`CheckboxGroup` editor template, `ChoiceColumnsStyle`, `client.css`): a third copy of the tier rules on `.sa-choice-columns` (container `field-group`).
- Shared already: `ResponsiveGrid` (resolution, 1–12 validation, inline `--sa-cols*`/`--sa-span*`/`--sa-rows*` variables) and the `@property` registrations in `structure.css`.
- Bug: responsive display fields in `sa-details` switch orientation at 32rem of the whole list (`details.css`), not their column, so a multi-column list puts labels beside values in narrow columns. Resolved by removing the responsive orientation (32).

## Decisions

Numbers continue the discussion of 2026-10-12.

7. Remove `sa-form-row` completely: tag helper, CSS, DocsSamples pages, website page and includes, skills reference. No deprecation period. Decided.
15. `sa-details` is not built on `sa-grid`; the two share `ResponsiveGrid`, the attribute names and one set of tier rules. Decided.
16. Superseded by 32. Fields measure their own column: inside either grid a field's responsive orientation follows the width of its column, not of the whole grid. Fixes the `sa-details` bug above. Decided; mechanism settled in the prototype (see Prototype).
17. Fields carry their own span: `column-span`, `column-span-sm`, `column-span-md` and `column-span-lg` (see 26) on `sa-field`, on every input that renders its own field (through `FieldInputBaseTagHelper`), and on `sa-display-field`. They apply inside `sa-grid` or `sa-details` and do nothing elsewhere. An input with `render-field="false"` is wrapped in a grid item (30). Decided.
18. Anything else spans through `sa-grid-item` (sections, groups, a `sa-details` in a form, arbitrary markup). `sa-grid-item` exists only in `sa-grid`; `sa-details` holds only display fields. Decided.
19. Name: `sa-grid` and `sa-grid-item`. Decided.
20. The tag helpers and the Dashboard cover the same four tiers: base (below 30rem), sm (30rem), md (40rem) and lg (56rem). The shorthands stay and match on both sides: `columns="N"`/`Columns(N)` set the md tier so forms stack in narrow containers, and `column-span="N"`/`ColumnSpan(N)` set the base tier, inherited upward and capped to each tier's columns. Decided (24–26).
21. The Dashboard's per-tier `SmallFull()`/`MediumFull()`/`LargeFull()` stay: they resolve the same as a span of 12 but state the intent. Decided.
22. Choice columns (8) are a separate follow-up after this plan; the Dashboard keeps its own choice column rules until then. Decided.
23. The website's Form Row page is removed without a redirect. Decided.
24. The narrowest tier is named Base everywhere: `GridColumnsBuilder.Base(n)` (was `Default`), `ColumnSpanBuilder.Base(n)` and `BaseFull()` (were `Default` and `DefaultFull`), `GridTiers.Base` (was `Default`), and the matching `ResponsiveGrid` parameters. Nothing is released, so the renames need no compatibility path. Decided.
25. `sa-grid` and `sa-details` get `columns-base` (1–12). Decided.
26. Span attributes take a number from 1 to 12 or `full`, at every tier: `column-span="full"`, `column-span-md="full"`, `column-span-lg="2"`. This replaces `column-span-full`, which is removed from `sa-display-field`. The attributes are strings, so an invalid value throws when the page renders. Decided.

27. Superseded by 32. Responsive display fields switch at 28rem of their own width, the same as responsive inputs in their column. Decided (prototype strip C1).
28. Nesting follows the normal rules: `columns="N"` means md, so an `sa-details columns="2"` in a grid item narrower than 40rem shows one column; authors set `columns-sm` when they nest. Decided.
29. Sections in neighbouring grid items are separated by the grid gap, not a rule, as in the Dashboard today. Decided.

30. An input with `render-field="false"` directly in `sa-grid` is wrapped in a grid item like a field, instead of carrying the span on the input element (17): several inputs render inside their own wrappers (select, checkbox), so the input element is not the grid's child. Decided.

31. The responsive layout of fields (label taking the spare width, the control at its natural width) suits nothing but small controls, and does not line up with display fields. Settled by 32.
32. Remove `FieldOrientation.Responsive` completely, from `sa-field`, `sa-display-field` and `sa-details`: it was ported from shadcn, nothing in the product used it, and the grid already collapses columns in narrow containers. Horizontal fields (switch, checkbox, radio, choice cards) are unaffected. Decided.
33. Fields and display fields directly in `sa-grid` keep their grid item, which carries the span (needed for inputs inside their own wrappers, 30) and stays a `field-group` container for content that measures its field group. Decided.

## Proposed API

```razor
<sa-grid columns="3" columns-sm="2">                @* columns = md tier; columns-base/-sm/-md/-lg *@
    <sa-input asp-for="FirstName" />
    <sa-input asp-for="Email" column-span="2" />
    <sa-display-field asp-for="Reference" />
    <sa-grid-item column-span="full" column-span-lg="2">
        <sa-form-section title="Audit">
            <sa-details columns="2">
                <sa-display-field asp-for="CreatedAt" />
                <sa-display-field asp-for="UpdatedAt" />
            </sa-details>
        </sa-form-section>
    </sa-grid-item>
</sa-grid>
```

- `sa-grid`: `columns` (shorthand for `columns-md`, one column below 40rem unless a smaller tier is set), `columns-base`, `columns-sm`, `columns-md`, `columns-lg`, each 1–12; setting both `columns` and `columns-md` throws. Renders a container `div` around the grid `div`, as `sa-details` renders a `div` around its `dl`.
- `sa-grid-item`: `column-span`, `column-span-sm`, `column-span-md`, `column-span-lg`, each 1–12 or `full`. Throws outside `sa-grid`.
- `sa-details`: gains `columns-base`; `orientation` takes `Vertical` or `Horizontal` (32). `sa-display-field` loses `column-span-full`; its span attributes take `full` (26).
- Fields (`sa-field`, implicit-field inputs, `sa-display-field`): the same span attributes as `sa-grid-item` (17).
- Span resolution, capping and validation are the existing `ResponsiveGrid` rules; a field finds the enclosing grid's column tiers through a grid context shared by `sa-grid` and `sa-details`.

## Work

### Prototype

Done: `sandbox/html/grid.html` against the built `stellar-admin.css` (live candidate, widths 420/560/720/1000px, today vs candidate, thresholds, spans per tier, display fields among inputs). It settles 16:

- `sa-grid`: every field sits in a `div.sa-grid-item` (`data-slot="grid-item"`) that its own tag helper renders with the field's span; `sa-grid-item` renders the same element. The item is the `field-group` container, so `field.css` is unchanged.
- `sa-details`: each display field is its own container (`display-field`). It is always a two-track grid (label 1fr, value 2fr); stacked, both parts span the two tracks, and horizontal or wide-enough responsive fields drop the span. This keeps the `dl` valid.
- `sa-grid` and `sa-details` share one container name (`grid`) and one set of tier rules on `.sa-grid-content` and `.sa-details-list`. Grid gap: 5u rows, 4u columns.

Checked in headless Chromium: default theme light, Ops dark.

### Tag helpers

Done (2026-10-12):

- `TagHelpers/Grid/`: `GridTagHelper` (`sa-grid`), `GridItemTagHelper` (`sa-grid-item`, throws unless directly in `sa-grid`), `GridContext` (published by `sa-grid` and `sa-details`; replaces `DetailsContext`, which is removed) and `GridLayout` (column validation, the one `1`–`12`/`full` span parser, grid-item wrapping).
- Only direct children place themselves: `StellarAdminTagHelperBase.GetEnclosingGrid` returns the grid only when it is the tag helper's parent, so a field inside a section inside a grid item is not wrapped again.
- Span attributes (`column-span`, `-sm`, `-md`, `-lg`, strings) on `FieldTagHelper`, `FieldInputBaseTagHelper` and `DisplayFieldTagHelper`; invalid values throw everywhere, spans apply only in a grid. Directly in `sa-grid`, fields, implicit-field inputs (also with `render-field="false"`, see 30) and display fields are wrapped in a `div.sa-grid-item` with their span; in `sa-details` a display field carries its span itself and renders `dt`/`dd`.
- `sa-details` gains `columns-base`; `column-span-full` is removed from `sa-display-field`.
- `GridTiers.Default` → `Base`, `ResponsiveGrid` parameters `@default` → `@base`; the Dashboard's `GridTiers` uses (`ChoiceColumnsStyle`, `_FormItems.cshtml`) are updated so it compiles. The Dashboard's own `Default` members are still to rename (Dashboard below).
- CSS: `grid.css` holds the tier rules for both grids (container `grid`) and the `sa-grid-item` field-group container; `details.css` keeps the list gap and the display field, now its own `display-field` container with the two-track grid (`align-content: start`, so a field in a tall row keeps its label/value gap); responsive display fields switch at 28rem (27). `form-row.css` and `TagHelpers/FormRow/` are removed.
- Tests: `GridTagHelperTests`, `GridItemTagHelperTests`, `FieldTagHelperTests`, `FieldInputBaseTagHelperTests.Grid`, updated details and display-field span tests.

Verified: solution build; TagHelpers tests 400/400; Dashboard tests 98/98; DocsSamples `Details/_Columns`, `_Responsive`, `_Horizontal`, `_InForm`, `_RowDetails` rendered in headless Chromium (dark). Until the samples step, pages using `sa-form-row` render unstyled rows and `column-span-full` no longer spans.

### DocsSamples

Done (2026-10-12):

- New Grid page (`Pages/Grid`, nav "Form Layout › Grid"): `_Intro` (two columns, a full-width input), `_Spans` (`columns="3" columns-sm="2" columns-lg="4"`, per-tier spans), `_DisplayFields` (display fields, an explicit `sa-field` and inputs), `_GridItems` (a checkbox, then sections in grid items: Audit details beside responsive Payment details), `_Responsive` (explicit responsive `sa-field`s and a display field; implicit inputs have no orientation attribute), `_Narrow`. Model `TravellerBooking`; DocsStatic maps the partials to `Grid.Index.SampleBooking`.
- FormRow pages and nav entry removed. `FormSection/_Intro`, `_Card`, `_Stacked`, `Showcase/_Forms` (Trip details) and `Details/_InForm` use `sa-grid columns="2"` with full-width spans; Showcase Availability keeps its field group with an `sa-grid` in place of the row. `Details/_Columns`, `_Values`, `_RowDetails` use `column-span="full"`.
- Generator: Grid entries in, FormRow entries out. Not run yet: regenerating writes the website, so it runs with the website step.

Verified: DocsSamples build; all six Grid partials and the converted samples return 200 from DocsStatic; Grid page, FormSection/_Intro, Details/_InForm, Details/_Columns and Showcase/_Forms checked in headless Chromium (dark).

### Responsive orientation removed (32)

Done (2026-10-12): `FieldOrientation.Responsive` and its data attribute and class are removed; the responsive rules are gone from `field.css`, `structure.css` (vertical fields still stretch their children) and `details.css` (display fields are a flex column again, with the two-column grid only when horizontal); grid item comments updated. Samples `Grid/_Responsive` and `Details/_Responsive` removed (pages, DocsStatic, generator); `Grid/_GridItems` uses horizontal Payment details; the ComponentPlayground Field demo's responsive fields are vertical, its buttons horizontal. Verified: solution build, TagHelpers tests 400/400, Switch, Checkbox, Radio Group, Checkbox Group, Field, Details and Grid pages rendered in headless Chromium (dark).

### Website, skills

Done (2026-10-12):

- Generator run: Grid demos and includes added; FormRow and `details-responsive` removed; the other demos changed only by the stylesheet hash and SVG attribute order.
- Website: `grid.mdx` (usage, columns and spans, inputs and display fields, grid items, narrow, API for `sa-grid`, `sa-grid-item` and field spans); `form-row.mdx` removed (no redirect); `meta.json`; `form-section.mdx` uses `sa-grid`; `details.mdx` has `columns-base`, `full` spans, no `column-span-full` and no responsive section; `field.mdx` has the orientation values without `Responsive` and the span attributes; `pnpm examples:export` refreshed the inline example assets.
- Skills: `skills.examples.json` has Grid examples; regenerated (`grid.md` in, `form-row.md` removed, every implicit input lists the span attributes); hand-written `grid.md` structure, `details.md` structure (grid spans, no responsive), and `forms.md` (the grouping example uses `sa-grid`).

Verified: website lint, type check and build; the Grid page in the dev server (headless Chromium, dark); SkillsGenerator `--check` (no drift); skill validation tests and check.

### Dashboard

- `_FormItems.cshtml`, `_FormSection.cshtml` and `_FormGroup.cshtml` emit `sa-grid`/`sa-grid-item`; fields get their span through the item, since editor templates render arbitrary markup.
- Remove the form grid rules from `client.css`; keep the multi-column section separator rule against the new classes.
- Fold `FormGrid.ValidateColumns` into `ResponsiveGrid` (10).
- Rename `GridColumnsBuilder.Default` → `Base`, `ColumnSpanBuilder.Default`/`DefaultFull` → `Base`/`BaseFull` and the internal `Default` members of `FormGridColumnDefinitions`/`FormColumnSpanDefinitions` (24); update callers in DashboardPlayground (`FormLayoutGalleryRegistration`), tests and the Dashboard consumer skill reference (`setup.md`).

### Verification

Build and tests for TagHelpers, Core and Dashboard; prototype and DocsSamples (port 5206) checked in headless Chromium at the widths above, light and dark; the DashboardPlayground Form layouts gallery before and after; generator diff reviewed; website lint, type check and build; skills `--check`.
