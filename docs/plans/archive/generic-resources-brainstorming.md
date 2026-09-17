# Generic resources brainstorming — 2026-09-08

Status: completed for the agreed implementation scope; session closed 2026-09-10. Basic EF resources, form layouts, editor part classes, and initial EF single-record references are implemented. Deferred work is tracked in the [generic resource follow-up backlog](../generic-resources-follow-ups.md); no follow-up is active or implicitly authorized.

## Session closeout

The completed implementation spans workspace, Pro, OSS, and website on `feature/generic-resources`. The consumer skills repo was not changed. This is a historical session record: earlier statements about uncommitted work, pending migrations, temporary files, or next steps describe their original checkpoints and are superseded by this closeout. Completion here does not mean the feature branch has been merged or packages released.

Final implementation commits: Pro `afa2e5c` (EF references) and `9628982` (playground Tailwind); OSS `4ced920` (remove temporary field-class after Pro integration), following `298bdd8` (control parts and demos); website `d57587ab` (part documentation and demos); workspace `ae0ec45` (reference implementation record). The playground database and requested IDE settings are committed. All five repos were clean at closeout before this documentation update.

Verification completed during implementation: EF resource, reference, scalar editor, and layout integration checks; OSS field rendering checks; website lint, type checks, and production build; Chromium checks for forms and styled control examples at desktop/mobile sizes and in dark mode. Reference SQL assertions verify joined label loading and key/label-only choice projections. The last reference build passed with existing warnings using SDK 10.0.400 from the workspace because the nested OSS SDK pin was unavailable. Tests and previews used temporary databases. Documentation closeout does not rerun those builds or imply that temporary screenshots remain available.

Current limitations include navigation-required EF references, complete dropdown choice lists intended for small datasets, single scalar keys, and no stale-form concurrency token round-trip. An index query transform is not an edit/delete access boundary. See the backlog for the agreed deferred directions and how to resume.

## Direction and implementation order

Extend the resource configuration explored through Pro.Identity into a mechanism for defining ordinary application resources. Keep the first implementation small and develop it in this order:

1. Basic EF Core-backed resources with configured index columns and create/edit fields, plus reading and writing.
2. Form sections and layout containers.
3. Single-record reference fields.
4. Optional operations class for overriding default EF behavior.
5. Consolidate sidebar item registration across Pro packages at the end of this effort.

Use `feature/generic-resources` in both the workspace and Pro repositories for this entire effort, including EF resources, form layout, references, and later operation overrides. Create a feature branch before modifying any additional repository. Do not commit or push without authorization.

This order is Jerrie's requested direction. Phases 1–2 and the initial EF reference slice are implemented on the feature branch. Earlier API sketches below are historical; the latest implementation notes describe the shipped shape. Collection relationships and nested editing are outside the current scope.

Jerrie clarified that StellarAdmin must handle reading and writing for EF-backed resources, with escape hatches for consumers to override behavior where necessary. The default must not require application-written CRUD handlers or controllers. The precise override API remains a design question.

The earlier [consumer resource API sketches](../resource-consumer-sketches.md) compare handlers, consumer controllers, EF convenience registration, and Razor overrides. They remain useful background, but their broader examples are not the implementation scope for this session.

## Existing foundation

`IdentityUsersOptions<TUser, TKey>` and `IdentityRolesOptions<TRole, TKey>` already inherit `ResourceOptions<TEntity>` in StellarAdmin.Pro. The shared `ResourceBuilder<TEntity>` exposes `Index`, `Create`, and `Edit`; their builders configure columns, query transforms, and fields. The base resource controller handles configured-field binding and shared page models, while Identity controllers perform persistence through Identity managers.

Form definitions currently contain a flat list of fields selecting direct properties. Fields support titles, editor templates, and read-only behavior. Only configured writable fields participate in binding. The generic resource work should preserve this separation between presentation and persistence and retain the binding allow-list.

## Phase 1: basic EF resources

The first example should be a simple entity such as Category with an ID, name, and description. Use the existing flat fields and index columns. Exercise list, create, edit, and delete before adding layout containers or relationships.

Jerrie specified that the demo must extend the existing `stellar-admin-pro/sandbox/IdentitySimplePlayground` app and its `Data/ApplicationDbContext.cs`, which already derives from `IdentityDbContext<ApplicationUser, ApplicationRole, string>` and uses SQLite. Add the demo entities to this context and register their resources alongside the existing Identity screens in `Program.cs`. Use EF migrations to extend the existing schema while preserving Identity data. Continue using this app for the later layout and reference demos rather than creating a separate playground or DbContext.

StellarAdmin owns EF CRUD; the proposed consumer experience is:

```csharp
pro.AddEfCoreResource<ApplicationDbContext, Category>("categories", resource =>
{
    resource.AuthorizationPolicy = "ManageCatalog";

    resource.Index(index =>
    {
        index.DefaultSortBy(c => c.Name);
        index.Columns(columns =>
        {
            columns.Add(c => c.Name);
            columns.Add(c => c.Description);
        });
    });

    resource.Create(create => create.Fields(fields =>
    {
        fields.Add(c => c.Name);
        fields.Add(c => c.Description);
    }));

    resource.Edit(edit => edit.Fields(fields =>
    {
        fields.Add(c => c.Name);
        fields.Add(c => c.Description);
    }));
});
```

The application supplies its DbContext registration, mapped entity, and resource configuration. StellarAdmin loads records, binds configured writable fields, validates, and persists changes. Custom services and non-EF backends can be explored later; they should not complicate the initial registration.

### Escape hatches

Jerrie subsequently approved the idea of an optional operations class but explicitly deferred it to a new phase. Phase 1 focuses on the most basic use case with StellarAdmin providing default CRUD, without introducing operation override contracts. Preserve existing customization mechanisms where applicable. The following layers remain design ideas for the later customization phase:

- Query customization: preserve `index.TransformQuery(...)` for list-specific shaping such as includes. Provide an overridable resource query for constraints that must apply to record loading for edit and delete as well as the index. An index filter alone must not be presented as a record-access boundary.
- Operation customization: supply overridable read/create/update/delete operations with the scoped DbContext available. A consumer should be able to customize one operation and keep the defaults for the others, including calling the default implementation. Keep shared binding, validation, antiforgery, and error redisplay outside ordinary persistence overrides.
- View customization: preserve resource-specific Razor overrides and the existing field-template and page-slot mechanisms, so changing markup does not require replacing persistence.
- Full request customization: allow a consumer controller when different HTTP responses or a custom workflow require it. It should replace the registered default controller, without duplicate endpoints.

For example, list customization already has a familiar shape:

```csharp
resource.Index(index =>
{
    index.TransformQuery(query => query.Where(c => !c.IsArchived));
});
```

This example only shapes the index. A common resource query override would be the appropriate place for filtering that must also apply to direct edit/delete requests. The preferred later design is an optional operations class registered through a proposed `resource.UseOperations<CategoryOperations>()`, with virtual CRUD defaults and access to the scoped DbContext. Consumers could override a single operation and call its base implementation. Exact signatures remain open.

Initial implementation questions: resource identity and route naming; EF key discovery and supported key shapes; scoped DbContext access; async query execution and cancellation; validation and save errors; concurrency behavior; and resource-specific Razor view overrides. Keep EF dependencies in an EF integration package rather than adding them to the shared screen builders. The existing options-builder convention keeps EF types out of the core query-transform signatures.

The first working sample should verify persisted CRUD, validation redisplay, exclusion of undeclared/read-only fields from writes, missing-record behavior, and independent routes/configuration for multiple resources. Check authorization and antiforgery on generated write endpoints. Add concurrency verification if concurrency tokens are supported by this first slice; otherwise record the limitation explicitly.

## Phase 2: form sections and layout

Allow the existing `Fields(...)` scope to contain fields and containers. Containers expose the same fields builder so individual field configuration remains consistent.

```csharp
resource.Edit(edit => edit.Fields(fields =>
{
    fields.Add(p => p.Name);

    fields.AddSection("Details", section =>
    {
        section.Description = "How this product appears in the catalog.";
        section.Fields(fields =>
        {
            fields.Add(p => p.Description);
            fields.Add(p => p.CategoryId);
        });
    });

    fields.AddSection("Pricing", section =>
    {
        section.Fields(fields =>
        {
            fields.AddRow(row =>
            {
                row.Fields(fields =>
                {
                    fields.Add(p => p.Price);
                    fields.Add(p => p.CompareAtPrice);
                });
            });

            fields.Add(p => p.Taxable);
        });
    });
}));
```

The `CategoryId` field above is ordinary scalar input until phase 3 adds reference behavior.

| Container | Purpose |
| --- | --- |
| Section | Heading, optional description, and related fields. |
| Group | Fields kept together without a visible heading. |
| Row | Fields side by side, stacking on small screens. |
| Other panels, later | Cards, tabs, or collapsible panels using the same composition pattern. |

The implemented starting set is sections, groups, and responsive rows. Form definitions are a tree of fields and containers: rendering traverses the tree, while binding and validation collect its actual fields. Layout does not change property names or make additional properties writable. Cards, tabs, collapsible panels, column spans, and configurable breakpoints remain deferred.

## Phase 3: single-record references

The current scope is references such as Product → Category. A small set of choices could use a dropdown; larger sets could use a searchable picker. Display the related record's label and save its key. Selecting a related record is separate from editing that record; inline creation and editing are deferred.

Three definition approaches were considered:

- Custom editor templates and application loading/saving code: fits the existing template mechanism but repeats plumbing.
- Explicit relationship definitions: describe the key, label, choice source, and persistence behavior; support consistent columns and editors.
- EF metadata inference: useful for discovering foreign keys and navigations, but insufficient to decide display labels, permitted choices, or editing behavior. Consider as an optional convenience.

The suggested direction is an explicit reference definition shared by columns and fields. A target need not have its own admin resource merely to supply choices. Resource links and filters are possible later uses, not part of the initial field implementation.

### EF query requirement

Jerrie explicitly requires related labels to come from navigation properties loaded through `.Include()` in the resource query. Do not populate row labels with separate lookup queries, even batched queries. Apply the same principle to the current selection on the edit screen.

```csharp
resource.AddReference(
    p => p.CategoryId,
    p => p.Category,
    reference =>
    {
        reference.Title = "Category";
        reference.DisplayMember(c => c.Name);
    });
```

The proposed EF adapter derives the equivalent of `query.Include(p => p.Category)` from the definition. A configured `CategoryId` column displays `product.Category?.Name`; a configured `CategoryId` form field becomes a category picker whose current label uses the loaded navigation. Sorting and filtering must operate on the related display property in the database rather than the foreign key or an in-memory label lookup.

Loading available choices when opening or searching a picker still requires querying selectable categories: including a product's assigned category does not load all categories. This was explained during brainstorming; the prohibition concerns follow-up queries to populate resource row labels and current selections. The choice-source API remains to be designed. A nullable foreign key permits clearing a selection; submitted choices need server-side validation against the application's permitted set.

### Earlier lookup sketch and its limits

Before the EF requirement was clarified, a provider-based API was sketched:

```csharp
resource.AddReference(p => p.CategoryId, reference =>
{
    reference.Title = "Category";
    reference.UseLookup<CategoryLookup>();
});
```

The proposed `IReferenceLookup<TKey>` offered `SearchAsync(search, cursor, ct)` returning a page of key/label choices, plus `ResolveAsync(ids, ct)` resolving existing IDs in batches. This could serve custom services or APIs later. It is not the EF display-loading design: EF must use the explicit navigation and eager loading described above. None of these lookup contracts exist yet.

### Relationship ideas intentionally deferred

Collections of existing records, such as User → Roles, could use multiselect or attach/remove tables, where unlinking does not delete the target. Owned child records, such as Order → Order lines, could use embedded tables or nested forms with coordinated validation and saving. Both were discussed for context and excluded when Jerrie narrowed the scope to single-record references.

## End-of-work sidebar registration cleanup

Jerrie requested that we address sidebar registration at the end of this effort. All Pro packages should be able to add sidebar items through a simple method on `StellarAdminProBuilder`, instead of registering an `ISidebarItemsProvider` for each controller/resource. Consolidate the standard registration path and migrate the Pro packages to it. Keep `ISidebarItemsProvider` available as an escape hatch for custom behavior. The exact builder method signature remains to be designed; this is a deferred cleanup, not part of the current implementation slice.

## Historical implementation and verification

The session is closed. Consult the [follow-up backlog](../generic-resources-follow-ups.md) when Jerrie selects further work; the checkpoints below record implementation history.

During this session, inspected the existing resource options/builders, field binding, Identity user persistence, MVC controller registration, and index query execution. The index executor currently materializes synchronously; controller registration currently identifies registrations by CLR controller type. Account for those constraints when implementing async EF execution and resource identity.

### Phase 1 implementation and verification

Added `StellarAdmin.Pro.EntityFrameworkCore` with `AddEfCoreResource<TContext, TEntity>(name, configure)`. It supplies async EF list/create/edit/delete, shared resource views, sidebar navigation, configured-field binding and validation, antiforgery, and an optional `AuthorizationPolicy`. Page titles use the existing page builders (`index.Title`, `create.Title`, `edit.Title`); there is no root `resource.Title` property. Registration names identify controller routes and must be unique. One registration per entity/DbContext pair is supported.

The EF controller uses the shared index query executor through an internal async path, preserving Identity's synchronous path without introducing EF dependencies in the core library. Existing per-controller Razor overrides remain available. The controller registry now rejects name/type collisions instead of silently producing ambiguous routing.

IdentitySimplePlayground registers Category alongside Identity and extends ApplicationDbContext with Categories. The generated `AddCategories` migration only creates the Categories table; its snapshot also reflects the playground's already-existing custom Identity entity types. The checked-in `app.db` has not been modified. Apply the migration before trying Categories against that database:

```bash
dotnet ef database update --project stellar-admin-pro/sandbox/IdentitySimplePlayground
```

Supported keys are single CLR properties of type int, long, Guid, or string. Writable form fields must be mapped scalar properties and cannot be keys, generated properties, or concurrency tokens. Application-assigned keys can be initialized by the entity constructor or existing `CreateInstanceUsing(...)`. Composite/shadow keys, stale-form concurrency detection, reference editing, layout containers, and operation overrides are not implemented. Edit loads the current entity again on POST; there is no concurrency token round-trip from GET to POST. Save failures redisplay a generic error and log the underlying exception.

Added an executable HTTP integration suite using WebApplicationFactory and a temporary SQLite database. It passed persisted CRUD, required-field redisplay, unconfigured/read-only field protection, malformed and missing keys, search, sorting, paging/clamping, multiple resources, string keys, authorization across all CRUD endpoints, antiforgery, continued Identity rendering, and preservation of an existing Identity role through the Category migration.

```bash
dotnet build stellar-admin-pro/tests/StellarAdmin.Pro.EntityFrameworkCore.Tests -m:1
dotnet run --project stellar-admin-pro/tests/StellarAdmin.Pro.EntityFrameworkCore.Tests --no-build
```

The playground and integration suite build successfully using installed SDK 10.0.400 from the workspace (the nested OSS pin is 10.0.100, which is not installed). The initial parallel build failed without diagnostics; single-node `-m:1` succeeded. Test dependency restore required network access. Build warnings include existing XML documentation/nullability warnings and the existing SQLitePCLRaw.lib.e_sqlite3 2.1.11 vulnerability warning. CSharpier 1.3.0 formatted touched code using its cached net10 DLL after the normal invocation encountered the unavailable SDK pin. No browser visual check was performed; HTTP tests exercise the rendered shared views. Workspace and Pro diff whitespace checks pass. No other repo has source changes; no commits or pushes were made.

Form sections/layout are now implemented and awaiting visual review. Single-record references remain the next phase, followed by optional operations overrides.

### Product playground follow-up

Added a second resource, Product, to IdentitySimplePlayground's existing ApplicationDbContext. It has general details, prices, inventory settings, shipping measurements, publishing fields, and an automatically initialized read-only CreatedAt timestamp. Create/edit forms remain flat. The optional Category relationship exists in the EF model but is excluded from forms and columns until reference fields are implemented. The index displays name, SKU, price, stock, and publishing status, with name/SKU search and All/Published/Draft scopes.

Generated `20260908121549_AddProducts` using `dotnet ef migrations add AddProducts --project stellar-admin-pro/sandbox/IdentitySimplePlayground --output-dir Data/Migrations --no-build` after building the playground. The migration adds Products and its optional Category foreign key/index; it has not been applied to the local playground database. Existing local app.db changes were preserved.

The playground and executable integration suite build successfully. The full suite passes, including additional Product CRUD checks, required SKU/nonnegative price validation, decimal/boolean/date persistence, nullable-field clearing, SKU search, and protection of the excluded CategoryId and read-only CreatedAt. Verification used a temporary SQLite database. Existing SQLite dependency and ApplicationUser nullability warnings remain. No layout or reference UI was added.

### Scalar editor templates

Added shared Pro editor templates for multiline text, all standard signed/unsigned integer types, decimal/currency, single/double, booleans, enums, Guid, email/URL, date/time, DateOnly, TimeOnly, and DateTimeOffset. Nullable value types reuse their underlying template; nullable booleans and enums offer a blank “Not set” option. Templates use existing StellarAdmin controls for labels, descriptions, validation, and styling. Form field titles and read-only settings now reach the templates through a single FormFieldProperties record stored under ViewDataKeys.FormFieldProperties, including the existing string/password/phone editors. Read-only checkboxes and selects are disabled; server-side binding exclusions remain in place.

Fractional numeric inputs preserve precision and allow any step, including currency rather than silently rounding values to two decimals. DateTime uses a local datetime input without timezone conversion. DateTimeOffset uses an ISO text input that includes its offset, avoiding implicit timezone conversion; a richer timezone-aware control remains a future design choice. Flags enums use a text input for combinations rather than a single-choice dropdown.

Verified all scalar templates through HTTP rendering checks, including labels, styles, enum display names, nullable boolean states, read-only controls, metadata descriptions, attempted values and errors after validation failure, date formats, decimal precision, and unsigned integer limits. Added binding checks for dates/times, explicit offsets, nullable clearing, and precision. The full EF resource integration suite passes, including Product CRUD and Identity rendering. Build used SDK 10.0.400 from the workspace with single-node MSBuild; existing warnings remain. No browser visual check was performed. The local playground database was left untouched.


### Form sections, groups, and rows

Implemented AddSection(title, configure), AddGroup(configure), and AddRow(configure) on FormFieldsBuilder. Each container exposes Fields(...) with the same typed field builder. Section titles and descriptions are scalar properties. Clear() affects only the current collection; root Clear() removes all defaults and containers. Existing field Title(), Template(), and ReadOnly() settings work at any depth.

FormPageOptions.Items retains the layout tree; Fields returns its fields in depth-first display order for the existing binding and EF metadata validation paths. Shared form view models expose both. Rendering uses overridable _FormItems, _FormSection, _FormGroup, and _FormRow Razor partials while preserving the entity model and binding prefix. Sections render sa-field-set, sa-field-legend, optional sa-field-description, and sa-field-group; groups render sa-field-group. Rows use sa-field-group with a Pro CSS grid rule: one column below a 40rem parent field-group container width, and equal-width columns above it. Layout definitions and CSS live in Pro; no OSS changes were necessary.

IdentitySimplePlayground now shares ProductForm.Configure between Create and Edit, with Details, Pricing, Inventory, Shipping, and Publishing sections. The demo exercises two- and three-column rows, an untitled checkbox group, and a root read-only creation timestamp. No reference fields or database migrations were added.

Verification passed: Pro CSS build, single-node .NET build with installed SDK 10.0.400, and the full executable integration suite. Added checks cover seeded defaults, recursive field order, nested and root Clear(), nested field settings, section/group/row rendering, original binding prefixes, create/edit/invalid-post layout, and read-only protection inside nested containers. Existing CRUD, scalar editor, and Identity rendering checks continue to pass. Chromium checks at 1280px and 390px confirm equal-width desktop rows, mobile stacking, and no horizontal overflow; desktop dark-mode layout was also checked. Screenshots are in /tmp/form-layout-desktop.png, /tmp/form-layout-mobile.png, and /tmp/form-layout-desktop-dark.png for this session.

Browser verification also exposed that seven fractional-second digits cause native date/time inputs to display an empty value in Chromium. DateTime, Time, and TimeOnly editors now format to milliseconds, which native inputs support; DateTimeOffset's explicit offset format is unchanged. Sub-millisecond precision is therefore not represented by those native editors. The read-only CreatedAt timestamp now displays correctly, and the existing server-side protection still preserves its stored value.

Changes remain on feature/generic-resources in the workspace and Pro repositories, uncommitted for user review. The user's local playground database was preserved; automated checks and the local browser preview used temporary databases. Remaining work starts with the user's review of this layout, then the separately planned reference-field phase.

### Playground entity metadata cleanup

Category and Product now use ModelMetadataType<CategoryMeta> and ModelMetadataType<ProductMeta>, following the existing Identity playground pattern. Their EF required/maximum-length/precision configuration lives in ApplicationDbContext.OnModelCreating. The companion metadata classes hold MVC validation and editor annotations, with Display names and descriptions on every property. Category's description uses the multiline editor. Product form/index label overrides were removed where metadata now provides the label, and every Product section has descriptive text.

The build and full integration suite pass, including new checks for MVC metadata on every Category/Product property, rendered labels/help text, Category maximum-length validation, and Product section descriptions. EF reports no pending model changes against the existing migration snapshot, so no migration is needed. Tests used a temporary database; the existing local app.db changes were preserved.

### Product condition and enum radio editor

Added ProductCondition (New = 0, Refurbished = 1, Used = 2) and Product.Condition, defaulting to New. ProductMeta provides its display label, description, EnumDataType validation, and UIHint("EnumRadioGroup"). The shared Product form adds Condition to Details using a plain fields.Add(...) call. Enum properties still automatically resolve to the existing dropdown editor unless a UIHint or builder Template selects another editor.

Added the Pro EnumRadioGroup editor using sa-field-set/legend/description/error, sa-field-group, and labeled sa-input radio controls. It uses enum display names, unique per-option IDs, one group description/error, and read-only settings. Nullable enums expose a blank option; flags enums retain a text input because combinations do not fit a single-choice radio group.

Generated 20260909070407_AddProductCondition with dotnet ef migrations add AddProductCondition --project stellar-admin-pro/sandbox/IdentitySimplePlayground --output-dir Data/Migrations --no-build. It adds an integer Condition column with 0 (New) for existing rows. The migration was exercised against the integration suite's temporary database and has not been applied to the user's local app.db.

Build and the full integration suite pass, including automatic UIHint selection, initial New selection, validation redisplay retaining Used, persisted create/edit enum values, rejection of undefined values, label IDs, and nonduplicated help text. Chromium checks cover desktop/mobile/dark layout, native arrow-key selection, and clicking option labels. A desktop screenshot is available at /tmp/product-condition-desktop.png for this session. The installed EF tool reported version 10.0.9 versus runtime 10.0.10; migration generation succeeded. No commits or pushes were made.

### Enum choice cards

Added EnumRadioChoiceCards alongside EnumRadioGroup and switched ProductMeta.Condition to UIHint("EnumRadioChoiceCards"). The template follows the existing DocsSamples Radio choice-card markup, using a clickable sa-field-label around a horizontal sa-field with sa-field-content/title/description and a radio input. Card titles use enum Display names; optional descriptions come from each enum member's DisplayAttribute.GetDescription(). ProductCondition now defines descriptions for all three choices. The ordinary radio editor remains available.

The full integration suite passes with checks for card structure and enum descriptions, in addition to existing selection, validation, and persistence checks. Chromium verification at desktop/mobile sizes and in dark mode confirms initial selection, arrow-key navigation, clicking the card body, and no horizontal overflow. The desktop preview is /tmp/enum-choice-cards-desktop.png. No additional schema change was required; the existing uncommitted AddProductCondition migration remains unapplied to the local playground database.

### Editor options

Added EditorOptions.Class and FormFieldBuilder.Editor(configure), passed through FormFieldProperties.Editor. Every built-in editor applies Class to its outermost element while preserving structural classes and metadata-based template selection. A brief code comment in EditorOptions records future typed options for masks and choice items. Product Condition configures the product-condition class; its responsive CSS is owned by the playground's admin.css and registered with AddStylesheet, replacing the experimental hardcoded template classes.

The OSS field input base now accepts field-class for its automatically generated wrapper; this required creating feature/generic-resources in the OSS repository as well. The build and full integration suite pass, including per-field option isolation, nested configuration, and outer-wrapper class placement across scalar editors. Browser checks confirm the configured choice cards remain horizontal on desktop, stacked on mobile, and interactive in light/dark mode. The normal OSS theme build unexpectedly produced empty files through its stdin entry path in this session; the same Tailwind CLI successfully rebuilt all themes using temporary file entries, and the preview was restarted to clear stale static responses. No build-script changes were made. Local database and IDE edits remain preserved; changes are uncommitted.

### OSS field part classes (2026-09-09)

Implemented the reviewed OSS-first styling API on `feature/generic-resources`: `class-names` accepts a typed object on all seven field-input helpers (input, textarea, select, switch, slider, OTP, and toggle group). `FieldClassNames` supplies `Root`, `Label`, `Description`, `Error`, and `Content`; specialized types expose the supported control parts. Classes supplement existing styling. `Root` applies only when the automatic field wrapper exists; explicitly composed fields map wrapper and text classes through their own `class` attributes. OTP and toggle groups pass classes to their composed descendants through typed contexts.

Added a generic field-input base to expose the specialized `ClassNames` property without accepting unrelated option types. Corrected MVC class-content serialization and preserved validation-message classes when custom classes are supplied. The existing uncommitted `field-class` attribute remains temporarily compatible with Pro; replacing it and adapting `EditorOptions` and the Pro templates are the next phase, not implemented in this OSS pass.

Verification: the OSS DocsSamples build passed, the new executable field-rendering checks passed for all input families (including model-state values and errors), and Chromium verified the Field page's new Part Classes sample: automatic and explicit wrappers, labels, inputs, descriptions, absence of leaked bound attributes, and label focus. The Pro integration-test project also built successfully and its existing EF resource integration checks passed against the updated OSS helpers. Existing build warnings remain. Commands ran from the workspace with installed SDK 10.0.400. The previously recorded empty generated theme-bundle issue recurred; regenerated ignored bundles using temporary file entries and the installed Tailwind CLI, leaving the build script and dependencies unchanged.

OSS source, sample, and tests remain uncommitted. Workspace changes add the test project to the umbrella solution and record this handoff; existing Pro changes, local playground database, and personal solution settings are preserved.

### Field part classes documentation review (2026-09-09)

Added the ClassNames sample to DocsSamplesGenerator and its model mapping to DocsStatic. The website Field page includes the generated demo, Razor/CSS tabs, all five shared FieldClassNames parts, and implicit versus explicit field behavior. Website is on feature/generic-resources. Full generation ran; unrelated output was restored from a pre-generation snapshot, retaining the new example, snippet, and required sample stylesheet.

Verified the generator build (existing warnings), generated HTML and asset references, browser rendering and computed styles, and website lint/type checks/production build. The compiled Field page module includes the new documentation and demo. Used the installed SDK from the workspace and regenerated empty local theme bundles using the documented workaround. Paused for review of this first page; document component-specific parts on the remaining input pages after approval.

### Remaining part classes documentation (2026-09-09)

Added top-level Part classes sections to Input, Checkbox, Radio, Textarea, Select, Switch, Slider, Input OTP, and Toggle Group. Each includes a configuration example, all shared and component-specific properties, and a class-names API entry. Documented model-bound radio error handling, OTP composed children and carets, and per-item toggle classes. Sections follow the approved Field heading structure without a Shared field parts subheading.

Verified table coverage against the public options classes, whitespace checks, website lint/type checks/production build, and all nine compiled page modules. The existing generated Field demo remains the shared styled example. Changes are uncommitted; Pro editor integration remains a separate next phase.


### Simplified control parts and individual demos (2026-09-09)

Simplified Input, Textarea, Select, and Switch class options to Control plus the inherited field parts. Control targets the native text input or textarea and the visible checkbox, radio, select, or switch wrapper, including when render-field is false. Removed Input/Indicator/Icon and Switch Thumb options. OTP retains Control, Group, Slot, and Separator; Slider retains Control, Track, Range, and Thumb; Toggle Group retains Control and Item. Pro editor integration remains a separate phase.

Added styled Part Classes examples last on all nine affected DocsSamples pages and registered them with DocsSamplesGenerator in Pro. Updated each website page with its own generated demo, Razor/CSS tabs, and revised parts table; updated the shared Field example to use Control. Preserved encodings and unrelated existing Pro edits.

Validation: rendering checks and DocsSamplesGenerator build pass; generated 396 demos and retained the relevant exports and assets. Website lint, type checks, and production build pass. Chromium checks cover all nine demos at desktop/mobile widths in light/dark mode, visible class effects, selection, slider keyboard input, OTP typing, and overflow. Used installed SDK 10.0.400 from the workspace because the nested 10.0.100 pin is unavailable; repaired empty ignored local theme bundles before export. Changes span OSS, website, Pro (generator registration only), and this workspace record; no commit or push.


The Checkbox, Radio, and Switch class demos now override --primary and --primary-foreground on their Control classes so selected backgrounds use the travel accent with contrasting indicators. The Switch demo also scopes a checked background rule to its control because the existing dark-mode background otherwise overrides the checked color. Updated the CSS tabs and regenerated demos; Chromium confirms green selected backgrounds and selection behavior at desktop/mobile widths in light/dark mode.


### Pro editor part classes (2026-09-09)

Replaced the temporary EditorOptions.Class with EditorOptions.ClassNames (EditorClassNames), which exposes the shared OSS field parts plus Control. Scalar templates convert these options to InputClassNames, SelectClassNames, or TextareaClassNames through a shared internal mapper. Nullable Boolean and flags-enum branches retain their existing controls. FormFieldProperties remains the single ViewData entry carrying editor settings, title, and read-only state.

```csharp
fields.Add(product => product.Name)
    .Editor(editor =>
    {
        editor.ClassNames.Root = "product-field";
        editor.ClassNames.Control = "product-input";
        editor.ClassNames.Label = "product-label";
    });

fields.Add(product => product.Condition)
    .Editor<RadioEditorOptions>(editor =>
    {
        editor.ClassNames.Control = "product-condition";
        editor.ClassNames.Option.Root = "product-condition-card";
    });
```

RadioEditorOptions has a covariant ClassNames getter returning RadioEditorClassNames, so ordinary and typed configuration mutate the same class object. Upgrading from common options preserves existing settings; repeated typed calls compose; conflicting specialized option types fail with the field name. Configuration does not select the template: explicit Template or MVC metadata still does that.

Both enum radio editors map Root to the fieldset, Label to the legend, Description/Error to overall supporting text, and Control to the choices group. Option uses InputClassNames: Root is the outer choice field or clickable card label, Label is the option label or card title, Description/Content style option text, and Control passes to the OSS radio wrapper. Radio editors reject outer Content and Option.Error because those elements do not exist; use Option.Content and group Error. Scalar templates reject incompatible specialized options, including RadioEditorOptions on flags-enum text fallbacks. Custom templates can consume their own EditorOptions subclasses through FormFieldProperties.Editor. Future typed behavior such as masks and choice sources remains deferred, with a brief note in EditorOptions.

The Product form now applies its responsive grid class directly to the choices group and applies the equal-height card class to each option. Removed the temporary OSS field-class attribute after migrating all Pro templates. This phase touches Pro, OSS, and the workspace; the website and consumer skills are unchanged.

Validation: the full EF resource integration suite and OSS field rendering checks pass. Tests cover every scalar template, both radio layouts, nullable choices, metadata descriptions, validation redisplay, common/typed configuration ordering and isolation, flags fallbacks, and incompatible options. Chromium verifies the Product form at desktop/mobile widths and in dark mode, including horizontal versus stacked cards, matching heights on desktop, keyboard selection, card clicks, and no overflow. Builds pass with existing warnings, using installed SDK 10.0.400 from the workspace; empty ignored theme bundles were repaired before the browser check. Preview used a temporary database; local app.db and personal solution settings were preserved. No commit or push.

### Identity playground Tailwind (2026-09-09)

Added the ComponentPlayground Tailwind build pattern to IdentitySimplePlayground: the same dependency versions and lockfile, shared theme token and utility imports, and the forms plugin in class mode. Explicit sources scan Pages and Forms, including C# editor configuration. The app registers the generated, ignored admin.css alongside the Stellar Admin theme. Condition now uses `grid grid-cols-1 @[40rem]/field-group:grid-cols-3` and `[&>[data-slot=field]]:h-full`; the handwritten admin.css is replaced by the generated output from Client/css/admin.css.

Validation: the playground build passes with existing warnings using installed SDK 10.0.400. Chromium confirms horizontal equal-height cards on desktop, stacked cards on mobile, dark mode, keyboard and card-click selection, and no overflow. Used a temporary database and stopped the preview. Changes touch Pro and this workspace plan only; no remaining work for this Tailwind setup.

Follow-up: exclude Tailwind Preflight from the app bundle by importing theme and utilities separately. A second Preflight loaded after the Stellar Admin theme reset default borders to currentColor. Browser verification confirms category row borders now match the theme border token; the app retains the theme’s existing base styles.

## Initial EF references (2026-09-10)

The consumer defines a reference once, then adds its foreign-key property to ordinary columns and form layouts:

```csharp
resource.AddReference(
    product => product.CategoryId,
    product => product.Category,
    category => category.Name,
    reference => reference.Choices(choices =>
    {
        choices.TransformQuery(query => query.Where(category => category.Name.StartsWith("Travel")));
        choices.DefaultSortBy(category => category.Name);
    })
);
```

The fourth argument is optional. Without it, choices include the target DbSet's records (subject to EF global query filters) and sort by the display property. Selectors must be direct properties; the EF mapping must have a single scalar foreign key on the dependent navigation and an int, long, Guid, or string principal key. The target does not need an admin resource. Composite keys, collection references, and external sources are outside this slice.

The EF controller applies Include to referenced columns and edit fields. Index cells and sorting use the navigation display property while the column retains its foreign-key sort name and metadata label. No separate queries resolve index labels or saved selections. Dropdown choices are the intentionally separate query, projecting only key and label, for reference fields on the current form. Choice transforms compose and also govern submitted-key validation. The form page model carries ReferenceChoices keyed by field name; the renderer passes each list through FormFieldProperties.Choices to the Reference editor. asp-for and ModelState determine selection, including invalid POST redisplay. Choices are request-local and reused for validation and redisplay.

The editor is a sa-select with existing metadata, help text, validation, read-only handling, and Select class-name mapping. Nullable EF relationships offer Not set; required relationships show a prompt and reject empty keys. Nonexistent, malformed, and filtered-out submitted keys cannot save. A previously assigned record outside the selectable set remains visible as a disabled selected option using its included navigation label; saving that disallowed value is rejected unless the field is read-only, in which case posted changes are ignored. Default dropdown behavior loads the complete selectable set, so this first version is intended for small lists.

Product now registers Category, adds its sortable category column and select within Details, and gives CategoryId a Category label and consumer-facing help text. The entity and database already contain this relationship, so no migration is needed.

Deferred by explicit agreement: configurable editors (Select, Autocomplete, radio/choice cards, custom editor options) and DI-created non-EF reference sources for web-service lookups. No editor-selection API or external-source contract was added. The brief reminder also lives in EfCoreReferenceBuilder.

Validation: full EF integration suite passes, including reference choice filtering/order, create/change/clear, ModelState redisplay, required references, disabled read-only references and forged POST protection, filtered current labels, database label sorting, and SQL assertions for Include joins and key/label-only choice projections. Existing scalar editor and layout checks pass; updated the stale pre-Tailwind Condition class assertion. Tests use an ephemeral data-protection provider to avoid writing user key storage. Build uses installed SDK 10.0.400 from the workspace and passes with existing warnings. Chromium checks the select and available choices at desktop/mobile widths and in dark mode without overflow, using a temporary database. No commits or pushes for this phase.
