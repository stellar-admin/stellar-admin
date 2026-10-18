# Home page setup checklist

Status: phases 1 and 2 done, and the skill docs of phase 3 (2026-10-18); the website docs wait for the admin panel docs batch. Approved design: `sandbox/html/home-setup.html` (option A only; the prototype stays as the record and is not rewritten after extraction).

## Goal

The panel's home page (`src/StellarAdmin/Areas/StellarAdmin/Views/Home/Index.cshtml`, currently empty) shows a setup checklist in Development, and links to the resources (or an empty state) in production or when the checklist is turned off.

## Public API

```csharp
builder.Services.AddStellarAdmin(admin =>
{
    admin.ConfigureHomePage(home => home.ShowSetupChecklist = false);
});
```

- `StellarAdminBuilder.ConfigureHomePage(Action<HomePageBuilder>)`, following [options builders](../conventions/options-builders.md): a `Configure*` method for a feature that always exists, a property for the scalar.
- `HomePageBuilder.ShowSetupChecklist` (`bool`, default `true`). The checklist only ever shows in Development; the property can't force it on in other environments.
- New internal `HomePageOptions` on `StellarAdminOptions`. Add both public members to `PublicAPI.Unshipped.txt`, with brief XML docs.

## Detection (all at request time, Development only)

The internal singleton `SetupStatusResolver` (`src/StellarAdmin/Home/`) produces a `SetupStatus` per request. Its constructor takes only `StellarAdminOptions`, `IHostEnvironment` and `IServiceProvider`; `IsEnabled` is false outside Development or when `ShowSetupChecklist` is false, and `ResolveAsync` then returns `null` before resolving any service or scanning. Everything else (resources, schemes, authorization and MVC options, the scanner) is resolved after that gate. The controller checks `IsEnabled` first too. Every step's state:

| Step | Done when | Notes |
| --- | --- | --- |
| Resource | at least one `ResourceRegistration` | Essential. |
| Protect | the current endpoint (`HttpContext.GetEndpoint()`) has `IAuthorizeData` or `AuthorizationPolicy` metadata and no `IAllowAnonymous`; or `AuthorizationOptions.FallbackPolicy` is set; or `MvcOptions.Filters` has an `AuthorizeFilter` | Essential; shows as a warning when not done. Covers `admin.RequireAuthorization`, `MapStellarAdmin().RequireAuthorization` and global rules. |
| Name and logo | `Branding.Title` or `Branding.Logo` set | |
| User menu | `UserMenu.Items` non-empty; or any of `NameClaimType`, `DescriptionClaimType`, `AvatarClaimType` set; or `Logout` differs from its defaults (`Path`, `RedirectPath` set, `AuthenticationSchemes` non-empty, `Show` false, `Icon` not `"log-out"`) | Optional. Hidden when the app has no authentication schemes. |
| Theme | `Stylesheets` contains a path outside `~/_content/StellarAdmin/` | Optional. The panel registers its own stylesheet, so exclude it. |
| Sidebar | `SidebarLinks` non-empty, or any resource's `ResolveSidebarItem(services)` has a group other than `"Resources"`, a non-zero order or an icon | Optional. Hidden when there are no resources. |

"App has authentication" = `IAuthenticationSchemeProvider.GetAllSchemesAsync()` is non-empty. It decides the user menu step's visibility and the protect step's text and code (no auth: say the panel is open and point at authentication docs).

Progress counts the visible steps. The first visible step that isn't done starts expanded.

### Entity scan

An internal `EntityCandidateScanner`, so the resource step can name candidates ("Found in VoyagerDbContext: Booking, Customer…") and tailor its code and agent prompt:

- `AddStellarAdmin` registers the scanner as a singleton that holds the `IServiceCollection` (the builder already has it as `Services`). Read registrations when first asked, not at registration time, so a `AddDbContext` after `AddStellarAdmin` is still seen.
- Find descriptors whose service type is the closed generic `DbContextOptions<TContext>`, matched by the generic type definition's full name `Microsoft.EntityFrameworkCore.DbContextOptions`1` (no EF Core reference in StellarAdmin). From each `TContext`, list public properties whose type is `DbSet<T>` by the same full-name check; collect `T`.
- Leave out entity types already registered as resources (in the resolver), and contexts left with none.
- Leave out DbSets declared by types in `Microsoft.*` namespaces, so Identity's claims and tokens tables aren't suggested.
- Is `StellarAdmin.EntityFrameworkCore` available: `Type.GetType("StellarAdmin.EntityFrameworkCore.StellarAdminBuilderExtensions, StellarAdmin.EntityFrameworkCore")`. Decides whether the resource step shows the `dotnet add package` line.
- Cache the result with `Lazy<T>`; the work is a linear pass over the descriptors plus reflection on a few context types, well under a millisecond. Only constructed in Development.
- No context found: the resource step shows the `AddResource<T>` + `UseDataSource<TDataSource>` + `UseKey` code from the prototype.

## View

- `HomeController.Index` checks `IHostEnvironment.IsDevelopment()` and `HomePageOptions.ShowSetupChecklist`; it passes either the checklist model or the production model (the visible resources the user may open, from the sidebar resource items so authorization already applies) to the view.
- `Index.cshtml` renders with sa-* tag helpers only, no raw elements with copied classes: `sa-page-container` (width `medium`), `sa-page-header`, `sa-card`, `sa-progress`, `sa-item` per step, `sa-collapsible` per step's details, `sa-tabs` (default variant) for Code / Ask your agent, `sa-button`, `sa-badge` for Optional, `sa-item` help links below the card, and `sa-empty` for production with no resources. Split into partials (`_SetupChecklist.cshtml`, `_SetupStep.cshtml`, `_HomeResources.cshtml`) in the same folder.
- Step copy, code and agent prompts move from the prototype's `steps(c)` into C# (one place: the checklist builder or a small `SetupSteps` static), tailored by the model's facts (entity name, context name, auth, EF package). Keep the prototype's text verbatim unless it no longer matches the API.
- Code blocks: the prototype's `.sa-setup-code` CSS goes into the panel stylesheet `src/StellarAdmin/Client/css/client.css` in `@layer components`, reading tokens only. C# syntax highlighting: drop it (plain monospace) unless it is trivial; the prototype's regex highlighter is not worth shipping.
- Footer: "Only shown in Development." with the "Turn off this checklist" collapsible (tabs with the `ConfigureHomePage` code and agent prompt, then the line about adding `Areas/StellarAdmin/Views/Home/Index.cshtml`).
- Skills note under each agent prompt: the compact warning line (triangle-alert, "Requires the skills:", the install command, an icon copy button).

## Client script (panel JS, `src/StellarAdmin/Client/js/stellar-admin-panel.js`)

- Copy buttons: `[data-copy]` copies the sibling `pre` text; `[data-copy-text]` copies its attribute. Use `navigator.clipboard.writeText`; show a toast or swap the icon briefly (check whether the UI library has a copy helper first).
- Remembered tab: on `tabshow` for a panel id `setup-*-(code|agent)`, save to `localStorage` key `stellaradmin:setup-tab` (try/catch) and select the matching tab in every other setup `sel-tabs`.
- Avoid the flash: the server renders Code active; a small inline script right after the checklist reads the key and, when it's `agent`, selects the agent tabs before paint (or sets an attribute on the container that CSS uses to pick the initial tab). Choose whichever `sel-tabs` supports without changes.

## Tests

- Unit (`tests/StellarAdmin.Tests`, TUnit, mirror folders): checklist builder per row of the detection table, including the user menu's each-default checks, the theme stylesheet exclusion and the sidebar heuristic; entity scanner with a fake `DbContextOptions<T>`-shaped registration (or a real EF Core context in `StellarAdmin.EntityFrameworkCore.IntegrationTests` if the name match needs the real types); `ConfigureHomePage` builder.
- Integration (`tests/StellarAdmin.IntegrationTests`): Development shows the checklist; Production shows resource links / empty state; `ShowSetupChecklist = false` shows the production page in Development; protection detected through `admin.RequireAuthorization`, `MapStellarAdmin().RequireAuthorization` and a fallback policy.
- Browser check (CDP) in AdminPlayground: collapsibles, tab switching and memory, copy buttons, light and dark, a narrow width.

## Docs and skills

- stellar-admin skill: mention `ConfigureHomePage(home => home.ShowSetupChecklist = false)` and the home page override in the customization reference.
- Website docs: a short section on the home page, batched in the website follow-up.

## Phases (stop after each for Jerrie)

1. API + options + detection (checklist builder, entity scanner) with unit tests.
2. Views, CSS and JS; integration tests; browser check in AdminPlayground.
3. Skill and website docs.


## Progress

Phase 1 (2026-10-18): `ConfigureHomePage`/`HomePageBuilder.ShowSetupChecklist`, `HomePageOptions`, `EntityCandidateScanner`, `SetupStatusResolver` and `SetupStatus`, registered in `AddStellarAdmin`. Unit tests in `tests/StellarAdmin.Tests/Home/` (fake `DbContextOptions<T>`/`DbSet<T>` types, since the project doesn't reference EF Core) cover the early exit with a service provider that throws on any resolve, every detection row and the scanner. `dotnet run --project tests/StellarAdmin.Tests --configuration Release`: 237 passed, 0 failed, with the 34 new tests listed by `--list-tests`. Not yet verified: the scanner against real EF Core registrations and `HasEfCorePackage` true; add an EF Core integration test in phase 2.

Phase 2 (2026-10-18): `HomeController` calls `ResolveAsync` first and only builds the checklist when it returns a status; otherwise it lists the resources the sidebar would show the current user (visible and authorized, in sidebar order), or an empty state. Step content lives in `Home/SetupSteps.cs`; views are `Home/Index.cshtml` with `_SetupChecklist`, `_SetupStep`, `_SetupTabs`, `_SetupCode`, `_SetupCopyButton`, `_SetupLink` and `_HomeResources`, built from sa-* tag helpers. The code block, copy button and toggle rules are in `Client/css/client.css`. Settled during implementation:

- The remembered-tab and copy script is inline in `_SetupChecklist.cshtml`, not in the panel JS: it must run before the deferred components load to avoid a flash, and only exists when the checklist renders.
- The website has no admin panel docs yet, so the docs buttons link to the stellar-admin skill references on GitHub (themes link to the website's theming page and the theme builder).
- The theme step names the first app-relative stylesheet, falling back to an absolute URL, so a web font (Google Fonts) isn't taken for the theme.
- When no entity candidate is left, the resource step's agent prompt asks the agent to ask the user which entity or class to add.
- The "Found in …" line only shows while there are no resources (as in the prototype).
- `SetupStatus.HasResources` became `ResourceCount`, and `HasTheme` became `ThemeStylesheet` (with a computed `HasTheme`).

Verification: `tests/StellarAdmin.IntegrationTests/Home/HomePageTests.cs` (8 tests: steps in Development, first open step, production links and empty state, turned off, protection through `admin.RequireAuthorization` and `MapStellarAdmin().RequireAuthorization`, user menu step with authentication) and `tests/StellarAdmin.EntityFrameworkCore.IntegrationTests/Home/HomePageTests.cs` (real `AddDbContext` with `DbSet` properties: suggests the entity that isn't a resource, with the EF Core package present). `dotnet test --solution StellarAdmin.slnx --configuration Release --minimum-expected-tests 1`: 1500 passed, 0 failed. Browser check over CDP against AdminPlayground in Development: first undone step open, toggles set `aria-expanded`, choosing a tab selects it in every step and survives a reload, copy writes the clipboard and shows the check mark, light, dark and 390px width without overflow, no console errors. Not checked: the theme fixtures (Ops, Soft) and density modes. At phone width the Item component clamps step descriptions to two lines.

Phase 3, skills (2026-10-18): the stellar-admin skill's task table and `references/customization.md` (a Home page section: the checklist, `ConfigureHomePage`, replacing `Home/Index.cshtml` without `@model`), and the stellar-admin-setup skill's verify step (the checklist is expected in Development). `dotnet run --project util/SkillsGenerator -- --check`: no drift. Outstanding: website docs for the home page, and correct docs link targets in `Home/SetupSteps.cs` and `_SetupChecklist.cshtml`, in the batch that creates the admin panel docs.

