# Info lists

Status: in progress (2026-10-12). Tag helpers implemented (uncommitted); Dashboard work not started.

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

```csharp
// Audit entries on the edit page
resource.AllowEdit(edit => edit.Fields(fields =>
{
    fields.Add(p => p.Name);
    fields.Add(p => p.Price);
    fields.AddSection("Audit", audit =>
    {
        audit.AddEntry(p => p.CreatedAt);
        audit.AddEntry(p => p.UpdatedAt, entry => entry.Placeholder = "Never");
    });
}));

// View page
resource.AllowView(view => view.Entries(entries =>
{
    entries.Columns(2);
    entries.Add(p => p.Name);
    entries.Add(p => p.Price, entry => entry.Format = "{0:C}");
    entries.Add(p => p.CategoryId, entry => entry.Title = "Category"); // lookup label
    entries.Add(p => p.Status, entry => entry.UseDisplay<BadgeDisplay>());
}));

// Row-detail panel from entries instead of a partial
index.EnableRowDetails(panels => panels.AddEntries("Overview", entries => { /* ... */ }));
```

`ResourceEntryBuilder` would mirror `ResourceFieldBuilder`: `Title`, `Description`, `Format`, `Placeholder`, `ColumnSpan`, and `UseDisplay<TDisplay>(configure)` as the counterpart of `UseEditor`. Entries render through `DisplayTemplates`, which wrap `sa-info-entry`.

## Decisions

Answers from 2026-10-12. Earlier sketches above use `sa-info-*`; the chosen names are `sa-details`/`sa-display-field`.

1. Layering: TagHelpers first, then the Dashboard builder. Decided.
2. Naming: `sa-details`/`sa-display-field`, built on the existing `sa-field` infrastructure: a display field is an `sa-field` (keeping `orientation`) with a `dt`/`dd` label and value inside `sa-details` (a container `div` around a `dl` grid), and plain `div`s outside a `dl`, such as beside inputs in a form section. Decided, subject to the prototype.
3. Mixing entries into forms (`AddEntry` on fields, sections and groups), with entries-only sections as the fallback. Agreed.
4. Dashboard view page: after the prototype and tag helpers.
5. Row-detail panels from entries: deferred.
6. Entries on create pages: deferred.
7. Separate edit models: deferred.
8. Value rendering: Dashboard `DisplayTemplates/{type}`. Decided.
9. Value rendering in the tag helper: built in C# inside the tag helper, as the data grid does (TagHelpers ships no views): format or `[DisplayFormat]`, encoded text, the placeholder for null, `[EmailAddress]`/`[Url]` as links, booleans as an icon (check, muted cross, screen-reader Yes/No), enums as plain text. Anything else is a named template or child content. Decided.
10. Arrangement: attributes, not a pick: `orientation` (`FieldOrientation`, on `sa-details` for all fields or per field) and `variant="DetailsVariant.Divided"`. Defaults: stacked, not divided. Decided.
11. Prototype first. Decided; the prototype is `sandbox/html/details.html`.
12. Supersede the crud-screen plan's `sa-details`/`sa-display-field` part: yes, once the prototype confirms the design.
13. Muted inline text after a value ("by Amelia Hart"): a `secondary` attribute. Decided.
14. Column span: `column-span="2"` and `column-span-full="true"`, mirroring the Dashboard's `ColumnSpan`/`ColumnSpanFull`, capped at the current column count. Column start is left out until the Dashboard's unscheduled `ColumnStart` (archived dashboard-form-grid plan) is taken up, so both stay aligned. Proposed; attribute shape awaiting confirmation.
15. Templates: `template="X"` or `[UIHint("X")]` resolves `DisplayTemplates/X` by name only, never by type, so MVC's built-in type templates (a `bool` disabled checkbox) and an app's existing type templates never apply. A missing named template throws rather than falling back silently. The template's model is the value. Decided.
16. Enums render as plain text by default; a badge is a template or child content. Decided.

## Prototype

Picks (2026-10-12): A1 (muted label, medium-weight value); C2 (only display fields placed directly among inputs, outside `sa-details`, take an input's label style and a control's height so rows line up); D1 (`—` placeholder, overridable per field); F2 (caption labels inside row details, through a contextual rule rather than an attribute). G (view-page sections) waits for the Dashboard view page; G1 (split sections) is the likely default.

`sandbox/html/details.html` uses the built `stellar-admin.css` and the candidate CSS in `@layer sa.components`. Sections: the live candidate (a booking view page), A label typography, B arrangement (stacked columns, horizontal, divided, responsive), C mixed with inputs on an edit page, D empty placeholder, E value kinds, F row-detail panel, G view-page section layouts. Checked by screenshot at 1280px light and dark and at 390px; theme fixtures and density were not captured.

## Tag helpers (implemented 2026-10-12)

- `src/StellarAdmin.TagHelpers/TagHelpers/Details/`: `DetailsTagHelper` (`sa-details`: `columns` 1–4, `orientation`, `variant`), `DetailsVariant` (`Default`, `Divided`), internal `DetailsContext`, and `DisplayFieldTagHelper` (`sa-display-field`: `asp-for`, `label`, `format`, `placeholder`, `href`, `template`, `secondary`, `column-span`, `column-span-full`, `orientation`).
- `sa-details` renders a `div` (the container query target) around a `dl`. A display field is an `sa-field` root (`data-slot="display-field"`) with a `dt`/`dd` label and value inside `sa-details`, and `div`s elsewhere.
- Value precedence: child content; then the placeholder for null, blank strings and empty collections (`placeholder`, then `[DisplayFormat(NullDisplayText)]`, then `—`); then a named template (`template` or `[UIHint]`, resolved as `DisplayTemplates/{name}`, by name only, missing throws, model is the value with the property's model explorer); then built-in rendering: booleans as a `BooleanTrue`/`BooleanFalse` icon with screen-reader Yes/No, enums by `[Display]` name, collections joined with commas, `format` or `[DisplayFormat]`, `[EmailAddress]` as `mailto:`, `[Url]` linked only for http/https, `href` linking the text; numbers get `data-kind="number"`, `MultilineText` gets `data-kind="text"`. `secondary` is appended to non-empty values.
- Column spans are emitted only inside `sa-details`, capped at the list's column count (`data-column-span` = `2`, `3` or `full`).
- `Client/css/components/details.css` holds the extracted prototype rules, with the among-inputs (C2) and row-detail (F2) rules. Layout rules name `.sa-field.sa-display-field` to out-rank `field.css`, which builds after it.
- Core: `SemanticIconRole.BooleanTrue` and `BooleanFalse` appended, mapped to `check` and `x` in the Lucide and both Tabler packs.

DocsSamples: `docs/DocsSamples/Pages/Details/` (navigation under Data Display) with Details, Columns and spans, Horizontal, Divided, Responsive, Empty values, Value types, Templates and custom content, Among form inputs, Record page and Row details examples; `Pages/Shared/DisplayTemplates/BookingStatus.cshtml` and `Reference.cshtml`; the partials are registered in `DocsStatic.cshtml.cs`.

Outstanding: the website docs page and generated demos, the consumer skills reference (including the two icon roles in `references/icons.md`), and the Dashboard entries work.

## Verification

Completed 2026-10-12:

- `dotnet build` of TagHelpers (client CSS rebuilt; `details.css` is in `stellar-admin.css`) and of both test projects.
- `StellarAdmin.TagHelpers.Tests`: 342 passed, including 37 new `DetailsTagHelperTests`/`DisplayFieldTagHelperTests` cases (confirmed in `--list-tests`). `StellarAdmin.Core.Tests`: 74 passed.
- Visual: the prototype's markup, adjusted to the emitted attributes, rendered with the built stylesheet instead of the candidate CSS, pixel-compared with the prototype at 1280px: identical except the boolean icons, which are 16px (`size-4`) instead of the prototype's accidental 24px.

- DocsSamples on port 5206: `/Details` returns 200; each example captured through `DocsStatic` in headless Chromium at 1100px light and dark, and the record page and form examples at 390px.

Not run: the full solution test command, the theme fixtures and density, the DocsSamplesGenerator export.
