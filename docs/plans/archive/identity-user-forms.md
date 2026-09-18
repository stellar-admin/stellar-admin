# Identity user Create/Edit screens — status + implementation plan

## Code audit — 2026-09-18

Current status: **superseded**. Create/edit and filtered entity binding are implemented in `UsersController.cs` and `ResourceControllerBase.cs`. The shared form prefix is now `Entity`, and Dashboard ships styled `Views/Shared/EditorTemplates/` using StellarAdmin controls, superseding the old no-template/no-tag-helper constraints. The exact deferred `FirstName`/`LastName`/`CountrySelect` playground proof is absent; do not mark that experiment as performed. Styled editors are covered by the current EF integration fixtures, but this audit did not run an Identity-specific custom-widget proof. Use `docs/design/identity-user-forms.md` and current sources for future work.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

**Index status:** needs-reconciliation. **Indexed:** 2026-09-05. Historical execution/review notes need reconciliation against current code before resuming. This migration does not infer outstanding approval or completion.

**Status (2026-08-06): design approved.** The canonical, up-to-date design lives at
`stellar-admin-pro/docs/design/identity-user-forms.md` — read that first; this file
is only the phase plan + session status. Research and rejected alternatives:
[identity-user-forms-approaches.md](identity-user-forms-approaches.md).

## Critical corrections from the 2026-08-06 discussion (do NOT regress)

1. **Rendering is 100% stock ASP.NET Core.** The RCL views call
   `Html.Label(expression, field.Title)` / `Html.Editor(expression, field.Template)` /
   `Html.ValidationMessage(expression)` with `expression = "User." + field.FieldName`
   against `@model UserFormViewModel` (whose `User` property is `object`).
   **Dead ideas — do not resurrect:** the `sa-user-form-field` renderer tag helper,
   metadata-from-`LambdaExpression` resolution, compiled value getters, the dedicated
   `FormEditorTemplates/` folder, any custom template search order. Jerrie rejected
   each explicitly ("piggyback standard asp.net core infra as much as possible").
2. **Why the string expression works** (spike-verify as the FIRST Phase 2 task):
   `ExpressionMetadataProvider.FromStringExpression` -> `ViewDataEvaluator.Eval`
   walks the *runtime* object graph; a property access yields metadata from the
   runtime container type (the actual `ApplicationUser`), so `[Display]`,
   `[DataType]`, `[UIHint]`, and buddy classes resolve. Declared-type fallback only
   fires when the value is null — `User` never is. (My earlier claim that the
   string overload loses metadata was wrong.)
3. **v1 ships NO editor templates at all** (Jerrie, 2026-08-06): MVC's built-in
   default HTML generators render plain inputs/checkboxes/textareas. Verify the
   entire flow before any styling. Styled templates are deferred wholesale (incl.
   bool->Switch, fail-loudly `Object.cshtml`) and **must not render StellarAdmin
   tag helpers** when they do come.
4. **Binding**: `TryUpdateModelAsync(user, prefix: "User", propertyFilter)` — the
   filter is the schema's writable field names (top-level, exact overload to
   confirm). Password inputs on Create bind unprefixed via the
   `CreateUserPasswordInput` action parameter — no name collisions possible.
5. **Overrides** = exact-path shadowing under the app's own
   `Areas/StellarAdmin/Views/...` (ASP.NET Identity default UI pattern). Custom
   widgets = standard app editor templates (`Views/Shared/EditorTemplates/X.cshtml`),
   selected via `.Template("X")` or `[UIHint("X")]`.

## Phases (stop-and-review after each; Jerrie commits himself)

**Phase 0 — DONE 2026-08-06.** Canonical design doc created; builder tree in
`identity-configuration.md` updated.

**Phase 1 — DONE 2026-08-06 (uncommitted).** Configuration layer in
`StellarAdmin.Identity`: `IdentityFormField`, abstract `UsersFormPageOptions<TUser>`
+ `UsersCreatePageOptions`/`UsersEditPageOptions` (seeds: Create UserName/Email,
Edit UserName/Email/PhoneNumber; defaults "Create user"/"Edit user"),
`Create(...)`/`Edit(...)` verbs, `UsersCreatePageBuilder`/`UsersEditPageBuilder`,
shared `UsersFormFieldsBuilder` (`Add` throws unless a direct `TUser` property) +
`UsersFormFieldBuilder` (`Title`/`Template`/`ReadOnly`). `BuildFieldExpression`
moved to `FieldExpressionHelper`. Playground `Program.cs` exercises everything;
builds clean; CSharpier run; smoke-run OK.

**Phase 2 — DONE 2026-08-06 (uncommitted).** Spike CONFIRMED: `Html.Editor` on
`"User.X"` string expressions against `object User` resolves runtime +
`[ModelMetadataType]` buddy metadata (label "Phone", `data-val-phone`, and
`type="tel"` all flowed from `ApplicationUserMeta`). Shipped: `UserFormViewModel`
(plain sealed non-generic), Edit GET action, `Edit.cshtml` stock-helper loop
(Jerrie tweaked: `PageContainerWidth.Medium`, `sa-button` submit), index row
links via `GetRowId(object)` on `IUsersIndexViewModel` + an
`sa-data-grid-item-template` actions column (`Html.GridItem<object>()`;
`@using StellarAdmin.Pro.TagHelpers` added to RCL `_ViewImports`). Bonus finding:
the MVC form tag helper auto-injects the antiforgery token into the bare
`<form method="post">`.

**Phase 3 — DONE 2026-08-06 (uncommitted).** `EditPost` (`[ActionName("Edit")]`)
on `UsersController`: the `TryUpdateModelAsync(model, prefix, propertyFilter)`
ControllerBase overload exists and works — filter is
`property.PropertyName is { } name && writable.Contains(name)` (PropertyName is
nullable). `AddIdentityErrors` maps codes via
`nameof(IdentityErrorDescriber.DuplicateUserName)` etc. to `"User.X"` ModelState
keys; unknown codes -> `Html.ValidationSummary(excludePropertyErrors: true)`
added to `Edit.cshtml`. `[Phone]` added permanently to playground
`ApplicationUserMeta.PhoneNumber` for the buddy-validation round trip. Verified:
happy path 302 + db persisted; invalid phone redisplays with field error +
attempted value; DuplicateUserName renders at `User.UserName`; over-posting
`EmailConfirmed`/`AccessFailedCount`/readonly `LockoutEnd` provably ignored.

**Phase 4 — DONE 2026-08-06 (uncommitted).** `CreateUserPasswordInput`
(ViewModels/; Required/DataType(Password)/Compare + [Display], no policy
annotations), Create GET/POST on `UsersController` (`Activator.CreateInstance`,
dual bind via the shared `BindFormFieldsAsync` helper — `EditPost` refactored onto
it too), `Create.cshtml` (schema loop + unprefixed `Html.Password` inputs;
password metadata does NOT resolve via string expressions on the view model — no
data-val on password inputs, server-side parameter validation only, accepted for
v1), `AddIdentityErrors` extended with the six `PasswordTooShort`/
`PasswordRequires*` codes -> `Password` key, index "New user" now
`<sa-linkbutton asp-action="Create">`. Verified: happy path 302 + row with hash;
PasswordTooShort renders at the Password input with schema values redisplayed and
passwords emptied; Compare mismatch at ConfirmPassword; missing password Required
errors; no rows persisted on failures. Addendum 2026-08-07: optional
`create.CreateInstanceUsing(Func<TUser>)` builder verb ->
`UsersCreatePageOptions.InstanceFactory`, with internal `CreateInstance()`
owning the `Activator` fallback; used by both Create GET and POST. Verified:
factory `EmailConfirmed = true` default persists while posted fields bind over
it. DI-aware/async factory overloads deliberately deferred.

**Phase 5 — custom widget proof + styling (deferred; scope TBD with Jerrie).**
Playground `FirstName`/`LastName`/`Country` + EF migration + app `CountrySelect`
standard editor template proving `.Template()`/`@inject`; then the styled built-in
template set (all still-open decisions live in the design doc's Deferred
decisions).

## Standing session rules

- Jerrie reviews and commits himself; never `git commit`.
- Discussion mode: answer, fold into docs, end turn — no approval prompts.
- Playground runs: my instances on port 5206, always stopped after; Jerrie uses 5205.
