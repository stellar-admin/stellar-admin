# Info lists

Status: in progress (2026-10-12). Tag helpers, samples, website and skills done and committed. Dashboard display fields (decisions 49, 52–56) implemented, uncommitted. View page and configured row details deferred.

Make read-only displays of a record's fields a first-class feature, similar to Filament's [infolists](https://filamentphp.com/docs/5.x/infolists/overview), instead of hand-written `dl`/`dt`/`dd` markup in partials such as row details. Developers must be able to show entries on a view page, in row details, and on edit pages (for example audit information beside the editable fields).

## Filament 5 reference (checked 2026-10-12)

- Entry types: text, icon, image, color, code, key-value, repeatable, plus custom entries.
- Common entry options: label, hidden label, inline label (beside rather than above the value), visible/hidden, state override, default, placeholder for empty values, alignment, URL and open in new tab, tooltip, extra attributes, and eight content slots around the label and content (above/before/after/below).
- Text entry: date/time/relative ("since") formatting, numeric and money, markdown/HTML, badge, color, icon, size, weight, font family, character/word limits, line clamp, lists (line breaks, bullets, limited/expandable lists, separator), copyable, prefix/suffix actions, custom formatting callback.
- Icon entry: `boolean()` shows check/cross icons, detected automatically for boolean attributes, with customizable icons and colors.
- Repeatable entry: repeats a nested entry schema per collection item, as a grid, a table, or contained cards.
- Layout comes from the shared schema system: grid, flex, fieldset, section, tabs, wizard, callout, empty state.
- Usage: resource view pages (falling back to a disabled form when no infolist is defined), view modals, relation managers, and edit pages.
- Mixing: forms and infolists share one schema, and the schema docs show the exact audit case — a "Details" section of inputs beside an "Auditing" section of `TextEntry` components for `created_at`/`updated_at`.

## What exists in StellarAdmin

- The Dashboard has index, create and edit pages but no view page. Read-only displays exist only as row-detail partials with hand-written markup (for example `sandbox/DashboardPlayground/Areas/StellarAdmin/Views/Products/_ProductOverview.cshtml`).
- Form layout is a polymorphic tree: `FormItemOptions` (private constructor) has field, section and group subclasses, and `_FormItems.cshtml` switches on the item type. Fields render through `EditorTemplates`, which wrap TagHelpers inputs.
- `ResourceFieldBuilder` has `Title`, `Description`, `ColumnSpan` and `UseEditor<TEditor>`; `ResourceFieldsBuilder` has `Add`, `AddSection`, `AddGroup` and `Columns`.
- Edit POST loads the record, binds only the configured field names on top of it, and re-renders that record on failure, so non-bound values are still present after an invalid POST.
- The grid formats values with `Format`, a `GridDisplayTemplates/{template}` partial, or the metadata template hint. Lookup labels (`LookupLabelContext`) already resolve a related record's display text.
- `crud-screen-tag-helpers.md` proposed `sa-details`/`sa-display-field` with display precedence (child content, then template, then metadata formatting; null shows a placeholder, zero and false are real values). This plan supersedes that part if accepted.

## Proposed approach

Two layers, matching how `sa-input` underpins `Fields(...)`:

1. A TagHelpers primitive for read-only label/value display, usable anywhere (row-detail partials, custom pages, beside `sa-input` inside `sa-form-section`).
2. Dashboard configuration in which an entry is a fourth kind of form item beside fields, sections and groups. The same layout tree then serves view pages, row details, and entries mixed into create/edit forms. Entries never bind and never require a setter.

Mixing appears cheap because the item tree is already polymorphic and binding already works from an explicit field-name list. If the spike shows otherwise, fall back to entries only in their own sections.

### TagHelpers sketch

```razor
<sa-info-list columns="4">
    <sa-info-entry asp-for="Name" />
    <sa-info-entry asp-for="Details.Sku" placeholder="—" />
    <sa-info-entry asp-for="Price" format="{0:0.00}" label="Unit price" />
    <sa-info-entry asp-for="IsPublished">@(Model.IsPublished ? "Published" : "Draft")</sa-info-entry>
</sa-info-list>

@* Mixed inside a form *@
<sa-form-section title="Audit">
    <sa-info-entry asp-for="CreatedAt" />
    <sa-info-entry asp-for="UpdatedBy" />
</sa-form-section>
```

### Dashboard sketch

Superseded 2026-10-12: the earlier "entries" sketch (`AddEntry`, `AllowView(view => view.Entries(...))`, row-detail panels from entries) is replaced by display fields (decisions 49, 52–55).

```csharp
resource.AllowEdit(edit => edit.Fields(fields =>
{
    fields.Add(p => p.Name);
    fields.AddDisplayField(p => p.Reference);              // beside normal fields
    fields.AddSection("Pricing", pricing =>
    {
        pricing.Add(p => p.Price);
        pricing.AddDisplayField(p => p.UpdatedAt, f => f.EmptyText = "Never");
    });
    fields.AddDetails("Audit", audit =>                    // display fields only
    {
        audit.Orientation = FieldOrientation.Horizontal;
        audit.Variant = DetailsVariant.Divided;
        audit.Columns(2);
        audit.Add(p => p.CreatedAt);
        audit.Add(p => p.CreatedBy, f => f.UseDisplay<BadgeDisplay>());
    });
}));
```

Public API (implemented 2026-10-12):

- `ResourceFieldsBuilder<TResource>` (so also sections and groups): `AddDisplayField(expression)` returning `ResourceDisplayFieldBuilder`, and `AddDisplayField(expression, configure)` returning the fields builder; `AddDetails(title)` returning `ResourceDetailsBuilder<TResource>`, and `AddDetails(title, configure)` returning the fields builder.
- `ResourceDisplayFieldBuilder`: setter-only `Title`, `Format` and `EmptyText`; `UseDisplay<TDisplay>()` and `UseDisplay<TDisplay>(configure)` (`TDisplay : FieldDisplay, new()`); `ColumnSpan(int)`, `ColumnSpan(Action<ColumnSpanBuilder>)`, `ColumnSpanFull()`, `ColumnStart(int)`, `ColumnStart(Action<ColumnStartBuilder>)`.
- `ResourceDetailsBuilder<TResource>`: `Add(expression)` returning `ResourceDisplayFieldBuilder` and `Add(expression, configure)` returning the details builder (display fields only); setter-only `Description`, `Layout`, `Orientation` (`FieldOrientation?`) and `Variant` (`DetailsVariant?`); `ColumnSpan`/`ColumnSpanFull`/`ColumnStart` as on sections; `Columns(int)`/`Columns(Action<GridColumnsBuilder>)`.
- Displays: abstract `FieldDisplay` (`TemplateName`), `FieldDisplayView<TDisplay>` (`Display`), and `ViewDataKeys.FieldDisplay`.
- Options: `FormDisplayFieldOptions` (with `Display`) and `FormDetailsOptions` (sealed, beside field, section and group). `ResourceFormOptions.Fields` still lists only editable fields, so binding and the EF field checks never see display fields.

## Decisions

Answers from 2026-10-12. Earlier sketches above use `sa-info-*`; the chosen names are `sa-details`/`sa-display-field`.

1. Layering: TagHelpers first, then the Dashboard builder. Decided.
2. Naming: `sa-details`/`sa-display-field`, built on the existing `sa-field` infrastructure: a display field is an `sa-field` (keeping `orientation`) with a `dt`/`dd` label and value inside `sa-details` (a container `div` around a `dl` grid), and plain `div`s outside a `dl`, such as beside inputs in a form section. Decided, subject to the prototype.
3. Mixing entries into forms (`AddEntry` on fields, sections and groups), with entries-only sections as the fallback. Agreed; renamed and refined by 49, 52 and 53.
4. Dashboard view page: after the prototype and tag helpers. Deferred again (2026-10-12): display fields and `DisplayTemplates` depend only on a model and a property, so a view page can later host the same item tree without API changes.
5. Row-detail panels from entries: deferred. Row details stay partials using `sa-grid` and `sa-details`; a configured panel could later reuse the details builder (53).
6. Entries on create pages: deferred. Replaced by 54.
7. Separate edit models: deferred. Replaced by 55.
8. Value rendering: Dashboard `DisplayTemplates/{type}`. Decided.
9. Value rendering in the tag helper: built in C# inside the tag helper, as the data grid does (TagHelpers ships no views): format or `[DisplayFormat]`, encoded text, the placeholder for null, `[EmailAddress]`/`[Url]` as links, booleans as an icon (check, muted cross, screen-reader Yes/No), enums as plain text. Anything else is a named template or child content. Decided.
10. Arrangement: attributes, not a pick: `orientation` (`FieldOrientation`, on `sa-details` for all fields or per field) and `variant="DetailsVariant.Divided"`. Defaults: stacked, not divided. Decided.
11. Prototype first. Decided; the prototype is `sandbox/html/details.html`.
12. Supersede the crud-screen plan's `sa-details`/`sa-display-field` part: yes, once the prototype confirms the design.
13. Muted inline text after a value ("by Amelia Hart"): a `secondary` attribute. Decided.
14. Column span: `column-span="2"` and `column-span-full="true"`, mirroring the Dashboard's `ColumnSpan`/`ColumnSpanFull`, capped at the current column count. Column start is left out until the Dashboard's unscheduled `ColumnStart` (archived dashboard-form-grid plan) is taken up, so both stay aligned. Decided; made responsive by 18.
15. Templates: `template="X"` or `[UIHint("X")]` resolves `DisplayTemplates/X` by name only, never by type, so MVC's built-in type templates (a `bool` disabled checkbox) and an app's existing type templates never apply. A missing named template throws rather than falling back silently. The template's model is the value. Decided.
16. Enums render as plain text by default; a badge is a template or child content. Decided.
17. The empty-value attribute is `empty-text` (`EmptyText`), not `placeholder`: it says when the text shows and that it is text, and avoids the input hint meaning. Decided.
18. Responsive columns and spans, matching the Dashboard form grid: `columns-sm`/`-md`/`-lg` and `column-span-sm`/`-md`/`-lg` at the form grid's 30/40/56rem container breakpoints, an unset breakpoint inheriting from the next smaller one. `columns="N"` is shorthand for `columns-md` (one column below 40rem, as the Dashboard's `Columns(N)`), so the old automatic two-column step is gone; `columns-sm` restores it. No base-breakpoint columns attribute. `column-span-full` sets the base span to full and the breakpoint spans still apply. Decided.
19. One public helper for both: `ResponsiveGrid` (resolve columns and spans, format the inline variables) and `GridTiers` in TagHelpers. The Dashboard's form grid and choice columns use it, replacing its internal `FormGridTiers` and `FormGridStyle`, and the `--sa-cols`/`--sa-span`/`--sa-rows` registrations moved from the Dashboard's `client.css` to TagHelpers' `structure.css`. Decided.
20. Column counts and spans are 1–12, the Dashboard's limit. Decided.

Dashboard display fields (2026-10-12; numbered in the conversation's sequence):

49. Naming: "display fields" and "details", matching `sa-display-field` and `sa-details`; "entries" is dropped. Decided.
52. A display field is a new form item, added with `AddDisplayField` at the same level as `Add`, in fields, sections and groups. It is never bound and needs no setter. Decided.
53. A details section is its own form item, added with `AddDetails(title)`. Its builder accepts only display fields and exposes the section options (description, layout, span, start) plus `sa-details`' layout: `Columns`, `Orientation`, `Variant`. Decided.
54. Display fields are allowed on create pages too (for example read-only details about other records); a null value shows the empty text. Decided.
55. A display field reads its value from the page's model, like an input: on `AllowEdit<TModel, THandler>`/`AllowCreate<TModel, THandler>` the property must be on `TModel`. No special rule. Decided.
56. Custom value rendering is `UseDisplay<TDisplay>()`/`UseDisplay<TDisplay>(configure)`, mirroring `UseEditor`, so a display can carry settings to its template. A display derives from the abstract `FieldDisplay` (in `StellarAdmin.Dashboard.Resources.Displays`) and names its template through `TemplateName`; the template, `DisplayTemplates/{TemplateName}`, inherits `FieldDisplayView<TDisplay>`, whose `Display` holds the configured instance (default settings when absent) and whose model is the value. No handlers or built-in displays yet. Decided (Jerrie chose this over a `Template` name).

## Prototype

Picks (2026-10-12): A1 (muted label, medium-weight value); C2 (only display fields placed directly among inputs, outside `sa-details`, take an input's label style and a control's height so rows line up); D1 (`—` placeholder, overridable per field); F2 (caption labels inside row details, through a contextual rule rather than an attribute). G (view-page sections) waits for the Dashboard view page; G1 (split sections) is the likely default.

`sandbox/html/details.html` uses the built `stellar-admin.css` and the candidate CSS in `@layer sa.components`. Sections: the live candidate (a booking view page), A label typography, B arrangement (stacked columns, horizontal, divided, responsive), C mixed with inputs on an edit page, D empty placeholder, E value kinds, F row-detail panel, G view-page section layouts. Checked by screenshot at 1280px light and dark and at 390px; theme fixtures and density were not captured.

## Tag helpers (implemented 2026-10-12)

- `src/StellarAdmin.TagHelpers/TagHelpers/Details/`: `DetailsTagHelper` (`sa-details`: `columns`, `columns-sm`, `columns-md`, `columns-lg`, `orientation`, `variant`), `DetailsVariant` (`Default`, `Divided`), internal `DetailsContext`, and `DisplayFieldTagHelper` (`sa-display-field`: `asp-for`, `label`, `format`, `empty-text`, `href`, `template`, `secondary`, `column-span`, `column-span-sm`, `column-span-md`, `column-span-lg`, `column-span-full`, `orientation`).
- `sa-details` renders a `div` (the container query target) around a `dl`. A display field is an `sa-field` root (`data-slot="display-field"`) with a `dt`/`dd` label and value inside `sa-details`, and `div`s elsewhere.
- Value precedence: child content; then the empty text for null, blank strings and empty collections (`empty-text`, then `[DisplayFormat(NullDisplayText)]`, then `—`); then a named template (`template` or `[UIHint]`, resolved as `DisplayTemplates/{name}`, by name only, missing throws, model is the value with the property's model explorer); then built-in rendering: booleans as a `BooleanTrue`/`BooleanFalse` icon with screen-reader Yes/No, enums by `[Display]` name, collections joined with commas, `format` or `[DisplayFormat]`, `[EmailAddress]` as `mailto:`, `[Url]` linked only for http/https, `href` linking the text; numbers get `data-kind="number"`, `MultilineText` gets `data-kind="text"`. `secondary` is appended to non-empty values.
- Grid (decisions 18–20): `sa-details` writes its resolved column counts on the `dl` and each display field its resolved span as inline variables (`--sa-cols-md:3`, `--sa-span-lg:2`; values at 1 left out), through `TagHelpers/Grid/ResponsiveGrid.cs`. Spans are emitted only inside `sa-details`, limited to the column count at each breakpoint. Setting both `columns` and `columns-md`, or `column-span` and `column-span-full`, throws.
- `Client/css/components/details.css` holds the extracted prototype rules, with the among-inputs (C2) and row-detail (F2) rules. Layout rules name `.sa-field.sa-display-field` to out-rank `field.css`, which builds after it.
- Core: `SemanticIconRole.BooleanTrue` and `BooleanFalse` appended, mapped to `check` and `x` in the Lucide and both Tabler packs.

DocsSamples: `docs/DocsSamples/Pages/Details/` (navigation under Data Display) with Details, Columns and spans, Horizontal, Divided, Responsive, Empty values, Value types, Templates and custom content, Among form inputs, Record page and Row details examples; `Pages/Shared/DisplayTemplates/BookingStatus.cshtml` and `Reference.cshtml`; the partials are registered in `DocsStatic.cshtml.cs`.

Website and skills (2026-10-12): the eleven Details partials are registered in `docs/DocsSamplesGenerator/Generator.cs` and exported; the website has `content/docs/tag-helpers/components/details.mdx` (under Data Display in `meta.json`). The skills reference has a generated `components/details.md` with a hand-written structure region (values, columns and spans, `ResponsiveGrid`), six curated examples in `util/SkillsGenerator/skills.examples.json`, the `BooleanTrue`/`BooleanFalse` rows in `references/icons.md`, and a read-only value row in `references/forms.md`.

Dashboard display fields (implemented 2026-10-12, uncommitted):

- `src/StellarAdmin.Dashboard/Resources/`: `Options/FormDisplayFieldOptions.cs`, `Options/FormDetailsOptions.cs`, `Builders/ResourceDisplayFieldBuilder.cs` (property paths need only public getters), `Builders/ResourceDetailsBuilder.cs`, and `AddDisplayField`/`AddDetails` on `ResourceFieldsBuilder`.
- Views: `_FormItems.cshtml` renders a display field as `sa-display-field` in its grid item, and a details item through the new `_FormDetails.cshtml` (an `sa-form-section` around `sa-details`, stacked in the create sheet like sections). Display fields render inline rather than through templates, so inside `sa-details` they are direct children and get `dt`/`dd` and spans. `FormFieldsBaseView.BeginField` (internal) points the page's view data at the field's value while the tag renders, since `asp-for` takes a compile-time expression, and puts the field's display in the view context's data for the template.
- TagHelpers `details.css`: the divided variant now draws a top rule on every field and clips the first row's, so rules stay between rows in multiple columns (before, the second column's first field had a rule).
- Playground: Form layouts → Display fields (`DisplayFieldsLayoutGallery`): a root display field (a getter-only property), display fields among inputs in `Columns(3)`, a stacked `AddDetails` in `Columns(3)` with spans, a custom `BadgeDisplay` (`Views/Shared/DisplayTemplates/Badge.cshtml`) on the cabin, an email link, a boolean and an empty value, and a horizontal divided audit section in `Columns(2)`.
- Skills: `references/setup.md` has a Display fields section and a link from Create forms.

Outstanding: the view page and configured row details (deferred, decisions 4 and 5); lookup labels for display fields (a foreign key shows its raw value); the website's inline-example stylesheet still has the old divided rule (single-column demos look the same).

## Verification

Dashboard display fields, completed 2026-10-12:

- Tests: `StellarAdmin.Dashboard.Tests` 112 passed (new `ResourceFieldsBuilderTests` cases for display fields, details, rejected expressions, blank titles and out-of-range spans, starts and columns); `StellarAdmin.Dashboard.IntegrationTests` 411 passed (new `ResourceDisplayFieldTests`: a custom display with default and configured settings, at the root and in details, via the `PriceDisplay` fixture and `Views/Shared/DisplayTemplates/Price.cshtml`; create page empty text, custom `TModel`, details layout and placement, display fields among inputs and in sections, redisplay after an invalid POST, posted values ignored); TagHelpers 435 + 19, Core 74, EF integration 80 passed.
- Playground on port 5207, headless Chromium: the Display fields gallery edit page at 1300px light and dark and at 420px; the divided audit section before and after the CSS change; the create page shows empty text; after `UseDisplay`, the badge display in the details section.
- DocsSamples on port 5206: `Details/_Divided` and `Details/_RecordPage` with the new divided rule look unchanged (no rule above the first row).
- `SkillsGenerator --check`: no drift; skill validation passes.
- Not checked: theme fixtures; DocsSamples light mode.

Tag helpers, completed 2026-10-12:

- `dotnet build` of TagHelpers (client CSS rebuilt; `details.css` is in `stellar-admin.css`) and of both test projects.
- `StellarAdmin.TagHelpers.Tests`: 342 passed, including 37 new `DetailsTagHelperTests`/`DisplayFieldTagHelperTests` cases (confirmed in `--list-tests`). `StellarAdmin.Core.Tests`: 74 passed.
- Visual: the prototype's markup, adjusted to the emitted attributes, rendered with the built stylesheet instead of the candidate CSS, pixel-compared with the prototype at 1280px: identical except the boolean icons, which are 16px (`size-4`) instead of the prototype's accidental 24px.

- DocsSamples on port 5206: `/Details` returns 200; each example captured through `DocsStatic` in headless Chromium at 1100px light and dark, and the record page and form examples at 390px.

Responsive grid and `empty-text` (decisions 17–20), completed 2026-10-12:

- Release build of `StellarAdmin.slnx` (TagHelpers CSS rebuilt), no new warnings.
- Full solution test command: 1035 passed, 0 failed, across Core, TagHelpers, TagHelpers integration, Dashboard, Dashboard integration and EF Core integration. New: `ResponsiveGridTests`, plus rewritten `sa-details` column and display-field span cases; the Dashboard's definition and choice-column tests now run through the helper.
- DocsSamples on port 5206: the Columns and spans example (`columns="3" columns-sm="2" columns-lg="4"`) measured in headless Chromium at 420, 560, 720 and 1000px: 1, 2, 3 and 4 columns, with each field's span as configured.
- The built `stellar-admin.css` holds the 12 grid `@property` registrations and the Dashboard bundle none; the Dashboard layout links `stellar-admin.css`.

Website and skills, completed 2026-10-12:

- DocsSamplesGenerator export into `../website`: 472 partials. Apart from the new Details demos and snippets, every changed demo differs only by the new stylesheet fingerprints, antiforgery tokens, SVG attribute order and random element ids (checked by normalized comparison against HEAD). The exported `stellar-admin.css` adds only the grid registrations and `details.css`; the samples' `site.css` gains `mb-8` and the unused `columns-sm`/`columns-lg` utilities Tailwind picks up from the attribute names.
- Website: `pnpm lint`, `pnpm types:check` and `pnpm build` pass, and the build prerenders `/docs/tag-helpers/components/details`; the page and its Columns demo were screenshotted from `pnpm dev` at 1400px.
- Skills: `util/SkillsGenerator` regenerated, `--check` reports no drift, and `build/skill-validation` tests and check pass.

Not run: the theme fixtures and density, and a rendered Dashboard form page after the registration move (covered only by the stylesheet check above and the integration tests' markup).
