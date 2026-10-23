# DbContext discovery

Status: completed (2026-10-23). Stage E of [configuration simplification](../configuration-simplification.md#e-the-dbcontext-is-found-not-named), committed in `bf4523b`, `a9c86b0` and the commit that closes this plan, on the `configuration-simplification` branch, which isn't merged yet.

## Goal

EF Core resources and EF Core item sources stop naming the DbContext. StellarAdmin finds the registered context whose model maps the entity. Naming a context is only needed when several contexts map the same entity, or to pin one.

## API

```csharp
// Before
admin.AddEfCoreResource<ApplicationDbContext, Product>(resource => ...);
select.UseItems<ApplicationDbContext, Category, int>(category => category.Id, category => category.Name);

// After
admin.AddEfCoreResource<Product>(resource => ...);
select.UseItems<Category, int>(category => category.Id, category => category.Name);

// Only when several contexts map the entity, or to pin one
admin.AddEfCoreResource<Product>(resource => resource.UseDbContext<ReportingDbContext>());
select.UseItems<Category, int>(category => category.Id, category => category.Name, items => items.UseDbContext<ReportingDbContext>());
```

- **Changed:** `TContext` is removed from:
  - the four `AddEfCoreResource` overloads;
  - `EfCoreResourceBuilder<TEntity>`, `EfCoreResourceIndexBuilder<TEntity>` and `EfCoreRowDetailPanelsBuilder<TEntity>`;
  - the six `UseItems<TEntity, TValue>` overloads on choice, lookup sheet and multi-lookup sheet editors.
- **New:** `UseDbContext<TContext>()` on `EfCoreResourceBuilder<TEntity>`, `EfCoreChoiceItemsBuilder<TEntity, TValue>` and `EfCoreLookupItemsBuilder<TEntity, TValue>`.
- No obsolete overloads are kept. The package is a preview, and none of this API has shipped (`PublicAPI.Shipped.txt` is empty).
- `UseItems` still needs `TValue`, because C# can't infer `TEntity` from an untyped lambda. Stage A removes it.

## Behaviour

- **Candidates.** A context type is a candidate when it has a `DbContextOptions<TContext>` registration (from `AddDbContext`, `AddDbContextPool` or `AddDbContextFactory`), and the context type itself is registered as a service, as today's `GetRequiredService<TContext>` requires. The home page's `EntityCandidateScanner` already reads registrations this way.
  - The service collection is read on first use, so contexts registered after `AddStellarAdmin` are found.
  - A context only reachable through a pooled factory isn't a candidate, as today.
- **Mapping.** A candidate maps an entity when its model has an entity type for that CLR type (`Model.FindEntityType`). DbSet properties aren't enough, because entities configured only in `OnModelCreating` would be missed.
- **Resource outcomes:**
  - One candidate maps the entity: that context is used.
  - `UseDbContext<T>()` was called: that context is used, without the search. If it doesn't map the entity, the resource is unavailable with the existing `EntityNotMapped` problem.
  - No candidate maps the entity: the resource is unavailable under `EntityNotMapped`. The message says no registered context maps it, and the fix is to add a `DbSet` to a context or register the context.
  - Several candidates map it: the resource is unavailable under a new `AmbiguousDbContext` (event 1088). The message names the contexts, and the fix is `UseDbContext<T>()`.
- **Item sources.** `UseItems` resolves its context the same way, on its first request, because it can be on any form, not only an EF Core resource's.
  - Where a pin is set, it is used.
  - An unmapped or ambiguous entity fails that request with an exception that names the fix. Step 1 checks what an unmapped entity does today, and the new failure must be no worse.
  - Items from a referenced resource (Stage B) use that resource's context.
- **Many-to-many binders and row details** use the context of the resource they belong to.

## Startup cost

- Options are already built at startup by `ConfigurationProblemsLogger`, and the EF Core PostConfigure already resolves the named context and reads its model there.
- A singleton caches each candidate context's mapped CLR types the first time it's asked. So each model is read once for the whole app, and each resource then needs a dictionary lookup.
- **One context:** no extra cost.
- **Several contexts:** each candidate's model is built once at startup instead of on its first query. EF Core caches models per context type, so the app pays this once either way.
- `UseDbContext<T>()` skips the search.

## Implementation

- **`EfCoreContexts`** is a new internal singleton, registered with `TryAddSingleton`. As built, it finds candidates from the non-generic `DbContextOptions` services that every `AddDbContext*` call registers (each names its `ContextType`), filtered with `IServiceProviderIsService`, rather than holding the `IServiceCollection`. It provides:
  - the candidate context types, found lazily;
  - a per-context cache of mapped CLR types, filled from a service scope;
  - `Resolve(entityType, pinned)`, which returns the context type or the reason none was found;
  - `Get(entityType, services)`, which returns the resolved `DbContext` from the request's services.
- **Internal types drop `TContext`.** They take a `DbContext` from `EfCoreContexts` instead of `TContext` from DI. This covers `EfCoreResourceDataSource`, `EfCoreLookupItems`, `EfCoreChoiceItemsLoader`, `ManyToManyFieldBinder`, `EfCoreRowDetailLoader` and `EfCoreResourceReference`. They only use `Set<T>()`, `Model` and `SaveChangesAsync`, so the base type is enough. The `MakeGenericType`/`MakeGenericMethod` calls lose their context argument.
- **The EF Core PostConfigure** resolves the context through `EfCoreContexts`, honouring the resource's pin, before the existing mapping checks. It reports `EntityNotMapped` or `AmbiguousDbContext` as above.
- **Pins** are stored on the resource's EF Core options (`EfCoreResourceReferenceOptions<TEntity>` or a sibling options type) and on the item options.
- **The home page** setup snippet in `SetupSteps` drops the context name.

## Samples, references and docs

- AdminPlayground registrations: 19 `AddEfCoreResource` calls and the `UseItems` call sites.
- Integration tests: the test host and each resource test that names the builder types.
- Consumer skills: `skills/stellar-admin` (the SKILL file and five references) and `skills/stellar-admin-setup`. The `build/skill-evals` fixtures are regenerated or updated.
- Maintained docs: `docs/development.md`, `docs/conventions/csharp-file-organization.md` (its example) and `docs/conventions/options-builders.md` where it names the builder. Historical plans aren't edited.
- The website has no `AddEfCoreResource` or `UseItems` code today, so it needs nothing.

## Tests

- A single context is found, for a resource and for `UseItems`.
- A context registered after `AddStellarAdmin` is found.
- No context maps the entity: the resource is unavailable with `EntityNotMapped`.
- Two contexts map the entity: the resource is unavailable with `AmbiguousDbContext`, naming both. `UseDbContext` resolves it.
- A pinned context that doesn't map the entity gives `EntityNotMapped`.
- Ambiguous `UseItems` with an item pin loads from the pinned context.
- The existing EF Core suites pass unchanged apart from the type arguments.

## Steps

1. `EfCoreContexts`, the internal types without `TContext`, the new public API, the problems and the tests.
2. Samples, the home page snippet, consumer references and maintained docs.
3. Verification (Release build and tests, SkillsGenerator check, a playground smoke run on port 5207) and this plan's record.

Checkpoint with Jerrie after step 1.

## Decisions (Jerrie, 2026-10-23)

1. A context is pinned per resource and per item source. There's no app-wide `admin.UseDbContext<T>()` until an app needs one.
2. The `TContext` overloads are replaced outright, with no compatibility overloads, since the API hasn't shipped.

## Progress

### Step 1 (2026-10-23, `bf4523b`)

Done as planned, with these differences:

- **Candidates come from services, not the service collection.** EF Core registers a non-generic `DbContextOptions` for each context, and it names the context type. So `EfCoreContexts` needs only `IServiceScopeFactory` and `IServiceProviderIsService`.
- **Apps without EF Core resources.** `UseItems` can be on a form of a resource that isn't EF Core. `EfCoreContexts` is only registered by `AddEfCoreResource`, so an app with no EF Core resources finds its items' context without the cache (`EfCoreContexts.For`).
- **The data source finds its context on first use.** An unavailable resource's data source is still created when its page is requested, so resolving in the constructor turned the 404 into a 500.
- **A candidate that can't be created** is passed over, unless no other context maps the entity, so one broken context doesn't hide the resources of another.
- **Unpinned `UseItems` of an entity that several contexts map** fails the request with an `InvalidOperationException` naming the contexts and `UseDbContext`. An unmapped entity fails the same way today, with EF Core's message, so it's no worse.
- **The home page snippet** (`SetupSteps`) moved into this step, because `HomePageTests` asserts it.

Tests: `ResourceDbContextTests` (6 tests), with a `ReportingDbContext` fixture over the catalog's tables, registered after `AddStellarAdmin`. They cover a context found when registered after `AddStellarAdmin`, two contexts mapping an entity (unavailable, event 1088), `UseDbContext` picking one, a pinned context that doesn't map the entity (event 1080), an item pin, and an unpinned ambiguous item source. `UnmappedEntity_MakesOnlyThatResourceUnavailable` now expects the new message, and the other EF Core tests lost their context type argument.

Verification: `dotnet test --project tests/StellarAdmin.EntityFrameworkCore.IntegrationTests` ran 153 tests, all passing. The samples don't compile until step 2, so the full solution wasn't built.

### Step 2 (2026-10-23, `a9c86b0`)

- **AdminPlayground:** the five `AddEfCoreResource` calls that named `ApplicationDbContext` (Categories, Departments, Excursions, Products and Tags) dropped it. The playground's `UseItems` calls use lookup sources, not EF Core, so they're unchanged. The Excursions comments now say no registered context maps `ExcursionBooking`.
- **Consumer skills:**
  - `skills/stellar-admin` changed its SKILL file (task table and checklist) and the authorization, editors, inline-editing, relationships, resources and sidebar references. Resources and relationships explain how the context is found and `UseDbContext`.
  - `skills/stellar-admin-setup` changed its SKILL file and the troubleshooting row for a missing context, which now covers both outcomes.
- **Skill evals:** the `build/skill-evals/admin-panel` installed fixture and task checks were updated.
- **Maintained docs:** `docs/development.md` explains how the context is found. The examples in `docs/conventions/options-builders.md` and `docs/conventions/csharp-file-organization.md` dropped `TContext`.
- **Unchanged:** the home page snippet was already done in step 1. The website has no EF Core registration code.

Verification: `dotnet build StellarAdmin.slnx -c Release` succeeded with no errors. The SkillsGenerator regeneration and its check reported no drift. Tests, the Release test run and the playground smoke run are step 3.

### Step 3 (2026-10-23)

- **Release tests:** `dotnet build StellarAdmin.slnx -c Release`, then `dotnet test --solution StellarAdmin.slnx --configuration Release --minimum-expected-tests 1 --no-build`, ran 1876 tests, all passing.
- **Skills:** the SkillsGenerator check reported no drift.
- **Playground smoke run on port 5207:**
  - Products, the Products edit page, Categories, Tags and Departments returned 200 from the context that was found.
  - The edit page's Category lookup sheet listed categories and searched them.
  - Excursion bookings returned 404, and its problem was logged.
  - `app.db` was unchanged after the run.
- **Message fix:** the smoke run showed "has a ExcursionBooking entity". The messages now say a context "maps" the entity ("no registered DbContext maps `ExcursionBooking`", "more than one DbContext maps `Product`: ..."). The tests and the troubleshooting row follow. After the change, the EF Core tests (153) passed again in Release, and the skills check reported no drift.

## Open

- An app-wide `admin.UseDbContext<T>()` (decision 1) waits until an app needs it.
- An unpinned item source of an entity that several contexts map is only reported when a request loads it. Checking it when options are built would need a hook for item sources on forms that aren't EF Core resources.
