# Options builder conventions

Conventions for the public configuration APIs of StellarAdmin libraries — the fluent builders consumers call from `Program.cs`, e.g. `AddStellarAdmin().AddDashboard(dashboard => dashboard.AddIdentity<TUser, TRole>(identity => ...))`. All StellarAdmin libraries (current and future) follow these so that configuring any product feels like configuring the others.

Status: adopted 2026-08-04, while the Identity options builder was still being prototyped. Expect the [Provisional decisions](#provisional-decisions) section to shrink as that work settles.

## The core rule

> **Verbs mark feature boundaries. Nouns drill into structure. Properties hold values.**

- A method with a **verb prefix** (`Configure*`, `Add*`, `Enable*`) marks a **feature boundary** — the entry point into a feature's configuration. Feature boundaries usually sit at the root level of a builder, but an opt-in feature nested below the root (e.g. search on the users index page) is still a boundary and still gets its verb: `index.EnableSearch(...)`.
- Inside a feature, **bare-noun methods** taking a configure lambda drill into structure: `users.Index(index => ...)`, `index.Columns(columns => ...)`, `sidebar.UsersItem(item => ...)`. A bare noun promises the thing already exists — it must never secretly gate a feature's existence.
- **Scalar settings are settable properties**, not `Set*`/`With*` methods: `index.Title = "Users"`, `item.Visible = false`.
- **Behavior is a verb method on the leaf**: `index.TransformQuery(q => q.Include(...))`, `index.DefaultSortBy(u => u.Email)`, `columns.Add(u => u.Email)`. Name the verb phrase for what the call registers — weak carrier verbs like `Apply*`, which describe when the behavior runs rather than what is being configured, don't qualify (`ApplyQuery` reads like it executes the query).

`Configure*` at every level gets tiring; no verbs at all reads as accidental. The verb at the boundary tells the reader "you are now configuring feature X"; everything inside is plain structure.

## Verb vocabulary

| Prefix | Meaning | Example |
|---|---|---|
| `Add*` (product entry point) | Register a whole product/module — top-level products on `StellarAdminBuilder`, specialized packages on their product's builder | `AddDashboard(...)`, `dashboard.AddIdentity<TUser, TRole>(...)` |
| `Configure*` (root of a product builder) | Tune a feature that **ships by default** — calling it or not, the feature exists | `identity.ConfigureUsers(...)`, `identity.ConfigureSidebar(...)` |
| `Add*` (root of a product builder) | **Opt in** to a feature that is otherwise absent — no call means no controllers, no routes, no sidebar item | (no current example — roles resolved to `Configure*`, see below) |
| `Enable*` (any level) | **Opt in** to a feature that is otherwise absent at that level — the call gates rendering and behavior, not just an option value | `index.EnableSearch(...)` |
| `Add` (on a collection scope) | Append an entry; returns the entry's builder for chaining | `columns.Add(u => u.Email)` |
| Bare noun + lambda | Drill into a structural level | `users.Index(...)`, `sidebar.RolesItem(...)` |
| Other verbs (leaf) | Attach behavior | `index.TransformQuery(...)`, `index.DefaultSortBy(...)` |

The `Configure*` vs `Add*`/`Enable*` distinction carries real information: the verb promises whether the feature exists without the call. Keep that promise — `Add*` and `Enable*` must actually gate the feature (controllers, sidebar items, rendered UI), not just tweak options. `Add*` registers something (routes, entries); `Enable*` turns on an affordance of an existing page — the OpenIddict toggle vocabulary (`Allow*`/`Enable*`/`Require*`) is the precedent.

## Resource action registration

The standalone resource API uses `AllowCreate`, `AllowEdit`, and `AllowDelete` as explicit action registration boundaries (decision 2026-09-21). These opt-in methods enable the action and replace any previous configuration for it. Their no-callback overloads return the action builder, and callback overloads return the resource builder. Omitted actions stay disabled even when the data source implements their handlers.

## Canonical example

The reference shape, from the Identity builder design:

```csharp
builder.Services.AddStellarAdmin().AddDashboard(dashboard => dashboard.AddIdentity<ApplicationUser, ApplicationRole>(identity =>
{
    identity.ConfigureSidebar(sidebar =>          // root: Configure* = feature boundary
    {
        sidebar.Title = "User management";        // property = scalar on the section
        sidebar.UsersItem(item =>                 // bare noun = structural drill-down
        {
            item.Icon = "users";
        });
        sidebar.RolesItem(item =>
        {
            item.Visible = false;
        });
    });

    identity.ConfigureUsers(users =>
    {
        users.Index(index =>                      // noun: a page
        {
            index.Title = "Users";
            index.Subtitle = "People who can sign in";
            index.DefaultSortBy(u => u.Email);                // verb: behavior (last call wins)
            index.TransformQuery(q => q.Include(u => u.Department));   // verb: behavior (composes)
            index.EnableSearch(                   // Enable* = nested opt-in feature;
                (q, term) => q.Where(u => u.Email!.Contains(term)),  // essential data required
                search => search.Placeholder = "Search name or email..."  // settings optional
            );
            index.Columns(columns =>              // noun: a collection scope
            {
                columns.Add(u => u.UserName);     // Add = append; returns column builder
                columns.Add(u => u.Email);
            });
        });
    });
}));
```

## Supporting rules

`StellarAdmin.Core` owns `StellarAdminBuilder` and `AddStellarAdmin()` in the `StellarAdmin` namespace. Shared icon registration (`AddIcon()` and `AddIconPack<T>()`) also belongs to this builder and configures `IconOptions`. Lucide defaults are seeded in the options; each provider owns its options instance. Consumers inject `IOptions<IconOptions>` and resolve `.Value` in their constructors; internal rendering helpers accept the resolved `IconOptions`. Feature packages supply extension methods on that builder; for example, TagHelpers owns `ConfigureForms()` and its form options. Keep feature dependencies out of Core.

- **Specialized packages extend their product's builder.** A product family's root package owns the entry point on `StellarAdminBuilder` (`AddDashboard()`); its specialized packages register on the builder that entry point returns (`dashboard.AddIdentity<TUser, TRole>(...)`), never directly on `StellarAdminBuilder` — the package hierarchy stays visible in the registration code. *(Decision Jerrie 2026-08-16, consolidating the former `AddShell()`/`AddResources()` entry points; renamed to `AddDashboard()` on 2026-09-16.)*
- **Registration overloads return the appropriate builder.** A no-callback `Add*` overload returns the added feature’s builder. The callback overload configures that builder and returns the parent builder.
- **Generic flows from the root.** The product builder is generic over the consumer's types (`StellarAdminIdentityBuilder<TUser, TRole, TKey>`) so that every nested lambda (`u => u.Email`, `q => q.Include(...)`) infers without annotations. Capture member selectors as `Expression<Func<T, TProp>>` (generic `Add<TProp>`), not `Expression<Func<T, object>>` — avoids boxing `Convert` nodes in the stored trees.
- **Essential data is a required argument on the boundary verb; settings live in the optional configure lambda.** Data without which the feature is meaningless — search's query, a scope's title — is demanded by the signature, so the compiler enforces it: `index.EnableSearch(applySearch, search => ...)`, `scopes.Add(title, predicate)`. Settings with sensible defaults (a placeholder, a flag) stay properties on the builder inside the lambda, never optional scalar parameters — optional parameters grow poorly and adding one later is binary-breaking for compiled consumers. Prior art: EF Core's `UseSqlServer(connectionString, sql => ...)`, CORS's `AddPolicy(name, policy => ...)`, OpenIddict 2.x's `EnableTokenEndpoint("/connect/token")`.
- **No third-party dependencies in signatures.** Query interception is `Func<IQueryable<T>, IQueryable<T>>`; the *consumer* calls `.Include()` from their own EF Core reference. Shared StellarAdmin screen builders never reference EF Core for this. Dedicated integration packages, such as `StellarAdmin.Dashboard.EntityFrameworkCore`, may depend on EF Core and expose its types at their registration boundary.
- **Default collection entries are seeded, visible, and extended.** The library seeds defaults as real, inspectable definitions on the options (never hardcoded in views), so every feature — rendering, sorting, whatever comes next — treats default and configured entries through one code path. `Add` therefore *extends* the seeded defaults, and a fully custom set starts with `Clear()`. Clearing without adding is honored literally (an empty collection renders empty — no silent fallback). *(Decision Jerrie 2026-08-05, superseding the earlier "first `Add` discards the defaults" rule: that rule existed to avoid appending to unseen defaults, and seeding makes them seen.)*
- **Defaults cascade; every string has one authoritative source.** A sidebar item's label defaults from its page's title, which defaults from the library. Overriding one level must not require repeating strings at another.
- **An overload of the root verb, not a second verb, states which entity types the application has.** Found by the Identity roles work (2026-08-10): roles are not opt-in, so `AddRoles` would break the `Configure*` promise, but `ConfigureRoles` needs `TRole` at registration time. The answer is `AddIdentity<TUser, TRole>()` — the overload carries the "this app has roles" fact, and `ConfigureRoles` stays a true `Configure*` (the screens exist whether or not it is called). A feature keyed to a consumer type is enabled by naming the type at the root verb, never by a separate opt-in verb. *(Revised 2026-08-10 once the Roles pages landed: the users-only `AddIdentity<TUser>()` overload was removed, so every host names both entity types — matching stock `services.AddIdentity<TUser, TRole>()` and the all-or-nothing entity set of OpenIddict's `ReplaceDefaultEntities<...>()`. The principle stands; the entity set is simply never partial.)*
- **Visibility follows enablement.** Hiding a navigation entry is cosmetic (`item.Visible`); actually removing a feature is the root verb's job (`Add*` not called). Consumers must never be led to hide a link and believe the routes are gone.
- **Builders register configuration through the standard options pipeline.** Resource builders hold an `IServiceCollection` and expose setter-only scalar properties that call `services.Configure<TOptions>(...)`. Read configuration from resolved `IOptions<>`, not from the builder. Do not register an `Options.Create` wrapper in place of the standard configuration and validation pipeline.
- **Name collisions across levels get a suffix.** `identity.ConfigureUsers(...)` (the feature) vs `sidebar.UsersItem(...)` (the nav entry) — never the same bare name meaning two things in adjacent scopes.

## Anti-patterns

- `Configure*` (or any uniform verb prefix) repeated below the root level.
- A bare-noun drill-down that secretly gates a feature's existence — opt-in gets a verb (`Enable*`), wherever it lives.
- Mixing verb-prefixed and bare-noun methods *at the same level* of a builder.
- `Set*`/`With*` methods for plain scalar values that a property serves.
- Scalar settings as optional parameters on a boundary verb — they belong in the configure lambda (see "Essential data is a required argument").
- Collection configuration that appends to *unseen* defaults — defaults live as visible seeded entries (clearable with `Clear()`), never as hidden view markup that `Add` silently extends.
- An `Add*` root method that doesn't actually gate the feature's existence.
- The same configurable concept named differently at different levels.

## Precedents

Tiebreakers when a new case isn't covered here — prefer whatever the closest precedent does:

- **OpenIddict** — the strict end: verb vocabulary at every level (`Use*` plug in, `Set*` assign, `Add*` append, `Allow*`/`Enable*`/`Disable*`/`Require*` toggle), no settable properties. Source of our `Add*` = opt-in semantics.
- **MassTransit** — verb at the top (`AddMassTransit`, `UsingRabbitMq`), mostly bare configuration inside. Closest to our overall shape.
- **EF Core `ModelBuilder`** — `Add`/`Property(...)` returning entry builders for per-entry chaining; statement-per-entry style for collections.
- **ASP.NET Core Identity** — nested option POCOs (`options.Password.RequiredLength`); the precedent for properties-as-values.

## Provisional decisions

Not yet settled — revisit when the Identity builder implementation lands, and update this doc rather than silently diverging. *(Resolved 2026-08-10 and moved to the rules above: Roles is `ConfigureRoles`; the role type arrives via the `AddIdentity` overload.)*

- **Sidebar configuration lives in one place** (`identity.ConfigureSidebar`, all items together) rather than each feature contributing its own nav entry (`users.SidebarItem(...)`). Chosen "for now"; per-feature placement would make the visibility-follows-enablement cascade more automatic.
- **Per-column configuration is settled for resource indexes.** `columns.Add(p => p.Name)` returns a column builder with setter-only `Title` and `Format` properties. `columns.Add(p => p.Name, column => column.Title = "Product name")` returns the columns builder. Column configuration actions are applied when the owning resource options resolve, creating separate column options for each options instance.
