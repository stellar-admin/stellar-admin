# Consumer resource API sketches

Status: proposed. Last updated: 2026-09-06. Affected repo: workspace documentation only; prospective implementation belongs in `stellar-admin-pro/`.

Follow-up: the [2026-09-08 brainstorming session](archive/generic-resources-brainstorming.md) narrows the initial work to StellarAdmin-managed EF CRUD with consumer override points, followed by form layout and single-record references. That session is now closed; use the [follow-up backlog](generic-resources-follow-ups.md) for deferred work. The broader alternatives below remain exploratory context.

## Purpose and agreed requirements

Explore the code a consumer would write before implementing library APIs. These are deliberately non-compiling sketches: proposed types, methods, routes, and view contracts are not shipped APIs. Application service methods stand in for consumer-owned behavior. Names and signatures are discussion material, not decisions.

Users need to build admin screens quickly for their own resources, with either custom reads/writes or an EF Core integration. Identity is a viability experiment, not a requirement to preserve every assumption in its design. Consumers must be able to override shipped Razor views at some useful level, in the same spirit as Identity UI. This requirement applies to the builder candidate as well as the controller candidate.

Authorized work is consumer API exploration. No library implementation or migration is authorized by this document.

## Common ground for the comparison

Both candidates use the same screen builders, RCL views, editor templates, and public page models. The difference is where request behavior lives: injected handlers behind library-owned controllers, or consumer-owned controllers with inherited standard actions.

These sketches assume the app has already configured MVC, its authentication/authorization, StellarAdmin Pro, and any DbContext or API client. Resource registration adds resource routes and navigation. An explicit resource authorization policy is shown; the library must enforce it on requests. Per-record checks remain necessary where the application has them.

Start with one writable model type per resource, and test a separate input type in the order example. Do not settle a large generic signature before comparing these examples.

## Example 1: EF-backed categories

Requirement: list, create, edit, and delete categories with `Id`, `Name`, and `Description`. The application already has `CatalogDbContext` and `Category`. Only the declared form fields are writable. No custom controller or view should be necessary for the default experience.

### A. Builder with an EF adapter

```csharp
// Program.cs
pro.AddEfCoreResource<CatalogDbContext, Category>("categories", resource =>
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

    resource.Create(create => create.Fields(CategoryFields.Configure));
    resource.Edit(edit => edit.Fields(CategoryFields.Configure));
});

// CategoryFields.cs — ordinary consumer code, not a special library abstraction.
public static class CategoryFields
{
    public static void Configure(FormFieldsBuilder<Category> fields)
    {
        fields.Add(c => c.Name);
        fields.Add(c => c.Description);
    }
}
```

Proposed defaults: EF metadata supplies the key; the registration name supplies the route and a predictable MVC controller/view name (`Categories`). CRUD is available for this convenience registration. Titles derive from the resource name and remain configurable. Fields are explicit, with no inferred writable collection. Stable pagination, async execution, cancellation, and concurrency handling belong to the adapter.

### B. Consumer controller with an EF base

```csharp
// Program.cs — registers configuration and binds this resource to one controller.
pro.AddResource<Category>("categories", resource =>
{
    resource.UseController<CategoriesController>();
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
    resource.Create(create => create.Fields(CategoryFields.Configure));
    resource.Edit(edit => edit.Fields(CategoryFields.Configure));
});

// Areas/StellarAdmin/Controllers/CategoriesController.cs
[Area("StellarAdmin")]
public sealed class CategoriesController(
    EfResourceServices<CatalogDbContext, Category> services
) : EfResourceController<CatalogDbContext, Category>(services)
{
    // Index/Create/Edit/Delete actions are inherited.
}
```

`EfResourceServices` is a placeholder for the dependencies required by the base; its ergonomics need review. This variant has the same screen definition plus an initially empty controller. It becomes attractive only when the consumer needs action-level customization. `UseController` must replace the library controller for this registration, not add a second set of conflicting routes.

Review question: does owning this initially empty controller provide enough familiarity and discoverability to justify making it the default?

## Example 2: customers managed through an API

Requirement: paged listing and full CRUD through an existing `CustomerApi`. `CustomerAdminModel` is a mutable admin DTO, not a database entity. Its annotations define field validation. The API is responsible for business rules.

### Shared screen definition

```csharp
public static class CustomerScreens
{
    public static void Configure(ResourceBuilder<CustomerAdminModel> resource)
    {
        resource.AuthorizationPolicy = "ManageCustomers";
        resource.Index(index =>
        {
            index.EnableSearch(); // UI opt-in; the handler implements the search.
            index.Columns(columns =>
            {
                columns.Add(c => c.Name).Sortable();
                columns.Add(c => c.Email);
            });
        });
        resource.Create(create => create.Fields(ConfigureFields));
        resource.Edit(edit => edit.Fields(ConfigureFields));
    }

    private static void ConfigureFields(FormFieldsBuilder<CustomerAdminModel> fields)
    {
        fields.Add(c => c.Name);
        fields.Add(c => c.Email);
    }
}
```

The parameterless search opt-in is proposed, unlike today's query-taking API. A validated list request conveys the search term, allowed sort, filters, and bounded page size to the handler. The API adapter maps those to its backend; it never returns `IQueryable`. Unsupported sorting/filtering must be omitted from the definition or rejected during setup, rather than silently ignored.

### A. Injected handler

```csharp
pro.AddResource<CustomerAdminModel>("customers", resource =>
{
    resource.UseHandler<CustomerHandler>();
    CustomerScreens.Configure(resource);
});

public sealed class CustomerHandler(CustomerApi api)
    : IResourceHandler<CustomerAdminModel, Guid>
{
    public Task<WriteResult<Guid>> CreateAsync(
        CustomerAdminModel input, CancellationToken ct)
        => api.CreateAsync(input, ct);

    public Task<WriteResult> DeleteAsync(
        Guid id, string? version, CancellationToken ct)
        => api.DeleteAsync(id, version, ct);

    public Task<ResourceRecord<CustomerAdminModel>?> GetAsync(
        Guid id, CancellationToken ct)
        => api.GetAsync(id, ct);

    public Task<ResourcePage<CustomerAdminModel>> ListAsync(
        ResourceListRequest request, CancellationToken ct)
        => api.ListAsync(request, ct);

    public Task<WriteResult> UpdateAsync(
        Guid id, CustomerAdminModel input, string? version, CancellationToken ct)
        => api.UpdateAsync(id, input, version, ct);
}
```

For brevity the example API already returns the proposed contracts. A real adapter would translate its DTOs, pagination, errors, and ETag/version. That mapping is consumer work in both candidates, not something the library can infer. `ResourceRecord` carries the model and version; page rows must expose keys through a documented envelope or configured selector. The exact key declaration and inference API remains open.

`WriteResult` distinguishes success, field errors, general rejection, not found, and concurrency conflict. The controller maps field names to form model-state keys and redisplays attempted input. The version is carried from GET through POST. The handler is scoped and resolved per request; configuration must not capture a scoped API client or DbContext.

### B. Overrides on a base controller

```csharp
pro.AddResource<CustomerAdminModel>("customers", resource =>
{
    resource.UseController<CustomersController>();
    CustomerScreens.Configure(resource);
});

[Area("StellarAdmin")]
public sealed class CustomersController(
    CustomerApi api,
    ResourceControllerServices<CustomerAdminModel, Guid> services
) : CrudResourceController<CustomerAdminModel, Guid>(services)
{
    protected override Task<WriteResult<Guid>> CreateResourceAsync(
        CustomerAdminModel input, CancellationToken ct)
        => api.CreateAsync(input, ct);

    protected override Task<WriteResult> DeleteResourceAsync(
        Guid id, string? version, CancellationToken ct)
        => api.DeleteAsync(id, version, ct);

    protected override Task<ResourceRecord<CustomerAdminModel>?> GetResourceAsync(
        Guid id, CancellationToken ct)
        => api.GetAsync(id, ct);

    protected override Task<ResourcePage<CustomerAdminModel>> ListResourcesAsync(
        ResourceListRequest request, CancellationToken ct)
        => api.ListAsync(request, ct);

    protected override Task<WriteResult> UpdateResourceAsync(
        Guid id, CustomerAdminModel input, string? version, CancellationToken ct)
        => api.UpdateAsync(id, input, version, ct);
}
```

The inherited actions handle binding, validation, antiforgery, view selection, and redirects. Protected methods supply data behavior; they are not MVC actions. This is nearly the same amount of consumer code as the handler, but tied to MVC. A full action override should also be possible when a custom response is needed; its author then owns the HTTP behavior and must retain the relevant checks.

Review question: are protected operations clearer than a handler interface, and how often would users actually need `ModelState`, `HttpContext`, or a custom response inside them?

## Example 3: EF order reads and an application approval command

Requirement: list orders from EF and approve an order with a comment. Do not expose generic create/edit/delete. `ApproveOrderInput` is distinct from the order entity. The application command service rechecks the current user's access, order state, and submitted version before making changes.

### A. Resource action and handler

```csharp
pro.AddEfCoreResource<SalesDbContext, Order>("orders", resource =>
{
    resource.AuthorizationPolicy = "ViewOrders";
    resource.Operations = ResourceOperations.Index | ResourceOperations.Details;
    resource.Index(index => index.Columns(columns =>
    {
        columns.Add(o => o.Number);
        columns.Add(o => o.Status);
        columns.Add(o => o.Total);
    }));

    resource.AddAction<ApproveOrderInput>("approve", action =>
    {
        action.Title = "Approve order";
        action.AuthorizationPolicy = "ApproveOrders";
        action.Fields(fields => fields.Add(i => i.Comment));
        action.UseHandler<ApproveOrderHandler>();
    });
});

public sealed class ApproveOrderInput
{
    [Required, StringLength(500)]
    public string Comment { get; set; } = "";
}

public sealed class ApproveOrderHandler(OrderCommands commands)
    : IResourceActionHandler<ApproveOrderInput, Guid>
{
    public Task<WriteResult> ExecuteAsync(
        Guid id, ApproveOrderInput input, string? version, CancellationToken ct)
        => commands.ApproveAsync(id, input.Comment, version, ct);
}
```

Proposal: an input-bearing action gets an ordinary GET confirmation/form page and an antiforgery-protected POST endpoint. Success returns to the resource; rejection redisplays the form. A modal could be layered on later. `Operations` removes unavailable endpoints as well as buttons. Action availability in the UI is separate from the server's permission and state checks.

### B. Explicit controller action

The registration uses the same columns and list/details operations, with `UseController<OrdersController>()`. Instead of `AddAction`, an overridden details view links to `Approve`. This variant intentionally leaves the custom workflow visible in the controller.

```csharp
[Area("StellarAdmin")]
public sealed class OrdersController(
    OrderCommands commands,
    EfResourceServices<SalesDbContext, Order> services
) : EfResourceController<SalesDbContext, Order>(services)
{
    [HttpGet]
    [Authorize(Policy = "ApproveOrders")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        // This consumer service checks record access and returns the version.
        var order = await commands.GetApprovableAsync(id, ct);
        if (order is null) return NotFound();

        return View(new ApproveOrderPage
        {
            Id = id,
            Version = order.Version,
            Input = new ApproveOrderInput(),
        });
    }

    [HttpPost]
    [ActionName("Approve")]
    [Authorize(Policy = "ApproveOrders")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApprovePost(
        Guid id, ApproveOrderPage page, CancellationToken ct)
    {
        page.Id = id; // The route is authoritative for the operation target.
        if (!ModelState.IsValid) return View("Approve", page);

        var result = await commands.ApproveAsync(
            id, page.Input.Comment, page.Version, ct);
        if (!result.Succeeded)
        {
            AddResourceErrors(result, prefix: "Input");
            return View("Approve", page);
        }

        return RedirectToAction("Details", new { id });
    }
}
```

`ApproveOrderPage` is a consumer-owned model containing `Id`, `Version`, and `Input`. The consumer supplies `Areas/StellarAdmin/Views/Orders/Approve.cshtml`, including its form, validation summary, version input, and antiforgery token. `AddResourceErrors` is a proposed shared helper. This costs more code but naturally accommodates a different success destination, extra data, or a multi-step approval flow.

Review question: how much of this workflow belongs in a declarative action API? The handler candidate saves HTTP plumbing but creates another public configuration surface to learn and maintain.

## Razor override contract: both candidates

Overriding markup must not require changing the persistence implementation or adopting a consumer controller. Builder-defined screens can still be served by MVC controllers returning ordinary RCL views. ASP.NET Core documents that host `.cshtml` files take precedence over RCL files at the same path: [Reusable Razor UI in class libraries](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/ui-class?view=aspnetcore-10.0).

For arbitrary resources, the RCL cannot ship a `Categories/Edit.cshtml` for every consumer type. Proposed solution: give each registration a stable controller/view name, use normal named-view lookup, and supply shared fallback pages. This resource-specific fallback design is a proposal to verify, distinct from already-supported exact-path RCL shadowing.

| Consumer customization | Host file relative to `Areas/StellarAdmin/Views/` | Intended behavior |
| --- | --- | --- |
| Categories edit page | `Categories/Edit.cshtml` | Replaces only that resource's page, under either candidate |
| Default edit page | `Shared/Edit.cshtml` | Shadows the proposed shared RCL fallback for generic resources |
| Categories grid | `Categories/_IndexDataGrid.cshtml` | Overrides the named grid partial for Categories |
| Shared form structure | `Shared/_FormPage.cshtml` | Shadows the existing shared partial |
| Reusable field editor | `Shared/EditorTemplates/CategoryPicker.cshtml` | Selected with `.Template("CategoryPicker")` |
| Builder-defined approval form | `Orders/Approve.cshtml` | Replaces the proposed action form fallback |

Illustrative additive override, using the existing form-page model and slot pattern:

```cshtml
@* Host: Areas/StellarAdmin/Views/Categories/Edit.cshtml *@
@model ResourceFormPageViewModel

<sa-form-page model="Model">
    <sa-slot-content name="pre-form-fields">
        <p>Category names appear in the public catalog.</p>
    </sa-slot-content>
</sa-form-page>
```

The host needs its own `_ViewImports.cshtml` for namespaces and tag helpers. Changing markup does not add writable fields or change validation: the declared input/binding contract remains authoritative. A fully handwritten form must preserve that contract, route keys, antiforgery, validation messages, and any version token. The additive example is about the slot mechanism, not a complete reproduction of all default edit actions.

Details/action page model names and the representation of concurrency data are still open. Public page models must expose enough typed data for full replacement views. View paths, model contracts, and slots become compatibility commitments; defaults can improve without forcing consumers to copy the entire page.

Implementation questions for a later spike:

- Verify resource-specific named-view lookup before shared fallback, including partial lookup when the outer view comes from Shared. Avoid hardcoded absolute fallback paths that bypass resource overrides.
- The current controller feature provider keys registrations by CLR controller type. Two resources using the same model must still get distinct configuration, routes, and view identities; a generic controller closed only over the model will not solve that by itself. A definition type could supply the additional identity.
- Verify how explicit controller registration, inherited actions, disabled operations, endpoint policies, and MVC discovery interact, without duplicate endpoints.
- The analogy here is to Identity UI's override experience. These sketches use MVC views; overriding a Razor Page and its PageModel is not the same mechanism as replacing a controller action.

## Optional organization: a resource definition class

This changes file organization, not behavior ownership. Both candidates could move their builder configuration into a discoverable class:

```csharp
pro.AddResource<CategoriesResource>();

public sealed class CategoriesResource : ResourceDefinition<Category>
{
    public override void Configure(ResourceBuilder<Category> resource)
    {
        resource.Name = "categories";
        resource.UseEfCore<CatalogDbContext>();
        resource.AuthorizationPolicy = "ManageCatalog";
        // Same index/create/edit builders as example 1.
    }
}
```

An alternative is the ordinary static configuration method shown for customers. Review whether a library-defined class earns its place through resource identity and discovery, or whether a simple method is sufficient. Do not require both inline and class-based APIs in the first implementation just because both are sketched here.

## Initial assessment, not an agreed decision

| Scenario | Builder + handlers | Base controller |
| --- | --- | --- |
| EF categories | Smallest setup; no application controller | Same setup plus empty derived controller |
| API customers | Explicit backend contract; independently testable | Similar operation code; MVC state immediately available |
| Order approval | Concise once a resource-action abstraction exists | More HTTP code; custom workflow is directly visible |
| Replace a Razor view | Must work without changing the handler | Must work without overriding an action |
| Change response/navigation | Needs an extension point or controller escape hatch | Natural action override |

Current leaning: builder + handlers as the convenient entry point, with a consumer controller path sharing the rendering and behavior services. The sketches do not yet justify two independent implementations of CRUD. The override requirement strengthens the case for an explicit, stable MVC view contract in both paths; it does not by itself force users into inheritance.

## Remaining decisions and next step

Review these consumer examples before implementation. First choose the preferred behavior ownership model and whether resource classes should be the default organization. Then refine operation enablement, key mapping, separate edit inputs, result/error types, read/write replacement, and action configuration against the examples. A read-only resource must not have to implement dummy write methods; split capability interfaces are a candidate, not yet sketched as a complete API.

After review, a small working spike should test EF categories, an API handler, resource-specific and shared view overrides, one validation redisplay, concurrency round-tripping, and two registrations sharing an entity type. Its purpose is to validate assumptions, not build the full resource system.

## Verification and handoff

Read the current resource builder, base controller, query execution, Identity controllers/views, and MVC feature provider. Checked the framework's RCL override documentation. Examples have been reviewed as design sketches only; no compilation, application run, or runtime override verification was performed. Product repos were not modified. This document and its plan-index entry are the only intended workspace changes.
