# Product and cross-repo plans

Reconciled against product and website source on 2026-09-18. All 49 plan/research records, including existing archives and the OSS backlog, have a dated code-audit note identifying current implementation or remaining work. Product and website working trees were clean at the start; website was inspected read-only. Historical session instructions do not authorize new work. No new plan was created.

The audit checks source, configuration, samples, generated website artifacts and test coverage. It does not claim fresh browser acceptance, installation smoke tests or hosted release verification. Historical release/account evidence remains labeled as such. Maintained architecture and execution guidance live in `docs/repos/` and `docs/development.md`.

## Still relevant

| Record | Status | Current assessment |
| --- | --- | --- |
| [crud-screen-tag-helpers](crud-screen-tag-helpers.md) | proposed | The independent screen-composition proposal is still unimplemented: there are no `sa-record-page`, `sa-form-actions`, `sa-details`, or `sa-display-field` helpers. |
| [resource-configuration-and-controller-unification](resource-configuration-and-controller-unification.md) | active | Resource reset committed on `resource-redesign`; EF is reattached with listing and CRUD. Identity and its sample remain detached. Rebuild the standalone core in eight steps, proving action-specific view models immediately after basic CRUD, before index features and integration adaptation. Steps 1 and 2 provide registration, naming, and a working index backed by an in-memory data source in DashboardPlayground. The basic create form is committed. Test consolidation and create factory callbacks are implemented. Global delegate-based label defaults and advanced layouts are implemented. Edit is committed after review. Delete is committed. Operation results are implemented and verified. The split data source contract and custom create are implemented. Create/edit/delete now require explicit `AllowCreate`/`AllowEdit`/`AllowDelete` registration, with fresh options on each registration. Custom edit is implemented with typed loading and saving, separate Customer create/edit models, and HTTP and browser verification. The full index design is recorded with data-source-owned search/scope execution. Paging and the listing contract are committed. HTMX index paging and deletion are committed using the previous redirect-and-select approach. Sorting is committed as `05af889`. Explicit-query preservation is committed as `e3e82c1`. Search is committed as `9bb3e2e`. Scopes are committed and pushed as `ffb2059`. EF checkpoint 1 is implemented with dedicated root/index builders, metadata keys, a shared-controller SQLite index, paging, sorting, and solution/build reattachment. All 316 active tests pass. Checkpoint 1 is committed as `ffb6f85`. Shared resource/index builder bases are committed as `0004768`. EF search is committed as `822084b`. EF scope predicates are committed as `1ab050b`. Sorting is configured through the generic column builder with Sortable() and Sortable(expression), with EF execution, a playground example, and all 335 tests passing. EF CRUD is implemented through the shared action contracts and Product playground, with all 348 solution tests passing. Query transformations are deferred pending a broader event/callback design. Awaiting review before integration closeout. |
| [identity-on-resource-baseline](identity-on-resource-baseline.md) | implemented | DashboardPlayground demonstrates Users and Roles through ordinary Dashboard resources and manager-backed handlers. The separate Identity package remains detached. |
| [resource-sidebar-registration](resource-sidebar-registration.md) | implemented | Automatic resource sidebar links use one Dashboard provider, with label, group, order, and visibility configuration. |
| [generic-resources-follow-ups](generic-resources-follow-ups.md) | parked | Identity sidebar migration, operations overrides, richer reference editors/sources, navigation-free references and editor options remain deferred. |
| [homepage-component-embeds](homepage-component-embeds.md) | proposed | The website retains `src/components/inline-example/inline-example.tsx`, generated fragments and `scripts/export-inline-example.mjs`, but `src/routes/index.tsx` does not consume the wrapper. |
| [component-parity](oss/component-parity.md) | active | Missing components remain backlog; Carousel is complete and DataGrid covers server-rendered data tables. |

## Retired implementation and research records

Completed work and superseded alternatives are retained for rationale. Each record's audit note takes precedence over old “pending”, “uncommitted” or review-gate text. A reference record can preserve an unimplemented optional idea without reopening the old task.

| Record | Current status |
| --- | --- |
| [aurora-theme](archive/aurora-theme.md) | completed |
| [carousel](oss/archive/carousel.md) | completed |
| [choice-groups](archive/choice-groups.md) | completed. Product helpers, typed binding, display variants, website docs and exports, consumer references, 76 passing TagHelpers tests, and Chromium checks verified 2026-09-18 |
| [component-showcases](archive/component-showcases.md) | completed |
| [concourse-theme](archive/concourse-theme.md) | completed |
| [consumer-skill-consolidation](archive/consumer-skill-consolidation.md) | completed |
| [demo-theme-selector](archive/demo-theme-selector.md) | completed |
| [ef-test-migration](archive/ef-test-migration.md) | completed; all 325 solution tests pass through discovery on 2026-09-18 |
| [form-section](oss/archive/form-section.md) | completed |
| [generic-resources-brainstorming](archive/generic-resources-brainstorming.md) | completed |
| [grid-field-expression-binding](archive/grid-field-expression-binding.md) | completed |
| [grid-nested-field-binding](archive/grid-nested-field-binding.md) | completed |
| [ice-theme](archive/ice-theme.md) | completed |
| [identity-entity-constraint-research](archive/identity-entity-constraint-research.md) | completed |
| [identity-options-builder](archive/identity-options-builder.md) | completed |
| [identity-resource-layer](archive/identity-resource-layer.md) | completed |
| [identity-user-forms](archive/identity-user-forms.md) | superseded |
| [identity-user-forms-approaches](archive/identity-user-forms-approaches.md) | superseded |
| [identity-view-override-hatches](archive/identity-view-override-hatches.md) | reference |
| [ledger-theme](archive/ledger-theme.md) | completed |
| [menu-color-appearance-accent](oss/archive/menu-color-appearance-accent.md) | completed |
| [meridian-theme](archive/meridian-theme.md) | completed |
| [observatory-default-theme](archive/observatory-default-theme.md) | completed |
| [observatory-theme](archive/observatory-theme.md) | completed |
| [optional-theme-fonts](archive/optional-theme-fonts.md) | completed |
| [oss-bug-sweep](archive/oss-bug-sweep.md) | completed |
| [oss-docs-content-sweep](archive/oss-docs-content-sweep.md) | completed |
| [oss-docs-new-pages](archive/oss-docs-new-pages.md) | completed |
| [oss-release](archive/oss-release.md) | completed |
| [parallax-theme](archive/parallax-theme.md) | completed |
| [pr-vrt](archive/pr-vrt.md) | completed |
| [pro-builder-collapse](archive/pro-builder-collapse.md) | superseded |
| [pro-release](archive/pro-release.md) | superseded |
| [project-reorganization](archive/project-reorganization.md) | superseded |
| [release-pipeline](archive/release-pipeline.md) | superseded |
| [repository-consolidation](archive/repository-consolidation.md) | completed |
| [resource-consumer-sketches](archive/resource-consumer-sketches.md) | superseded |
| [resource-form-header-actions](archive/resource-form-header-actions.md) | completed |
| [resource-index-page-view-model](archive/resource-index-page-view-model.md) | completed |
| [resources-project-extraction](archive/resources-project-extraction.md) | superseded |
| [segmented-control](oss/archive/segmented-control.md) | completed |
| [semantic-icons](archive/semantic-icons.md) | completed; IconOptions tests migrated to TUnit on 2026-09-18 |
| [shadcn-theme-fonts](archive/shadcn-theme-fonts.md) | completed |
| [shadcn-theme-namespace](archive/shadcn-theme-namespace.md) | completed |
| [taghelper-test-migration](archive/taghelper-test-migration.md) | completed; solution-wide CI/release test discovery verified on 2026-09-18 |
| [templated-view-slots](archive/templated-view-slots.md) | completed |
| [upstream-references-cleanup](oss/archive/upstream-references-cleanup.md) | completed |
| [workspace-retirement](archive/workspace-retirement.md) | completed |

## Audit validation

Confirmed that all 49 records appear in this index and carry a dated assessment. Checked relative Markdown links affected by the 13 archive moves: no newly broken targets; historical links to retired checkouts remain historical. File BOM/newline conventions were preserved and `git diff --check` passed. The initial audit changed only documentation. The subsequent consumer-skill closeout added CI bundle validation and passed eight regression tests plus the real-bundle check; application tests, browser checks and external service checks were not run. The website working tree remains clean.

## Maintaining these records

Update the relevant existing record when work resumes. Record the current scope, actual source paths, remaining decisions and checks performed; keep historical results separate. Archive closed work and update incoming links. Do not treat proposals or past session permissions as authorization to implement, commit or publish.
