# Project reorganization plan

## Code audit — 2026-09-18

Current status: **superseded**. The historical project arrangement is superseded by Core, TagHelpers, Dashboard, Dashboard.Identity and Dashboard.EntityFrameworkCore in StellarAdmin.slnx. Core has been extracted again; do not replay the old merge steps.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

**Index status:** completed. **Indexed:** 2026-09-05. Historical record; completion is recorded in the phase/progress notes below. Deferred items require a separately scoped task.

Status: COMPLETE 2026-08-16 (Phases 0–4 recorded complete below).

Reorganize the two repos around the target packaging: one OSS tag-helpers package, one
consolidated `StellarAdmin.Pro` package (pro tag helpers + admin shell + resource layer),
and specialized pro packages named `StellarAdmin.Pro.*` (starting with Identity).

## Current state

| Project | Repo | Contents |
|---|---|---|
| `StellarAdmin.Core` | stellar-admin (OSS) | `AddStellarAdmin()` + `StellarAdminBuilder` (2 files, ns `StellarAdmin`) |
| `StellarAdmin.TagHelpers` | stellar-admin (OSS) | OSS tag helpers, icon packs, Client pipeline (`_content/StellarAdmin.TagHelpers`) |
| `StellarAdmin.TagHelpers.Generators` | stellar-admin (OSS) | source generators (analyzer ref) |
| `StellarAdmin.Shell` | stellar-admin (OSS) | admin shell RCL: `_Layout`, sidebar VC, `HomeController`, `AddShell()`, endpoint mapping |
| `StellarAdmin.Pro` | stellar-admin-pro | data grid tag helpers only (ns `StellarAdmin.Pro.TagHelpers`), no wwwroot |
| `StellarAdmin.Resources` | stellar-admin-pro | resource layer: `ResourceControllerBase`, builders/options, shared views, htmx + CSS Client pipeline (`_content/StellarAdmin.Resources`) |
| `StellarAdmin.Identity` | stellar-admin-pro | Users/Roles controllers + views, `AddIdentity<TUser,TRole,TKey>()`, sidebar provider |
| ShellPlayground | stellar-admin (OSS) | sandbox, refs Shell |
| ComponentPlayground | stellar-admin (OSS) | sandbox, refs TagHelpers |
| DataGridSpike, IdentitySimplePlayground | stellar-admin-pro | sandboxes |
| DocsSamples, DocsSamplesGenerator, SkillsGenerator | stellar-admin-pro | docs tooling |

Dependency chain today: `Identity -> Resources -> Shell -> TagHelpers -> Core`,
with `Resources`/`Identity` also refing `Pro` (data grid).

## Target state

| Project | Repo | Contents |
|---|---|---|
| `StellarAdmin.TagHelpers` | stellar-admin (OSS) | OSS tag helpers, with Core folded in (`AddStellarAdmin()` keeps ns `StellarAdmin`) |
| `StellarAdmin.TagHelpers.Generators` | stellar-admin (OSS) | unchanged |
| `StellarAdmin.Pro` | stellar-admin-pro | pro tag helpers (data grid) **+ admin shell + resource layer** |
| `StellarAdmin.Pro.Identity` | stellar-admin-pro | renamed from `StellarAdmin.Identity` |
| docs tooling | stellar-admin-pro | unchanged location |

Resulting dependency chain: `Pro.Identity -> Pro -> TagHelpers`.

The OSS repo becomes purely "the tag helper library" (TagHelpers + generators +
ThemeGenerator + ComponentPlayground). Everything admin-app-shaped is pro.

Note: this supersedes the earlier decision that the shell would be OSS
(`StellarAdmin.Shell` was moved OSS-side around 2026-08-01); the shell is now pro.

## Phases

Each phase ends with: full solution builds, playgrounds run, stop for review.

### Phase 0 — Fold StellarAdmin.Core into StellarAdmin.TagHelpers (OSS repo) — DONE 2026-08-16

1. ~~Move `StellarAdminBuilder.cs` + `StellarAdminExtensions.cs` into `StellarAdmin.TagHelpers`,
   keeping the `StellarAdmin` root namespace so `AddStellarAdmin()` call sites don't change.~~
2. ~~Delete the `StellarAdmin.Core` project; drop the Core project refs from
   `StellarAdmin.TagHelpers` and `StellarAdmin.Shell`; update `StellarAdmin.slnx`.~~

Done as planned (`git mv`, so history follows the files). Verified: full solution build with
0 errors; ShellPlayground serves `/admin` with `_content/StellarAdmin.TagHelpers` assets resolving.
Uncommitted — Jerrie reviews and commits.

### Phase 1 — Move the shell into StellarAdmin.Pro (cross-repo) — DONE 2026-08-16

Executed as planned, with these calls made during execution:

- **Part discovery (Jerrie's call):** the merged `StellarAdmin.Pro` keeps the shell's
  `NullApplicationPartFactory` opt-out. Data-grid-only consumers (DocsSamples, DataGridSpike)
  now call `AddShell()` to register the Pro application part — they no longer rely on automatic
  discovery. Revisit when `.AddPro()` lands.
- Area-scoped files take folder-aligned namespaces (`StellarAdmin.Pro.Areas.StellarAdmin.*`,
  the house convention in Resources/Identity); the shell API keeps its `Shell` identity as
  `StellarAdmin.Pro.Shell` / `StellarAdmin.Pro.Shell.Sidebar` under the `Shell/` folder.
- `ISiderbarItemsProvider.cs` renamed to `ISidebarItemsProvider.cs` in passing (filename typo;
  the interface was already spelled correctly).
- Verified: full solution build 0 errors; ShellPlayground `/admin` renders the shell with
  sidebar; DocsSamples `/DataGrid` renders real tables (no unprocessed `<sa-data-grid>` tags);
  IdentitySimplePlayground `/admin/Users` + `/admin/Roles` render.

Original steps:

1. Move `stellar-admin/src/StellarAdmin.Shell/*` into `stellar-admin-pro/src/StellarAdmin.Pro/`:
   - `Areas/StellarAdmin/**` (controller, sidebar VC, views, `_ViewImports`/`_ViewStart`) — Pro has no `Areas/` yet, so no merge conflicts.
   - `Sidebar/`, `StellarAdminShellBuilder`, `StellarAdminShellOptions`, `AddShell()` extensions, endpoint-mapping extensions, `AssemblyInfo.cs`.
2. Namespaces: `StellarAdmin.Shell.*` -> `StellarAdmin.Pro.Shell.*` (decision 2).
3. `StellarAdmin.Pro.csproj`: add `GenerateRazorAssemblyInfo=false` (comes with `AssemblyInfo.cs` opt-out).
4. Update refs: `StellarAdmin.Resources` and `StellarAdmin.Identity` drop the Shell project ref (they already ref Pro).
5. Move `stellar-admin/sandbox/ShellPlayground` -> `stellar-admin-pro/sandbox/ShellPlayground`; fix its project ref + `using`s.
6. Delete `StellarAdmin.Shell` from the OSS repo; update `StellarAdmin.slnx`.
7. Git: plain move (delete in OSS repo, add in pro repo). History stays in the OSS repo; the shell is ~16 young files, not worth a filter-repo graft.

### Phase 2 — Merge StellarAdmin.Resources into StellarAdmin.Pro (in-repo) — DONE 2026-08-16

Executed as planned. Layout in the merged project: resource layer under `Resources/`
(mirroring `Shell/`; namespaces `StellarAdmin.Pro.Resources[.Builders|.Controllers|
.Infrastructure.*|.Options]`), area content merged into `Areas/StellarAdmin/` with
folder-aligned namespaces, and the `sa-form-page`/`sa-index-page` tag helpers in
`TagHelpers/FormPage|IndexPage/` under `StellarAdmin.Pro.TagHelpers`. The Client pipeline,
`wwwroot/*` .gitignore, and the Rider-workaround comments moved intact; the bundle is now
`stellar-admin-pro.css` under `_content/StellarAdmin.Pro/`. Note: the stylesheet's `@source`
now also scans the shell views that merged in, so their utility classes (e.g. the sidebar's)
are included — additive only. Verified: solution build 0 errors; bundle + htmx emitted and
served from the new paths; IdentitySimplePlayground Users index/create + Roles pages render
with zero stale `StellarAdmin.Resources` references.

Original steps:

1. Move all Resources sources into `StellarAdmin.Pro`:
   - `Areas/StellarAdmin/**` merges with the shell's area from Phase 1. Only shared file: `Views/_ViewImports.cshtml` — merge directives. No other filename collisions (shell owns `_Layout`/Sidebar/Home, Resources owns the `_Index*`/`_Form*` partials and editor templates).
   - `Builders/`, `Controllers/`, `Infrastructure/`, `Options/`, `TagHelpers/` (FormPage/IndexPage tag helpers sit next to the DataGrid ones).
   - `StellarAdminBuilderExtensions` (`AddResources()`): kept as-is (decision 3); no class collision since the shell's and resources' extension classes live in their own sub-namespaces.
   - `AssemblyInfo.cs`: single merged file.
2. Move the Client pipeline (Client/ folder, `ClientItems`/`Client` targets, wwwroot outputs) into `StellarAdmin.Pro.csproj`, preserving the Rider-strips-items workaround comments.
3. Asset paths: `_content/StellarAdmin.Resources/...` -> `_content/StellarAdmin.Pro/...` in `AddResources()` (htmx script + stylesheet). Stylesheet renamed to `stellar-admin-pro.css` (decision 4) — update the Client build output name and the `ClientOutput` item.
4. Update refs: `StellarAdmin.Identity` drops the Resources ref; DocsSamples/DataGridSpike already ref Pro.
5. Delete the `StellarAdmin.Resources` project; update `StellarAdmin.slnx`.

### Phase 3 — Rename StellarAdmin.Identity to StellarAdmin.Pro.Identity — DONE 2026-08-16

Executed as planned (`git mv` for directory + csproj, `StellarAdmin.Identity.*` ->
`StellarAdmin.Pro.Identity.*` throughout, IdentitySimplePlayground ref + using, slnx).
No collisions with `Microsoft.AspNetCore.Identity` — the namespace already ended in
`Identity` before the rename. Verified: solution build 0 errors; IdentitySimplePlayground
Users/Roles/shell home all 200. Only remaining old-name mentions are the two
`docs/design/identity-*.md` docs — Phase 4's sweep.

Original steps:

1. Rename directory + csproj: `src/StellarAdmin.Identity` -> `src/StellarAdmin.Pro.Identity`.
2. Namespaces `StellarAdmin.Identity.*` -> `StellarAdmin.Pro.Identity.*` (watch collisions with `Microsoft.AspNetCore.Identity` usings in views/controllers).
3. No wwwroot / `_content` references to update (Identity has no static assets).
4. Update IdentitySimplePlayground ref + `using`s; update `StellarAdmin.slnx`.

### Phase 4 — Sweep — DONE 2026-08-16 (plan complete)

Executed, with two finds beyond the planned list:

- The **OSS repo's own `StellarAdmin.slnx` and CI were already broken** — the slnx still
  listed `StellarAdmin.Core`/`StellarAdmin.UI`/`StellarAdmin.UI.Generators` and the
  long-moved `docs/` projects, and `ci.yml` ran the SkillsGenerator check against a
  project now in the pro repo. Both fixed (slnx rewritten to the real projects; the
  SkillsGenerator step removed — that check belongs to the pro repo's tooling now);
  the OSS slnx builds clean.
- The OSS `CLAUDE.md` was still pre-rename throughout (StellarAdmin.UI naming, old
  bundle names, DocsSamples paths); updated to current reality, including the VRT
  `--pages` flag. Workspace `CLAUDE.md` table updated (pro repo real, shell paid,
  Core folded). The pro repo still has no `CLAUDE.md` — noted as such, worth creating
  separately.
- Design docs (`identity-configuration.md`, `identity-user-forms.md`) renamed to
  `StellarAdmin.Pro.Identity` / `StellarAdmin.Pro.Resources` with a historical note
  where the old assembly is mentioned; historical `.claude/plans/*` left untouched.
- Memory notes updated (project scope, oss-paid-split, pro-taghelpers, resources
  extraction marked superseded, this plan marked complete).

Original steps:

1. `StellarAdmin.slnx` final pass (folder groupings still make sense: src/pro split).
2. Both repos' `CLAUDE.md` + workspace `CLAUDE.md` (project tables, tier description — shell is now pro).
3. `docs/design/*.md` in the pro repo that name `StellarAdmin.Resources` / `StellarAdmin.Identity` — update the live ones, leave historical plans as-is with a superseded note where confusing.
4. Auto-memory updates (oss-paid-split, pro-taghelpers-project, resources-project-extraction-plan).
5. Full build + run IdentitySimplePlayground + ShellPlayground + DocsSamples smoke check; VRT if views were touched.

## Decisions (Jerrie, 2026-08-16)

1. **`StellarAdmin.Core` folds into `StellarAdmin.TagHelpers`** (Phase 0). The
   `AddStellarAdmin()`/`StellarAdminBuilder` entry point keeps the `StellarAdmin` root namespace.
2. **Namespaces align with the assembly**: `StellarAdmin.Pro.Shell`, `StellarAdmin.Pro.Resources`,
   `StellarAdmin.Pro.TagHelpers` (unchanged), `StellarAdmin.Pro.Identity.*`.
3. **DI surface stays as-is during unification** (`AddShell()` + `AddResources()`); the end goal
   is a single `.AddPro()` — see follow-up work below.
4. **Stylesheet renamed** to `stellar-admin-pro.css`.

## Follow-up work (after unification, separate effort)

- Collapse the pro DI surface into a single `.AddPro()`, removing `AddShell()` (and
  reconsidering `AddResources()`) as public entry points. This is a deliberate
  options-builder design change — design it against docs/conventions/options-builders.md,
  don't fold it into the file moves.
- Retire ShellPlayground along with `AddShell()`; it exists to exercise the standalone
  shell entry point, which goes away. Keep it running during the unification phases.

## Out of scope

- Any DI/options API redesign beyond what the merge forces.
- NuGet packaging metadata (package IDs will be `StellarAdmin.TagHelpers`, `StellarAdmin.Pro`, `StellarAdmin.Pro.Identity` when packaging is set up).
- The prior-generation sibling repos (`stellar-admin-core`, `stellar-admin-efcore`).
