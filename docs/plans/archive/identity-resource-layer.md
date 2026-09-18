# Plan: shared page classes for Users and Roles

## Code audit — 2026-09-18

Current status: **completed**. UsersController and RolesController use the shared ResourceControllerBase/options/view models; shared views live in Dashboard and Identity-specific pages remain in its integration project.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

**Index status:** completed. **Indexed:** 2026-09-05. Historical record; completion is recorded in the phase/progress notes below. Deferred items require a separately scoped task.

## Progress

All commits are on `master` in `stellar-admin-pro`. All eleven phases are done and verified.

| Phase | State | Commit |
|---|---|---|
| 1 — Close the search extraction, move `FieldExpressionHelper` | done | `8777c5f` |
| 2 — Index page options go general | done | `3ea55c6` |
| 3 — Form page options go general | done | `ff65bdf` |
| 4 — `ResourceOptions<TEntity>` | done | `f88cf78` |
| 5 — The builders go general | done | `f489fff` |
| 6 — DI shape for a second resource | done | `04f6a3c` |
| 7 — Query pipeline and shared controller base | done | `44f02b7` |
| 8 — The view models | done | `3a33621` |
| 9 — Views move to `Views/Shared/` | done | `83e49ff` |
| 10 — Micro-cleanups and the documents | done | `2fe5fbe` |
| 11 — The Roles pages | done | `9f1b317` |

**Where the code stands after Phase 10**

- `Options/` holds only `StellarAdminIdentityOptions` (**not generic**),
  `IdentityUsersOptions<TUser>` and `IdentitySidebarOptions`. Everything else moved to
  `Options/Base/` (13 files).
- `Builders/` holds only `StellarAdminIdentityBuilder<TUser>` and `IdentitySidebarBuilder`.
  The other 12 builders moved to `Builders/Base/`.
- `IdentityUsersOptions<TUser> : ResourceOptions<TUser>` is the single file that holds every
  user-specific default string in the library. It is **its own singleton**, and
  `UsersController` injects it directly, so the paths read `options.IndexPage`.
- `Infrastructure/IdentityResourceRegistration.cs` describes one resource to the code that
  treats every resource alike. It is `internal` and not generic.
- `Infrastructure/Expressions/FieldExpressionHelper.cs` is where the expression helper lives.
- `Infrastructure/Query/` holds the whole index pipeline: `IndexPageRequest`,
  `IndexPageResult<TEntity>`, `IndexPageQuery` and `QueryableSortExtensions`. No MVC type
  and no Identity type appears in any of them.
- `Areas/StellarAdmin/Controllers/ResourceControllerBase.cs` holds `BindFormFieldsAsync`
  and `BuildFormViewModel`. It carries no actions and no `[Area]`.
- `Areas/StellarAdmin/Views/Shared/` holds `_IndexPage`, `_IndexDataGrid`, `_FormPage` and
  `_FormFields`. Every file there is underscore-prefixed, so none can become the area-wide
  fallback view for another controller. `Views/Users/Index.cshtml` and `Edit.cshtml` are
  three lines each. Every user-facing string now lives in `IdentityUsersOptions`.
- `UsersController` is down to 132 lines from 232, and everything left in it is about
  Identity: the `UserManager` calls, the password input and the Identity error map.
- Both `Base` folders have a `NamespaceFoldersToSkip` entry in
  `StellarAdmin.Identity.csproj.DotSettings`, so neither adds a namespace level.
- `sandbox/IdentitySimplePlayground/Program.cs` has not changed once. It must stay that way
  until Phase 11.

**Deliberately still true after Phase 10, do not treat as bugs**

- The design documents are corrected: both use the new class names, the configuration doc
  has a "Resource layer" section recording the settled decisions, and its Roles section
  states the `ConfigureRoles` + overload design. The workspace conventions repo gained
  `docs/conventions/xml-documentation.md` and the overload rule in `options-builders.md`.
- The htmx `<script>` sits in the body of `_IndexPage.cshtml` rather than in the layout's
  Styles section, because a partial cannot declare a section. Move it once the OSS shell
  builder gains `AddScript`, which is on the deferred list.
- `Views/Users/Create.cshtml` is the one page that does not use a shared page partial. It
  carries the password inputs, which no other resource has.

## Context

The Users screens in `StellarAdmin.Identity` are complete. The Roles screens do not
exist. Almost all of the Users code does not touch the ASP.NET Core Identity API. It is
general page code with the word "User" in the names. No file in `Options/` or
`Builders/` refers to `Microsoft.AspNetCore.Identity`. Each general type has only the
constraint `where TUser : class`.

You started to pull this general code out:

- You changed `IdentityGridColumn` to `DataGridColumnOptions`, `IdentityGridDefaultSort`
  to `DataGridDefaultSortOptions` and `IdentityFormField` to `FormFieldOptions`.
- You moved the three page options into `IdentityUsersOptions<TUser>`.
- You moved the form field loop into `_EditFormFields.cshtml` and `FormFieldsBaseView<TModel>`.
- Your uncommitted change replaces `UsersIndexPageSearchOptions<TUser>` with
  `Options/Base/IndexPageSearchOptions<TEntity>`, and it makes `EffectivePlaceholder`
  public.

The direction is clear. A type that names no Identity idea drops the `Users` prefix and
the `Identity` prefix, and it moves to a `Base` folder. A type that holds Identity
strings or Identity plumbing keeps its name and stays.

This plan continues that work to the end. It then adds the Roles pages on the new
classes. At the end, all Identity-specific strings are in two small files, one for users
and one for roles.

## Your decisions

| Item | Decision |
|---|---|
| Scope | The plan contains the shared layer **and** the Roles pages. |
| Builders | One shared builder for each page type. `IndexPageBuilder<TEntity>` serves users and roles. No `Users*` builder stays. |
| Name of the page group | `Resource`. The classes are `ResourceOptions<TEntity>` and `ResourceBuilder<TEntity>`. |
| Location | The shared classes stay in `StellarAdmin.Identity` for now. The `Base` folders mark the classes that move to a separate assembly later. |
| Roles | `ConfigureRoles`, not `AddRoles`. The Roles screens are not opt-in. |
| Role type | New overloads: `AddIdentity<TUser, TRole>()` and `AddIdentity<TUser, TRole, TKey>()`. |
| Root builder | Two separate classes. `StellarAdminIdentityBuilder<TUser, TRole>` does not derive from `StellarAdminIdentityBuilder<TUser>`. |

Vocabulary rule: `TEntity` names the data type. `Resource` names one CRUD section over
one entity type. Do not use "section" — the sidebar code already uses that word.

## The assembly boundary rule

The `Base` folders are a temporary marker. Those classes move to a separate assembly
later. Write the code today as if the boundary already exists.

**A `Base` type must expose to the Identity layer with `public` or `protected`, never
with `internal`.** The compiler cannot find a mistake here until the move, so apply the
rule by hand.

The test for each member is one question: **does the Identity layer touch it?**

| Member group | Access | Reason |
|---|---|---|
| Each `Effective*` member | `public` | The view models and the controllers read them. |
| `ResolveScope`, `ResolveSortableColumn`, `CreateInstance()` | `public` | The controllers call them. |
| The constructor of `IndexPageOptions`, `CreatePageOptions`, `EditPageOptions` | `public` | `ResourceOptions` and `IdentityUsersOptions` **construct** them. |
| The constructor of `ResourceOptions` and `FormPageOptions` | `protected` | These two really are subclassed, so only subclasses need them. |
| `SidebarItemOptions` constructor | `public` (implicit) | `IdentitySidebarOptions` constructs it. |
| `IndexPageDefaults`, `FormPageDefaults` | `public` records | `IdentityUsersOptions` makes them. |
| The constructor of `ResourceBuilder` and `SidebarItemBuilder` | `public` | The Identity-side builders construct them. |
| The constructor of every other shared builder | `internal` | Only other shared builders construct them. |
| The constructor of `IndexViewModel<TEntity>` | `public` | `UsersController` makes it. |
| The collection methods `AddColumn`, `AddScope`, `AddField`, `Clear*` | `internal` | Only the shared builders call them, and the builders move too. |
| The setters on `DataGridColumnOptions`, `FormFieldOptions`, `IndexPageScopeOptions` | `internal` | Only the shared builders write them. |

The last two rows make the "read-only outside the builders" rule **stronger** after the
move. The Identity layer will not be able to write these values at all.

*(Corrected during execution. The first draft of this table said every page options
constructor was `protected` and every shared builder constructor was `public`. Both were
wrong: there are no `Users*` page options subclasses, so the page options are constructed
rather than derived; and only two builders are constructed from the Identity side.)*

These files are also destined for the separate assembly, but they need no `Base` folder,
because the whole folder is general:

- `Infrastructure/Expressions/`, `Infrastructure/Query/`
- `Areas/StellarAdmin/Controllers/ResourceControllerBase.cs`
- `Areas/StellarAdmin/ViewModels/` — all of them except `CreateUserPasswordInput.cs`
- `Areas/StellarAdmin/Views/Shared/`

## The XML comment rule

An XML comment is documentation for the consumer of the library. It says what the class,
property or method does — and nothing else.

**ASD-STE100 does not apply to code comments.** Its word list reserves "return" for the
physical sense, which pushes you into "Gives the scope..." — unfamiliar to a .NET
developer and reading like a translation. Write the vocabulary .NET developers already
know: **Returns**, **Gets**, **Sets**, **Adds**, **Removes**, **Throws**, **Applies**,
**Transforms**. Properties stay noun phrases, as the existing code already writes them.

The two things that matter:

- **Be brief.** One sentence for most members. Say what it does and stop.
- **Never expose internals.** No order of operations, no validation timing, no "last call
  wins", no "the view model reads this", no design history. That belongs in
  `docs/design/`, not on the API surface.

Also: do not document an `internal` type — leave it uncommented. Put a default value in
`<remarks>`, not in the summary.

Every phase that moves or renames a class must also correct its comments. Three examples
from the code today:

```csharp
// Too long. It explains when the failure happens and what the library does not do.
/// <summary>
///     Applies the search term the user entered to the users query. The query is not
///     validated at configuration time; one the query provider cannot translate fails
///     when the users index page renders with a search term applied.
/// </summary>

// Correct.
/// <summary>Applies the search term to the query.</summary>
```

```csharp
// Too long. The reader does not need the fall-back logic.
/// <summary>
///     The field the user list is sorted by, or <c>null</c> when it is unsorted —
///     including when the requested sort field did not match a sortable column.
/// </summary>

// Correct.
/// <summary>The field the list is sorted by, or <c>null</c> when the list is unsorted.</summary>
```

```csharp
// Unfamiliar verb. "Gives" is not what a .NET developer expects.
/// <summary>Gives the scope with the slug, or the default scope when the slug matches none.</summary>

// Correct.
/// <summary>Returns the scope for the slug, or the default scope when the slug matches none.</summary>
```

## Target configuration API

```csharp
// users only
builder.Services.AddStellarAdmin().AddIdentity<ApplicationUser>();

// users and roles
builder.Services.AddStellarAdmin()
    .AddIdentity<ApplicationUser, ApplicationRole>(identity =>
    {
        identity.ConfigureSidebar(sidebar =>
        {
            sidebar.Title = "User management";
            sidebar.RolesItem(item => item.Label = "Groups");
        });

        identity.ConfigureUsers(users =>            // ResourceBuilder<ApplicationUser>
        {
            users.Index(index => ...);              // IndexPageBuilder<ApplicationUser>
            users.Create(create => ...);            // CreatePageBuilder<ApplicationUser>
            users.Edit(edit => ...);                // EditPageBuilder<ApplicationUser>
        });

        identity.ConfigureRoles(roles =>            // ResourceBuilder<ApplicationRole>
        {
            roles.Index(index => index.Columns(c => c.Add(r => r.Name).Sortable()));
        });
    });

// users and roles, custom key
builder.Services.AddStellarAdmin().AddIdentity<ApplicationUser, ApplicationRole, Guid>();
```

`ConfigureRoles` agrees with the conventions document: the Roles screens exist whether or
not you call it. The overload, not a verb, states that the application has roles. A host
that calls `AddIdentity<TUser>()` gave no role type, so there is nothing to manage.

**The overload set changes.** `AddIdentity<TUser, TKey>()` must go away. It has the same
shape as `AddIdentity<TUser, TRole>()`, and C# does not permit two methods that differ
only by their constraints. A host with a custom key must then also give a role type. This
agrees with the stock `services.AddIdentity<TUser, TRole>()`.

## Two design points that keep the work simple

**There is no CRTP problem in the page builders.** CRTP is only necessary if you subclass
a fluent builder. No page builder has a user-only member today. `CreateInstanceUsing` is
also necessary for roles. Thus users and roles use the same builder classes directly. If a
user-only builder method becomes necessary later, add a C# extension member with a
constraint on the user type. It returns the same shared type, so the fluent chain still
works.

**The shared controller holds no actions.** It holds only protected helper methods. The
`Create` action is really different for the two resources: the users form has a password
and the roles form does not. Shared actions would need six abstract hooks that no code
needs today. A base class is still necessary (not a static class), because
`TryUpdateModelAsync` is protected on `ControllerBase`.

## Verification gates

Each phase must pass all three gates before you commit it.

**Gate 1 — the build is clean.**

```bash
cd /home/jerriep/projects/stellar-admin/workspace
dotnet build StellarAdmin.slnx -p:AllowMissingPrunePackageData=true
```

**Gate 2 — `sandbox/IdentitySimplePlayground/Program.cs` compiles with no change.**
That file names none of the classes. It has only lambdas. If it needs an edit, the public
API lost its shape. This is the most important gate. Phase 11 is the one exception: it
changes `AddIdentity<ApplicationUser>` to `AddIdentity<ApplicationUser, ApplicationRole>`.

**Gate 3 — the screen test.**

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run \
  --project stellar-admin-pro/sandbox/IdentitySimplePlayground --urls http://localhost:5206
```

Then, at `http://localhost:5206/admin/Users`:

1. The sidebar shows "User management" and "Team members".
2. The grid shows the six configured columns.
3. The e-mail column sorts, and the arrow changes direction.
4. The tabs "All" and "Unconfirmed" filter the list.
5. The search box filters after 400 ms and changes the URL.
6. The pager moves to page 2.
7. "New user" opens the create form with the password inputs.
8. The edit form shows `LockoutEnd` as read-only text.
9. A duplicate e-mail on create shows the error **at the e-mail input**.

Use port 5206. Port 5205 is yours. Stop the server at the end of each phase. The
environment variable is necessary. Without it the `_content` assets give a 500 and the
pages have no style.

A phase with only new names needs gates 1 and 2 and one page load.

---

## Phase 1 — Close the current work and break a bad dependency

`Options/UsersFormPageOptions.cs` and `Options/UsersIndexPageOptions.cs` use
`StellarAdmin.Identity.Builders`, because `FieldExpressionHelper` is there. The options
thus depend on the builders. That is the wrong direction.

- `Options/Base/IndexPageSearchOptions.cs`: keep `EffectivePlaceholder` public, per the
  boundary rule. Do **not** put the old long comment back on `Query`. Write the short
  form: "Applies the search term to the query."
- Move `Builders/FieldExpressionHelper.cs` to
  `Infrastructure/Expressions/FieldExpressionHelper.cs`. It stays `internal`: only shared
  code calls it. Give the class no XML comment, per the comment rule. Correct the code
  comments that speak about a user.
- Correct the `using` lines in the five files that use the helper.

**Verify:** gates 1 and 2. Then
`grep -rn "using StellarAdmin.Identity.Builders" src/StellarAdmin.Identity/Options/`
must find nothing. Commit.

---

## Phase 2 — The index page options go general

| From | To |
|---|---|
| `Options/UsersIndexPageOptions.cs` | `Options/Base/IndexPageOptions.cs` — `IndexPageOptions<TEntity>` |
| `Options/IdentityUserScope.cs` | `Options/Base/IndexPageScopeOptions.cs` — `IndexPageScopeOptions<TEntity>` |
| new | `Options/Base/IndexPageDefaults.cs` — a public record |

`IndexPageDefaults` holds the seed data: `Title`, `CreateLabel`, `EmptyTitle`,
`EmptyDescription`, `EmptyIcon`, `Columns` and `SortBy`. The seeding constructor changes
to `protected IndexPageOptions(IndexPageDefaults defaults)`.

Add `EffectiveCreateLabel`, `EffectiveEmptyTitle`, `EffectiveEmptyDescription` and
`EffectiveEmptyIcon`, all public. Each one falls back to the defaults. Also make the
existing `EffectiveTitle` and `EffectiveDefaultScope` public. Do **not** add builder
properties for the new strings. The `Sortable()` rule in the conventions says: do not
ship a knob before the feature exists.

Move `ResolveScope` and `ResolveSortColumn` from `UsersController` to public methods on
`IndexPageOptions<TEntity>`. They are pure lookups over its own collections. Keep the two
fall-back behaviours and their comments: an unknown scope falls back to the default
scope, and an unknown sort field is ignored.

`IdentityUsersOptions<TUser>` gives the user defaults:

```csharp
new IndexPageDefaults(
    Title: "Users",
    CreateLabel: "New user",
    EmptyTitle: "No users yet",
    EmptyDescription: "User accounts will appear here once they have been created.",
    EmptyIcon: "users",
    Columns: ["UserName", "Email", "EmailConfirmed"],
    SortBy: "UserName")
```

**Verify:** all three gates. The sort, the scopes and the search all go through the moved
methods.

---

## Phase 3 — The form page options go general

| From | To |
|---|---|
| `Options/UsersFormPageOptions.cs` | `Options/Base/FormPageOptions.cs` — abstract, `protected` constructor |
| `Options/UsersCreatePageOptions.cs` | `Options/Base/CreatePageOptions.cs` |
| `Options/UsersEditPageOptions.cs` | `Options/Base/EditPageOptions.cs` |
| new | `Options/Base/FormPageDefaults.cs` — public record: `Title`, `SubmitLabel`, `Fields` |

`CreatePageOptions<TEntity>` keeps `InstanceFactory` and `CreateInstance()`. Roles needs
both. Make `CreateInstance()` public. `EditPageOptions<TEntity>` stays as an empty
subclass. The difference between the two pages is real, and it gives a home to a later
page-only option.

Add a public `EffectiveSubmitLabel`, and make `EffectiveTitle` public. The defaults are
"Create user" and "Save changes".

**Verify:** all three gates, with the create form and the edit form as the focus. Test
`CreateInstanceUsing` and the read-only `LockoutEnd` field.

---

## Phase 4 — `ResourceOptions<TEntity>`

- New `Options/Base/ResourceOptions.cs`. It holds `IndexPage`, `CreatePage` and
  `EditPage`. Its constructor is `protected` and takes one `IndexPageDefaults` and two
  `FormPageDefaults`.
- `Options/IdentityUsersOptions.cs` changes to
  `IdentityUsersOptions<TUser> : ResourceOptions<TUser>`. It is about 30 lines. It then
  holds **every** user-specific string in the assembly.
- Change `IdentitySidebarItemOptions` to `Options/Base/SidebarItemOptions.cs`.
- Move `DataGridColumnOptions`, `DataGridDefaultSortOptions` and `FormFieldOptions` into
  `Options/Base/` too. They are entity-agnostic and move to the separate assembly with the
  rest, so leaving them outside `Base/` would make the marker unreliable. *(Added during
  execution; the first draft of this plan missed them.)*
- Read each option class once against the boundary rule table. Correct any member that
  the Identity layer touches and that is still `internal`.

**Verify:** gates 1 and 2.

---

## Phase 5 — The builders go general

Make `Builders/Base/`. Add a `builders_005Cbase` entry with the value `True` to
`StellarAdmin.Identity.csproj.DotSettings`.

| From | To |
|---|---|
| `IdentityUsersBuilder<TUser>` | `ResourceBuilder<TEntity>` |
| `UsersIndexPageBuilder<TUser>` | `IndexPageBuilder<TEntity>` |
| `UsersIndexPageColumnsBuilder<TUser>` | `IndexPageColumnsBuilder<TEntity>` |
| `UsersIndexPageColumnBuilder` | `IndexPageColumnBuilder` |
| `UsersIndexPageScopesBuilder<TUser>` | `IndexPageScopesBuilder<TEntity>` |
| `UsersIndexPageScopeBuilder<TUser>` | `IndexPageScopeBuilder<TEntity>` |
| `UsersIndexPageSearchBuilder<TUser>` | `IndexPageSearchBuilder<TEntity>` |
| `UsersCreatePageBuilder<TUser>` | `CreatePageBuilder<TEntity>` |
| `UsersEditPageBuilder<TUser>` | `EditPageBuilder<TEntity>` |
| `UsersFormFieldsBuilder<TUser>` | `FormFieldsBuilder<TEntity>` |
| `UsersFormFieldBuilder` | `FormFieldBuilder` |
| `IdentitySidebarItemBuilder` | `SidebarItemBuilder` |

Only `ResourceBuilder` and `SidebarItemBuilder` need a `public` constructor — the
Identity-side builders construct those two across the future assembly line. The other ten
are constructed only by other shared builders, so they stay `internal` and stay
unreachable from consumer code.

`StellarAdminIdentityBuilder<TUser>` and `IdentitySidebarBuilder` keep their names and
stay in `Builders/`. `ConfigureUsers` now takes `Action<ResourceBuilder<TUser>>`.

Correct the XML comments against the comment rule. Change "the user property" to "the
entity property". Cut each comment that explains an internal detail, such as "last call
wins", "composes on repeat calls" or "the query is not validated". Change
`typeof(TUser).Name` in the `Add` error message to `typeof(TEntity).Name`. The
`EnableSearch` comment must not point to `UsersIndexPageBuilder`; that link breaks.

**Verify:** gates 1 and 2. Gate 2 is the purpose of this phase. It proves that the shared
builders infer the types in the same way.

---

## Phase 6 — A DI shape that a second resource can use

`StellarAdminIdentityOptions<TUser>` is generic on `TUser` only. It cannot hold role
options. Split it.

*Why the root goes non-generic rather than generic on both types.* A
`StellarAdminIdentityOptions<TUser, TRole>` leaves `AddIdentity<TUser>()` with no role type
to supply, so a users-only host must invent one. It also makes each reader of the root name
both type arguments to reach configuration that concerns neither — `SidebarItemsProvider`
wants only the sidebar. Take the entity-typed members off the root and only the sidebar
remains, which is the one part of the configuration that spans the resources instead of
belonging to one. Each resource then keeps its own singleton, each controller injects only
the options it serves, and `IdentityResourceRegistration` gives the sidebar a uniform view
of the resources with no entity type in sight.

- `StellarAdminIdentityOptions` becomes **not generic**. It holds only `Sidebar`.
- `IdentityUsersOptions<TUser>` becomes its own singleton. `UsersController` injects it
  directly. The paths get shorter: `options.IndexPage`, not `options.Users.IndexPage`.
- New `Infrastructure/IdentityResourceRegistration.cs`. It is **not generic**:
  `ControllerName`, `SidebarItem`, `Func<string> IndexTitle` and `Order`. `AddIdentity`
  registers one for users, and later one for roles. The `Func<string>` keeps the label
  cascade to the index page title.
- `SidebarItemsProvider` becomes **not generic**. It takes `StellarAdminIdentityOptions`
  and `IEnumerable<IdentityResourceRegistration>`. It makes a list, it drops each hidden
  item, and it gives no group when the list is empty. The comment in the file asks for
  this. This also removes a fault: `TryAddEnumerable` today adds a second provider if
  `AddIdentity` runs with a different `TUser`.
- `IdentityControllerFeatureProvider` takes a **list** of closed controller types in place
  of one type. It also checks for a duplicate before it adds one; today it does not.
  The list is complete at registration time, because the overload gives both type
  arguments. No late change to the list is necessary.
- `StellarAdminIdentityBuilder<TUser>` now holds `StellarAdminIdentityOptions` and
  `IdentityUsersOptions<TUser>`, in place of the one root options object.
- Keep the "find the instance in the service collection, or make one" logic for each
  singleton. Two calls to `AddIdentity` must still configure the same objects.

*(Dropped during execution: the first draft also said the builder must hold the
`StellarAdminBuilder`, "because Phase 11 needs it". Phase 11 does not. Roles registration
happens inside `AddIdentity<TUser, TRole>()`, which already has the builder in scope;
`ConfigureRoles` only writes options. An unread field is dead weight, so it was left out.
Add it in Phase 11 if a real use appears.)*

*(Added during execution: a repeat `AddIdentity` call would register a second
`IdentityResourceRegistration` for the same controller and so show the sidebar item twice.
`AddResource` skips a registration whose controller name is already registered. The
`GetOrAddOptions<TOptions>` helper carries the "find or make" logic for both singletons.)*

**Verify:** all three gates, with the sidebar as the focus. Set
`sidebar.UsersItem(item => item.Visible = false)` for a short test. The item must go away,
and `/admin/Users` must still work. Also add a bare second
`AddStellarAdmin().AddIdentity<ApplicationUser>()` call: the sidebar must still show one
item, and the title configured by the first call must survive.

---

## Phase 7 — The query pipeline and the shared controller base

New files:

- `Infrastructure/Query/QueryableSortExtensions.cs` — the `ApplySort` code. It is pure
  LINQ. It has no MVC and no entity coupling.
- `Infrastructure/Query/IndexPageRequest.cs` — it replaces the six loose action
  parameters. It limits `pageNo` to 1 or more, and it reads `sortDir`.
- `Infrastructure/Query/IndexPageResult.cs` — items, total, active scope, active sort
  field and sort direction.
- `Infrastructure/Query/IndexPageQuery.cs` — `Execute(options, query, request)`. It keeps
  the documented order: transform, scope, search, sort, count, page.
- `Areas/StellarAdmin/Controllers/ResourceControllerBase.cs` — it holds
  `BindFormFieldsAsync` and `BuildFormViewModel` as protected methods. It has **no**
  actions and no `[Area]` attribute.

`AddIdentityErrors` stays on `UsersController`. Its error codes are user codes. Roles gets
its own short version.

Keep `TKey` on the controller. The constraint `where TUser : IdentityUser<TKey>` gives a
compile error in place of a run-time DI failure.

**Access levels.** `IndexPageRequest` **must** be `public`: it is a parameter of a public
action method, and an `internal` type there is a compile error (CS0051). `IndexPageResult`
and `IndexPageQuery` are `public` too, because the Identity controllers touch them.
`QueryableSortExtensions` stays `internal` — only `IndexPageQuery` calls it, and the two
move to the separate assembly together.

*(Decided during execution: `ResourceControllerBase.cs` sits directly in `Controllers/`,
with no `Base/` folder. Its `Base` suffix already marks it as one of the classes that move
to the separate assembly, so the folder would add nothing.)*

*(Added during execution: `IndexPageResult` also carries the effective search term. The
index page nulls the term when search is not configured, and the view model needs the
nulled value, so the result has to carry it. `IndexPageRequest` clamps `PageSize` to 1 or
more with the same guard as `PageNo`; without it `?pageSize=0` makes
`PagedListViewModel.TotalPages` divide by zero and the pager renders nonsense. There is
still no upper bound on `PageSize` — decide one when the screens go in front of real data.
`ApplySort` became an `OrderByField` extension on `IQueryable<TEntity>`, named so it cannot
collide with `Queryable.OrderBy` during overload resolution.)*

`IndexPageRequest.SortDirection` keeps the inline `"desc"` string comparison. Phase 10
replaces it with the shared direction extension.

**Verify:** all three gates. Also test `?scope=unknown` (it must fall back to the default
scope) and `?sortBy=NotAColumn` (the page must render with no sort). The URL parameters
must not change — check the query strings the page *emits* as well as the ones it accepts.

---

## Phase 8 — The view models

| From | To |
|---|---|
| `Internal/IUsersIndexViewModel` | `Internal/IIndexViewModel` |
| `UsersIndexViewModel<TUser, TKey>` | `IndexViewModel<TEntity>` |
| `UserScopeViewModel` | `IndexScopeViewModel` |
| `UserFormViewModel` | `FormViewModel` |
| `PagedListViewModel<TModel>` | no change |

`IIndexViewModel` gets `CreateLabel`, `EmptyTitle`, `EmptyDescription`, `EmptyIcon` and
`SortDirectionQueryValue`. The last one removes the `sortDir` block at the top of
`Index.cshtml`.

`IndexViewModel<TEntity>` drops `TKey`, and its constructor becomes public. The controller
gives it a `Func<TEntity, string>` for the row id. `UsersController` gives
`u => u.Id.ToString()!`. This removes the last `Microsoft.AspNetCore.Identity` reference
outside the controllers.

*(Decided during execution: `UserList` becomes the private field `_page`. It was public so
that a consumer overriding `Views/Users/Index.cshtml` could reach the typed list — a real
need, but one weakly-motivated public member on an entity-agnostic view model is worse than
none. The hatch is on the deferred list under "Points not in this plan"; do not treat it as
settled. Note the ergonomics got slightly worse either way: an overriding view would now
have to write `@model IndexViewModel<ApplicationUser>` to reach it, so a typed accessor may
be the better answer.)*

**The binding prefix is the risk of this phase.** `<partial for="@Model.User">` makes the
posted field prefix `User.`. The controller repeats that text in two more places. Nothing
connects them. Let the compiler hold the link together:

```csharp
public sealed class FormViewModel
{
    /// <summary>The posted input names are "{BindingPrefix}.{FieldName}".</summary>
    public const string BindingPrefix = nameof(Entity);

    public object Entity { get; }
}
```

Then `BindFormFieldsAsync` uses `prefix: FormViewModel.BindingPrefix`, and
`AddIdentityErrors` builds `$"{FormViewModel.BindingPrefix}.{nameof(IdentityUser.Email)}"`.
After this, a new property name gives a compile error at each dependent place.

**Verify:** all three gates. Read the page source of the edit page. The inputs must be
`name="Entity.UserName"` and `id="Entity_UserName"`, and the label and the validation span
must agree. Then post a bad phone number, and create a user with an e-mail that already
exists. Both errors must show at the correct input, not in the summary.

---

## Phase 9 — The views move to `Views/Shared/`

`Areas/StellarAdmin/Views/Shared/` is a folder that two assemblies share. The OSS Shell
already has `_Layout.cshtml` there. A view with an action name in that folder becomes the
fallback view for **each** controller in the area. Today the Shell `Home` controller has
its own `Index.cshtml`, so nothing breaks, but that is luck.

**Rule: only a file with an underscore prefix goes in `Views/Shared/`.** No action has a
name that starts with an underscore, so the fault cannot happen.

```
Views/Shared/_IndexPage.cshtml       @model IIndexViewModel
Views/Shared/_IndexDataGrid.cshtml   @model IIndexViewModel
Views/Shared/_FormPage.cshtml        @model FormViewModel
Views/Shared/_FormFields.cshtml      FormFieldsBaseView<object>   (moved, no change)

Views/Users/Index.cshtml    two lines: @model + <partial name="_IndexPage" />
Views/Users/Edit.cshtml     two lines: @model + <partial name="_FormPage" />
Views/Users/Create.cshtml   stays special: the fields partial, then the password block
```

*(Superseded 2026-08-13: the pages now render `_FormPage`/`_IndexPage` through the
`<sa-form-page model="Model">` and `<sa-index-page model="Model">` templated tag
helpers with slot fills — the edit pages' delete trigger and the create page's
password fieldset go through slots, so `Create.cshtml` is no longer special. See
`templated-view-slots.md`.)*

This keeps the override method of the design documents, and it makes it better. A user
application can now override at three levels: `Views/Users/_IndexDataGrid.cshtml` for
users only, `Views/Shared/_IndexDataGrid.cshtml` for each resource, or
`Views/Users/Index.cshtml` for the full page.

Replace the user text with view model members: `@Model.CreateLabel`, `@Model.EmptyTitle`,
`@Model.EmptyDescription`, `@Model.EmptyIcon` and `@Model.SubmitLabel`. The tag helpers
`asp-action="Edit"`, `asp-action="Create"` and `Url.Action` use the current controller, so
they work for roles with no change.

**The htmx `<script>` must move.** It is in `@section Styles` today. A partial cannot
declare a section, so the move to a partial forces this. The OSS layout renders only a
"Styles" section, and `StellarAdminShellBuilder` has only `AddStylesheet`, and the OSS
repository is out of scope. Put the script in the body of `_IndexPage.cshtml`, after
`</sa-page-container>`, with a comment. htmx starts at `DOMContentLoaded`, so a body
script is correct. Make a separate OSS task for `StellarAdminShellBuilder.AddScript`.

*(Settled during execution: the submit label renders the option's **sentence case**
"Save changes", replacing the hardcoded title-case "Save Changes". Sentence case is what
every current design system uses except Apple's HIG, and it matches the library's other
defaults. See the deferred item on column-title casing — grid headers and form labels do
**not** follow this, because they come from the PascalCase humanizer.)*

*(Verification note: the six strings this phase wires up have the same values as the
hardcoded text they replace, so rendering them proves nothing. Prove the wiring by
temporarily changing the defaults in `IdentityUsersOptions` to distinct values, checking
the page, and reverting. There is deliberately no builder property for them, so they cannot
be set from `Program.cs`.)*

**Verify:** all three gates, with a hard refresh and the browser console open. There must
be no 404 for the htmx file and no console error. Load the Shell's own page in the area
(`/admin`) and confirm it did **not** pick up a shared view. Confirm that the CSS rebuild
found the moved files. The Tailwind entry has `@source "../../Areas/StellarAdmin/Views"`, so the
glob covers `Shared/`, but check that `wwwroot/stellar-admin-identity.css` is newer than
the views. Use `?search=zzzz` to see the empty state.

---

## Phase 10 — Small cleanups and the documents

- Give the file `Areas/StellarAdmin/FormFieldPartialView.cs` the name of its class:
  `FormFieldsBaseView.cs`.
- New `Areas/StellarAdmin/ViewDataKeys.cs` with
  `public const string FormFields = "StellarAdminFormFields"`. The text is in three files
  today. The constant must be `public`, so that an overridden view can use it.
- Add `@using StellarAdmin.Identity.Areas.StellarAdmin` to `_ViewImports.cshtml`, so the
  `@inherits` line gets shorter.
- The text `"asc"` and `"desc"` is in the controller and in the index view.
  `StellarAdmin.Pro` has `GetQueryValueText()`, but the class is `internal` and
  there is no `InternalsVisibleTo`. Your convention says an enum extension class must stay
  internal. Thus **do not** make it public. Add a small internal extension in the shared
  `Infrastructure/Query/` folder with both directions, and use it in `IndexPageRequest` and
  in `IIndexViewModel.SortDirectionQueryValue`.
- `docs/design/identity-configuration.md`: correct the builder tree, the options model,
  the search section and the "one options object" line. The last one is now "one object
  for each resource, and one shared root". Rewrite the Roles part: it is no longer a
  design, and the verb is `ConfigureRoles`.
- `docs/design/identity-user-forms.md`: correct `IdentityFormField` to `FormFieldOptions`,
  `ReadOnly` to `IsReadOnly`, `UserFormViewModel` to `FormViewModel`, and the new prefix.
- Add a short "Resource layer" part to the configuration document. Write down the
  `TEntity` and `Resource` words, the "shared builders, no CRTP" decision and the assembly
  boundary rule, so that nobody argues them again.
- `docs/conventions/options-builders.md` in the workspace repository: the open question
  "`ConfigureRoles` or `AddRoles`" now has an answer. Also add the new rule that this work
  found: **an overload of the root verb, not a second verb, states which entity types the
  application has.** `Configure*` then keeps its promise.
- New `docs/conventions/xml-documentation.md` in the workspace repository. Write the
  comment rule from the top of this plan. That folder already holds the cross-repository
  conventions, and `CLAUDE.md` points to it. Add a line for it in the `CLAUDE.md` list.
- Read the XML comments of each file this work touched one more time against the rule.
- Order the members of the files you edited, per `csharp-file-organization.md`. Do not
  order files you did not touch.

*(Notes from execution, 2026-08-10: the comment sweep found and fixed three violations —
`CreateUserPasswordInput` still documented the old `User.` prefix, the two internal MVC
classes carried XML docs (now `//` comments per the rule), and the `AddIdentity` overloads
had empty `<returns>`/`<exception>` tags. One ordering fix: `BuildFormViewModel` (static)
now precedes `BindFormFieldsAsync` in `ResourceControllerBase`. The direction extension got
both directions as planned — `GetQueryValueText()` and a static `ParseQueryValue()` — and
the desc-sort screen check exercises both.)*

**Verify:** gates 1 and 2, and one load of each screen.

---

## Phase 11 — The Roles pages

If phases 1 to 10 are correct, this phase is small. That makes it the honest test of the
work.

**Registration**

- New `Builders/StellarAdminIdentityBuilder2.cs` — the class
  `StellarAdminIdentityBuilder<TUser, TRole>`. It does **not** derive from
  `StellarAdminIdentityBuilder<TUser>`. It repeats `ConfigureSidebar` and `ConfigureUsers`,
  about six lines each, and it adds `ConfigureRoles(Action<ResourceBuilder<TRole>>)`. Each
  method returns its own type, so a chain works.
- `StellarAdminBuilderExtensions`: delete `AddIdentity<TUser, TKey>()` and its configure
  overload. Add `AddIdentity<TUser, TRole>()`, `AddIdentity<TUser, TRole, TKey>()` and a
  configure overload for each. Keep `AddIdentity<TUser>()` and its configure overload for
  a host with no roles.
- The roles overloads also: make or find the `IdentityRolesOptions<TRole>` singleton, add
  `RolesController<TRole, TKey>` to the controller list, and register the roles
  `IdentityResourceRegistration`.
- `AddIdentity<TUser, TRole, ...>` needs `RoleManager<TRole>` in DI. That is present only
  if the host called `AddIdentity<TUser, TRole>()` and not `AddDefaultIdentity<TUser>()`.
  Throw a clear message at registration if the service descriptor is absent.

**The pages**

- `Options/IdentityRolesOptions<TRole> : ResourceOptions<TRole>` — about 15 lines of role
  strings. The seed column and the seed field are `Name`. The titles are "Roles", "Create
  role" and "Edit role". The empty icon is `shield`.
- `Areas/StellarAdmin/Controllers/RolesController<TRole, TKey> : ResourceControllerBase<TRole>`
  — about 80 lines, with `RoleManager<TRole>` and `where TRole : IdentityRole<TKey>`. The
  create action calls `roleManager.CreateAsync(role)`. There is no password. Its error map
  sends `DuplicateRoleName` and `InvalidRoleName` to the `Name` field.
- `Views/Roles/Index.cshtml`, `Create.cshtml` and `Edit.cshtml` — two or three lines each.
  The roles create view is the same shape as the edit view, because there is no password.
- `IdentitySidebarOptions.RolesItem` and `IdentitySidebarBuilder.RolesItem`.
- One more entry in `IdentityControllerNameConvention`:
  `typeof(RolesController<,>) = "Roles"`. The convention only fires for a controller that
  MVC found, so the entry is safe for a users-only host.
- No route work. `MapStellarAdmin` in the OSS Shell already routes
  `{prefix}/{controller}/{action}/{id?}` in the area.
- No CSS work. The Tailwind `@source` glob covers the new view folder.
- **No change to any shared class.** If a change is necessary, the abstraction is wrong.

**Verify:** change the playground to `AddIdentity<ApplicationUser, ApplicationRole>` and
make some roles. Open `/admin/Roles`. Test the sort, the pager, the create form and the
edit form. A duplicate role name must show an error at the input. Then change the
playground back to `AddIdentity<ApplicationUser>`: `/admin/Roles` must give a 404, and the
sidebar must show only the users item.

*(Notes from execution, 2026-08-10: **the gate held — no shared class changed.** The diff
is two new classes (`IdentityRolesOptions<TRole>`, `StellarAdminIdentityBuilder<TUser,
TRole>`), the `RolesController`, three Roles views, and the four Identity-side files the
plan names: the sidebar options and builder gain `RolesItem`, the name convention gains the
`RolesController<,>` entry, and the registration extensions. The extensions were refactored
into private helpers — `AddIdentityCore<TUser, TKey>` holds the registration every overload
shares, `AddRolesCore<TRole, TKey>` adds the roles options, controller and resource on top,
and `AddControllerType` holds the find-or-create feature provider logic both call.
`ConfigureApplicationPartManager` runs its delegate immediately, so the second call from
`AddRolesCore` lands in the same feature provider. The missing-`RoleManager<TRole>` guard
throws at registration with a message naming both stock fixes. Verified against 14 seeded
roles: default-seeded `Name` column sorts both directions, pager pages correctly at
`?pageSize=5`, create redirects and the role appears, a duplicate name renders "Role name
'Testers' is already taken." at the `Entity.Name` input, edit renames end-to-end, the Users
screens are untouched, and both sidebar items show under the configured group title. The
users-only sweep then passed: `/admin/Roles` 404s and the sidebar shows one item.)*

*(Revised after review, 2026-08-10: the users-only `AddIdentity<TUser>()` overload and
`StellarAdminIdentityBuilder<TUser>` were **removed** — every host supplies both entity
types, mirroring stock `services.AddIdentity<TUser, TRole>()` and the all-or-nothing
entity set of OpenIddict's `ReplaceDefaultEntities<...>()`. Four `AddIdentity` overloads
remain, all role-typed, and one root builder class. A host on
`AddDefaultIdentity<TUser>()` must add `.AddRoles<IdentityRole>()`; the registration
guard's message says so. The playground now uses
`AddIdentity<ApplicationUser, ApplicationRole>` permanently, so the users-only sweep
above documents behavior that no longer has a public entry point. Both design docs and
`options-builders.md` record the revision.)*

---

## Points not in this plan

- **Revisit the `TUser : class` constraint on the Identity-side classes** (Jerrie,
  2026-08-11). `IdentityUsersOptions<TUser>` and its kin constrain `TUser` only to
  `class`, so they cannot reach Identity members - the delete display-name selector had
  to be threaded in from the registration (which holds the `IdentityUser<TKey>`
  constraint) through a `GetOrAddOptions` factory overload. Workarounds like that keep
  accumulating; consider constraining the Identity-side (non-`Base`) classes to
  `IdentityUser<TKey>`/`IdentityRole<TKey>` after the delete work is done. The shared
  `Base` classes stay `TEntity : class`.
  *(Done 2026-08-11 after researching how ASP.NET Core Identity, OpenIddict and the
  Identity admin-UI libraries place their constraints — research doc:
  `identity-entity-constraint-research.md` in this folder. The three Identity-side
  classes became `IdentityUsersOptions<TUser, TKey>`, `IdentityRolesOptions<TRole,
  TKey>` and `StellarAdminIdentityBuilder<TUser, TRole, TKey>`, constrained to
  `IdentityUser<TKey>`/`IdentityRole<TKey>`; the options write their own display-name
  defaults (`user => user.UserName`, `role => role.Name`), the ctor-threading and the
  `GetOrAddOptions` factory overload are gone, and the `role.ToString()` hack is gone.
  The `Base/` layer stays `TEntity : class`. Consumer code and the playground
  `Program.cs` compile unchanged - the extra parameter is inferred. The constraint
  decision is recorded in `identity-configuration.md` under "Resource layer".)*

- The move of the `Base` classes to a separate assembly. The plan prepares for it, and the
  boundary rule makes the move mechanical. Do it when a second product needs the classes,
  or when consumers must write their own resources.
- Builder properties for the button text and the empty-state text. The defaults are read
  only for now. Add the knobs when a user asks.
- A delete action and a details page.
  *(Delete done 2026-08-11, confirmation reworked 2026-08-12: the grid's delete
  button posts with `hx-post` + the antiforgery token in `hx-vals`, gated by
  `hx-confirm="js:confirmResourceDelete(this)"` — dialog and script live in
  `Views/Shared/_IndexDeleteDialog.cshtml`, rendered by `_IndexPage.cshtml` outside
  `#result` so grid swaps cannot destroy it. The Edit pages delete through the
  server-rendered `Views/Shared/_DeleteConfirmDialog.cshtml` — since 2026-08-13 the
  trigger fills `<sa-form-page>`'s `post-form-actions` slot and the partial is just
  the dialog (see `templated-view-slots.md`). The first cut — a
  delegated-listener TypeScript module bundled with rolldown — was built and then
  **removed** in the rework; the `Client/` build is again Tailwind + the htmx copy
  only, so don't revive the rolldown pipeline for a page script. Documented in the
  pro repo's `docs/design/identity-configuration.md` under "Delete". The details
  page remains open.)*
- An icon and an order for a sidebar item. The OSS sidebar model has no support yet.
- Styled editor templates. The design document keeps this open.
- **Grid column and form label casing.** Settled 2026-08-10: user-facing copy is **sentence
  case** ("Save changes", "New user"), matching every current design system except Apple's
  HIG, and matching the library's own defaults. But column headers and form labels do not
  obey it — they come from the DataAnnotations display-name humanizer splitting PascalCase,
  so a grid reads "User Name", "Access Failed Count", "Email Confirmed" no matter what the
  button copy does. Only `[Display(Name = "...")]` overrides it. Making the whole page
  uniformly sentence case means deciding how a column title derives from a property name,
  which affects form labels the same way. Revisit as its own item; not in this plan.
- `StellarAdminShellBuilder.AddScript` in the OSS repository. Make a task for it.
  *(Done 2026-08-10: the shell options gained `Scripts` and the builder `AddScript`,
  the layout links each registered script in the `<head>` with `defer` — head-with-defer
  being current best practice, strictly better than the end-of-body legacy pattern —
  and the Identity library registers htmx next to its stylesheet, so the script block
  left `_IndexPage.cshtml`. htmx is an exact-pinned npm dependency in the Identity
  `Client/package.json`; `build:js` (`Client/scripts/copy-js.mjs`) copies it from
  `node_modules` into `wwwroot/htmx.min.js` (byte-identical to the previously linked
  CDN file — integrity hash verified), with a matching `ClientOutput` entry in the
  csproj's execution-time `ClientItems` target. `wwwroot/` stays fully generated and
  gitignored. Gotcha: the `Client` target only runs `npm install` when `node_modules`
  is absent, so a checkout that already has it needs one manual `npm install` in
  `Client/` after pulling this change.)*
- **A review of the consumer escape hatches, after the Roles pages are in.** The shipped
  views read the non-generic `IIndexViewModel`, but a consumer who overrides
  `Views/Users/Index.cshtml` wants the strongly-typed list. That is why
  `UsersIndexViewModel.UserList` was public. Phase 8 makes it a private field, because
  `IndexViewModel<TEntity>` is entity-agnostic and one weakly-motivated public member is
  worse than none — but the need it served is real and does not go away. Decide the hatches
  as a set once both resources exist: what an overriding view can reach, and how.
  *(Closed 2026-08-10: reviewed and decided in `identity-view-override-hatches.md` (this
  folder), implemented as pro commit `32128ed` — an overriding view declares
  `@model IndexViewModel<TEntity>`; `Page` exposes the paged list as-is, adapted values
  are public members, and `IIndexViewModel` stays internal plumbing. The override story
  is recorded in the pro repo's `docs/design/identity-configuration.md` under "View
  overrides".)*

## Risks

| Risk | Control |
|---|---|
| `AddIdentity<TUser, TKey>()` goes away. A host with a custom key and no roles cannot register. | Nothing is released. The host gives a role type. This is what the stock `AddIdentity<TUser, TRole>` also asks for. |
| The new binding prefix `Entity.` breaks a page override. | The `BindingPrefix` constant makes each dependent place a compile error. |
| A shared view catches a different controller in the area. | Only files with an underscore prefix go in `Views/Shared/`. |
| An `internal` member of a shared class blocks the later assembly move. | Read the boundary rule table at the end of each phase. Phase 4 has an explicit check. |
| A large set of new names is hard to review. | One phase, one commit. Phases 2 to 5 change names and move seed data only. No logic changes. |
| The abstraction is wrong. | Phase 11 must touch no shared class. If it does, stop and correct the layer. |
