# Plan: extract the generic resource layer into StellarAdmin.Resources

**Index status:** completed. **Indexed:** 2026-09-05. Historical record; completion is recorded in the phase/progress notes below. Deferred items require a separately scoped task.

## Progress

**PLAN COMPLETE 2026-08-14.** Code commits landed on `master` in `stellar-admin-pro`;
the `.slnx` and plan changes landed in the workspace repo.

| Phase | State | Commit |
|---|---|---|
| 0 — Settle the design decisions | done 2026-08-14 | (plan doc only) |
| 1 — Project skeleton and solution wiring | done | `8a572e5` (pro), `27f6904` (workspace) |
| 2 — Move the non-DI C# core | done | `bd0d33d` |
| 3 — Move the views, editor templates and client pipeline | done | `2cf0214` |
| 4 — Public `AddResources()` entry point | done | `6979226` |
| 5 — Cleanup, docs and plan close-out | done | `ae97e1a` |

## Context

`StellarAdmin.Identity` already separates its generic resource layer from its
Identity-specific code — the earlier `identity-resource-layer.md` plan did that split
*within* the project (`Options/Base/`, `Builders/Base/`, `Resource*` view models,
`Views/Shared/`, `Infrastructure/`). The generic layer's only remaining tie to Identity
is namespaces (`StellarAdmin.Identity.*`); no generic file references
`Microsoft.AspNetCore.Identity`.

This plan moves that generic layer into a new **`StellarAdmin.Resources`** project in
`stellar-admin-pro/src/`, so future paid packages (and eventually consumers) can build
CRUD screens for any entity type on the same controllers-base, views, view models,
editor templates, options and builders. `StellarAdmin.Identity` becomes a consumer of
`StellarAdmin.Resources`.

Reference state at planning time: pro repo at `95f0f86` ("Renames Pro project").

## Non-goals (explicitly out of scope)

- **No data-access abstraction or resource-registration DSL.** Consumers build a
  resource by deriving from `ResourceControllerBase<TEntity>` with their own data
  access, exactly like `UsersController` does with `UserManager`; their controllers are
  found by MVC's normal discovery (D2). An EF Core-backed generic controller or a
  fluent `AddResource<TEntity>(...)` registration API is a separate, later effort,
  informed by using the layer ourselves first.
- **No behavior changes.** After every phase the playground must behave exactly as
  before. `sandbox/IdentitySimplePlayground/Program.cs` must not change.
- **No new docs/DocsSamples pages for Resources**, no NuGet/packaging work.
- **No renames beyond dropping the `Identity` prefix** where a moved type carries it.

## Target project shape

`stellar-admin-pro/src/StellarAdmin.Resources` — Razor class library, same csproj shape
as StellarAdmin.Identity (Razor SDK, `AddRazorSupportForMvc`,
`GenerateRazorAssemblyInfo=false`, `AssemblyInfo.cs` opting out of automatic
application-part discovery). References: `StellarAdmin.Shell`,
`StellarAdmin.TagHelpers` (both OSS repo), `StellarAdmin.Pro` (DataGrid tag helpers used
by `_IndexDataGrid`). `StellarAdmin.Identity` then references `StellarAdmin.Resources`.

Namespaces: the `Base` folder split dissolves — the new project doesn't need it.

```
StellarAdmin.Resources/
  Areas/StellarAdmin/
    ViewModels/... (+ Internal/)                 StellarAdmin.Resources.Areas.StellarAdmin.ViewModels
    Views/Shared/... (+ EditorTemplates/)
    Views/_ViewImports.cshtml
    FormFieldsBaseView.cs, TempDataKeys.cs, ViewDataKeys.cs
  Builders/                                      StellarAdmin.Resources.Builders
  Controllers/ResourceControllerBase.cs          StellarAdmin.Resources.Controllers
  Options/                                       StellarAdmin.Resources.Options
  Infrastructure/{Expressions,Query}/            StellarAdmin.Resources.Infrastructure.*
  TagHelpers/{FormPage,IndexPage}TagHelper.cs    StellarAdmin.Resources.TagHelpers
  StellarAdminBuilderExtensions.cs               AddResources() — StellarAdmin.Resources (Phase 4)
  Client/  (css + copy-js pipeline)
  wwwroot/ (htmx.min.js, stellar-admin-resources.css — build outputs)
```

Per D2/D3 there is **no** `Infrastructure/Mvc/` and **no** `Sidebar/` in Resources —
that machinery stays in Identity. `ResourceControllerBase` and the tag helpers sit at
the project root, not under `Areas/` — only views are path-bound (Jerrie, Phase 2
review); `TagHelpers/` matches the sibling `StellarAdmin.TagHelpers`/`StellarAdmin.Pro`
layout. The `AssemblyInfo.cs` part-discovery opt-out **stays** even though the assembly
has no routable controllers: parts also feed tag-helper discovery and (from Phase 3)
the compiled views, and referencing the package must remain inert until
`AddResources()`/`AddIdentity()` is called (settled with Jerrie, Phase 2 review).

Views **must stay at the exact paths** `Areas/StellarAdmin/Views/...` — host-app view
overriding relies on exact-path area shadowing (the Identity UI pattern), and partial /
editor-template lookup relies on the area-relative `Views/Shared` location.

## File-by-file disposition

Everything not listed moves nowhere. Paths relative to `src/StellarAdmin.Identity/`.

### Moves to StellarAdmin.Resources (namespace change only unless noted)

| Current file(s) | Notes |
|---|---|
| `Options/Base/*.cs` **except `SidebarItemOptions.cs`** (14 files) | → `Options/`, namespace `StellarAdmin.Resources.Options` |
| `Builders/Base/*.cs` **except `SidebarItemBuilder.cs`** (11 files) | → `Builders/` |
| `Infrastructure/Expressions/FieldExpressionHelper.cs` | as-is |
| `Infrastructure/Query/*.cs` (6 files) | as-is |
| `Areas/StellarAdmin/Controllers/ResourceControllerBase.cs` | as-is |
| `Areas/StellarAdmin/FormFieldsBaseView.cs`, `TempDataKeys.cs`, `ViewDataKeys.cs` | as-is (public today — keep public, they are consumer hatches) |
| `Areas/StellarAdmin/TagHelpers/{FormPage,IndexPage}TagHelper.cs` | `@addTagHelper` source assembly changes to `StellarAdmin.Resources` |
| `Areas/StellarAdmin/ViewModels/`: `Resource*` (7 files), `PagedListViewModel.cs`, `Internal/IPagedListViewModel.cs`, `Internal/IResourceIndexPageViewModel.cs` | as-is |
| `Areas/StellarAdmin/Views/Shared/*.cshtml` (6 partials) | as-is; delete from Identity in the same commit — two RCLs must never both compile the same view path |
| `Areas/StellarAdmin/Views/Shared/EditorTemplates/{String,Password,PhoneNumber}.cshtml` | generic (keyed by type / DataType hint), move |
| `Client/` (package.json, client.css, copy-js.mjs) | package name → `stellar-admin-resources`; CSS output → `stellar-admin-resources.css`. The `@source` glob and the `theme-tokens.css` import keep the same relative depth because the project sits at the same level under `src/` |
| `wwwroot/` outputs + the csproj `Client`/`ClientItems` targets | move verbatim (keep the items-inside-target pattern — Rider strips evaluation-time items) |

### Stays in StellarAdmin.Identity

- `Areas/StellarAdmin/Controllers/{Users,Roles}Controller.cs`
- `Areas/StellarAdmin/ViewModels/{UserCreateViewModel,CreateUserPasswordInput}.cs`
- `Areas/StellarAdmin/Views/{Users,Roles}/*.cshtml` (thin, tag-helper-only — verified:
  no raw utility classes, so Identity needs **no** Client pipeline afterwards)
- `Options/{StellarAdminIdentityOptions,IdentityUsersOptions,IdentityRolesOptions,IdentitySidebarOptions}.cs`
- `Options/Base/SidebarItemOptions.cs` and `Builders/Base/SidebarItemBuilder.cs` —
  sidebar vocabulary stays with the sidebar mechanism (D3); they move up to `Options/`
  and `Builders/` inside Identity as the `Base` folders dissolve
- `Builders/{StellarAdminIdentityBuilder,IdentitySidebarBuilder}.cs`
- `Infrastructure/Mvc/{IdentityControllerFeatureProvider,IdentityControllerNameConvention}.cs`
  — unrenamed, unmoved (D2): this machinery exists only because Identity closes *open
  generic* controllers over the host's TUser/TRole/TKey. Consumers write ordinary
  controllers deriving from `ResourceControllerBase<TEntity>`, which MVC's default
  discovery registers with sane names — they need none of this
- `Infrastructure/IdentityResourceRegistration.cs` and `Sidebar/SidebarItemsProvider.cs`
  (D3)
- `StellarAdminBuilderExtensions.cs` (`AddIdentity` overloads) with its private helpers
  `AddControllerType`, `AddResource`, `GetOrAddOptions`. Only the application-part and
  shell-asset registration for the *Resources* assembly leaves it, into the public
  `AddResources()` (D1/D4), which `AddIdentityCore` calls
- `AssemblyInfo.cs` (Identity still has controllers + compiled views of its own)
- Its own `Areas/StellarAdmin/Views/_ViewImports.cshtml`, updated to
  `@using StellarAdmin.Resources...` + `@addTagHelper *, StellarAdmin.Resources`

## Design decisions (settled with Jerrie, 2026-08-14)

**D1 — Public API, no `InternalsVisibleTo`.** The whole point of the extraction is that
developers use Resources to rapidly build CRUD screens for their own resources, so the
surface must be public. Concretely for this plan: the already-public page layer
(options, builders, view models, `ResourceControllerBase`, base view, keys, tag
helpers, views) moves as-is, and Resources gains one new public entry point,
`StellarAdminBuilder.AddResources()` (Phase 4), which registers the Resources
assembly's application parts and its shell assets. It must be idempotent (Identity and
the host may both call it) — reuse the existing part-presence guard pattern, and check
how the OSS Shell's `AddScript`/`AddStylesheet` behave on repeated calls before relying
on them. Consult `docs/conventions/options-builders.md` before shaping it; a
`Action<...>` configure overload is *not* added yet since there is nothing to
configure. No `InternalsVisibleTo` anywhere.

**D2 — Controller registration machinery stays in Identity.** A consumer using
Resources gets the default MVC behaviour: they write ordinary controllers deriving from
`ResourceControllerBase<TEntity>` and MVC's normal discovery registers them.
`IdentityControllerFeatureProvider` and `IdentityControllerNameConvention` exist only
for Identity's open-generic controllers and remain in Identity, unrenamed. Revisit only
when building on Resources ourselves surfaces a real need.

**D3 — Sidebar mechanism stays in Identity.** `IdentityResourceRegistration`,
`SidebarItemsProvider`, `SidebarItemOptions` and `SidebarItemBuilder` all remain in
Identity (the latter two move out of the dissolving `Base` folders but keep their
`StellarAdmin.Identity.*` namespaces). Resources ships nothing sidebar-related. Adapt
later if needed.

**D4 — Static assets move to Resources.** htmx + CSS ship from
`_content/StellarAdmin.Resources/htmx.min.js` and `.../stellar-admin-resources.css`;
the `AddShell(...AddScript/AddStylesheet)` calls end up inside `AddResources()`;
Identity ends with no `Client/`, no `wwwroot/`. Verified safe: Identity's remaining
views contain no raw utility classes (all styling comes from tag helpers + the shared
partials, whose cshtml moves with the Tailwind `@source` scan). Gotcha to carry over:
if a later Identity view adds a raw utility class, Identity needs its own bundle again
— note this in the Identity csproj or CLAUDE.md.

## Phases

Each phase ends at a buildable, playground-verifiable state and stops for Jerrie's
review. Jerrie commits; do not run `git commit`.

### Phase 0 — Settle the design decisions ✓

Done 2026-08-14; outcomes recorded under "Design decisions" above. Project name
`StellarAdmin.Resources` stands (Jerrie's original naming).

### Phase 1 — Project skeleton and solution wiring

1. Create `stellar-admin-pro/src/StellarAdmin.Resources/`: csproj (copy the Identity
   csproj shape minus the Client targets for now), `AssemblyInfo.cs` (part-discovery
   opt-out), `.csproj.DotSettings` if needed (no `Base` folders, so probably none).
2. Reference it from `StellarAdmin.Identity.csproj`. Resources references Shell,
   TagHelpers, StellarAdmin.Pro.
3. Add the project to the workspace `StellarAdmin.slnx` (workspace-repo change).
4. Verify: `dotnet build -p:AllowMissingPrunePackageData=true` from the workspace.

### Phase 2 — Move the non-DI C# core

Everything that is plain C# with no service-registration implications, in one sweep
(the namespaces are interdependent, so a partial move won't compile):

1. Move `Options/Base/*` → `Options/` and `Builders/Base/*` → `Builders/` **except**
   `SidebarItemOptions`/`SidebarItemBuilder` (those move up to Identity's own
   `Options/`/`Builders/`, namespaces unchanged, per D3). Move
   `Infrastructure/Expressions/`, `Infrastructure/Query/`, the `Resource*`/paged view
   models + `Internal/` interfaces, `TempDataKeys`, `ViewDataKeys`,
   `FormFieldsBaseView`, `ResourceControllerBase`, and the two tag helpers.
   Namespace-only edits (`StellarAdmin.Identity.X` → `StellarAdmin.Resources.X`).
2. Sweep Identity's remaining `.cs` files' `using` directives; update both
   `_ViewImports.cshtml` files (`@using` + `@addTagHelper *, StellarAdmin.Resources`);
   create Resources' own `Areas/StellarAdmin/Views/_ViewImports.cshtml` (needed once
   views arrive in Phase 3; harmless earlier).
3. In `AddIdentityCore`, temporarily register the Resources assembly's `AssemblyPart` +
   `CompiledRazorAssemblyPart` (tag-helper discovery needs the part; Phase 4 relocates
   this into the public `AddResources()`).
4. Verify: build; run the playground (port 5206, `ASPNETCORE_ENVIRONMENT=Development`);
   click through Users + Roles index/create/edit/delete. Restore
   `IdentitySimplePlayground/app.db` after form POSTs. Stop the server when done.

### Phase 3 — Move the views, editor templates and client pipeline

1. Move `Views/Shared/*` (6 partials) and `Views/Shared/EditorTemplates/*` (3 files) to
   Resources at identical area paths; delete the Identity copies in the same change.
2. Move `Client/` (rename npm package `stellar-admin-resources`, CSS output
   `stellar-admin-resources.css`) and the csproj `Client`/`ClientItems` targets; delete
   Identity's `Client/`, `wwwroot/` and client targets; drop Identity's now-unneeded
   csproj folder items.
3. Point Identity's `AddShell` asset registrations at
   `_content/StellarAdmin.Resources/...` (they migrate into `AddResources()` in
   Phase 4). Grep the whole pro repo + playground for `_content/StellarAdmin.Identity`
   — must come back empty.
4. Verify: clean build (watch that `npm run build` actually ran in the new location);
   grep a known utility class into `wwwroot/stellar-admin-resources.css` (`dotnet build`
   can succeed while `build:css` fails); playground click-through with special attention
   to: page styling (a 500/unstyled page means static assets broke — remember
   `ASPNETCORE_ENVIRONMENT=Development` when using `--no-launch-profile`), the
   Password/PhoneNumber/String editor templates on Users create/edit (cross-assembly
   editor-template lookup is the riskiest bit), the htmx delete dialogs on index and
   edit pages, and TempData error alerts (delete the signed-in user to trigger one).
   Restore `app.db`; stop the server.

### Phase 4 — Public `AddResources()` entry point

The one new piece of public API this plan adds (D1). Consult
`docs/conventions/options-builders.md` before writing it; XML docs per
`docs/conventions/xml-documentation.md` (consumer vocabulary, no internals).

1. Add `StellarAdminBuilderExtensions.AddResources()` in namespace
   `StellarAdmin.Resources` (root of the new project, mirroring Identity's file):
   registers the Resources `AssemblyPart` + `CompiledRazorAssemblyPart` (guarded by the
   existing part-presence check pattern) and the two shell assets. Verify repeated
   calls don't double-register the assets (check the Shell builder's `AddScript`/
   `AddStylesheet` semantics; add a guard if they append blindly).
2. Rewrite `AddIdentityCore` to call `builder.AddResources()` and delete the temporary
   part registration from Phase 2 and the asset calls from Phase 3. Identity's own
   assembly-part registration, feature provider, name convention, options wiring and
   resource/sidebar registrations stay exactly as they are.
3. Verify: build + full playground click-through; also confirm a double `AddIdentity`
   call in a scratch copy of Program.cs doesn't duplicate sidebar items or scripts
   (revert the scratch change; `Program.cs` must not change).

### Phase 5 — Cleanup, docs and plan close-out

1. Remove the now-empty `Base` `NamespaceFoldersToSkip` entries from
   `StellarAdmin.Identity.csproj.DotSettings`; add equivalents to Resources only if any
   folder needs skipping (target shape says none).
2. XML-doc pass over moved public types: comments must not mention Identity as if it
   were the host (most are already generic — spot-check `ResourceOptions`,
   `ResourceControllerBase`, tag helpers). Internal types stay uncommented.
3. Update `stellar-admin-pro/docs/design/identity-configuration.md` where it names
   types/namespaces that moved; check `identity-user-forms.md` likewise.
4. Update the workspace `CLAUDE.md` sub-repo table only if it enumerates projects (it
   doesn't today — skip unless changed meanwhile). Update auto-memory
   (`pro-taghelpers-project`, `identity-resource-layer-plan` pointers) to mention
   StellarAdmin.Resources.
5. Mark this plan complete.

## Risks and gotchas (carry-overs from memory + new)

- **Rider silent file revert** hit `Internal/UserViewModels.cs` twice before. The file
  no longer exists, but during the ViewModels move, diff against `HEAD` before
  re-doing any "missing" work, and warn Jerrie after editing files in that folder.
- **Rider strips csproj items** whose referenced files are deleted — keep the
  `ClientItems`-inside-a-target pattern verbatim in the Resources csproj.
- **Duplicate view paths across RCLs**: while both projects compile, never let the same
  `Areas/...` view path exist in both — move and delete in the same commit.
- **Cross-assembly editor-template/partial lookup** is expected to work via the
  `CompiledRazorAssemblyPart` (same mechanism as today, just a second assembly), but it
  is the least-proven piece — Phase 4's verification targets it explicitly.
- **Build flags/env**: `-p:AllowMissingPrunePackageData=true` for `dotnet build`;
  `DOTNET_SYSTEM_NET_DISABLEIPV6=1` if anything downloads; playground on **5206** only
  (5205 is Jerrie's).
- **Tailwind up-to-date check**: the `Touch` on `@(ClientOutput)` must survive the move,
  or the Client target re-runs every build (or never runs).
- The `sa-slot-content`/`sa-slot-outlet` slot helpers live in the OSS layer
  (`StellarAdmin.TagHelpers`) — nothing to move there; listed to avoid a false lead.

## Resuming on another computer

Read `## Progress` and the phase you're in; each phase lists its own verification. The
plan file is tracked by the **workspace** repo — commit it (Jerrie does) so it travels.
Check `git -C stellar-admin-pro log --oneline -5` against the Progress table to see
whether a phase's commit landed. If the working tree has uncommitted changes, diff them
against the phase's step list before continuing.
