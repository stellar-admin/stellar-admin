# Product and cross-repo plans

Reconciled against product and website source on 2026-09-18. All 49 plan/research records, including existing archives and the OSS backlog, have a dated code-audit note identifying current implementation or remaining work. Product and website working trees were clean at the start; website was inspected read-only. Historical session instructions do not authorize new work. No new plan was created.

The audit checks source, configuration, samples, generated website artifacts and test coverage. It does not claim fresh browser acceptance, installation smoke tests or hosted release verification. Historical release/account evidence remains labeled as such. Maintained architecture and execution guidance live in `docs/repos/` and `docs/development.md`.

## Still relevant

| Record | Status | Current assessment |
| --- | --- | --- |
| [crud-screen-tag-helpers](crud-screen-tag-helpers.md) | proposed | The independent screen-composition proposal is still unimplemented: there are no `sa-record-page`, `sa-form-actions`, `sa-details`, or `sa-display-field` helpers. |
| [table-row-details](oss/table-row-details.md) | implemented, awaiting review | Expandable table rows at the `sa-table-*` level with a `sa-data-grid` convenience layer, plus DocsSamples demos, website docs and skills references. Visuals approved in `sandbox/html/expandable-rows.html`. Dashboard integration is a later plan. |
| [token-spec-concept](token-spec-concept.md) | concept | A three-tier token spec for nine components (button, input, card, table, chip, dropdown menu, tabs, dialog, sidebar), prototyped in `sandbox/html/token-spec/`; Nova, Lyra, Maia, Aurora, Concourse and Ledger expressed as 7–301 line token files and compared against the current bundles; a knob layer and theme builder for themes unlike any shipped one. Not in the library. |
| [theme-token-migration](theme-token-migration.md) | proposed | Replace the fifteen theme bundles with one token-driven `stellar-admin.css`, curated knob presets, Tailwind-free tag helper CSS (the Dashboard keeps Tailwind through an adapter), a website theme builder and a theme-writing consumer skill. |
| [homepage-component-embeds](homepage-component-embeds.md) | proposed | The website retains `src/components/inline-example/inline-example.tsx`, generated fragments and `scripts/export-inline-example.mjs`, but `src/routes/index.tsx` does not consume the wrapper. |

## Retired implementation and research records

Completed work and superseded alternatives are retained for rationale. Each record's audit note takes precedence over old “pending”, “uncommitted” or review-gate text. A reference record can preserve an unimplemented optional idea without reopening the old task.

| Record | Current status |
| --- | --- |
| [aurora-theme](archive/aurora-theme.md) | completed |
| [carousel](oss/archive/carousel.md) | completed |
| [chip](oss/archive/chip.md) | completed |
| [choice-groups](archive/choice-groups.md) | completed. Product helpers, typed binding, display variants, website docs and exports, consumer references, 76 passing TagHelpers tests, and Chromium checks verified 2026-09-18 |
| [command](oss/archive/command.md) | completed |
| [component-parity](oss/archive/component-parity.md) | archived; backlog checked 2026-09-18, no longer maintained |
| [component-showcases](archive/component-showcases.md) | completed |
| [concourse-theme](archive/concourse-theme.md) | completed |
| [consumer-skill-consolidation](archive/consumer-skill-consolidation.md) | completed |
| [custom-sidebar-links](archive/custom-sidebar-links.md) | completed; `AddSidebarLink` for host Razor Pages, MVC actions and URLs, merged with resource groups in the sidebar and command palette, and host pages that opt into the Dashboard layout with `StellarAdminLayouts.Dashboard`. Icons, route values, sub-menus and a partial override test are unscheduled. |
| [dashboard-authorization](archive/dashboard-authorization.md) | completed; `RequireAuthorization` on Dashboard and resource builders, an authorization-aware async sidebar, and the `MapStellarAdmin` convention builder, with HTTP integration tests. Per-action requirements are deferred. |
| [dashboard-choice-editors](archive/dashboard-choice-editors.md) | completed; `SegmentedControlEditor` (renamed from `ToggleButtonsEditor`), checkbox group appearance, columns and flow, a `ToggleGroupEditor` with chips, joined and buttons appearances, public `ChoiceItem` and `ChoiceGroup` items for every source and editor (replacing the lookup item types), an editor-owned empty choice, and shared item media for choices and lookups. The sheet and popover multi-select lookup patterns and an enum member icon attribute are unscheduled. |
| [dashboard-form-grid](archive/dashboard-form-grid.md) | completed; column counts and spans per breakpoint for Dashboard create and edit forms, set with `Columns` and `ColumnSpan` on the form, sections, groups and fields, per-section `Layout`, and `AddRow` removed, with a Form layouts gallery in the playground. `ColumnStart` and a Small start for plain `Columns(n)` are unscheduled. |
| [dashboard-theme-configuration](archive/dashboard-theme-configuration.md) | completed |
| [demo-theme-selector](archive/demo-theme-selector.md) | completed |
| [ef-test-migration](archive/ef-test-migration.md) | completed; all 325 solution tests pass through discovery on 2026-09-18 |
| [field-editor-catalog](archive/field-editor-catalog.md) | completed; built-in `UseEditor` field editors with data-type templates forwarding to them, a DashboardPlayground editor gallery and a consumer reference section. A slider value display and gallery pixel comparison are follow-ups. |
| [form-section](oss/archive/form-section.md) | completed |
| [generic-resources-brainstorming](archive/generic-resources-brainstorming.md) | completed |
| [generic-resources-follow-ups](archive/generic-resources-follow-ups.md) | superseded; most items settled by later work, items 2 and 5 not re-checked |
| [grid-field-expression-binding](archive/grid-field-expression-binding.md) | completed |
| [grid-nested-field-binding](archive/grid-nested-field-binding.md) | completed |
| [ice-theme](archive/ice-theme.md) | completed |
| [identity-entity-constraint-research](archive/identity-entity-constraint-research.md) | completed |
| [identity-on-resource-baseline](archive/identity-on-resource-baseline.md) | retired; historical Identity example. The role description migration was applied on 2026-09-27. |
| [identity-options-builder](archive/identity-options-builder.md) | completed |
| [identity-resource-layer](archive/identity-resource-layer.md) | completed |
| [identity-user-forms](archive/identity-user-forms.md) | superseded |
| [identity-user-forms-approaches](archive/identity-user-forms-approaches.md) | superseded |
| [identity-view-override-hatches](archive/identity-view-override-hatches.md) | reference |
| [ledger-theme](archive/ledger-theme.md) | completed |
| [lookup-create](archive/lookup-create.md) | completed; `LookupEditor.EnableCreate()` opens the referenced resource's create form in the shared sheet and selects the created item. Follow-ups (theme-level sheet body padding, nested sheet stacking, key-less handler message, New in the search sheet) are unscheduled. |
| [lookup-editor](archive/lookup-editor.md) | completed; searchable `LookupEditor` with the display redesign, a shared dashboard sheet that loads its content from the server, and plain custom elements in the dashboard script. Creating items from the lookup followed in [lookup-create](archive/lookup-create.md). |
| [menu-color-appearance-accent](oss/archive/menu-color-appearance-accent.md) | completed |
| [meridian-theme](archive/meridian-theme.md) | completed |
| [multi-lookup-editor](archive/multi-lookup-editor.md) | completed |
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
| [resource-configuration-and-controller-unification](archive/resource-configuration-and-controller-unification.md) | completed; shared Dashboard resources, EF Core integration, and the host-owned Identity example were delivered. Deferred ideas require separately scoped work. |
| [resource-consumer-sketches](archive/resource-consumer-sketches.md) | superseded |
| [resource-form-header-actions](archive/resource-form-header-actions.md) | completed |
| [resource-index-page-view-model](archive/resource-index-page-view-model.md) | completed |
| [resource-sidebar-registration](archive/resource-sidebar-registration.md) | completed |
| [resources-project-extraction](archive/resources-project-extraction.md) | superseded |
| [segmented-control](oss/archive/segmented-control.md) | completed |
| [semantic-icons](archive/semantic-icons.md) | completed; IconOptions tests migrated to TUnit on 2026-09-18 |
| [shadcn-theme-fonts](archive/shadcn-theme-fonts.md) | completed |
| [shadcn-theme-namespace](archive/shadcn-theme-namespace.md) | completed |
| [sheet-stack](archive/sheet-stack.md) | completed; Dashboard sheets stack to any depth from one template, with covered levels receding behind the top one, Esc closing one level, click-back to a covered level, a phone layout, a `sel-dialog` scroll lock that holds while any modal is open, per-level create form prefixes with New at every level, and a `lookup-created` event that hands a created key to the editor below. A breadcrumb trail, a depth limit, New in the search sheet and turning New off per field are unscheduled. |
| [slider-value-display](archive/slider-value-display.md) | completed; composable `sa-slider-value` and `sa-slider-marks`, `SliderEditor` value and mark settings, website docs and tests. A field-level `aria-describedby` fix is a follow-up. |
| [toast](oss/archive/toast.md) | completed; `<sa-toaster>`, `window.stellarAdmin.toast` and the TempData-backed `IToastNotifier` with page-load, redirect and `SA-Toasts` header delivery, across all themes, with demos, website docs, skills reference and tests. Dashboard use is a separate task. |
| [taghelper-test-migration](archive/taghelper-test-migration.md) | completed; solution-wide CI/release test discovery verified on 2026-09-18 |
| [templated-view-slots](archive/templated-view-slots.md) | completed |
| [upstream-references-cleanup](oss/archive/upstream-references-cleanup.md) | completed |
| [workspace-retirement](archive/workspace-retirement.md) | completed |

## Audit validation

Confirmed that all 49 records appear in this index and carry a dated assessment. Checked relative Markdown links affected by the 13 archive moves: no newly broken targets; historical links to retired checkouts remain historical. File BOM/newline conventions were preserved and `git diff --check` passed. The initial audit changed only documentation. The subsequent consumer-skill closeout added CI bundle validation and passed eight regression tests plus the real-bundle check; application tests, browser checks and external service checks were not run. The website working tree remains clean.

## Maintaining these records

Update the relevant existing record when work resumes. Record the current scope, actual source paths, remaining decisions and checks performed; keep historical results separate. Archive closed work and update incoming links. Do not treat proposals or past session permissions as authorization to implement, commit or publish.
