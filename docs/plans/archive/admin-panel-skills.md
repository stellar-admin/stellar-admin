# Admin panel consumer skills

Status: completed (2026-10-14); Opus rerun with the skills on 2026-10-15.

## Summary

The admin panel has no installable consumer skill. `skills/stellar-admin/` holds a 595-line `references/setup.md`, written as a behavioural spec (overload return types, option composition, fallback chains), and three generated component stubs for `sa-index-page`, `sa-form-page` and `sa-view-page`. It has no `SKILL.md`, no installation steps, no recipes and no common-mistakes guidance. The EF Core path, the one most applications will use, comes last. `MultiLookupSheetEditor` is not mentioned. The website has no admin panel docs, so these notes are currently the only documentation.

This plan replaces those notes with two installable skills, organised by task, with one reference file per topic. Content is written from the current code and checked against evaluation tasks run before and after writing.

## Research

The research, from 2026-10-14, compared first-party agent guidance for other admin frameworks:

- Filament, react-admin and Payload each ship one domain skill. Filament's (`filament-development`, 251 lines) is short recipes plus a "Common mistakes" list of Never/Use rules aimed at APIs models invent. Payload's opens with a task → API → reference table over 11 topic reference files.
- Avo ships about 45 skills, largely following its separately installed paid add-ons.
- Claude Code keeps every installed skill's description in context on every turn, within a listing budget of about 1% of the context window, and drops the least-used descriptions when it overflows. Many small skills from one product compete for that budget.
- Anthropic's skill guidance: a `SKILL.md` under 500 lines; focused reference files one level deep, with a table of contents over 100 lines; checklists for multi-step work; examples over prose; evaluation tasks before content.

Common admin panel topics across Filament, Nova, Avo, ActiveAdmin and react-admin: resources, index tables (columns, sorting, search, filters, scopes, row actions), actions and bulk actions, forms (fields, layout, validation), relationships, detail pages, navigation, authorization, dashboards and widgets, global search, notifications, theming, custom pages, testing and upgrades.

## Decisions

1. Structure: a core `stellar-admin` skill for day-to-day building, a separate `stellar-admin-setup` skill for installation, with topic references inside the core skill. Settled 2026-10-14.
2. Write evaluation tasks first and run them without the skill to find the actual failures before writing content. Settled 2026-10-14.
3. Rewrite rather than edit `references/setup.md`: the new references are written from current source, and the old file is removed once its content is covered or deliberately dropped. Settled 2026-10-14.
4. Write the editor and display tables by hand for now; generating them from the C# classes through `util/SkillsGenerator` can follow later if they drift. Settled 2026-10-14.
5. Drop the three generated admin component stubs (`form-page.md`, `index-page.md`, `view-page.md`) and cover `sa-index-page`, `sa-form-page` and `sa-view-page` in `customization.md`. Settled 2026-10-14; the generator's admin output must stop producing them.
6. The website's future admin panel docs stay separate from the skill. Settled 2026-10-14.
7. The product gaps are recorded as one backlog record, [admin-panel-backlog](../admin-panel-backlog.md), not as individual plans. Settled 2026-10-14.

## Target layout

```
skills/
  stellar-admin-setup/
    SKILL.md                  packages, AddStellarAdmin, MapStellarAdmin and middleware order,
                              _ViewImports, first EF resource, verifying it renders, theme link
    LICENSE
  stellar-admin/
    SKILL.md
    LICENSE
    references/
      resources.md
      index-pages.md
      columns.md
      forms.md
      fields.md
      editors.md
      relationships.md
      view-pages.md
      sidebar.md
      authorization.md
      customization.md
```

`stellar-admin/SKILL.md` contains, under 500 lines:

- Two lines of framing, the version compatibility note used by the UI skill, and the boundary with `stellar-admin-ui` and `stellar-admin-theme`.
- A quick-reference table: task → API → reference file.
- One end-to-end EF Core example: a resource with columns, search, a create/edit form with a foreign-key editor, and a sidebar group.
- An "add a resource" checklist.
- Namespaces and packages (`StellarAdmin`, `StellarAdmin.EntityFrameworkCore`, `StellarAdmin.Resources.Editors`, `StellarAdmin.Resources.Displays`, `StellarAdmin.Sidebar`).
- Common mistakes, from the evaluation runs.
- Unsupported features, so agents say so instead of inventing APIs.

Reference files:

| File | Covers |
| --- | --- |
| `resources.md` | `AddEfCoreResource` (default) vs `AddResource` with a data source, slugs, labels, keys, `Allow*` actions, delete, custom data source interfaces and `ResourceOperationResult` |
| `index-pages.md` | Title, paging, search, scopes, default sort, row details; EF predicates vs data-source interpretation |
| `columns.md` | Adding columns, titles, empty text, sorting and sort selectors, displays in cells, reference paths, cell templates |
| `forms.md` | Create and edit pages, form layout (columns, spans, starts, sections, groups, section layouts), validation, factories, custom create/edit models and handlers |
| `fields.md` | Editable fields vs display fields, `AddDisplayField`, `AddDetails`, read-only through metadata, built-in displays and custom displays |
| `editors.md` | Which editor to choose for which property (picker table), settings for each, choice items, empty choice, media, custom editors |
| `relationships.md` | Foreign keys with `SelectEditor` and EF `UseItems`, `LookupSheetEditor`, `MultiLookupSheetEditor`, create-in-sheet, reference paths in columns and display fields |
| `view-pages.md` | `AllowView`, layout, header actions, custom view models |
| `sidebar.md` | Resource items (label, group, order, visibility), custom links, group merging and ordering, host pages in the admin shell |
| `authorization.md` | Panel, resource and link requirements, `MapStellarAdmin` conventions, what hiding does and does not protect |
| `customization.md` | View overrides and fallback names, `sa-index-page`, `sa-form-page` and `sa-view-page` with their slots, global labels, editor and display template overrides |

Writing rules: one sentence of intent, a snippet, then the option table; behavioural detail only where an agent would otherwise get it wrong. EF Core first, custom data sources second. Snippets use the Voyager Travel theme and must compile against current source. Link to `stellar-admin-ui` for component markup instead of duplicating it.

## Unsupported features

The skill must state these as unavailable. Source of the list: the API survey of 2026-10-14; the product side is tracked in [admin-panel-backlog](../admin-panel-backlog.md).

- Bulk actions and custom row or page actions (row actions are view, edit and delete only).
- Filters beyond scopes.
- Sidebar icons for resources and links.
- Form tabs and wizards.
- Dashboards and widgets (the home page is a bare landing page).
- File and image upload editors.
- A fluent read-only field option (`[ReadOnly]` / `[Editable(false)]` metadata only).
- Before/after save hooks.
- Search and scope predicates for non-EF data sources (the data source interprets `Search` and `Scope`).
- Displays with handlers in index columns (they throw; see [display-defaults](../display-defaults.md)).
- Localization through `IStringLocalizer` (labels are configured through `ConfigureResourceLabels`).

## Phases

### Phase 1: evaluation tasks and baseline

The tasks, grading checks and fixtures live in [`build/skill-evals/admin-panel/`](../../../build/skill-evals/admin-panel/README.md), outside the shipped skill. Five tasks:

1. Install the admin panel into a fresh ASP.NET Core app with EF Core and expose one entity.
2. Add a Bookings resource: index with customer and status columns (status as a badge), search on reference, a scope per status, default sort by date.
3. Add create and edit forms for Bookings: two sections in a two-column layout, a customer lookup with create-in-sheet, an enum as a radio card group, a read-only reference display field.
4. Group Bookings and Customers under "Sales" in the sidebar, add a link to a host Razor Page rendered in the admin shell, and restrict Bookings to a "Sales" role.
5. Add a view page for Bookings and override the index view to add content above the grid.

Instead of a copy of AdminPlayground, each task runs in Voyager, a small Razor Pages app that consumes StellarAdmin as NuGet packages packed from this checkout into a local feed, so agents see only what an application developer would. `prepare.sh` creates a run directory outside the repository. Run each task with a fresh agent that has no admin skill and no access to this repository. Record the failures: invented APIs, wrong namespaces, wrong ordering, missed configuration. Report them at the checkpoint.

### Phase 2: write the skills

Create both skills with their licenses and the references above, written from current source. Seed "Common mistakes" from the phase 1 failures. Move what is still accurate and useful from `setup.md` into the matching reference, then remove `setup.md`. Remove the admin component stubs and their generator output (decision 5). Update the incoming references: `skills/README.md` (status, install commands), `docs/repos/skills.md` (product boundaries and generated ownership) and the plan index.

### Phase 3: verify

- Rerun the phase 1 tasks with the skills installed and compare against the baseline; fix the skills where tasks still fail.
- Build every snippet's API against current source (compile check or a scratch project).
- Run the bundle validation (`npm test` and `npm run check` in `build/skill-validation`) and `dotnet run --project util/SkillsGenerator -- --check`.
- Check `SKILL.md` line counts and description lengths.

Record the evaluation results and checks performed in this plan.

## Verification

### Phase 1

Baseline runs on 2026-10-14. Tasks, fixtures, `prepare.sh` and `probe.sh` written. Fresh general-purpose agents with no admin skill, told not to read this repository, ran one task each in run directories prepared outside the repository, on Opus 5.5, Sonnet 5.5 and Haiku 4.5. Sonnet and Haiku runs were graded by building, running and probing each app with `probe.sh`; Opus runs were graded from the agents' reports, with task 2 also probed. The agents' self-reports proved unreliable on the weaker models: two Haiku and one Sonnet run reported success for work that did not render or did not exist.

Checks passed (partial = 0.5), out of 6, 8, 7, 7 and 5:

| Task | Opus | Sonnet | Haiku |
| --- | --- | --- | --- |
| 1 Install | 6 | 6 | 4.5: customer field is a number input; forms not submitted |
| 2 Index | 8 | 8 | 4: no badge, no search, scope tabs that don't filter (predicate text passed as the scope id) |
| 3 Forms | 7 | 7 | 0: hand-written Razor Pages outside the admin panel, reported as overrides |
| 4 Sidebar and auth | 7 | 6: departures page styled with Tailwind utilities that the shipped CSS doesn't contain | 5.5: same styling problem; page as a controller added to the admin area with `AddController` |
| 5 View and override | 5 | 1.5: no area `_ViewImports`, so `<sa-index-page>` rendered as literal text and the list vanished; used the `.Internal` view model interface | 2: summary placed inside every row's details panel |
| Total (of 33) | 33 | 28.5 | 16 |

| Model | Tool calls per task | Time per task |
| --- | --- | --- |
| Opus | 18–37 | 2.6–4.7 min |
| Sonnet | 10–33 | 1.1–3.9 min |
| Haiku | 55–154 | 5.0–11.8 min |

Opus succeeds without the skill, so with the skill it is measured on tool calls and time. Sonnet and Haiku are measured on checks passed.

Knowledge the agents had to decompile, guess or discover by failure (seed for "Common mistakes" and the references):

- View overrides: `Areas/StellarAdmin/Views/{Resource}/Index.cshtml` (or `Create`, `Edit`, `View`) replaces a page, with a `ResourceIndexPageViewModel<T>` model and `<sa-index-page model="Model" />`; the app needs its own `Areas/StellarAdmin/Views/_ViewImports.cshtml`. Not in the XML docs.
- Index cells render `GridDisplayTemplates/{TemplateName}`, not `DisplayTemplates/`.
- EF Core resources load references on column and display field paths automatically.
- Generic argument order of `AddEfCoreResource<TContext, TEntity>` and EF `UseItems<TContext, TEntity, TValue>` was guessed.
- `SectionLayout` takes `FormSectionLayout` from `StellarAdmin.UI`; section builders' `Add`, `AddDisplayField` and `Columns` were found by trial.
- Enum attribute values must be fully qualified in Razor (`PageContainerWidth.Full`); the UI skill's conventions already cover this.
- EF search with `Contains` is case-sensitive on SQLite.
- New `DateOnly` fields default to 0001-01-01 on create; use `create.UseFactory`.
- Form and view labels showed raw property names ("DepartureDate") while index headers read "Departure Date"; agents added `[Display(Name)]` or `Title`. To check whether this is a product inconsistency.
- The create sheet posts with a `Sheet.` field prefix (only relevant to tests that post forms directly).
- Overrides and host pages need the app's own `Areas/StellarAdmin/Views/_ViewImports.cshtml` with `@addTagHelper *, StellarAdmin` and `*, StellarAdmin.UI`; without it `<sa-index-page>` renders as literal text, silently.
- Use the public `ResourceIndexPageViewModel<T>`, not `IResourceIndexPageViewModel` from `.Internal`.
- Use `sa-*` components in admin and host pages; the shipped CSS has almost no Tailwind utilities, so utility-styled markup renders unstyled.
- Create and edit pages come from `AllowCreate` and `AllowEdit` with fields and editors, never hand-written pages.
- A foreign key such as `CustomerId` needs `SelectEditor` or `LookupSheetEditor` with EF `UseItems`, or it renders as a number input.
- EF `EnableSearch` requires a predicate factory, and EF scopes take the predicate as the third argument of `Add(id, title, predicate)`; the id is only a URL value.
- `Allow*` calls don't chain (each returns its page builder); `AllowView` without fields fails every admin request with a 500; `ChoiceEditor` is abstract.
- Host pages in the admin shell use `Layout = StellarAdminLayouts.Default` and their own `[Authorize]`, rather than controllers added to the admin area.

Product findings, added to [admin-panel-backlog](../admin-panel-backlog.md): the sidebar view hard-codes "Voyager Travel / Admin Console" and a placeholder user, "Amelia Hart", with no setting to change them; sidebar groups cannot be ordered; the index page has no slot between its header and the grid.
### Phase 2

Written on 2026-10-14 from current source (`src/StellarAdmin`, `src/StellarAdmin.EntityFrameworkCore`), the removed `setup.md` and the working Opus baseline solutions.

- `skills/stellar-admin-setup/`: `SKILL.md` (checklist, packages, registration, routes, protection, theme, verification) and `references/troubleshooting.md`. The bundle validator requires a `references/` folder in every skill, so the troubleshooting table moved there.
- `skills/stellar-admin/`: `SKILL.md` (task table, one complete EF Core resource, checklist, namespaces, common mistakes from phase 1, unsupported features) and the eleven references in the target layout.
- Removed `references/setup.md`, `references/components-index.md` and the three admin component stubs; `util/SkillsGenerator` no longer generates the StellarAdmin product, and its now-unused package header was removed from `Renderer`.
- Skill descriptions in all four `SKILL.md` files are single-line strings (they were folded YAML blocks).
- Updated `skills/README.md` (status, install commands), `docs/repos/skills.md` (boundaries, ownership) and the `port-shadcn-component` development skill, which mentioned the generated admin stubs.

Checks run: `npm test` (8 pass) and `npm run check` in `build/skill-validation` (4 skills, 84 Markdown files, links and anchors valid); `dotnet run --project util/SkillsGenerator -- --check` (no drift). `SKILL.md` lengths: 166 (stellar-admin) and 145 (setup) lines. Signatures were checked against source while writing, but no snippet has been compiled yet; that and the evaluation rerun are phase 3.

Two behaviours were read from the controller code rather than run; phase 3 verified both.

### Phase 3

Run on 2026-10-14.

**Snippets.** A scratch copy of Voyager outside the repository, built against packages packed from this checkout, compiled the API calls from every reference: registration, custom data source, labels, delete, index (search, paging, sorting, row details in all forms), columns and displays, forms layout and editors, EF and custom lookups, the multi-lookup handler, custom create model and handler, sidebar links, authorization overloads, `ConfigureUI`, `ConfigureResourceLabels`, and the badge and custom display templates. Build succeeded with no warnings.

**Behaviour checks** (same scratch app, run and probed):

- An index override in `Areas/StellarAdmin/Views/Reservations/` applies to a resource registered with the slug `"reservations"`: override folders follow the slug.
- An EF edit form renders an `[Editable(false)]` field read-only, and a post that changes it leaves the stored value unchanged while other fields save.

**Evaluation rerun with the skills.** Same tasks and fixtures as phase 1, fresh run directories (`prepare.sh`), with `stellar-admin-setup`, `stellar-admin` and `stellar-admin-ui` copied into each task's `.claude/skills/` and the agent told to use them. Sonnet 5.5 and Haiku 4.5, one fresh agent per task. Graded independently with `probe.sh`, plus form posts for tasks 1 and 3.

| Task | Sonnet baseline | Sonnet with skills | Haiku baseline | Haiku with skills |
| --- | --- | --- | --- | --- |
| 1 Install (6) | 6 | 6 | 4.5 | 6 |
| 2 Index (8) | 8 | 8 | 4 | 8 |
| 3 Forms (7) | 7 | 7 | 0 | 7 |
| 4 Sidebar and auth (7) | 6 | 7 | 5.5 | 7 |
| 5 View and override (5) | 1.5 | 5 | 2 | 5 |
| Total (33) | 28.5 | 33 | 16 | 33 |

| Model | Tool calls per task | Time per task |
| --- | --- | --- |
| Sonnet baseline | 10–33 | 1.1–3.9 min |
| Sonnet with skills | 6–18 | 0.8–2.0 min |
| Haiku baseline | 55–154 | 5.0–11.8 min |
| Haiku with skills | 57–88 | 4.4–6.2 min |

Every agent named the skills as its main or only source; none decompiled the packages. Not exercised by grading: saving a customer from the lookup's New sheet (task 3), and visual layout in a browser. Haiku task 3's edit page puts its read-only details section in one of the two columns, so Trip and Customer are not side by side there; counted as a pass against the written check.

Skill fixes from the rerun reports, applied after the runs (Haiku ran with the earlier text):

- `PageSize` must be one of `PageSizes` (default 10, 25, 50, 100), or every panel page is a 500 (index-pages, SKILL.md common mistakes, setup troubleshooting).
- `Add(selector, callback)` returns the parent builder; two Sonnet agents chained `UseEditor` onto it (SKILL.md common mistakes).
- With cookie authentication the forbidden response is a redirect to `AccessDeniedPath`, not a 403 (setup, authorization, troubleshooting).
- `[DataType(DataType.Date)]` brings MVC's short date format, overriding the `d MMM yyyy` default (columns, fields).
- The badge display sample gained a namespace and the matching `@using` in its template; a Haiku agent's template failed to compile without it (columns).

After the fixes: `npm test` (8 pass) and `npm run check` (4 skills, 84 files) in `build/skill-validation`; `SkillsGenerator --check` no drift. `SKILL.md`: 168 (stellar-admin) and 145 (setup) lines; descriptions 447–566 characters.

**Opus rerun with the skills** (2026-10-15). The skills by then included the sidebar branding and user menu work and task 6 ([sidebar-branding-user-menu](sidebar-branding-user-menu.md)). Same procedure: fresh run directory from `prepare.sh`, the three skills in each task's `.claude/skills/`, one fresh Opus 5.5 agent per task, graded by building, running and probing each app (`probe.sh`, curl, form posts for tasks 1 and 3, role matrix for task 4, sign-in and log out post for task 6).

| Task | Opus with skills | Tool calls | Time |
| --- | --- | --- | --- |
| 1 Install (6) | 6 | 17 | 2.2 min |
| 2 Index (8) | 8 | 9 | 1.0 min |
| 3 Forms (7) | 7 | 15 | 1.5 min |
| 4 Sidebar and auth (7) | 7 | 12 | 1.2 min |
| 5 View and override (5) | 5 | 12 | 1.1 min |
| 6 Branding and user menu (5) | 5 | 9 | 1.0 min |
| Total (38) | 38 | | |

| Model | Tool calls per task | Time per task |
| --- | --- | --- |
| Opus baseline (tasks 1–5) | 18–37 | 2.6–4.7 min |
| Opus with skills | 9–17 | 1.0–2.2 min |

With the skills Opus used about half the tool calls and well under half the time. Its task 3 edit page spans the read-only section across both columns (`ColumnSpanFull`), so Trip and Customer stay side by side, which the Haiku runs did not. Task 6 points at `/MyBookings`, which the fixture doesn't have, so the item now shows as a link problem ("No page /MyBookings") in Development; the agent noticed and left the page to the app, as the prompt describes it as existing. Graded as a pass against the written check. Afterwards the fixture gained a stub `Pages/MyBookings.cshtml` (in `fixtures/fresh`, so every task's app has it) so the link resolves in later runs. Not exercised by grading: saving a customer from the lookup's New sheet (task 3) and visual layout in a browser.
