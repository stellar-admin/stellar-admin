# Info lists

Status: completed (2026-10-13). Tag helpers, samples, website, skills, Dashboard display fields and details sections, displays and display handlers, EF Core reference loading, grid column displays and the Dashboard view page are done and committed. Configured row details (decision 5) moved to [configured-row-details](../configured-row-details.md); app-level display defaults (90) and column display handlers (129) moved to [display-defaults](../display-defaults.md).

Make read-only displays of a record's fields a first-class feature, similar to Filament's [infolists](https://filamentphp.com/docs/5.x/infolists/overview), instead of hand-written `dl`/`dt`/`dd` markup in partials such as row details. Developers must be able to show read-only fields on a view page, in row details, and on edit pages (for example audit information beside the editable fields).

## Filament 5 reference (checked 2026-10-12)

- Entry types: text, icon, image, color, code, key-value, repeatable, plus custom entries.
- Common entry options: label, hidden label, inline label (beside rather than above the value), visible/hidden, state override, default, placeholder for empty values, alignment, URL and open in new tab, tooltip, extra attributes, and eight content slots around the label and content (above/before/after/below).
- Text entry: date/time/relative ("since") formatting, numeric and money, markdown/HTML, badge, color, icon, size, weight, font family, character/word limits, line clamp, lists (line breaks, bullets, limited/expandable lists, separator), copyable, prefix/suffix actions, custom formatting callback.
- Icon entry: `boolean()` shows check/cross icons, detected automatically for boolean attributes, with customizable icons and colors.
- Repeatable entry: repeats a nested entry schema per collection item, as a grid, a table, or contained cards.
- Layout comes from the shared schema system: grid, flex, fieldset, section, tabs, wizard, callout, empty state.
- Usage: resource view pages (falling back to a disabled form when no infolist is defined), view modals, relation managers, and edit pages.
- Mixing: forms and infolists share one schema, and the schema docs show the exact audit case — a "Details" section of inputs beside an "Auditing" section of `TextEntry` components for `created_at`/`updated_at`.

## What existed in StellarAdmin (before this plan)

- The Dashboard has index, create and edit pages but no view page. Read-only displays exist only as row-detail partials with hand-written markup (for example `sandbox/DashboardPlayground/Areas/StellarAdmin/Views/Products/_ProductOverview.cshtml`).
- Form layout is a polymorphic tree: `FormItemOptions` (private constructor) has field, section and group subclasses, and `_FormItems.cshtml` switches on the item type. Fields render through `EditorTemplates`, which wrap TagHelpers inputs.
- `ResourceFieldBuilder` has `Title`, `Description`, `ColumnSpan` and `UseEditor<TEditor>`; `ResourceFieldsBuilder` has `Add`, `AddSection`, `AddGroup` and `Columns`.
- Edit POST loads the record, binds only the configured field names on top of it, and re-renders that record on failure, so non-bound values are still present after an invalid POST.
- The grid formats values with `Format`, a `GridDisplayTemplates/{template}` partial, or the metadata template hint. The lookup editors resolve a related record's title through `LookupItems.FindAsync` (`LookupLabelContext` only carries UI labels such as Add and Choose).
- `crud-screen-tag-helpers.md` proposed `sa-details`/`sa-display-field` with display precedence (child content, then template, then metadata formatting; null shows a placeholder, zero and false are real values). This plan supersedes that part.

## Proposed approach

Two layers, matching how `sa-input` underpins `Fields(...)`:

1. A TagHelpers primitive for read-only label/value display, usable anywhere (row-detail partials, custom pages, beside `sa-input` inside `sa-form-section`).
2. Dashboard configuration in which a display field, and a details section of display fields, are new kinds of form item beside fields, sections and groups. The same layout tree then serves create and edit forms now, and a view page and row details later. Display fields never bind and never require a setter.

Mixing proved cheap: the item tree was already polymorphic and binding already worked from an explicit field-name list.

### TagHelpers

```razor
<sa-details columns="2">
    <sa-display-field asp-for="Name" />
    <sa-display-field asp-for="Details.Sku" empty-text="None" />
    <sa-display-field asp-for="Price" format="{0:0.00}" label="Unit price" />
    <sa-display-field asp-for="IsPublished">@(Model.IsPublished ? "Published" : "Draft")</sa-display-field>
</sa-details>

@* Among inputs in a form *@
<sa-form-section title="Audit">
    <sa-display-field asp-for="CreatedAt" />
    <sa-display-field asp-for="UpdatedBy" />
</sa-form-section>
```

### Dashboard

The earlier "entries" sketch (`AddEntry`, `AllowView(view => view.Entries(...))`, row-detail panels from entries) was replaced by display fields (decisions 49, 52–56).

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
- Displays (`StellarAdmin.Dashboard.Resources.Displays`): abstract `FieldDisplay` (`TemplateName`); `IFieldDisplay`/`IFieldDisplay<THandler>`, `IFieldDisplayHandler`/`IFieldDisplayHandler<TDisplay>`, `FieldDisplayHandler<TDisplay>`, `FieldDisplayContext`; built-in `BadgeDisplay` (`Variant`). `FieldDisplayView<TDisplay>` (`Display`, `DisplayData`), `ViewDataKeys.FieldDisplay`, `FieldDisplayData` and `DisplayData`, and `ResourceFormPageViewModel.DisplayData`.
- Options: `FormDisplayFieldOptions` (with `Display`) and `FormDetailsOptions` (sealed, beside field, section and group). `ResourceFormOptions.Fields` still lists only editable fields, so binding and the EF field checks never see display fields.

## Decisions

Answers from 2026-10-12. Early sketches used `sa-info-list`/`sa-info-entry` and "entries"; the chosen names are `sa-details`/`sa-display-field` and display fields (2, 49).

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

Built-in displays (2026-10-12):

63. Display handlers mirror editor handlers: a display that needs request data implements `IFieldDisplay<THandler>`; the handler (`IFieldDisplayHandler<TDisplay>`, base `FieldDisplayHandler<TDisplay>`, DI constructor) returns data from `PrepareAsync(FieldDisplayContext, CancellationToken)`, prepared by the controller for every display field in the form tree, and the template reads it as `FieldDisplayView.DisplayData`. `UseDisplay` checks that the handler implements `IFieldDisplayHandler<TDisplay>`. Decided.
64. `LookupDisplay` has the lookup editor's `UseItems` overloads and resolves the value through `LookupItems.FindAsync`: it shows each item's title, and a value no item matches shows the value. Decided; superseded by 80.
65. A per-value badge function: dropped by 68.
66. Built-in templates live in `Areas/StellarAdmin/Views/Shared/DisplayTemplates/Displays/` (`Badge`, `Lookup`), mirroring `EditorTemplates/Editors/`. Decided.
67. The Display fields gallery uses both, plus a `[UIHint]` template that picks the badge by value. Decided.
68. `BadgeDisplay` has only a fixed `Variant` (default `Secondary`); choosing a badge by value is a display template (`[UIHint]`), documented in the skill. The display field builders stay non-generic. Decided.
69. Display handlers and `LookupDisplay` as planned in 63 and 64. Decided.

EF Core references (2026-10-12):

76. The EF Core data source includes the reference navigations on each display field's path when it loads the edit record (`Category.Name` includes `Category`; owned types extend the path without an include; a collection or plain property ends it), so a display field reads a related value with no extra query. No new API; `ResourceFormOptions.DisplayFields` became public so the data source can read it. Decided; implemented.
77. An EF Core `LookupDisplay.UseItems` extension: dropped (79). With 76, an EF resource displays `p => p.Category.Name` directly.
78. Rename `LookupDisplay.UseItems` to `UseSource<TSource, TEntity, TValue>(value, title)`, drop its `LookupItems` and description/media overloads and make `Items` internal: it only fetches the referenced records by key, never a list. Decided; not yet implemented.
79. Drop 77. Decided.
80. Drop `LookupDisplay` (and with it 78): with 76 an EF resource displays `p => p.Category.Name`, and a custom model can carry the related value, so a key-to-title display only served keys without a navigation or model, which a custom display with a handler covers. Its handler and template are removed; display handlers (63) and `BadgeDisplay` stay, and `LookupItems.Create` stays as the lookup editors' shared factory. Decided; implemented.

Standard displays (agreed 2026-10-12; 81–89, 91–92 and 99–102 were options in the discussion that the decisions below replaced):

90. App-level configuration of the default displays: deferred, to be designed together with editor defaults and the grid's column displays. The mapping is hard-coded for now.
93. Without `UseDisplay`, a Dashboard display field uses a default display chosen from the property's metadata by an internal map. TagHelpers knows nothing of displays: the Dashboard passes the display's template name to `sa-display-field`. Precedence: `UseDisplay`, then `UseBuiltInRendering()`, then `[UIHint]`, then the default display, then the tag helper's own rendering (collections, GUIDs, other types). Decided.
94. `UseBuiltInRendering()` opts a field out of the default display, back to the tag helper's rendering (which honours `[DisplayFormat]`). Decided.
95. A `Format` on `FieldDisplayView`: superseded by 116.
96. `PhoneDisplay` renders a `tel:` link. Decided.
97. `LinkDisplay` links http(s) only, with `OpenInNewTab` (default `true`). Decided.
98. `DateDisplay` renders `<time datetime>` in ISO 8601, defaulting to `d MMM yyyy`, `d MMM yyyy, HH:mm` and `HH:mm`. Decided.
103. The standard set covers all common cases: `TextDisplay` (`LineClamp`; multiline keeps line breaks), `EmailDisplay`, `LinkDisplay`, `PhoneDisplay`, `DateDisplay` (including `TimeOnly`), `BooleanDisplay`, `EnumDisplay`, `NumberDisplay` (tabular figures), and `BadgeDisplay`, which is opt-in only. Decided; 104–111 were the individual displays.
112. `BooleanDisplay` defaults to a check or a muted cross with Yes/No for screen readers; null shows the empty text. Decided.
113. `BooleanDisplay.Appearance` is a `BooleanDisplayAppearance` (`Icon`, `Text`, `IconAndText`), with `TrueText`/`FalseText` defaulting to "Yes"/"No". Decided.
114. The false icon is muted, not red. Decided.
115. `Format` belongs to the display: `NumberDisplay.Format` and `DateDisplay.Format` take a plain .NET format string (`"N2"`, `"d MMM yyyy"`), falling back to `[DisplayFormat]`, then the display's default. Decided.
116. The display field builder and `FormDisplayFieldOptions` lose `Format`; `EmptyText` stays on the field. `FieldDisplayView<TDisplay>.FormatValue(format)` formats the value for templates. Decided.
117. `UseBuiltInRendering` still honours `[DisplayFormat]`, through the tag helper. Decided.
118. The default display is resolved in the view (`FormFieldsBaseView.BeginField`) from the field's model metadata rather than in the controller: default displays need no handler or request data, so the controller is unchanged. Implementation choice.
125. Index columns use the same displays and precedence as display fields: `UseDisplay`, then `UseBuiltInRendering()`, then `[UIHint]`, then the default display, then the tag helper's rendering. Decided.
126. A cell renders a display's template as `GridDisplayTemplates/{TemplateName}`, beside `DisplayTemplates/{TemplateName}`, following the grid's existing convention; a custom display used in a column needs a grid template. Decided.
127. `sa-data-grid-column` gains `empty-text` (an empty value skips the template; default `[DisplayFormat(NullDisplayText)]`, then an empty cell), and its templates receive the field property's model explorer. TagHelpers still knows nothing of displays. Decided.
128. `Format` leaves the column builder and `DataGridColumnOptions` (as 115–116); the unused `DataGridColumnOptions.Template` is removed. Decided.
129. Displays with a handler are not supported in columns: `UseDisplay` throws `NotSupportedException`, since a handler would run once per row; revisit with a batched design. Decided.
130. `NumberDisplay` columns are right-aligned, header and cells. Decided.
140. The EF Core index includes the reference navigations on each column's path, as the edit page does for display fields, so `product => product.Category.Name` shows the category. The column's own expression gives the path; collections end it and owned types load with their owner. Decided.
147. View page (planned 2026-10-13, replaces the deferral in 4): opt-in `AllowView()`/`AllowView(view => ...)` and `AllowView<TModel, THandler>()`, a GET-only `View` action at `/{slug}/view/{id}`, overridable as `Views/{Resource}/View.cshtml` or `ResourceView.cshtml`. Done.
149. A view with no display fields (empty sections included) fails startup validation; no fallback derived from the edit form. Done.
150. New `IResourceViewHandler<TModel>` with `FindAsync`; `IResourceEditHandler<TModel>` extends it, so a read-only data source can back a view page. Startup validation checks the data source as for edit. Done.
151. The EF Core `FindAsync` includes display-field reference paths from both the edit and view configurations (when each uses the entity), since it cannot tell which page calls it. Done.
152. Navigation: an eye-icon View row action before Edit; the view page header offers Edit and Delete (the existing dialog; a failed delete returns to the view page) and a Back link to the index. Done.
153. Display handlers run on the view page as on edit. Done.
154. Labels: `ResourceViewLabelOptions.Title` ("View {singular}"), `ResourceIndexLabelOptions.ViewLabel` and `index.ViewLabel`. The row label defaults to "View {singular}", matching "Edit {singular}", rather than the planned "View". Done.
156. View builders where every field is a display field: `ResourceViewFieldsBuilder`, `ResourceViewSectionBuilder` and `ResourceViewGroupBuilder` with `Add` (display field), `AddSection`, `AddGroup`, `Columns`, `Clear`, spans, starts and per-section `Layout`; no `AddDisplayField` or `AddDetails`. Supersedes 148 and 157–159. Done.
160. View sections and groups build `FormSectionOptions`/`FormGroupOptions` and render through `_FormItems`, `_FormSection` and `_FormGroup`, so the layout flexibility (nesting, grids, spans, starts) matches create and edit. Done.
161. The view builders share the form builders' implementation through an internal `FormScopeBuilder` (container and display-field building) rather than a common base class, since their public return types differ. Done.
162. On the view page, display fields take the details style (muted label, medium value), not the beside-inputs style of C2; the page marks its context. Done.
163. `Orientation` is offered on view fields and sections and inherited downward; `Divided` is not offered on view sections for now. Done.
164. The standard view page renders through a public `<sa-view-page model="Model" />` (`_ViewPage.cshtml`), as `<sa-form-page>` does, with `pre-page-actions`, `post-page-actions`, `pre-view-fields` and `post-view-fields` slots, so an override can reuse it. Done.

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

Dashboard display fields (implemented 2026-10-12, committed):

- `src/StellarAdmin.Dashboard/Resources/`: `Options/FormDisplayFieldOptions.cs`, `Options/FormDetailsOptions.cs`, `Builders/ResourceDisplayFieldBuilder.cs` (property paths need only public getters), `Builders/ResourceDetailsBuilder.cs`, and `AddDisplayField`/`AddDetails` on `ResourceFieldsBuilder`.
- Views: `_FormItems.cshtml` renders a display field as `sa-display-field` in its grid item, and a details item through the new `_FormDetails.cshtml` (an `sa-form-section` around `sa-details`, stacked in the create sheet like sections). Display fields render inline rather than through templates, so inside `sa-details` they are direct children and get `dt`/`dd` and spans. `FormFieldsBaseView.BeginField` (internal) points the page's view data at the field's value while the tag renders, since `asp-for` takes a compile-time expression, and puts the field's display in the view context's data for the template.
- TagHelpers `details.css`: the divided variant now draws a top rule on every field and clips the first row's, so rules stay between rows in multiple columns (before, the second column's first field had a rule).
- Playground: Form layouts → Display fields (`DisplayFieldsLayoutGallery`): a root display field (a getter-only property), display fields among inputs in `Columns(3)`, a stacked `AddDetails` in `Columns(3)` with spans, a custom `BadgeDisplay` (`Views/Shared/DisplayTemplates/Badge.cshtml`) on the cabin, an email link, a boolean and an empty value, and a horizontal divided audit section in `Columns(2)`.
- Skills: `references/setup.md` has a Display fields section and a link from Create forms.

Built-in displays (implemented 2026-10-12, committed): the controller's `PrepareDisplaysAsync` runs display handlers for `ResourceFormOptions.DisplayFields` (public since 76; every display field, including in containers and details) on create, edit, the create sheet and redisplay, keyed by field options; `_FormPage` and `_CreateSheet` pass the data, and `BeginField` puts the field's entry in the view context's data. `LookupItems.Create` (internal) now builds the items for `LookupSheetEditor` and `MultiLookupSheetEditor`. `_FormDetails.cshtml` writes a full span as `full` (it wrote the integer, which the tag helper would reject). Playground: the gallery's custom `BadgeDisplay` is replaced by the built-in one, and `Status` uses `Views/Shared/DisplayTemplates/BookingStatus.cshtml` through `[UIHint]`. Skills: `setup.md` documents `BadgeDisplay`, per-value templates and custom displays with handlers.

Standard displays (implemented 2026-10-12, committed):

- `src/StellarAdmin.Dashboard/Resources/Displays/`: `TextDisplay`, `EmailDisplay`, `LinkDisplay`, `PhoneDisplay`, `DateDisplay`, `NumberDisplay`, `BooleanDisplay`, `BooleanDisplayAppearance`, `EnumDisplay`; internal `FieldDisplayDefaults` (the map: data type `EmailAddress`/`Url`/`PhoneNumber` first, then dates and times, `bool`, non-flags enums, numbers, strings) and `FieldValueFormatter` (format string, then `[DisplayFormat]`, then enum `[Display]` names, current culture), shared by `FieldDisplayView.FormatValue` and the badge template.
- `ResourceDisplayFieldBuilder.UseBuiltInRendering()` (last call between it and `UseDisplay` wins); `Format` removed from the builder, `FormDisplayFieldOptions` and the form views.
- `BeginField` returns a scope carrying the resolved template name, which `_FormItems` and `_FormDetails` pass as `template`.
- Templates in `Areas/StellarAdmin/Views/Shared/DisplayTemplates/Displays/` (`Text`, `Email`, `Link`, `Phone`, `Date`, `Number`, `Boolean`, `Enum`; `Badge` now uses the shared formatter). Razor writes `data-*` attributes even when null, so `Text` branches for the clamped case and writes `data-multiline` as `true`/`false`.
- Dashboard `client.css`: `.sa-text-display` (multiline, line clamp through `--_line-clamp`), `.sa-number-display` (tabular figures), `.sa-boolean-display` (success check, muted cross).
- Playground: the Display fields gallery's `Total` uses `NumberDisplay` with a format, and two new details sections show every default display and the display settings, including `UseBuiltInRendering`; the gallery model gained a hotel website, phone, check-in time, nights and a multiline itinerary.
- Skills: `setup.md` documents the precedence, every built-in display and its settings, `UseBuiltInRendering`, `FormatValue` and overriding a built-in template.

Grid column displays (implemented 2026-10-13, not committed):

- `ResourceColumnBuilder`: `UseDisplay<TDisplay>()`, `UseDisplay<TDisplay>(configure)`, `UseBuiltInRendering()` and setter-only `EmptyText`; `Format` removed. `DataGridColumnOptions`: `Display` and `EmptyText` added, `Format` and `Template` removed, internal `UsesBuiltInRendering`.
- `FieldDisplayDefaults.Resolve(DataGridColumnOptions, IModelMetadataProvider)` resolves a column's display from its leaf property's metadata. `_IndexDataGrid.cshtml` resolves every column's display once, outside the per-row content, puts each in the view context's data before its column (cleared after the columns, so row detail partials don't see it), passes `template` and `empty-text`, and adds `text-right` to `NumberDisplay` columns.
- Grid templates in `Areas/StellarAdmin/Views/Shared/GridDisplayTemplates/Displays/`, copies of the details templates except `Text`, which keeps to one line unless clamped.
- `DataGridColumnTagHelper`: `empty-text`, the empty check before any template (null, blank string, empty collection), and the field's `ModelExplorer` on the template's view data. Templates no longer receive null values.
- Playground demos (136–139, 2026-10-13): the Display fields gallery's index shows every type's default column display, and a new Display columns gallery (`DisplayColumnsGallery`, the same booking) shows configured displays, the `[UIHint]` status through `Views/Shared/GridDisplayTemplates/BookingStatus.cshtml`, a clamped itinerary, empty text and built-in rendering. The demos found three defects, now fixed: `FieldDisplayView.Display` cached the display on the page instance, which the grid reuses for every cell, so a second column using the same template got the first's display; cell links had no link style (the grid templates now add `sa-display-link`); and clamped text widened its column (capped at `80 * --sa-unit` in cells).
- Playground and the integration test host move their price column to `NumberDisplay`. Skills: `setup.md` documents column displays; the generated data-grid reference picks up `empty-text`.

Outstanding at close: the website's inline-example stylesheet still has the old divided rule (single-column demos look the same). Configured row details and display defaults continue in their own plans.

## Verification

View page (decisions 147–164), completed 2026-10-13:

- Full solution tests: 1209 passed, 0 failed. New: `ResourceViewFieldsBuilderTests` (nested layout, orientation, custom model, validation, blank titles) and a view case in `ResourceBuilderTests`; `ResourceViewPageTests` (nested sections and groups, orientation inheritance, page actions, rejected delete returning to the view page, custom model, display handler, 404s, index row action, labels); EF `ResourceViewTests` (the view's reference paths join in one query; own properties don't).
- Playground on port 5207, headless Chromium: the Products and Display fields view pages at 1280px and 390px, light and dark, and the Products index with the View row action.
- `SkillsGenerator`: regenerated (adds `components/view-page.md` and its index row); `--check` reports no drift.
- Not checked: theme fixtures and density; a custom `View.cshtml` override.

Grid column displays (125–130), completed 2026-10-13:

- Tests: `StellarAdmin.Dashboard.Tests` 145 passed (new `ResourceColumnBuilderTests`; column cases in `FieldDisplayDefaultsTests`: default for a nested leaf, configured display, built-in rendering, `[UIHint]`); `StellarAdmin.Dashboard.IntegrationTests` 425 passed (new `ResourceColumnDisplayTests` over a `ProductFactsModel` index: every default display, `NumberDisplay` format and right alignment, a custom display's grid template, `[UIHint]`, `UseBuiltInRendering`, empty text); `StellarAdmin.TagHelpers.Tests` 439 passed (new `DataGridColumnTagHelperTests`: `empty-text`, `NullDisplayText`, template metadata, empty values skipping the template); TagHelpers integration 19 and EF integration 82 passed.
- Playground on port 5207, headless Chromium at 1280px light: the products index right-aligns Id, Category Id and Unit price, headers included.
- Demos: Dashboard unit 145 and integration 426 passed (new: two columns sharing the Text template keep their own displays); both gallery indexes screenshotted at 1700px light after the fixes.
- Column references (140), completed 2026-10-13: EF integration 84 passed (new `ResourceColumnTests`: a sortable `Category.Name` column shows each category, sorts descending, and the page query joins Category; columns on own and owned properties join nothing); Dashboard integration 426 passed.
- Skill validation and `SkillsGenerator --check` pass.
- Not checked: dark mode, theme fixtures, phone widths; the Users index (behind sign-in) for booleans and email links in the Playground.

Standard displays (90–118), completed 2026-10-12:

- Tests: `StellarAdmin.Dashboard.Tests` 136 passed (new `FieldDisplayDefaultsTests` for every mapped and unmapped type, and `UseBuiltInRendering`/`UseDisplay` last-call-wins); `StellarAdmin.Dashboard.IntegrationTests` 419 passed (new: every default display through a `ProductFactsModel` custom edit model, display settings, `[UIHint]` over the default and `UseDisplay` over `[UIHint]`, `UseBuiltInRendering` with `[DisplayFormat]` and a GUID, empty values skipping the display; the `Format` cases moved to `NumberDisplay`); EF integration 82 passed.
- Playground on port 5207, headless Chromium at 1280px light: the two new gallery sections (links, `<time>` dates, the check, enum name, the multiline itinerary, the formatted total and date, icon-and-text and text booleans, the clamped itinerary).
- Skill validation and `SkillsGenerator --check` pass.
- Not checked: dark mode, theme fixtures, phone widths; TagHelpers tests (TagHelpers unchanged).

Dropping `LookupDisplay` (80), completed 2026-10-12: the handler path is covered by a test-only `CategoryNameDisplay` (integration) and a mismatched-handler display (unit).

EF Core references (76), completed 2026-10-12: EF integration tests 82 passed (new `ResourceDisplayFieldTests`: `Category.Name` and the owned `Details.Sku` render from one query that joins `Category`; a display field on the key alone doesn't join); Dashboard unit 113 and integration 414 passed.

Built-in displays, completed 2026-10-12:

- Tests: `StellarAdmin.Dashboard.Tests` 113 passed (new: a display whose handler is for another display throws); `StellarAdmin.Dashboard.IntegrationTests` 414 passed (new: `BadgeDisplay` variant, `LookupDisplay` matched and unmatched keys, a full span in details); EF integration 80 passed.
- Playground on port 5207, headless Chromium at 1300px light and dark: the badge, lookup and `[UIHint]` status in the details section; the agent notes span the full row; the create page returns 200.
- Skill validation and `SkillsGenerator --check` pass.

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
