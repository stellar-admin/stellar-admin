# Design: StellarAdmin Identity user Create/Edit screens

The Create and Edit user pages for `StellarAdmin.Dashboard.Identity`, following the [options builder conventions](../conventions/options-builders.md) in this repository and extending the builder tree designed in [identity-configuration.md](identity-configuration.md) (which reserved `users.Edit(...)` / `users.Create(...)`). **Direction approved 2026-08-06** after the approaches comparison in the product plan docs (`docs/plans/identity-user-forms-approaches.md`); this is its "approach 1" — field schema + stock editor-template rendering. A hybrid that later adds a `UseModel<T>` escape hatch remains open.

## Problem

The Users index is feature-complete, including custom columns over the developer's extended user type (`columns.Add(u => u.FirstName)`). For Create/Edit, the library-owned generic `UsersController<TUser, TKey>` must render, bind, and validate custom properties on a `TUser` it cannot know about — without over-posting risk, with minimal developer ceremony, and with room for arbitrary customization (data-driven dropdowns, custom widgets, non-entity inputs like password + confirm).

## Design principles

1. **The declared field schema is the binding allow-list.** Fields are declared in the options builder as typed expressions (mirroring `Columns.Add`); the controller binds directly onto the entity via `TryUpdateModelAsync` with a property filter built from the schema. Over-posting is structurally impossible; no developer view model exists.
2. **Rendering is 100% stock ASP.NET Core editor-template machinery** (decision Jerrie, 2026-08-06 — no custom renderer, no custom template resolution, no `sa-*-form-field` tag helper, ever). The views call `Html.Label` / `Html.Editor` / `Html.ValidationMessage` with string expressions; template selection is MVC's own candidate chain (`.Template()` argument → `[UIHint]` → `[DataType]` → CLR type); v1 ships **no editor templates at all** — MVC's built-in defaults render plain HTML inputs until the whole flow is verified. There is no field-type system and no library options API; custom widgets are one app-supplied standard editor template.
3. **Templates own their data.** A data-driven widget (`CountrySelect`) uses `@inject` + `await` to fetch its own options at render time. Lookup data never passes through the library — no factories, no `Prepare` hooks, no view-data channel — and redisplay repopulation is automatic because rendering *is* fetching. (Stock caveat: `TemplateRenderer` blocks synchronously on the async render — accepted as part of piggybacking.)
4. **Schema fields = entity properties. UserManager operations = built-in page features.** Password on Create, and later role assignment / lockout / password reset / 2FA, are never schema fields.
5. **Validation has one source per concern.** Field validation: data annotations on `TUser` or a `[ModelMetadataType]` buddy class (playground-proven). Password policy: Identity's configured `PasswordOptions` / `IPasswordValidator<TUser>` via `CreateAsync`. The library adds no validation vocabulary.
6. **Views erase `TUser`** behind a plain non-generic view model class (no interface — see View model below).

## Target usage

```csharp
builder.Services.AddStellarAdmin().AddDashboard(dashboard => dashboard.AddIdentity<ApplicationUser, ApplicationRole>(identity =>
{
    identity.ConfigureUsers(users =>
    {
        users.Edit(edit =>
        {
            edit.Title = "Edit user";
            edit.Fields(fields =>
            {
                // Seeded defaults extend unless cleared (same convention as columns)
                fields.Add(u => u.FirstName);
                fields.Add(u => u.LastName);
                fields.Add(u => u.Country).Template("CountrySelect");
                // alternative: [UIHint("CountrySelect")] on the property → bare Add(...)
            });
        });

        users.Create(create =>
        {
            create.Fields(fields =>
            {
                fields.Add(u => u.FirstName);
                fields.Add(u => u.LastName);
            });
        });
    });
}));
```

**Per-page field lists** (decision Jerrie, 2026-08-06): `Edit(...)` and `Create(...)` each own their field list, mirroring `Index(...)`. No new sharing convention; the duplication cost for shared custom fields is one `fields.Add` line per page, and a shared shorthand can layer on later without breaking anything.

## Builder surface

```
ResourceBuilder<TEntity>              // entity-agnostic since the resource layer refactor
├─ Create(Action<CreatePageBuilder<TEntity>>)
├─ Edit(Action<EditPageBuilder<TEntity>>)
└─ Index(Action<IndexPageBuilder<TEntity>>)      // existing

CreatePageBuilder<TEntity> / EditPageBuilder<TEntity>
├─ Title, Subtitle                     // scalar page settings = properties
├─ CreateInstanceUsing(Func<TEntity>)  // Create only — replaces the default factory
└─ Fields(Action<FormFieldsBuilder<TEntity>>)

FormFieldsBuilder<TEntity>             // shared by both pages — it configures form
├─ Add<TProp>(Expression<Func<TEntity, TProp>>)     // fields, not a specific page
│    → FormFieldBuilder
│         └─ .Title(string) .Template(string) .ReadOnly()   // EF-style chaining
└─ Clear()                             // drop the seeded defaults; empty honored literally
```

- Unlike the index naming (`IndexPageColumnsBuilder`), the fields builders are **not page-prefixed**: the same builder type serves both pages, because a form field means the same thing on either. Two identical page-prefixed copies would be pure duplication.
- The entry builder has exactly `.Title()`, `.Template()`, `.ReadOnly()` in v1 — no `.Options()`, no `.Editor(enum)`, no validation methods. `ReadOnly()` renders the field but excludes it from the binding allow-list.
- **`Add` requires a direct property of `TUser`** and throws `ArgumentException` at configuration time otherwise (unlike columns, which capture unvalidated). The schema is a binding allow-list keyed by top-level property name; a nested chain (`u => u.Department!.Name`) or non-member expression could render but would silently never bind — fail loudly instead. Nested/complex editing is out of scope for v1.

## Options model

Same shape as the index: builders are facades over eagerly-built, publicly readable, builder-only-writable options ([identity-configuration.md](identity-configuration.md)).

- `IdentityUsersOptions<TUser, TKey>` (a `ResourceOptions<TUser>`, its own DI singleton) holds `CreatePage` (`CreatePageOptions<TUser>`) and `EditPage` (`EditPageOptions<TUser>`).
- Both page options derive from an abstract `FormPageOptions<TEntity>` holding the shared storage (`Fields`, `Title`, `Subtitle`, public `EffectiveTitle` and `EffectiveSubmitLabel`); the derived types exist so page-specific options (for example `CreatePageOptions.InstanceFactory`) have a home that does not pollute the other page.
- Fields are stored **non-generically**, mirroring `DataGridColumnOptions`:

  ```csharp
  public sealed class FormFieldOptions
  {
      public LambdaExpression FieldExpression { get; }   // as captured — metadata resolution
      public string FieldName { get; }                   // never null: Add validates direct property
      public bool IsReadOnly { get; internal set; }
      public string? Template { get; internal set; }
      public string? Title { get; internal set; }        // null = derive from metadata
  }
  ```

- **Seeded default field sets** (decision Jerrie, 2026-08-06), each entry skipped if `TUser` lacks the property, same as index column seeding:
  - Edit: `UserName`, `Email`, `PhoneNumber`.
  - Create: `UserName`, `Email` — plus the password block, which is a built-in page feature, not a field.
  - Flags like `EmailConfirmed`/`LockoutEnabled`/`TwoFactorEnabled` are deliberately not seeded: they belong to future page features (operations), not the form.
- Default page titles (public `EffectiveTitle`, one authoritative site each): "Create user" / "Edit user". Submit labels follow the same pattern (`EffectiveSubmitLabel`): "Create user" / "Save changes".

## Rendering: stock `Html.Editor` (decision Jerrie, 2026-08-06)

There is **no custom renderer of any kind** — no tag helper, no partial-dispatch layer, no custom template folder, no custom search order. Earlier drafts proposed an `sa-user-form-field` tag helper resolving `ModelMetadata` from the field's `LambdaExpression` with a dedicated `FormEditorTemplates/` folder; **all of that is dead**. The RCL view is a loop of stock HTML helpers:

```cshtml
@* RCL: Areas/StellarAdmin/Views/Shared/_FormFields.cshtml — rendered by _FormPage.cshtml
   via <partial for="@Model.Entity">, which sets the field prefix *@
@inherits FormFieldsBaseView<object>

@foreach (var field in Fields)
{
    var expression = field.FieldName;
    <div>
        @Html.Label(expression, field.Title)              @* null -> [Display] -> property name *@
        @if (field.IsReadOnly)
        {
            @Html.Display(expression)                      @* stock display templates *@
        }
        else
        {
            @Html.Editor(expression, field.Template)       @* null -> stock candidate chain *@
        }
        @Html.ValidationMessage(expression)
    </div>
}
```

The prefix is not string-built in the view: `<partial for="@Model.Entity">` makes MVC derive it from the property name, and `ResourceFormPageViewModel.BindingPrefix` (`nameof(ResourceFormPageViewModel.Entity)`) gives the controller the same string, so renaming the property is a compile error everywhere the prefix matters.

**Why the string expression works against an `object`-typed `Entity` property** (the load-bearing mechanism — spike-verify first in Phase 2): `ExpressionMetadataProvider.FromStringExpression` evaluates the expression via `ViewDataEvaluator.Eval`, which walks the **runtime** object graph. When it lands on a property access, metadata is built from the runtime container type (`viewDataInfo.Container.GetType()` — the actual `ApplicationUser`), so `[Display]`, `[DataType]`, `[UIHint]`, and `[ModelMetadataType]` buddy classes all resolve. The declared-type fallback (where `object` would lose everything) only applies when evaluation fails, i.e. a null value — and `Entity` is never null (Edit loads the entity; Create makes a fresh instance).

What the stock machinery then gives us for free:

- **Template selection** is `TemplateRenderer`'s own candidate chain: the `templateName` argument (our `.Template("X")`) → `metadata.TemplateHint` (`[UIHint]`) → `metadata.DataTypeName` (`[DataType]`, `[EmailAddress]`, ...) → CLR type name → `Enum`/`Collection`/`String`/`Object` fallbacks. Each candidate is looked up as `EditorTemplates/{name}` through the standard view search locations (the folder name is hardcoded in MVC — a dedicated folder is not possible, and not wanted).
- **When no template file matches, MVC's built-in default HTML generators render plain inputs** (text input, checkbox, textarea, ...). **v1 ships exactly this: no editor templates in the RCL at all** (decision Jerrie, 2026-08-06) — verify the entire flow end-to-end with plain HTML before touching styling.
- **Input naming**: the partial binding supplies the prefix — inputs are `Entity.FirstName` (ids `Entity_FirstName`), labels and validation spans match automatically.
- **`data-val-*` client validation attributes** generated from the resolved metadata by the stock helpers inside templates (`Html.TextBox("", ...)` etc.).
- **Redisplay**: after a failed POST, `ModelState` attempted values take precedence automatically (standard `HtmlHelper` behavior) — conversion failures round-trip with zero code.
- **A custom widget is a plain standard editor template** in the app, found via the normal search locations (`Views/Shared/EditorTemplates/` or the app's `Areas/StellarAdmin/Views/Shared/EditorTemplates/`):

  ```cshtml
  @* App: Views/Shared/EditorTemplates/CountrySelect.cshtml *@
  @model string
  @inject ICountryLookup Countries
  @Html.DropDownList("",
      (await Countries.AllAsync()).Select(c => new SelectListItem(c.Name, c.Code, c.Code == Model)),
      "Select a country...")
  ```

  Any widget that funnels its result into a single named input binds with zero extra machinery. `@inject` + `await` works (with the sync-block caveat from principle 3).
- **Overriding**: when the RCL eventually ships styled templates, the app overrides one by exact-path shadowing under its own `Areas/StellarAdmin/Views/Shared/EditorTemplates/` — the ASP.NET Identity default UI pattern, same as overriding our page views. Until then there is nothing to override.

Consequences of going fully stock, accepted 2026-08-06:

- `FormFieldOptions.FieldExpression` is **not used by rendering** — `FieldName` is; the typed expression remains for compile-time safety in the config API. The grid's `GetFieldMetadata` / compiled-getter mechanisms are not needed here.
- `[UIHint]` names we don't ship fall through to the app's global `EditorTemplates/` — standard `EditorTemplates` semantics, no longer treated as a problem to engineer around.
- Fail-loudly for unknown complex types moves from startup to render time and is deferred with styling (a shipped `Object.cshtml` that throws is the likely implementation).

## View model

A plain sealed non-generic class — no interface, no generics (decision Jerrie, 2026-08-06):

```csharp
public sealed class ResourceFormPageViewModel   // ViewModels/ — views use @model ResourceFormPageViewModel
{
    public const string BindingPrefix = nameof(Entity);   // posted names are "{BindingPrefix}.{FieldName}"

    public object Entity { get; }          // overridden views cast to their entity type
    public IReadOnlyList<FormFieldOptions> Fields { get; }   // config-time defs, already non-generic
    public string SubmitLabel { get; }
    public string? Subtitle { get; }
    public string Title { get; }

    internal ResourceFormPageViewModel(...) { }   // consumers read, never construct
}
```

Why this differs from `IResourceIndexPageViewModel`: erasure only requires the `@model` type to be *non-generic*, which a plain class satisfies. The index needs the interface + explicit-implementation dual face because `ResourceIndexPageViewModel<TEntity>` carries genuinely `TEntity`-typed payload (its `PagedListViewModel<TEntity>` page) projected through the non-generic `IPagedListViewModel`. The form model has no entity-typed member at all — the stock helpers evaluate field values off `Model.Entity` at runtime and the controller already holds the typed entity. Revisit only if a member ever needs to be entity-typed for library code while appearing erased to views. With templates owning their data, building the view model is synchronous and the field list is the config-time definitions directly.

## Binding and saving (Edit)

```csharp
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(string id)
{
    var user = await userManager.FindByIdAsync(id);
    if (user is null) return NotFound();

    var writable = page.Fields.Where(f => !f.IsReadOnly).Select(f => f.FieldName).ToHashSet();
    await TryUpdateModelAsync(user, prefix: ResourceFormPageViewModel.BindingPrefix,   // matches the input names
        propertyFilter: m => writable.Contains(m.PropertyName));   // schema = allow-list

    // Shipped as ResourceControllerBase<TEntity>.BindFormFieldsAsync — shared with roles.

    if (!ModelState.IsValid)
        return View(BuildFormPageViewModel(user));                     // templates re-fetch on render

    var result = await userManager.UpdateAsync(user);
    if (!result.Succeeded)
    {
        AddIdentityErrors(result, ModelState);
        return View(BuildFormPageViewModel(user));
    }
    return RedirectToAction(nameof(Index));
}
```

- Validation failure redisplays without `UpdateAsync`; the modified tracked entity dies with the request scope — nothing persists.
- `AddIdentityErrors`: known `IdentityResult` error codes map to field keys (`DuplicateEmail` → `Email`, `PasswordTooShort`/`PasswordRequires*` → `Password`, ...); unknown codes go to the validation summary.
- Exact filtered-`TryUpdateModelAsync` overload to confirm at implementation time (`ModelBindingHelper.TryUpdateModelAsync` has the `propertyFilter` shape if the controller-level overload doesn't fit).

## Create page: password + confirm

Password is an argument to `CreateAsync(user, password)`, not entity state — a built-in page feature per principle 4, bound to a library-owned input model:

```csharp
public sealed class CreateUserPasswordInput
{
    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = "";
}
```

No length/complexity annotations — Identity's password policy is the single source of truth; `CreateAsync` returns all violations in one `IdentityResult`.

```csharp
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Create(CreateUserPasswordInput passwordInput)  // MVC binds + validates
{
    var user = options.CreatePage.CreateInstance();   // InstanceFactory / CreateInstanceUsing hook
    await BindFormFieldsAsync(user, options.CreatePage);

    if (!ModelState.IsValid)
        return View(BuildFormPageViewModel(user));

    var result = await userManager.CreateAsync(user, passwordInput.Password);
    if (!result.Succeeded)
    {
        AddIdentityErrors(result, ModelState);
        return View(BuildFormPageViewModel(user));
    }
    return RedirectToAction(nameof(Index));
}
```

- Two independent binds against one form post: filtered entity bind (schema, prefix `User`) + parameter bind (password, unprefixed).
- `Create.cshtml` renders the schema loop, then the password inputs — plain markup in the view for v1 (`Html.Editor` over the `CreateUserPasswordInput` members or direct inputs; no shipped template); the view is overridable by shadowing like any RCL view. *(As built 2026-08-13: the page is `<sa-form-page model="Model">` with the password fieldset filling the form's `post-form-fields` slot; the fill executes in the page's own context, so the unprefixed editor names and the parameter bind are unchanged.)*
- On failed validation, schema values redisplay (on the entity); password inputs rerender empty (standard practice).
- No reserved-name problem: entity fields live under the `Entity.` prefix, so the unprefixed `Password`/`ConfirmPassword` inputs cannot collide with any `TUser` property.

*(Revised 2026-08-13: the annotated properties had ended up twice — on `CreateUserPasswordInput` for binding and duplicated on `UserCreateViewModel` for rendering — which let client and server validation drift. `UserCreateViewModel` now composes the input as its `PasswordInput` property; the view renders `m.PasswordInput.*` and the unchanged POST parameter binds the same `PasswordInput.` prefix via its parameter name, so the password form contract has one owner. The no-collision argument holds: `PasswordInput.*` cannot collide with the `Entity.`-prefixed fields any more than the unprefixed names could. Password policy errors map to the `PasswordInput.Password` key.)*

## Customization ladder

1. Nothing — seeded default fields, default view.
2. `fields.Add(...)` — one line per custom property.
3. `.Template("X")` / `[UIHint("X")]` + one standard editor template `.cshtml` — custom widget, reusable everywhere. (Once the RCL ships styled templates, shadowing one at its area path replaces it across all admin forms.)
4. Shadow a view — `Areas/StellarAdmin/Views/Users/Edit.cshtml` for one resource's page, or `Views/Shared/_FormPage.cshtml` / `_FormFields.cshtml` for every resource at once. The schema still drives binding; the view model still provides `Fields` + `Entity`.
5. Skip our screens entirely — point row actions/"New user" at the app's own controller.

## Boundaries (explicit non-goals for v1)

- **Non-entity custom inputs** beyond password (file upload `IFormFile`, inline collection editors): out of scope. Future options that layer on without rework: per-field bind/apply callbacks (Nova `fillUsing` shape) or the `UseModel<T>` escape hatch from the approaches doc. Multi-select onto `List<string>` does work via MVC repeated-name collection binding.
- **Posted-value-in-options validation** (Filament's implicit `in:` rule): impossible generically since the library never sees options; left to entity validation (`[CustomValidation]`/`IValidatableObject`) if an app cares.
- **UserManager operations on Edit** (set/reset password, roles, lockout, 2FA): future built-in page features, never schema fields.
- **Nested property editing** (`u => u.Department!.Name`): `Add` rejects it (see Builder surface). Rendering nested fields would work (the grid proves it); the write path is the problem: `TryUpdateModelAsync`'s property filter is top-level-only, so letting `Department.Name` bind requires allowing `Department` wholesale — opening every property of the child object to over-posting. Binding onto a navigation property also mutates a shared row (renaming the department for every user in it) or fabricates a phantom child when the intermediate is null. The admin-correct relation pattern is a FK dropdown (`fields.Add(u => u.DepartmentId).Template("DepartmentSelect")`), which is a direct property and fully supported. A future version could bind nested leaves manually (compiled setters + per-leaf conversion/validation) for owned/complex types where the child is not a shared row.

## Settled implementation facts

- `TKey` need not reach the options layer: `UserManager.FindByIdAsync(string)`; route ids stay strings.
- Create/Edit are new actions on the existing `UsersController<TUser, TKey>`; `IdentityControllerFeatureProvider` and `IdentityControllerNameConvention` are untouched.
- One rendering code path for zero-config and customized screens alike: the view always loops `Model.Fields` through the stock helpers.
- The Phase 1 configuration layer (options, builders, seeded defaults, direct-property validation in `Add`) shipped 2026-08-06 and is unaffected by the rendering approach.

## Deferred decisions

- **Styled editor templates**: everything about them is deferred until the plain flow is verified end-to-end (decision Jerrie, 2026-08-06). This includes the exact set of conventionally-named templates to ship (`Boolean`, `String`, `Enum`, ...), whether `bool` renders as Switch or Checkbox (the earlier bool→Switch decision moves here), the fail-loudly `Object.cshtml`, and *how* templates are styled — **they must not render StellarAdmin tag helpers** (Jerrie, 2026-08-06); plain styled HTML is the leaning, exact approach TBD.
- **Password knobs**: optional/passwordless create (`CreateAsync(user)` overload), invite-email flows — deferred until needed (the `Sortable()` precedent).
- **`.Editor(enum)` sugar** for built-in template names — v1 is names only.
- **Shared field-list shorthand** across Create/Edit — layer on later if the per-page duplication proves annoying in practice.
