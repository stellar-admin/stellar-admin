# Identity Create/Edit user screens — contrasting the approaches, end to end

## Code audit — 2026-09-18

Current status: **superseded**. The field-schema approach is implemented through shared resource options, filtered binding and MVC editor templates. `UseModel<T>`/Load/Prepare/Apply alternatives in this exploration are not shipped APIs. The later shared resource and editor implementation replaces this decision-stage document; retain it only for rationale.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

**Index status:** reference. **Indexed:** 2026-09-05. Research/specification record; read the current design before using historical examples.

**Status (2026-08-06): reference document.** This preserves the initial research and the three candidate architectures with complete code for each: everything the developer writes, everything the library does behind the scenes, and the full request flow. Repercussions follow the code in each section.

The direction Jerrie is leaning toward is **approach 1**, detailed as a focused working design in [identity-user-forms.md](identity-user-forms.md). Approaches 2 and 3 are kept here in full because a **hybrid remains on the table**: approach 1 layers cleanly with approach 2's `UseModel<T>` escape hatch (Django precedent), so this document is the starting point if that route is ever taken. Approach 1's section below reflects its evolved form from the design discussion (no `.Options()` API; named editor templates own their data via `@inject`; password as a built-in Create-page feature).

---

## The scenario used in every example

The developer has extended `IdentityUser` and wants an Edit screen showing Email (a built-in field) plus their three custom properties, where `Country` is a dropdown fed by a service:

```csharp
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Country { get; set; }   // ISO code, e.g. "ZA"
}

public interface ICountryLookup
{
    Task<IReadOnlyList<Country>> AllAsync();   // app's own type: record Country(string Code, string Name)
}
```

Desired screen:

```
Email        [ jerrie@example.com      ]
First name   [ Jerrie                  ]
Last name    [ Pelser                  ]
Country      [ South Africa         ▼ ]
                              [ Save ]
```

---

# Approach 1 — Field schema (no view model anywhere)

The developer declares form fields in the options builder, exactly like `columns.Add()` on the index. The Django-admin / Laravel-Filament pattern.

## 1a. Everything the developer writes

```csharp
// Program.cs — this is the complete developer-side code for the scenario
builder.Services
    .AddStellarAdmin()
    .AddIdentity<ApplicationUser>(identity =>
    {
        identity.ConfigureUsers(users =>
        {
            users.Edit(edit =>
            {
                edit.Fields(fields =>
                {
                    // Email is a seeded default (same convention as seeded columns;
                    // fields.Clear() starts from scratch)
                    fields.Add(u => u.FirstName);
                    fields.Add(u => u.LastName);
                    fields.Add(u => u.Country).Template("CountrySelect");
                    // alternative: [UIHint("CountrySelect")] on the property -> bare Add(u => u.Country)
                });
            });
        });
    });
```

Validation and labels come from data annotations on the entity — directly or via a buddy class if the developer doesn't want attributes on the EF type (the playground already proves this pattern for the grid):

```csharp
[ModelMetadataType<ApplicationUserMeta>]
public partial class ApplicationUser { /* as above */ }

public class ApplicationUserMeta
{
    [Display(Name = "First name"), MaxLength(100)]
    public string? FirstName { get; set; }

    [Display(Name = "Last name"), MaxLength(100)]
    public string? LastName { get; set; }
}
```

The only other file is the country dropdown's editor template — **it owns its own data via `@inject`**, and it's reusable by every screen and entity with a country field:

```cshtml
@* Views/Shared/FormEditorTemplates/CountrySelect.cshtml — the whole dropdown story *@
@model string?
@inject ICountryLookup Countries
<select name="@ViewData.TemplateInfo.HtmlFieldPrefix" class="sa-select">
    <option value="">Select a country...</option>
    @foreach (var country in await Countries.AllAsync())
    {
        <option value="@country.Code" selected="@(country.Code == Model)">@country.Name</option>
    }
</select>
```

That's it. No view model, no mapping code, no library options API — lookup data never passes through the library at all.

## 1b. What the library builds (developer never touches this)

New actions on the existing `UsersController<TUser, TKey>`:

```csharp
[HttpGet]
public async Task<IActionResult> Edit(string id)
{
    var user = await userManager.FindByIdAsync(id);
    if (user is null) return NotFound();

    // Field descriptors: stored LambdaExpression -> ModelMetadata (labels from [Display],
    // required-ness, [UIHint]) + current values. No data loading beyond the user itself.
    return View(BuildFormViewModel(user));
}

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(string id, CancellationToken ct)
{
    var user = await userManager.FindByIdAsync(id);
    if (user is null) return NotFound();

    var writable = options.UsersEditPage.Fields
        .Where(f => !f.ReadOnly)
        .Select(f => f.FieldName)
        .ToHashSet();

    // Binds posted values ONTO THE ENTITY, but only properties in the declared
    // schema pass the filter — this is MVC's documented over-posting mitigation.
    // Validation attributes on ApplicationUser run and populate ModelState.
    await TryUpdateModelAsync(user, prefix: "",
        propertyFilter: m => writable.Contains(m.PropertyName));

    if (!ModelState.IsValid)
        return View(BuildFormViewModel(user));   // templates re-fetch their own data on render

    var result = await userManager.UpdateAsync(user);
    if (!result.Succeeded)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
        return View(BuildFormViewModel(user));
    }

    return RedirectToAction(nameof(Index));
}
```

Default view in the RCL — non-generic erasure, same pattern as `IUsersIndexViewModel`:

```cshtml
@model IUserFormViewModel   @* Fields: non-generic descriptors; User: the entity as object *@

<form method="post">
    <div asp-validation-summary="ModelOnly"></div>
    @foreach (var field in Model.Fields)
    {
        @* Renders label + editor + validation span. The editor is a named template
           resolved via Template -> [UIHint] -> metadata/CLR-type inference (section 1e)
           and rendered as an async partial, so templates can @inject and await.
           Input name = property name ("FirstName"). *@
        <sa-form-field field-for="field.FieldExpression" for-object="Model.User"
                       title="@field.Title" readonly="field.ReadOnly"
                       template="@field.Template"/>
    }
    <sa-button type="submit">Save</sa-button>
</form>
```

The view model behind that view — the sketch below reused the index's interface pattern, but this was later simplified to a plain sealed non-generic `UserFormViewModel` class (see identity-user-forms.md): erasure only needs a non-generic `@model` type, and unlike the index (typed `PagedListViewModel<TUser>` projected to `object` rows) the form model has no `TUser`-typed member to hide.

```csharp
// ViewModels/Internal/IUserFormViewModel.cs — the erasure boundary
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IUserFormViewModel
{
    IReadOnlyList<UserFormFieldViewModel> Fields { get; }
    string Title { get; }
    string? Subtitle { get; }
    object User { get; }              // overridden views cast to their TUser
}

// One rendered field — with no options pipeline this may collapse into the
// config-time IdentityFormField itself (it is already non-generic, like IdentityGridColumn)
public sealed class UserFormFieldViewModel
{
    public LambdaExpression FieldExpression { get; }      // -> ModelMetadata via field-for trick
    public string? FieldName { get; }                     // input name attribute
    public string? Title { get; }                         // null -> [Display] name
    public string? Template { get; }                      // named editor template override
    public bool ReadOnly { get; }
}

// Generic implementation, mirrors UsersIndexViewModel<TUser, TKey>
public class UserFormViewModel<TUser, TKey> : IUserFormViewModel
    where TUser : IdentityUser<TKey> where TKey : IEquatable<TKey>
{
    public TUser User { get; }
    object IUserFormViewModel.User => User;
    IReadOnlyList<UserFormFieldViewModel> IUserFormViewModel.Fields => _fields;
    // Title/Subtitle projected from UsersEditPageOptions
}
```

Notes:
- With no per-request data to resolve, `BuildFormViewModel` is synchronous and the "descriptor" is just the config-time field definition plus the current user — the view-model layer gets *thinner* than the index's.
- No `Value` property: the renderer reads values from `User` via cached compiled getters (the `DataGridFieldGetters` mechanism), preferring `ModelState.AttemptedValue` on redisplay so conversion failures ("abc" into an `int`) round-trip correctly — same as `asp-for` inputs.
- Create reuses the same interface; password/confirm are built-in markup in `Create.cshtml` bound to a small library-owned input model, not entries in `Fields`.

Rendered HTML (sketch):

```html
<label for="Email">Email</label>
<input type="email" name="Email" value="jerrie@example.com" required />

<label for="FirstName">First name</label>          <!-- from [Display] -->
<input name="FirstName" value="Jerrie" maxlength="100" />

<label for="Country">Country</label>
<select name="Country">
    <option value="ZA" selected>South Africa</option>
    ...
</select>
```

## 1c. The request, end to end

1. GET `/admin/users/edit/123` → user loaded → default view loops the field descriptors; the `CountrySelect` template fetches its own countries via `@inject` as it renders.
2. User clears First name past 100 chars, posts.
3. `TryUpdateModelAsync` copies `Email`, `FirstName`, `LastName`, `Country` (and nothing else — a hostile extra `LockoutEnd=...` form value is ignored by the filter) onto the entity; `[MaxLength]` fails → `ModelState` error.
4. Redisplay: posted values are on the entity, and the template re-fetches its data simply by rendering again. **The "empty dropdown after failed validation" bug cannot happen — there is no lookup state to forget.**
5. Fixed and re-posted → `userManager.UpdateAsync` → redirect to index.

## 1d. Customizing further (the ladder)

```csharp
// Rung 1: custom editor for one field
fields.Add(u => u.Country).Template("CountryPicker");
// -> app provides Views/Shared/EditorTemplates/CountryPicker.cshtml (grid precedent)
```

```cshtml
@* Rung 2: full markup control — shadow Areas/StellarAdmin/Views/Users/Edit.cshtml.
   The schema still drives binding; custom markup just uses matching input names. *@
@model IUserFormViewModel
<form method="post">
    <div class="grid grid-cols-2 gap-4">
        <input name="FirstName" value="@Model.Value("FirstName")" />
        ...
    </div>
</form>
```

Rung 3: don't use our screen at all — point the "New user"/row actions at the developer's own controller.

## 1e. Field editors beyond input/select — one mechanism: named editor templates

There is **no library options API** — data-driven widgets (selects, comboboxes, radio groups) are just templates that `@inject` their own services. Editor resolution chain, first match wins:

1. `.Template("X")` on the field builder — screen-specific override.
2. `[UIHint("X")]` on the property/buddy — entity-wide choice.
3. Inference from metadata: `[DataType(MultilineText)]` → TextArea, `[EmailAddress]` → email, `[Phone]` → tel, `[DataType(Password)]` → password; then CLR type: `bool` → Switch, numeric → Number, `DateTime`/`DateOnly`/`TimeOnly` → date/time pickers, `enum` → Select from `[Display]` member names (nullable adds an empty option; needs no external data, so it stays a built-in), `string` → Text.
4. Unknown complex type → fail loudly at startup ("specify a Template for this field").

Built-ins ship as named templates in the RCL (`FormEditorTemplates/` folder convention, mirroring the grid's `GridDisplayTemplates`); the app overrides any of them by dropping a same-named `.cshtml` in `Views/Shared/FormEditorTemplates/`. Rendering dispatches through a `ModelExplorer` rooted at the entity property, so input naming (TemplateInfo prefix), value round-tripping, and client-side `data-val-*` attributes all come from standard MVC machinery. No `.Editor(enum)` API in v1 — one mechanism (names) for built-in and custom alike; enum sugar can come later if wanted (Sortable precedent: no knobs before need).

A complete custom widget — Markdown editor for `Bio` — is one file plus one attribute:

```csharp
[UIHint("Markdown")]                    // or per-screen: fields.Add(u => u.Bio).Template("Markdown")
public string? Bio { get; set; }
```

```cshtml
@* Views/Shared/FormEditorTemplates/Markdown.cshtml — the entire extension *@
@model string?
<textarea name="@ViewData.TemplateInfo.HtmlFieldPrefix" data-markdown-editor
          class="sa-input min-h-40">@Model</textarea>
```

Postback needs nothing extra: the widget posts one value named `Bio`, which the schema already declares. The general rule: **any widget that funnels its result into a single named input (visible or hidden) binds with zero additional machinery** — tag pickers, color pickers, rich text, comboboxes. And because templates are async partials, a data-driven widget carries its own data (`@inject` + `await`, like `CountrySelect` in 1a) — no `Prepare` hooks, no options factories, no view-data channel anywhere in the design.

Honest boundary: widgets whose posted shape is NOT the property's scalar value — file uploads (`IFormFile`), inline collection editors — are outside the schema and need either future per-field bind callbacks or the approach-2 escape hatch. (Multi-selects onto `List<string>` do bind via MVC's repeated-name collection binding.)

## 1f. The Create screen: password + confirm, and the non-entity boundary

Password is not an entity property — it's an *argument* to `UserManager.CreateAsync(user, password)`, validated by Identity's configured password policy (`PasswordOptions` / `IPasswordValidator<TUser>`) and hashed by the store. Treating it as a schema field would be wrong, not just awkward: there is no property to bind onto, and its validation source is Identity's policy, not data annotations. So the design rule is:

> **Schema fields = entity properties. UserManager operations = built-in page features.**

Password/confirm ships as a built-in on the Create page, bound to a small library-owned input model:

```csharp
// Library-owned — the developer never sees this unless they override the view
public sealed class CreateUserPasswordInput
{
    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = "";
}
```

Deliberately absent: length/complexity annotations. Identity's configured password policy stays the single source of truth — `CreateAsync` runs all `IPasswordValidator`s and returns every violation in one `IdentityResult`.

Controller flow — the schema bind and the password bind are two independent binds against the same form post:

```csharp
[HttpGet]
public IActionResult Create()
{
    var user = Activator.CreateInstance<TUser>();          // blank entity = default values + metadata root
    return View(BuildFormViewModel(user));
}

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Create(CreateUserPasswordInput passwordInput)   // MVC binds + validates this
{
    var user = Activator.CreateInstance<TUser>();
    await TryUpdateModelAsync(user, prefix: "",
        propertyFilter: m => writableCreateFields.Contains(m.PropertyName));     // schema fields

    if (!ModelState.IsValid)                               // entity annotations + [Required]/[Compare] on password
        return View(BuildFormViewModel(user));

    var result = await userManager.CreateAsync(user, passwordInput.Password);
    if (!result.Succeeded)
    {
        // Known error codes map to field keys: PasswordTooShort/PasswordRequires* -> "Password",
        // DuplicateUserName/DuplicateEmail -> that field; unknown codes -> summary.
        AddIdentityErrors(result, ModelState);
        return View(BuildFormViewModel(user));
    }

    return RedirectToAction(nameof(Index));
}
```

`Create.cshtml` renders the schema field loop, then the password block — which is itself a **named built-in template** (`FormEditorTemplates/PasswordCreate.cshtml`), so its markup is overridable through the exact same shadowing mechanism as every other editor. Repositioning it between schema fields = shadow `Create.cshtml` (rung 2 of the ladder).

Details worth pinning:
- Input names are `Password` / `ConfirmPassword` at the root prefix. A `TUser` with a property literally named `Password` would collide — vanishingly unlikely (Identity entities carry `PasswordHash`), documented rather than engineered around.
- Failed validation redisplays with schema-field values intact (they're on the entity); password inputs intentionally rerender empty, standard security practice.
- This boundary generalizes forward: role assignment (`AddToRolesAsync`), lockout actions, password *reset* on the Edit screen, 2FA reset — all UserManager operations, so all future built-in page features (buttons/sections/dialogs), never schema fields. If an app ever needs a truly custom non-entity input, that's the future per-field bind-callback idea or the approach-2 escape hatch — password doesn't force that machinery into v1.

## 1g. Repercussions

- **Ceremony:** ~4 lines of config plus one reusable template file for the whole scenario. Reads like the columns API consumers already know; the field entry builder is tiny (`Title` / `Template` / `ReadOnly`).
- **Upgrade story:** the default view renders custom fields, so our future markup/UX improvements reach consumers without re-work. Nothing is forked.
- **Over-posting:** structurally impossible — undeclared and `ReadOnly()` properties never pass the filter. No VM needed to get this.
- **Validation location:** must live on the entity or a buddy class. Form-only rules ("required on create, optional on edit") don't fit naturally — the schema would need per-field validation config later (`.Required()` etc.) if that's ever wanted.
- **Form shape is chained to entity shape.** Inputs that aren't 1:1 entity properties don't fit the schema: ConfirmPassword, role-membership checkboxes, a single "Full name" box split into two properties. Password/Confirm on the Create screen are library-built special fields (bound to a small internal input model, passed to `CreateAsync(user, password)`), but *arbitrary* non-entity inputs need a future concept (per-field `Apply` callbacks à la Nova `fillUsing`) or a drop to approach 2.
- **Binding target is a live tracked entity.** On a failed POST the in-memory entity is modified but discarded with the request scope; nothing persists (`UpdateAsync` is only called on success). Technically fine; worth being honest that some people dislike binding into entities on principle.
- **Library build cost: medium** — field descriptors, the `sa-form-field` metadata-driven renderer with async-partial template dispatch, filtered binding. The options pipeline (option types, sync/async factory overloads, view-data channel, select/radio knobs) no longer exists. Counterpoint stands: the zero-config default screens need the field-rendering mechanism *anyway*; default screens and customization are the same code path.
- **Templates own their data** (`@inject`): the library never transports lookup data, and redisplay repopulation is automatic because rendering *is* fetching. Flip side: data access from a Razor file — idiomatic (`@inject` is a first-class Razor feature; Identity UI uses it) but some teams prefer data access out of views; those teams can keep the fetch in a service and the template one line.

---

# Approach 2 — Explicit custom view model (classic MVC)

The library owns only the controller skeleton and lifecycle; the developer owns the model, the mapping, and the markup. This is the shape Jerrie's instinct pointed at.

## 2a. Everything the developer writes

The view model:

```csharp
public class EditUserModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Display(Name = "First name"), MaxLength(100)]
    public string? FirstName { get; set; }

    [Display(Name = "Last name"), MaxLength(100)]
    public string? LastName { get; set; }

    public string? Country { get; set; }

    [BindNever, ValidateNever]                       // view data, not a form input
    public IReadOnlyList<SelectListItem> Countries { get; set; } = [];
}
```

The wiring — three lifecycle hooks:

```csharp
users.Edit(edit => edit.UseModel<EditUserModel>(model =>
{
    // GET: entity -> form
    model.Load((user, vm) =>
    {
        vm.Email     = user.Email!;
        vm.FirstName = user.FirstName;
        vm.LastName  = user.LastName;
        vm.Country   = user.Country;
    });

    // GET *and* every invalid-POST redisplay: populate lookup data
    model.Prepare(async (vm, sp) =>
    {
        vm.Countries = await sp.GetRequiredService<ICountryLookup>().AllAsync();
    });

    // valid POST: form -> entity
    model.Apply((vm, user) =>
    {
        user.Email     = vm.Email;
        user.FirstName = vm.FirstName;
        user.LastName  = vm.LastName;
        user.Country   = vm.Country;
    });
}));
```

The **mandatory view override** — the library's shipped view cannot render a type it has never heard of, so the developer copies and owns `Areas/StellarAdmin/Views/Users/Edit.cshtml`:

```cshtml
@model EditUserModel
@{ ViewData["Title"] = "Edit user"; }

<sa-page-container>
    <sa-page-header title="Edit user"/>
    <form method="post">
        <div asp-validation-summary="ModelOnly"></div>

        <sa-form-field asp-for="Email"/>
        <sa-form-field asp-for="FirstName"/>
        <sa-form-field asp-for="LastName"/>
        <sa-form-field asp-for="Country">
            <select asp-for="Country" asp-items="Model.Countries"></select>
        </sa-form-field>

        <sa-button type="submit">Save</sa-button>
    </form>
</sa-page-container>
```

(We'd ship an `asp-for`-based `sa-form-field` tag helper so hand-written forms stay pleasant — label + input + validation span in one tag.)

Total: ~80 lines across three files, per screen. Create needs its own VM (`CreateUserModel` with `Password`/`ConfirmPassword`), its own hooks, its own view.

## 2b. What the library builds

```csharp
[HttpGet]
public async Task<IActionResult> Edit(string id)
{
    var user = await userManager.FindByIdAsync(id);
    if (user is null) return NotFound();

    var vm = Activator.CreateInstance(editOptions.ModelType)!;
    await editOptions.Load(user, vm);
    await editOptions.Prepare(vm, HttpContext.RequestServices);
    return View(vm);                                   // resolves to the dev's overridden view
}

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(string id)
{
    var user = await userManager.FindByIdAsync(id);
    if (user is null) return NotFound();

    var vm = Activator.CreateInstance(editOptions.ModelType)!;

    // The VM is the over-posting allow-list, classically.
    // Validation comes from the VM's own annotations.
    await TryUpdateModelAsync(vm, editOptions.ModelType, prefix: "");

    if (!ModelState.IsValid)
    {
        await editOptions.Prepare(vm, HttpContext.RequestServices);  // refill Countries
        return View(vm);                                             // posted values kept
    }

    await editOptions.Apply(vm, user);
    var result = await userManager.UpdateAsync(user);
    if (!result.Succeeded) { /* errors -> ModelState, Prepare, redisplay */ }

    return RedirectToAction(nameof(Index));
}
```

The zero-config default (developer configured nothing): the library ships a fixed internal `DefaultEditUserModel` (Email, PhoneNumber, ...) with built-in Load/Apply and a fixed view. **Customization is wholesale replacement** — there is no rung between "stock screen" and "own the model, mapping, and markup".

## 2c. The request, end to end

1. GET → `Load` maps entity to VM → `Prepare` fills `Countries` → dev's view renders.
2. Invalid POST → VM bound, `[MaxLength]` on the **VM** fails → library re-runs `Prepare` (if the dev forgot to register lookups in `Prepare` and did it in `Load` instead, the dropdown comes back **empty** — this classic bug is now the developer's to avoid, every screen) → redisplay.
3. Valid POST → `Apply` copies VM to entity → `UpdateAsync` → redirect.

## 2d. Repercussions

- **Ceremony: the highest.** VM + three hooks + a forked view (~80 lines) to add three fields. Doubled for Create.
- **Zero magic, maximum control.** Every behavior is code the developer can read and step through. Form shape fully decoupled from the entity: ConfirmPassword, computed fields, role checkboxes are all just VM properties handled in `Apply`.
- **Validation lives on the VM** — its canonical home; no attributes forced onto EF entities; create-vs-edit rules are just different VMs.
- **The view fork is the expensive part.** Once `Edit.cshtml` is copied, our styling/UX/accessibility improvements never reach that consumer again — the exact upgrade problem ASP.NET Identity scaffolding has.
- **Lifecycle sharp edges move to the developer**: the `Load`/`Prepare` split, keeping input names in sync between VM and view, remembering `[BindNever]` on view-data properties (forgetting it = an over-posting hole *they* created).
- **Create needs a password convention**: the library must know which VM property to pass to `CreateAsync(user, password)` — a marker attribute or a builder call like `model.Password(vm => vm.Password)`.
- **Library build cost: the lowest.** Bind-a-`Type`, call three delegates. Least code, least to test, least to document — for us. The cost moved to every consumer.

---

# Approach 3 — Convention-bound view model ("the VM *is* the schema")

Middle path: the developer writes only the VM; conventions handle entity↔VM mapping and rendering. VM ergonomics without the view fork.

## 3a. Everything the developer writes

```csharp
public class EditUserModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Display(Name = "First name"), MaxLength(100)]
    public string? FirstName { get; set; }

    [Display(Name = "Last name"), MaxLength(100)]
    public string? LastName { get; set; }

    [SelectOptions(nameof(Countries))]               // render as <select>, options from Countries
    public string? Country { get; set; }

    [BindNever, ValidateNever, ScaffoldColumn(false)]  // view data: not bound, not rendered
    public IReadOnlyList<SelectListItem> Countries { get; set; } = [];
}
```

```csharp
users.Edit(edit => edit.UseModel<EditUserModel>(model =>
{
    model.Prepare(async (vm, sp) =>
        vm.Countries = await sp.GetRequiredService<ICountryLookup>().AllAsync());
    // No Load/Apply: Email, FirstName, LastName, Country auto-map by name
    // to ApplicationUser, both directions.
}));
```

No view files. ~30 lines total.

## 3b. What the library builds

Everything from approach 2's controller, **plus**:

```csharp
// Startup, once per registered VM type — fail fast, no silent drift:
// for each writable, bindable VM property, find the same-named property on TUser.
// No match and no [NotMapped] -> throw with a "did you rename?" message.
// [NotMapped] properties are bound + validated but left for a dev Apply hook.
```

```cshtml
@* Default view: renders ANY view model generically from ModelMetadata,
   ordered by declaration order / [Display(Order)] *@
@model object
<form method="post">
    @foreach (var prop in ViewData.ModelMetadata.Properties
                              .Where(p => p.ShowForEdit))    // ScaffoldColumn(false) excluded
    {
        <sa-form-field for-metadata="prop" model="Model"/>   @* select if [SelectOptions] *@
    }
    <sa-button type="submit">Save</sa-button>
</form>
```

## 3c. Repercussions

- **VM benefits at a fraction of approach 2's cost**: validation on the VM, decoupled shape, ConfirmPassword is just a `[NotMapped]` property with a small `Apply` hook — and the default view keeps working, so upgrades flow.
- **The most magic.** Name-based mapping must fail loudly at startup or it becomes silent data loss on a rename; there's an attribute vocabulary to learn (`[SelectOptions]`, `[NotMapped]`, `[ScaffoldColumn]`); and "why did this field render as X?" is answered by convention rules, not code the developer wrote.
- **We build approaches 2 and 3.** Everything conventions can't express falls back to `Load`/`Apply`/`Prepare` hooks — so approach 2's machinery is built regardless, with the convention layer on top. **Highest library build cost**, most edge cases (arbitrary property types in the generic renderer, mapping type mismatches, collections).

---

# Side by side

| | 1: Field schema | 2: Explicit VM | 3: Convention VM |
|---|---|---|---|
| Developer code for the scenario | ~4 lines config + 1 reusable template | VM + 3 hooks + forked view (~80 lines) | VM + attributes + 1 hook (~30 lines) |
| Extra classes/files per screen | 0 | 3 (VM, config, view) | 1 (VM) |
| Validation attributes live on | Entity / buddy class | VM | VM |
| Countries dropdown | Self-contained `CountrySelect` template (`@inject`), reusable everywhere | VM property + `Prepare` + markup | VM property + `[SelectOptions]` + `Prepare` |
| Our default view still used | Yes | No — fork mandatory | Yes |
| Non-entity inputs (ConfirmPassword, roles) | Built-in special fields; else future callbacks | Trivial | `[NotMapped]` + small hook |
| Lookups on invalid-POST redisplay | Automatic (template re-fetches on render) | Developer's job | `Prepare` re-run by library |
| Over-posting protection | Schema allow-list filter | VM is the allow-list (dev must remember `[BindNever]`) | VM + startup-checked mapping |
| Magic level | Low-medium | None | High |
| Library build cost | Medium-high | Lowest | Highest (includes 2's) |
| Consumer upgrade story | Best | Worst | Good |
| Symmetry with existing API | Mirrors `Columns.Add` exactly | New shape | New shape |

## How they combine

1 and 2 layer cleanly (Django precedent: declarative field config as the baseline, custom-form class as the escape hatch). 3 is an alternative *flavor* of 2, not a companion to it. The realistic long-term shapes are:

- **1 now, 2 later** — schema baseline; `UseModel<T>` added when real cases outgrow the schema.
- **2 only** — smallest library, all flexibility, all ceremony on the consumer.
- **3 (with 2's hooks as fallback)** — VM-centric world, biggest build.

## Where this document's author lands (opinion, not a decision)

Approach 1 as the baseline: it is the only option where adding `FirstName` costs one line, it's symmetric with the columns API consumers already learned, and its renderer must exist anyway for the zero-config screens. Approach 2's `UseModel<T>` as the *documented escape hatch* — added in v1 or later — for form shapes the schema can't express. Approach 3 dropped: it buys ergonomics approach 1 already provides, at the highest cost and magic level.

Held for review — no decision requested until Jerrie has read this.

---

## Appendix: settled facts that apply regardless of choice

- `TKey` need not reach the options layer: `UserManager.FindByIdAsync(string)` — route ids stay strings.
- Create/Edit are new actions on the existing `UsersController<TUser, TKey>`; `IdentityControllerFeatureProvider` / `IdentityControllerNameConvention` untouched.
- Create needs a `TUser` instance: `Activator.CreateInstance<TUser>()` (adding a `new()` constraint to existing overloads would be binary-breaking).
- `Create(...)`/`Edit(...)` are already reserved as future siblings of `Index(...)` in the identity-configuration design doc.
- Prior-art research (ABP dynamic field registry, Orchard display drivers, ASP.NET Identity scaffolding, Django/Filament/Nova) is summarized in the conversation; ABP's per-operation field flags and lookup-on-the-field-declaration influenced approach 1's design.
- Exact `TryUpdateModelAsync` overload for the filtered entity bind to be confirmed at implementation time (`ModelBindingHelper.TryUpdateModelAsync` has the `propertyFilter` shape if the controller-level overload doesn't fit).
