# CRUD screens through high-level Tag Helpers

Status: proposed. Last updated: 2026-09-06.

Scope: design exploration and non-compiling consumer sketches in the workspace. Future high-level components belong in `StellarAdmin.Pro`; existing OSS controls remain the field primitives. No product implementation, migration, or publishing is authorized by this plan.

Related exploration: [resource consumer sketches](resource-consumer-sketches.md). This is a separate alternative, not an amendment to or implementation of that proposal.

## Direction

Help consumers rapidly build the presentation of CRUD screens while they own ordinary MVC controllers or Razor Page handlers, input models, queries, and writes. A screen should need a short Razor file and normal application code, without resource registration, a library handler interface, or a special controller base class.

The data grid provides the precedent: the application supplies items and handles querying; the component renders a consistent, customizable UI. Apply that division to edit and display screens.

This is a narrower product promise than automatic CRUD. It saves layout and field markup, but the application still writes GET/POST handling, mapping, authorization, and persistence. The experiment must evaluate the whole consumer task, including that remaining work. If handler repetition later proves to be the bottleneck, starter templates or scaffolding could address it without introducing a runtime resource framework.

## What already exists

| Existing capability | Implication |
| --- | --- |
| OSS `sa-input asp-for`, `sa-textarea asp-for`, and `sa-select asp-for` | Already provide compact bound controls with automatic field wrappers, labels, descriptions, and field errors. Do not invent another mandatory editor abstraction to reproduce this. |
| OSS field groups, field sets, legends, and page headers | Much of the visual vocabulary exists. New Pro components must save meaningful composition work beyond renaming these elements. |
| Pro `sa-data-grid` | Accepts application-supplied items, declarative columns, and custom content/templates. Keep it as the list and related-record component. |
| Pro `sa-form-page` | Already renders a packaged Razor view, but requires `ResourceFormPageViewModel`, whose entity and field definitions are populated internally by the resource layer. It is not currently a general-purpose composition surface for a consumer's own view model. |
| `StellarAdminTemplatedTagHelperBase` | Supports an instance `view` override and named slots through normal partial-view resolution. Its current implementation discards ordinary child output, so the proposed body-content composition needs explicit support. |

Inspected sources: [field input base](../../stellar-admin/src/StellarAdmin.TagHelpers/TagHelpers/Field/FieldInputBaseTagHelper.cs), [implicit form example](../../stellar-admin/docs/DocsSamples/Pages/Field/_Implicit.cshtml), [data grid](../../stellar-admin-pro/src/StellarAdmin.Pro/TagHelpers/DataGrid/DataGridTagHelper.cs), [form page helper](../../stellar-admin-pro/src/StellarAdmin.Pro/TagHelpers/FormPage/FormPageTagHelper.cs), [form page view model](../../stellar-admin-pro/src/StellarAdmin.Pro/Areas/StellarAdmin/ViewModels/ResourceFormPageViewModel.cs), and [templated helper base](../../stellar-admin/src/StellarAdmin.TagHelpers/TagHelpers/StellarAdminTemplatedTagHelperBase.cs).

## Three possible levels

| Approach | Consumer writes | Assessment |
| --- | --- | --- |
| Layout composition | Existing inputs inside reusable screen, section, and action components | Recommended starting point: explicit, familiar Razor and easy customization. |
| Declarative field renderer | Something like `sa-editor asp-for="Input.Name"`, optionally choosing an editor template | Consider only if template selection saves enough work over existing input helpers or `EditorFor`. It does not need resource registration. |
| Automatic form from a model | One tag discovers and renders model properties | Very short for simple cases, but introduces field inclusion, ordering, grouping, relationship choices, and override configuration. Defer until consumer evidence justifies it. |

Start with layout composition plus a compact display-field component. A display counterpart is useful because read-only screens currently lack the convenience already available for inputs.

## Candidate vocabulary

All new tags, attributes, template paths, and behaviors below are proposals. These sketches have not been compiled. Existing controls are used where possible, but the containing examples are not runnable today.

| Proposed component | Responsibility |
| --- | --- |
| `sa-record-page` | Consistent title, description, header actions, and body layout for create, edit, and details screens. Does not emit a form or infer routes. |
| `sa-form-section` | Title, description, responsive field layout, and spacing; candidate `columns="2"` means two columns when space permits and one on small screens. Uses appropriate grouping semantics. |
| `sa-form-actions` | Consistent placement of consumer-authored buttons and links. No implicit save, cancel, or delete behavior. |
| `sa-details` | Labeled-value layout, with optional title and column count, normally using definition-list semantics. |
| `sa-display-field` | One labeled value from `asp-for`, metadata, formatting, or custom child content. Never emits an input. |

Use existing `sa-slot-content name="actions"` for record-page header actions. Ordinary children supply the body. Avoid introducing separate create-page, edit-page, and details-page tags until their behavior actually differs.

Names are deliberately distinct from the existing resource-driven `sa-form-page`. Review whether a common implementation can eventually serve both, but do not change that existing API as part of the first experiment.

## Sketch: a small edit screen

The view uses an application-owned `EditProductPage` containing `Input`, `Version`, `Id`, and `CategoryOptions`. `Input` is a dedicated writable model with normal validation/display annotations. `CategoryOptions` is a list of `SelectListItem` supplied by the application.

```razor
@model EditProductPage

<sa-record-page title="Edit product" description="Update catalog information.">
    <form asp-action="Edit" asp-route-id="@Model.Id" method="post">
        <input asp-for="Version" type="hidden" />
        <div asp-validation-summary="ModelOnly"></div>

        <sa-form-section title="Product information" columns="2">
            <sa-input asp-for="Input.Name" />
            <sa-input asp-for="Input.Price" />
            <sa-select asp-for="Input.CategoryId" asp-items="Model.CategoryOptions">
                <option value="">Choose a category</option>
            </sa-select>
            <sa-input asp-for="Input.IsPublished" />
        </sa-form-section>

        <sa-form-section title="Description">
            <sa-textarea asp-for="Input.Description" />
        </sa-form-section>

        <sa-form-actions>
            <sa-button type="submit">Save product</sa-button>
            <a asp-action="Index">Cancel</a>
        </sa-form-actions>
    </form>
</sa-record-page>
```

The native `form` deliberately remains visible. MVC route attributes, Razor Pages handlers, multipart uploads, validation summaries, and antiforgery follow normal ASP.NET Core conventions. Turning a custom element into a `form` at render time does not automatically cause the framework Form Tag Helper to run on it; avoiding a custom form tag also avoids having to reproduce that integration. Framework-bound inputs use model metadata and ModelState, which the new layout layer must preserve. See [Microsoft's forms documentation](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/working-with-forms?view=aspnetcore-10.0).

A create screen uses the same field markup, changes the title/action, and omits the concurrency token when not needed. Consumers can extract an ordinary `_ProductFields.cshtml` partial with a shared, typed form view model. Reuse is optional; no library schema class is required.

For Razor Pages the same composition sits inside `<form method="post" asp-page-handler="Save">`, and inputs can still use `asp-for="Input.Name"`. No component should assume an MVC area or the resource layer's `Entity` prefix.

### Remaining application work

The consumer's GET loads an authorized record, maps it into `Input`, supplies category options, and carries the version if their write path uses concurrency checks. Their POST binds an input model, validates permissions and submitted relationships, invokes their chosen write operation, and redirects on success. On failure it adds appropriate ModelState errors and repopulates option/display data before rendering the view again.

An EF consumer queries and updates through their own DbContext; an API consumer calls their own client. Both use the same Razor. There is no EF-specific presentation adapter to build in this approach.

This also leaves overposting control with ordinary input-model design. A field's presence or absence in the layout is not a binding or authorization rule.

## Sketch: record details

```razor
@model ProductDetailsPage

<sa-record-page title="@Model.Product.Name" description="Product details">
    <sa-slot-content name="actions">
        <a asp-action="Edit" asp-route-id="@Model.Product.Id">Edit product</a>
    </sa-slot-content>

    <sa-details title="Catalog information" columns="2">
        <sa-display-field asp-for="Product.Name" />
        <sa-display-field asp-for="Product.Price" format="{0:C}" />
        <sa-display-field asp-for="Product.CategoryName" label="Category" />
        <sa-display-field asp-for="Product.IsPublished" label="Availability">
            @(Model.Product.IsPublished ? "Published" : "Draft")
        </sa-display-field>
    </sa-details>

    <sa-details title="Description">
        <sa-display-field asp-for="Product.Description" empty-text="No description" />
    </sa-details>
</sa-record-page>
```

Proposed display precedence: explicit child content replaces value rendering; otherwise an explicit template selects rendering; otherwise use metadata-aware default formatting. An explicit label overrides the property's display name. Explicit format overrides metadata formatting for default value rendering. Null gets an empty placeholder; zero and false remain real values. Encode ordinary values and do not enumerate navigation properties or load data while rendering.

Prefer alignment with existing grid formatting conventions. Whether display fields can share grid templates needs investigation: the grid currently resolves `GridDisplayTemplates`, and a record display template may need different context. Do not promise interchangeability before verifying model and metadata contracts.

## Relationships become ordinary UI composition

| Relationship | Presentation | Application responsibility |
| --- | --- | --- |
| Product belongs to category | Existing `sa-select` bound to `Input.CategoryId`, with supplied options | Load available choices and validate the submitted ID when saving. |
| Product has tags | A multiple select bound to `Input.TagIds`, or an existing/custom selection control | Supply choices and reconcile membership through the application's write operation. |
| Customer has orders | Existing data grid on the customer details page | Query the customer's orders, handle paging/filtering, provide create/edit links, and enforce the parent scope in handlers. |
| Order contains editable lines | Consumer-authored indexed inputs/partial, with layout helpers for each line | Collection binding, add/remove behavior, validation, and saving the aggregate. A general repeater is outside the initial scope. |

Large relationship sets eventually benefit from a remote combobox. That is a separate control capability with an application-supplied endpoint and a contract for search and selected values; it does not require a resource registry. Start with supplied options and existing controls to avoid making lookup infrastructure a prerequisite for form layout.

Related records can appear after `sa-details` in the same record-page body. A normal heading, create link, and `sa-data-grid items="Model.Orders"` are sufficient; a dedicated related-resource tag is not necessary unless repeated consumer markup demonstrates a clear benefit.

## RCL customization is a requirement

The consuming app owns its complete page from the start, so changing page structure is simply editing its Razor. Packaged high-level components should additionally render through overridable Razor partials, retaining the original requirement for replacing library markup. RCL same-path view/page replacement is supported by ASP.NET Core; component-specific discovery and context preservation still need verification. See [Microsoft's RCL documentation](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/ui-class?view=aspnetcore-10.0).

Provide three useful customization levels:

1. Ordinary child content and named slots for local composition, custom controls, and actions.
2. An instance `view` attribute, following the existing templated helper, to replace one component's layout.
3. Host partials at documented library paths to replace default component markup application-wide.

Candidate default partials are `/Views/Shared/StellarAdmin/Records/_RecordPage.cshtml`, `_FormSection.cshtml`, `_FormActions.cshtml`, `_Details.cshtml`, and `_DisplayField.cshtml`. They must work from MVC views, areas, and Razor Pages without requiring a StellarAdmin area. These paths are proposals, not existing files. Prefer a local named lookup with a packaged fallback if it works consistently; verify precedence before documenting it as an API.

Component view models should describe presentation: title, description, rendered body, slots, and layout settings. They should not require resource definitions, DbContext, or an untyped editable entity. Capture consumer body markup in its original view context so its `asp-for` expressions retain the consumer's model and prefix; do not re-execute that body against the component's presentation model.

The existing templated base evaluates children for named slots and discards other output. A future implementation must explicitly capture and render ordinary body content exactly once, without duplicating forms, inputs, or slot registration. A Pro-specific implementation may be enough; do not broaden the OSS base API without evaluating its existing callers.

Changing a component partial should not require replacing a whole screen. Existing low-level controls remain customizable through their existing APIs; this proposal does not claim they all become RCL templates.

## What makes this worth paying for?

The strongest value is a coherent set of production-ready screen compositions: responsive sections, consistent actions, well-presented record details, accessibility, and customization through Razor. Merely wrapping a field group in a tag with a title is a weak addition when the OSS controls already make forms concise.

The first review should compare an actual edit/details pair built with existing controls against the proposed composition. Measure the repeated structural markup removed, the custom CSS avoided, and the effort needed to insert a custom control or rearrange the page. A new abstraction earns its place only if the consumer example becomes meaningfully easier to author and maintain.

After that evidence, possible additions include main-content/sidebar layouts, error-summary presentation, and reusable confirmation UI. They are not required for the first iteration. A delete operation can initially use an existing dialog or a separate confirmation page with an ordinary protected POST form. Keep it outside the edit form to avoid nested forms.

## Boundaries for the first experiment

- Keep the existing data grid for lists; add presentation composition for edit and details.
- Reuse existing inputs, metadata, validation, buttons, and field primitives.
- Keep routes, handlers, input models, reads, writes, and relationship semantics in the consuming app.
- Preserve arbitrary child Razor, instance template overrides, and app-level RCL overrides.
- Defer auto-discovery of model fields, handler interfaces, base controllers, EF adapters, remote lookup infrastructure, editable collection management, and runtime screen schemas.

This does not require removing the existing resource or Identity code. The new components can be evaluated independently; Identity might later compose them internally if the design proves useful.

## Review decisions and next step

The recommended starting point is the five-component vocabulary above, with explicit existing inputs and an ordinary HTML form. The consumer owns the screen model. New convenience belongs primarily in composition and record display.

Review whether `sa-record-page` saves enough over the existing page header/container, whether one form-section layout is enough initially, and whether display-field template selection is needed immediately. Names and exact attributes remain open.

The next concrete step after reviewing these sketches is a visual prototype of one product edit page and one details page using the real theme, following the workspace prototype-component workflow. Compare with the existing-primitives version before extracting components. This document does not start that implementation phase.

A later functional spike should verify MVC and Razor Pages rendering, invalid POST value/error retention, nested input prefixes, checkbox behavior, category selection after an invalid POST, null/false/zero display, mobile layout, custom child content, and both instance and app-level partial overrides. Ensure body content renders once and that the form's antiforgery/binding behavior survives component composition. Use the same Razor with application-supplied in-memory data first; persistence integration is not necessary to prove the presentation API.

## Verification and handoff

Inspected the current OSS field helpers, Pro form-page/resource model, data grid, and shared templated helper. Consulted official ASP.NET Core forms and RCL documentation. This is a documentation-only exploration; no code snippets were compiled, no UI was rendered, and no product tests were run.

Workspace changes: this plan and its index entry. The earlier resource-consumer sketch and its existing index change are preserved. Product repositories were read only.
