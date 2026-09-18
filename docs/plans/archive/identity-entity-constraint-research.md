# Research: the `TUser : class` / `TRole : class` constraint rethink

## Code audit — 2026-09-18

Current status: **completed**. The recommended constraint change is implemented: `IdentityUsersOptions<TUser,TKey>` and `IdentityRolesOptions<TRole,TKey>` constrain to Identity user/role types and read `UserName`/`Name` directly. `StellarAdminIdentityBuilder` carries the key type; shared `ResourceOptions<TEntity>` remains entity-agnostic. The external-library comparison is historical research, not freshly revalidated.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

**Index status:** reference. **Indexed:** 2026-09-05. Research/specification record; read the current design before using historical examples.

Researched 2026-08-11. Question: the Identity-side classes in `StellarAdmin.Identity`
constrain their entity types only to `class`, which blocks them from reaching Identity
members and forces workarounds (the ctor-threaded delete display-name selector, the
`role.ToString()` hack). How do OpenIddict and other Identity-related libraries constrain
their entity generics — do they go beyond `class`? What should we do?

All external facts below were verified against source on GitHub (dotnet/aspnetcore main,
openiddict-core dev + 6.4.0, and each library's main branch), not recalled.

## Where our code stands today

| Layer | Constraint | Notes |
|---|---|---|
| `Options/Base/*`, `Builders/Base/*`, `Infrastructure/Query/*`, view models, `ResourceControllerBase<TEntity>` | `TEntity : class` | The entity-agnostic Resource layer. Deliberate; destined for its own assembly. |
| `IdentityUsersOptions<TUser>`, `IdentityRolesOptions<TRole>` | `class` | **The friction point.** Cannot read `UserName`, `Email`, `Name`. |
| `StellarAdminIdentityBuilder<TUser, TRole>` | `class`, `class` | Holds the two options singletons. |
| `AddIdentity<TUser, TRole>()` / `<TUser, TRole, TKey>()` | `IdentityUser<TKey>`, `IdentityRole<TKey>`, `TKey : IEquatable<TKey>` | The only entry points into the library. |
| `UsersController<TUser, TKey>`, `RolesController<TRole, TKey>` | `IdentityUser<TKey>` / `IdentityRole<TKey>` | Full member access. |

The workarounds the `class` constraint has forced so far:

- `IdentityUsersOptions<TUser>` cannot write its own delete display-name default, so the
  registration threads `user => user.UserName ?? user.ToString()` in through a ctor
  parameter, which in turn needed a `Func<TOptions>` factory overload on
  `GetOrAddOptions`.
- `IdentityRolesOptions<TRole>` uses `role => role.ToString()` with a comment explaining
  that `IdentityRole.ToString()` happens to return the name — instead of `role.Name`.
- Every future library-authored default or feature that touches a member (a details
  page, an email-confirmation column, a lockout toggle) will need another threaded
  delegate. The cost is linear in features.

Note what the constraint is *not* needed for: consumer lambdas. `users.Index(index =>
index.Columns(c => c.Add(u => u.Email)))` already reaches every member, including custom
ones, because the consumer closes the generic over their concrete `ApplicationUser`. The
constraint question only affects code *the library itself* writes against `TUser`.

## How ASP.NET Core Identity does it

Three packages, and the constraint tightens exactly once, at the storage layer:

| Layer | Package | Constraint |
|---|---|---|
| `UserManager<TUser>`, `SignInManager<TUser>`, `RoleManager<TRole>`, all store *interfaces* (`IUserStore`, `IUserEmailStore`, ...), `AddIdentity<TUser, TRole>`, `AddIdentityCore<TUser>`, `AddDefaultIdentity<TUser>` | Microsoft.Extensions.Identity.Core / Microsoft.AspNetCore.Identity | `class` only |
| `UserStoreBase<TUser, TKey, ...>`, `RoleStoreBase<TRole, TKey, ...>` | Microsoft.Extensions.Identity.**Stores** | `TUser : IdentityUser<TKey>`, `TRole : IdentityRole<TKey>`, `TKey : IEquatable<TKey>` |
| `UserStore<TUser, TRole, TContext, TKey>` | ...Identity.EntityFrameworkCore | inherits the above, adds `TContext : DbContext` |

The managers never touch a member. `UserManager.GetUserNameAsync` is
`Store.GetUserNameAsync(user, ct)`; the property read `user.UserName` happens inside
`UserStoreBase`, where the `IdentityUser<TKey>` constraint exists. Optional capabilities
are runtime casts of the injected store (`Store is IUserEmailStore<TUser>`). Ids cross
the manager boundary as strings (`ConvertIdToString(user.Id)`).

The scaffolded Identity UI is generic over `TUser : class` and never casts: it reads
username/phone through `UserManager` methods only.

Why the managers stay `class`: the docs are explicit — "You don't need to inherit from a
particular type to implement your own custom identity storage solution." A custom store
over an arbitrary POCO is a supported scenario, so the manager layer must stay opaque.

## How OpenIddict does it

The same two-layer shape, with the boundary between Core and the storage providers:

| Layer | Constraint |
|---|---|
| `OpenIddictApplicationManager<TApplication>` (and the other managers), `IOpenIddictApplicationStore<TApplication>` (and the other store interfaces) | `class` only |
| EF Core provider: `ReplaceDefaultEntities<TApplication, TAuthorization, ..., TKey>()` and the store classes | `TApplication : OpenIddictEntityFrameworkCoreApplication<TKey, TAuthorization, TToken>` etc., `TKey : notnull, IEquatable<TKey>` — custom entities **must derive** from the provider base models |
| MongoDB provider: `ReplaceDefaultApplicationEntity<TApplication>()` etc. | `TApplication : OpenIddictMongoDbApplication` — same rule, simpler shape (no TKey, no navigation properties) |

Managers access nothing directly; every read is a per-property store accessor
(`Store.GetClientIdAsync(application, ct)`). The provider store's implementation is a
plain property read (`=> new(application.ClientId)`) because *its* constraint guarantees
the base model.

History worth knowing: through 6.x the "does the entity derive from the provider base"
check happened at **runtime** in store resolvers (`FindGenericBaseType` +
`MakeGenericType`). OpenIddict 7.0 deleted the resolvers for trimming/AOT and moved the
check to **compile time** as generic constraints on the provider builder methods — i.e.
the project's trajectory was *toward* stronger compile-time constraints at the layer
that knows the storage shape.

Why Core stays `class`: entity opacity is what lets each provider optimize its own
entity shape (EF stores redirect URIs as a JSON string column, Mongo as a native indexed
array) and lets a custom store use arbitrary POCOs (issue #608, the RC3 redesign).

## The other libraries

| Library | What it is | Constraint on the user type | Member access mechanism |
|---|---|---|---|
| **Duende IdentityServer** `AddAspNetIdentity<TUser>` | Token server integration | `class` only | None, ever. Delegates entirely to `UserManager<TUser>` and the claims-principal factory; profile data is read from claims. |
| **IdentityManager2** (AspNetIdentity adapter) | Admin UI over Identity | `TUser : IdentityUser<TUserKey>, new()`, `TRole : IdentityRole<TRoleKey>, new()` | UI layer is a non-generic `IIdentityManagerService` over string-keyed property metadata; the adapter's metadata wraps `UserManager.Get*/Set*Async` delegates plus reflection for extra properties. |
| **Skoruba Duende.IdentityServer.Admin** | Full admin UI, generic over user/role — the closest analog to StellarAdmin | `TUser : IdentityUser<TKey>`, `TRole : IdentityRole<TKey>`, `TKey : IEquatable<TKey>` — threaded through a 20+-parameter generic surface (services, mappers, controllers) | Direct property access on the stock members; reflection name-matching copy plus registered customizers for custom members, into a parallel DTO hierarchy. |
| **cloudscribe** | CMS with user management screens | `TUser : SiteUser` — its **own concrete base class** (key fixed to Guid); the shipped admin controller closes the generic to `SiteUser` outright | Direct property access; custom fields via an extension-point interface, not subtyping. |
| **ABP Framework** (Volo.Abp.Identity) | Full framework with identity management UI | No generics at all — its own concrete `IdentityUser` aggregate (Guid key) | Direct access; custom fields via an `ExtraProperties` dictionary flowing through entity, DTO and auto-generated form fields. |
| **AspNetCore.Identity.Mongo** | Store package | `MongoUser<TKey> : IdentityUser<TKey>`; consumers derive from `MongoUser` | Store-internal, against the known base. |

Two cross-library observations:

1. **The `class`-only libraries are exactly the ones that never read a member.** Duende
   reads claims; the Identity managers call stores. Every library that *displays or
   edits* user data — which is what StellarAdmin.Identity does — anchors on
   `IdentityUser<TKey>` or on its own concrete base class.
2. **A constraint only ever buys the stock members.** Nobody reaches *custom* members of
   an open `TUser` through a constraint; for those, every library adds a second
   mechanism (metadata, reflection copy, extension dictionary, extension points). For
   us that second mechanism already exists and is better than all of theirs: the
   consumer's own typed lambdas in the builder.

## The pattern, extracted

Every serious library splits into the same two layers:

- an **entity-opaque core** (`class` only) that owns the machinery and never touches a
  member, and
- a **framework-aware provider layer** (constrained to the framework's base entity)
  where direct property access is legal, closed over the concrete types at registration.

Our architecture already has both layers — that is what the resource-layer refactor
built. `Options/Base` + `Builders/Base` + `Infrastructure/Query` + the shared views are
the entity-opaque core, and they must stay `TEntity : class`. The Identity-side classes
(`IdentityUsersOptions`, `IdentityRolesOptions`, the root builder, the controllers, the
registration) are the provider layer. The controllers and registration are already
constrained. **The anomaly is that two-and-a-half classes of our provider layer —
the options and the root builder — are under-constrained relative to the layer they
belong to.** The workarounds are the cost of that anomaly: registration (constrained)
keeps having to hand member access down to options (unconstrained) that sit on the same
side of the boundary.

One more point that matters: ASP.NET Identity and OpenIddict keep their cores at `class`
*to support arbitrary POCOs through custom stores*. StellarAdmin.Identity has no such
scenario — the only entry points, the `AddIdentity` overloads, already require
`IdentityUser<TKey>`/`IdentityRole<TKey>`. A consumer cannot get a non-Identity type
into `IdentityUsersOptions<TUser>` today. The loose constraint therefore buys consumers
nothing; it only costs the library member access.

## Options

### Option A — status quo: keep `class`, thread delegates from the registration

Library side (what exists today):

```csharp
// IdentityUsersOptions.cs
public class IdentityUsersOptions<TUser> : ResourceOptions<TUser>
    where TUser : class
{
    // The delete display name comes from the registration: this class only knows
    // TUser as a class, so it cannot reach the Identity members itself.
    public IdentityUsersOptions(Func<TUser, string?> deleteDisplayName)
        : base(
            /* ... */
            new DeleteDefaults<TUser>(
                Title: "Delete user",
                MessageFormat: "Are you sure you want to delete the user {0}?",
                ConfirmLabel: "Delete",
                CancelLabel: "Cancel",
                DisplayName: deleteDisplayName
            )
        ) { }
}

// StellarAdminBuilderExtensions.cs — the registration closes the gap
var usersOptions = GetOrAddOptions(
    builder.Services,
    () => new IdentityUsersOptions<TUser>(user => user.UserName ?? user.ToString())
);
```

Consumer side: unchanged, `AddIdentity<ApplicationUser, ApplicationRole>(identity => ...)`.

- For: matches the ASP.NET Identity manager / OpenIddict core precedent on its face; no
  type-parameter growth.
- Against: the precedent does not actually apply — those cores stay `class` to support
  POCOs via custom stores, a scenario we do not have. Each future member-touching
  default adds another ctor parameter and factory lambda. The role options already show
  the second workaround species (`ToString()` relied on for behavior). The classes
  *say* less than the library requires: `where TUser : class` reads as "any class
  works", which is false everywhere in this assembly.

### Option B — constrain the Identity-side classes, carrying `TKey`

`IdentityUser<TKey>` is the lowest type that exposes `UserName`/`Email`, and there is no
non-generic base or interface above it, so the constraint necessarily brings a `TKey`
parameter. Three classes change: the two options classes and the root builder.

Library side:

```csharp
// IdentityUsersOptions.cs — writes its own defaults, parameterless ctor again
public class IdentityUsersOptions<TUser, TKey> : ResourceOptions<TUser>
    where TUser : IdentityUser<TKey>
    where TKey : IEquatable<TKey>
{
    public IdentityUsersOptions()
        : base(
            /* ... unchanged ... */
            new DeleteDefaults<TUser>(
                Title: "Delete user",
                MessageFormat: "Are you sure you want to delete the user {0}?",
                ConfirmLabel: "Delete",
                CancelLabel: "Cancel",
                DisplayName: user => user.UserName
            )
        ) { }
}

// IdentityRolesOptions.cs
public class IdentityRolesOptions<TRole, TKey> : ResourceOptions<TRole>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    public IdentityRolesOptions()
        : base(
            /* ... */
            new DeleteDefaults<TRole>(
                /* ... */
                DisplayName: role => role.Name   // no ToString() hack
            )
        ) { }
}

// StellarAdminIdentityBuilder.cs
public class StellarAdminIdentityBuilder<TUser, TRole, TKey>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    // holds IdentityUsersOptions<TUser, TKey> and IdentityRolesOptions<TRole, TKey>;
    // ConfigureUsers/ConfigureRoles/ConfigureSidebar unchanged in shape
}

// StellarAdminBuilderExtensions.cs — the threading disappears
var usersOptions = GetOrAddOptions<IdentityUsersOptions<TUser, TKey>>(builder.Services);
// the Func<TOptions> factory overload of GetOrAddOptions can be deleted
```

The controllers inject `IdentityUsersOptions<TUser, TKey>` — they already carry `TKey`,
so nothing else about them changes.

Consumer side — **unchanged**, because the type parameters are inferred or supplied
exactly as today:

```csharp
builder.Services.AddStellarAdmin()
    .AddIdentity<ApplicationUser, ApplicationRole>(identity =>
    {
        identity.ConfigureUsers(users =>
            users.Index(index => index.Columns(c => c.Add(u => u.Email).Sortable()))
        );
    });
```

The string-key `AddIdentity<TUser, TRole>()` overload returns
`StellarAdminIdentityBuilder<TUser, TRole, string>`; the `TKey` overload returns it
closed over the host's key. The playground's `Program.cs` compiles unchanged (the
standing acid test). The third parameter surfaces only if a consumer stores the builder
in an explicitly-typed variable or writes a helper method over it — the same places
stock Identity's `TKey` surfaces.

- For: eliminates the workaround category outright rather than per-instance; the
  classes state the truth (`AddIdentity` already enforces exactly these constraints);
  matches what every member-reading analog does (Skoruba, IdentityManager2's adapter,
  the store layers of Identity/OpenIddict, and OpenIddict's own 7.0 move from runtime
  checks to compile-time constraints); future user-specific builder extension members
  get natural, consistent constraints; `GetOrAddOptions` loses its factory overload.
- Against: one more type parameter on three public classes. Skoruba is the cautionary
  tale for generic explosion — but their 20+ parameters come from genericizing the
  DbContext, every join entity, and a parallel DTO hierarchy; constraining to
  `IdentityUser<TKey>` costs exactly one parameter here and it stops at these three
  classes (the `Base/` layer, the view models, and the views stay `TEntity : class`).

### Rejected variants

- **Constrain to non-generic `IdentityUser`** (i.e. `IdentityUser<string>`): kills
  custom-key hosts; a `Guid`-keyed user does not derive from `IdentityUser<string>`.
- **Own interface (`IStellarAdminUser`) instead of `IdentityUser<TKey>`**: consumers'
  existing `ApplicationUser : IdentityUser` cannot retroactively implement it; would
  force a base-class swap on every host. cloudscribe/ABP can own the base class because
  they own the whole stack; we sit on top of the host's Identity registration.
- **Per-property accessor abstraction (a store-interface analog of our own)**: the
  full Identity/OpenIddict core pattern. Correct for a library that must support
  arbitrary POCOs; pure ceremony for one that requires `IdentityUser<TKey>` at its only
  entry point.

## Recommendation

Option B. Keep the `Base/` resource layer at `TEntity : class` — that boundary is the
part of our architecture the research validates emphatically — and constrain the three
Identity-side classes to `IdentityUser<TKey>` / `IdentityRole<TKey>`, accepting the
`TKey` parameter. The libraries that stay at `class` are the ones that never read a
member; we read members, our entry points already demand the constraint, and the cost
is one inferred type parameter against a growing tax of threaded delegates.
