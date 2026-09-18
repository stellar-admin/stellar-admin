# Review: consumer view-override escape hatches

## Code audit — 2026-09-18

Current status: **reference**. The identified typed-row gap is closed by `ResourceIndexPageViewModel<TEntity>.Page` and `PagedListViewModel<TEntity>.Items`. Dashboard shared partials, Identity page views, and the templated tag-helper `view`/slot mechanism remain. No configurable `RowActions` builder was found; copying `_IndexDataGrid.cshtml` remains the existing route. Keep this as historical rationale; names in the body predate the current resource models.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

**Index status:** reference. **Indexed:** 2026-09-05. Research/specification record; read the current design before using historical examples.

Status: **decided 2026-08-10** — Gap A resolved as a fourth option Jerrie recalled from
the original design (see the decision note in Gap A); the rest of the recommendations
stand. This was the
escape-hatch review the resource layer plan deferred to "after the Roles pages are in":
what an application overriding the shipped Identity views can reach, and how granular
the overrides can be.

The short answer: the override surface is already granular — seven levels, from a single
grid cell up to the whole page — because everything rides the stock ASP.NET Core
mechanisms ([[piggyback-standard-aspnet-infrastructure]]). Six of the seven levels need
**no library change at all**. The one real gap is the typed entity list on the index
page (the old `UserList` need from Phase 8), and closing it is a ~20-line change.

## How overriding works (the mechanism)

The Identity views are compiled into the RCL. ASP.NET Core's view resolution gives the
host application two levers, both standard and both used by the ASP.NET Core Identity
default UI:

1. **Exact-path shadowing.** A `.cshtml` in the app at the same path as an RCL view wins
   (documented framework behavior: app views take precedence over RCL views).
2. **Search-order interception.** A `<partial name="..."/>` resolves at request time
   through the view engine's search path — `/Areas/{area}/Views/{controller}/{partial}`
   before `/Areas/{area}/Views/Shared/{partial}`. The RCL only ships the partials in
   `Shared/`, so an app file in the *controller* folder intercepts the partial for that
   one resource without the RCL shipping anything there.

## The override ladder (what already works)

All paths are in the **host application**, under `Areas/StellarAdmin/Views/`.

| # | To change... | App file / config | Scope | Library change |
|---|---|---|---|---|
| 1 | One grid cell's rendering | `Views/Shared/GridDisplayTemplates/X.cshtml` + `.Template("X")` or `[UIHint("X")]` | one column | none |
| 2 | One form field's widget | `Views/Shared/EditorTemplates/X.cshtml` (+ `.Template("X")` / `[UIHint]` / by type) | one field | none |
| 3 | The grid, one resource | `Users/_IndexDataGrid.cshtml` | Users only | none |
| 4 | The grid, every resource | `Shared/_IndexDataGrid.cshtml` | all resources | none |
| 5 | Page chrome (header, tabs, search) | `Shared/_IndexPage.cshtml`, `Shared/_FormPage.cshtml`, `Shared/_FormFields.cshtml` | all resources | none |
| 6 | The whole page | `Users/Index.cshtml`, `Users/Edit.cshtml`, `Users/Create.cshtml`, `Roles/...` | one page | none |
| 7 | The shell layout | `Shared/_Layout.cshtml` (shadows the OSS Shell's) | everything | none |

Levels 1–2 are configuration + a template file — no view is overridden at all. Level 2
is already the documented design in `identity-user-forms.md` ("a custom widget is a
plain standard editor template"). Levels 3–6 are shadowing. A level-6 override can also
be *additive*: keep `<partial name="_IndexPage"/>` and wrap it with extra markup.

*(Added 2026-08-13: the page views render through templated tag helpers —
`<sa-form-page model="Model">` and `<sa-index-page model="Model">` — so a level-6
override has a lighter additive shape: keep the tag helper and fill slot outlets
instead of copying the chrome. `_FormPage` exposes `pre`/`post-form-fields` and
`pre`/`post-form-actions`; `_IndexPage` exposes `pre`/`post-page-actions` around the
create button. See `templated-view-slots.md`.)*

Two hatches that already reach the **typed entity** with no change:

- **Inside a grid column** (level 3/4): `Html.GridItem<ApplicationUser>()` — the helper
  pattern-matches the row's runtime type, so the app's type works directly:

  ```cshtml
  <sa-data-grid-column title="Actions">
      <sa-data-grid-item-template>
          @{ var user = Html.GridItem<ApplicationUser>(); }
          <a asp-action="Edit" asp-route-id="@user.Id">Edit</a>
          <a asp-action="Impersonate" asp-route-id="@user.Id">Impersonate</a>
      </sa-data-grid-item-template>
  </sa-data-grid-column>
  ```

- **On the form pages**: `FormViewModel.Entity` is `object` with a documented cast —
  `@((ApplicationUser)Model.Entity)` — decided 2026-08-06 and unchanged.

And for anything not on a view model, an overridden view can read the configuration
directly — the options are public singletons precisely for this (recorded in
`identity-configuration.md`):

```cshtml
@inject StellarAdmin.Identity.Options.IdentityUsersOptions<ApplicationUser> UsersOptions
```

## Prerequisite the ladder needs documented: the app's `_ViewImports`

An override view compiles in the **app's** project, so the RCL's `_ViewImports.cshtml`
does not apply to it. Before any level 3–6 override renders, the app needs its own
`Areas/StellarAdmin/Views/_ViewImports.cshtml`:

```cshtml
@using StellarAdmin.Identity.Areas.StellarAdmin.ViewModels
@using StellarAdmin.Identity.Areas.StellarAdmin.ViewModels.Internal
@using StellarAdmin.Pro.TagHelpers
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, StellarAdmin.TagHelpers
@addTagHelper *, StellarAdmin.Pro
```

One-time setup, same as the Identity UI scaffolder generates. This is a documentation
item (and, much later, maybe a scaffolding story).

## Gap A — the typed row list on the index page (the decision to make)

> **Decision (Jerrie, 2026-08-10): none of the three options below.** The original
> design intent resurfaced: `IIndexViewModel` is *for the library* — it exists only
> because the shipped views, shared between resources, cannot declare an open-generic
> `@model`. The consumer surface is the **class**: an overriding view declares
> `@model IndexViewModel<ApplicationUser>` (the controller passes exactly that type, so
> it binds) and gets typed members directly. Implemented same day: the adapted members
> (`Title`, `CreateLabel`, `Empty*`, `Scopes`, sort/search state) became public implicit
> implementations, while the unadapted paging data is exposed as the object it already
> is — `public PagedListViewModel<TEntity> Page` (so `Model.Page.Items`,
> `Model.Page.PageNo`; the five object-typed paging members of the interface stay
> explicit implementations) — plus a typed `GetRowId(TEntity)`. The rule this sets:
> expose adapted values as members, unadapted data as the object it already is. The
> interface stays in `Internal` with
> `[EditorBrowsable(Never)]`, and the `Rows<TEntity>()` extension was dropped as
> pointless — which also drops Option 2's `@(...)` Razor syntax tax. Verified end-to-end
> in the playground. The untyped `@model IIndexViewModel` remains the (undocumented)
> fallback for an override of `Shared/_IndexDataGrid.cshtml` spanning all resources.

An overriding index view (level 3, 4, or 6) declares `@model IIndexViewModel`. Two
problems:

1. **The interface disowns its consumers.** It lives in namespace
   `...ViewModels.Internal`, is `[EditorBrowsable(Never)]`, and its summary reads "For
   internal use to support default views" — yet it is the *required* `@model` for the
   override story. `EditorBrowsable(Never)` also hides it (and its members) from
   IntelliSense in the app.
2. **`Rows` is `IReadOnlyList<object>`.** The typed list — the reason
   `UsersIndexViewModel.UserList` used to be public — is unreachable in typed form:
   every member of `IndexViewModel<TEntity>` is an explicit interface implementation and
   the page is a private field, so even `@model IndexViewModel<ApplicationUser>` reaches
   nothing.

### Option 1 — bless the cast (minimum)

Library side: move `IIndexViewModel` out of `Internal` (namespace
`...Areas.StellarAdmin.ViewModels`), drop `[EditorBrowsable(Never)]`, reword the summary
to "The view model of the index page views." No new members.

Dev side:

```cshtml
@model IIndexViewModel

@foreach (var user in Model.Rows.Cast<ApplicationUser>())
{
    <li>@user.Email — @user.LockoutEnd</li>
}
```

### Option 2 — Option 1 plus a generic accessor (recommended)

Library side, on top of Option 1 (~10 lines in `ViewModels/`):

```csharp
public static class IndexViewModelExtensions
{
    /// <summary>Returns the rows of the current page as the application's entity type.</summary>
    /// <exception cref="InvalidCastException">A row is not of type <typeparamref name="TEntity" />.</exception>
    public static IEnumerable<TEntity> Rows<TEntity>(this IIndexViewModel model)
        where TEntity : class
    {
        return model.Rows.Cast<TEntity>();
    }
}
```

Dev side:

```cshtml
@model IIndexViewModel

@foreach (var user in Model.Rows<ApplicationUser>())
{
    <li>@user.Email — @user.LockoutEnd</li>
}
```

(`Model.Rows<T>()` resolves to the extension despite the `Rows` property: member lookup
with type arguments only considers generic methods. Verify with a compile before
committing to the name; the fallback name is `Model.Items<T>()`.)

This is the same shape as the existing typed hatches — `Html.GridItem<T>()` and the
`FormViewModel.Entity` cast — so the story stays consistent: *the shipped views are
entity-agnostic; an override names the entity type at the point of use.*

### Option 3 — typed `@model` (not recommended)

Library side: add public typed members to `IndexViewModel<TEntity>` (e.g.
`public PagedListViewModel<TEntity> Page => _page;`).

Dev side:

```cshtml
@model IndexViewModel<ApplicationUser>

@foreach (var user in Model.Page.Items) { ... }
```

Rejected in Phase 8 for good reason: it duplicates the interface surface with members
the shipped views never read, and it makes the concrete class part of the compatibility
contract. It also reads worse in an override that still wants the untyped members —
they are explicit implementations, so the dev must cast back to the interface anyway.
Option 2 delivers the same capability without any of this.

## Gap B — the row-actions column (recommend: defer)

The Edit link is baked into `_IndexDataGrid.cshtml`. Adding an action (Delete,
Impersonate...) means copying the whole grid partial (~33 lines) as a level-3 override —
there is no smaller unit, and there *cannot* be one via partials: tag-helper `ParentTag`
binding is lexical and `context.Items` does not cross partial boundaries (recorded in
`identity-configuration.md` — grid and columns must share one file). So finer
granularity inside the grid can only come from configuration, e.g. a
`index.RowActions(...)` builder knob.

Recommendation: **defer the knob and document the copy.** Copying a 33-line partial you
then own is an acceptable hatch (it is the shadcn philosophy the whole library follows),
and the natural time to design `RowActions` is together with the delete action, which is
already on the deferred list — designing the knob now would mean inventing its
requirements.

## A caveat to document, not fix

Shadowing is point-in-time: an app override freezes the markup it copied, and library
markup improvements stop reaching that page. That is inherent to the Identity UI pattern
and fine — but it makes the **view models and partial names the compatibility surface**.
Renaming `_IndexDataGrid`, changing a partial's model type, or removing an
`IIndexViewModel` member is a breaking change once consumers override; keep the partials
small and the models additive.

## Recommendation summary

1. ~~Adopt Option 2 for the typed rows.~~ **Superseded by the decision note in Gap A**:
   the typed hatch is `@model IndexViewModel<ApplicationUser>` with public typed members
   on the class; the interface stays internal-facing. Still the only library change in
   the whole review.
2. **No other new hatches.** Levels 1–7 plus `GridItem<T>()`, the `Entity` cast, and
   `@inject`ed options already cover the granularity; the gaps are documentation.
3. **Defer the row-actions knob** to the delete-action work; the interim hatch is
   copying `_IndexDataGrid.cshtml`.
4. **Record the ladder** (table above), the `_ViewImports` prerequisite, and the
   compatibility-surface caveat in `identity-configuration.md` once decided.
5. Verify empirically in the playground when implementing: one level-3 override
   (`Users/_IndexDataGrid.cshtml` with `GridItem<ApplicationUser>()`), one level-6, and
   the `Rows<T>()` name compiling — not done in this review to leave the tree untouched.
