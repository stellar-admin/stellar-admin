# Product and cross-repo plans

Reconciled against product and website source on 2026-09-18. All 49 plan/research records, including existing archives and the OSS backlog, have a dated code-audit note identifying current implementation or remaining work. Product and website working trees were clean at the start; website was inspected read-only. Historical session instructions do not authorize new work. No new plan was created.

The audit checks source, configuration, samples, generated website artifacts and test coverage. It does not claim fresh browser acceptance, installation smoke tests or hosted release verification. Historical release/account evidence remains labeled as such. Maintained architecture and execution guidance live in `docs/repos/` and `docs/development.md`.

## Still relevant

| Record | Status | Current assessment |
| --- | --- | --- |
| [admin-panel-backlog](admin-panel-backlog.md) | backlog | Admin panel features comparable frameworks offer and StellarAdmin lacks (custom and bulk actions, filters, form tabs, dashboards, file uploads and others). Unscheduled; an item becomes a plan when picked up. |
| [crud-screen-tag-helpers](crud-screen-tag-helpers.md) | proposed | The independent screen-composition proposal is still unimplemented: there are no `sa-record-page`, `sa-form-actions`, `sa-details`, or `sa-display-field` helpers. |
| [row-detail-expand-when](row-detail-expand-when.md) | deferred | Per-row expand rules for row details. Deferred until an application needs them; the plan records the smallest route (an admin panel `ExpandWhen` bool backed by a grid marker element) and the reviewed reason-tooltip variant. Prototype in `sandbox/html/row-details-expand-when.html`. |
| [configured-row-details](configured-row-details.md) | proposed | Row-detail panels configured from display fields, like the view page, instead of hand-written partials. Carried over from info-lists decision 5. |
| [ef-many-to-many-fields](ef-many-to-many-fields.md) | proposed | Many-to-many navigations as fields on EF Core resources, so a multi-select editor needs no form model or handlers. From the Voyager Travel session feedback. |
| [grid-keyboard-navigation](grid-keyboard-navigation.md) | proposed | Keyboard navigation and shortcuts in the index grid. From the Voyager Travel session feedback. |
| [rich-text-editor](rich-text-editor.md) | proposed | An editor for formatted long text. From the Voyager Travel session feedback. |
| [display-defaults](display-defaults.md) | proposed | App-level configuration of the default displays, and display handlers in index columns. Carried over from info-lists decisions 90 and 129. |
| [homepage-component-embeds](homepage-component-embeds.md) | proposed | The website retains `src/components/inline-example/inline-example.tsx`, generated fragments and `scripts/export-inline-example.mjs`, but `src/routes/index.tsx` does not consume the wrapper. |

## Retired implementation and research records

Completed work and superseded alternatives are retained for rationale. Each record's audit note takes precedence over old “pending”, “uncommitted” or review-gate text. A reference record can preserve an unimplemented optional idea without reopening the old task.

| Record | Current status |
| --- | --- |
| [save-and-add-another](archive/save-and-add-another.md) | completed (2026-10-20); `EnableAddAnother` on the create builder adds Save and add another to the create page and inline create rows, with `KeepForNext` carrying values to the next record, and Ctrl/Cmd+Enter and Ctrl/Cmd+Shift+Enter on every form, with tests, AdminPlayground samples and the consumer reference. Prototype in `sandbox/html/save-and-add-another.html`. |
| [checklist-component](archive/checklist-component.md) | completed (2026-10-20); `sa-checklist` with `sa-checklist-item` and `sa-checklist-item-link` rows, `Todo`, `InProgress` and `Done` statuses, custom status, actions, size and divider options, used by the admin home page's setup checklist, with samples, website docs and the consumer reference. |
| [timeline-component](archive/timeline-component.md) | completed (2026-10-20); `sa-timeline` for lists of events, with media, time placement, marker and media style options, per-item rails and `current` progress, with samples, website docs and the consumer reference. |
| [admin-panel-rename](archive/admin-panel-rename.md) | completed (2026-10-19); the panel package became `StellarAdmin` (from Dashboard) and Core and TagHelpers merged into `StellarAdmin.UI`, shipped in the releases since. |
| [admin-panel-skills](archive/admin-panel-skills.md) | completed; installable `stellar-admin` and `stellar-admin-setup` consumer skills organised by task with one reference per topic, measured against six evaluation tasks in `build/skill-evals/admin-panel`. With the skills, Sonnet, Haiku and Opus passed every check; Opus used about half the tool calls and time of its baseline. |
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
| [dashboard-row-details](archive/dashboard-row-details.md) | completed; expandable rows on the Dashboard resource index with inline, handler and EF Core projection panels (`EnableRowDetails`), several panels as tabs, panel tabs in the library (`sa-tabs`, `sa-tab`, `sa-tab-panel`, `sel-tabs`), a lighter band, a `TableRowDetailDivider` setting and row details tabs aligned with the content. `ExpandWhen`, a fields panel, opening rows from the URL and a segmented tab look are unscheduled. |
| [dashboard-theme-configuration](archive/dashboard-theme-configuration.md) | completed |
| [data-grid-inline-editing](archive/data-grid-inline-editing.md) | completed (2026-10-16); StellarAdmin.UI work for inline editing: one `sel-table` web component for selection, row details and editing, row keys, edit mode on `sa-data-grid` (`editing-keys`, `new-item`, `rows-only`, per-column edit templates) and `sa-data-grid-add-row`. The website DataGrid docs were merged into the website's `master` on 2026-10-17. |
| [demo-theme-selector](archive/demo-theme-selector.md) | completed |
| [ef-test-migration](archive/ef-test-migration.md) | completed; all 325 solution tests pass through discovery on 2026-09-18 |
| [field-editor-catalog](archive/field-editor-catalog.md) | completed; built-in `UseEditor` field editors with data-type templates forwarding to them, a DashboardPlayground editor gallery and a consumer reference section. A slider value display and gallery pixel comparison are follow-ups. |
| [form-section](oss/archive/form-section.md) | completed |
| [generic-resources-brainstorming](archive/generic-resources-brainstorming.md) | completed |
| [generic-resources-follow-ups](archive/generic-resources-follow-ups.md) | superseded; most items settled by later work, items 2 and 5 not re-checked |
| [grid-field-expression-binding](archive/grid-field-expression-binding.md) | completed |
| [grid-nested-field-binding](archive/grid-nested-field-binding.md) | completed |
| [home-setup-checklist](archive/home-setup-checklist.md) | completed (2026-10-19); a Development-only setup checklist on the panel's home page, detected from the configuration at request time, configured with `ConfigureHomePage`. Website docs belong to the wider admin panel docs work. |
| [ice-theme](archive/ice-theme.md) | completed |
| [icon-keywords](archive/icon-keywords.md) | completed (2026-10-19); the Lucide and Tabler packs pinned to 1.59.0 and 3.49.0 by `util/icon-packs/update.mjs`, the upstream keywords generated into `IconDefinition.Keywords`, and the icon picker searching them for child icons (`data-icon-keywords`) and the admin icons endpoint. |
| [icon-picker-editor](archive/icon-picker-editor.md) | completed (2026-10-19); a public `sa-icon-picker` (popover grid or inline tiles, child `sa-icon` elements or an `icons-url` with a documented payload), and `IconDisplay` and `IconEditor` for icon-name fields in the admin panel. |
| [image-field](archive/image-field.md) | completed (2026-10-16); `ImageDisplay` and `ImageEditor` for a string property with a per-field `ResolveUrl` and `StoreImage`, the upload endpoint, `sa-image` and `sa-image-upload` in StellarAdmin.UI, `template-renders-empty` and `FieldDisplay.RendersEmptyValue`, samples, skill references and website docs (merged into the website's `master` on 2026-10-17). UX in `sandbox/html/image-field.html`. |
| [info-lists](archive/info-lists.md) | completed; `sa-details` and `sa-display-field`, Dashboard display fields, details sections, displays and display handlers, EF Core reference loading, grid column displays and the `AllowView` view page. Configured row details and display defaults continue in their own plans; the website inline-example stylesheet still has the old divided rule. |
| [identity-entity-constraint-research](archive/identity-entity-constraint-research.md) | completed |
| [identity-on-resource-baseline](archive/identity-on-resource-baseline.md) | retired; historical Identity example. The role description migration was applied on 2026-09-27. |
| [identity-options-builder](archive/identity-options-builder.md) | completed |
| [identity-resource-layer](archive/identity-resource-layer.md) | completed |
| [identity-user-forms](archive/identity-user-forms.md) | superseded |
| [identity-user-forms-approaches](archive/identity-user-forms-approaches.md) | superseded |
| [identity-view-override-hatches](archive/identity-view-override-hatches.md) | reference |
| [inline-editing](archive/inline-editing.md) | completed (2026-10-16); row edit mode and inline add on the resource index: `UseInlineRow` on the edit and create builders, `EditsField` on columns, the Add buttons, dimming and locking, and `AfterSave` (reload the page or keep the row in place). Departments in AdminPlayground edit and add inline; consumer docs in the stellar-admin skill's `references/inline-editing.md`. Prototypes in `sandbox/html/inline-editing.html` and `sandbox/html/inline-row-edit.html`. |
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
| [public-api-review](archive/public-api-review.md) | completed; public API review of StellarAdmin.UI, StellarAdmin and StellarAdmin.EntityFrameworkCore before the first preview: 12 design decisions (D1–D12) and 41 changes (J1–J41), the website follow-up, the menu options removed (J32) and PublicApiAnalyzers added (J34). Releases stay at 0.x without API promotion or package validation. |
| [release-pipeline](archive/release-pipeline.md) | superseded |
| [repository-consolidation](archive/repository-consolidation.md) | completed |
| [resource-configuration-and-controller-unification](archive/resource-configuration-and-controller-unification.md) | completed; shared Dashboard resources, EF Core integration, and the host-owned Identity example were delivered. Deferred ideas require separately scoped work. |
| [resource-consumer-sketches](archive/resource-consumer-sketches.md) | superseded |
| [resource-form-header-actions](archive/resource-form-header-actions.md) | completed |
| [resource-index-page-view-model](archive/resource-index-page-view-model.md) | completed |
| [resource-sidebar-registration](archive/resource-sidebar-registration.md) | completed |
| [resources-project-extraction](archive/resources-project-extraction.md) | superseded |
| [sa-grid](archive/sa-grid.md) | completed; `sa-grid` and `sa-grid-item` with responsive columns, spans and starts, shared with `sa-details`; the Dashboard form grid and choice columns use it; `sa-form-row` removed |
| [segmented-control](oss/archive/segmented-control.md) | completed |
| [semantic-icons](archive/semantic-icons.md) | completed; IconOptions tests migrated to TUnit on 2026-09-18 |
| [shadcn-theme-fonts](archive/shadcn-theme-fonts.md) | completed |
| [shadcn-theme-namespace](archive/shadcn-theme-namespace.md) | completed |
| [sheet-stack](archive/sheet-stack.md) | completed; Dashboard sheets stack to any depth from one template, with covered levels receding behind the top one, Esc closing one level, click-back to a covered level, a phone layout, a `sel-dialog` scroll lock that holds while any modal is open, per-level create form prefixes with New at every level, and a `lookup-created` event that hands a created key to the editor below. A breadcrumb trail, a depth limit, New in the search sheet and turning New off per field are unscheduled. |
| [sidebar-branding-user-menu](archive/sidebar-branding-user-menu.md) | completed; `ConfigureBranding` (title, subtitle, icon, image or wordmark logo) and a claims-driven `ConfigureUserMenu` with ordered items and a built-in logout endpoint, joined multi-value description claims, link problems shown in Development and hidden elsewhere for sidebar and user menu links whose page or action doesn't exist, and sidebar icons for resources and links. Prototype in `sandbox/html/sidebar-branding.html`. |
| [slider-value-display](archive/slider-value-display.md) | completed; composable `sa-slider-value` and `sa-slider-marks`, `SliderEditor` value and mark settings, website docs and tests. A field-level `aria-describedby` fix is a follow-up. |
| [toast](oss/archive/toast.md) | completed; `<sa-toaster>`, `window.stellarAdmin.toast` and the TempData-backed `IToastNotifier` with page-load, redirect and `SA-Toasts` header delivery, across all themes, with demos, website docs, skills reference and tests. Dashboard use is a separate task. |
| [table-row-details](oss/archive/table-row-details.md) | completed; expandable table rows at the `sa-table-*` level and the `sa-data-grid-row-detail` layer, with demos, website docs and skills references; visuals in `sandbox/html/expandable-rows.html` |
| [taghelper-test-migration](archive/taghelper-test-migration.md) | completed; solution-wide CI/release test discovery verified on 2026-09-18 |
| [templated-view-slots](archive/templated-view-slots.md) | completed |
| [theme-builder](archive/theme-builder.md) | completed; a website theme builder over the docs exports: preset seeds, simple and advanced controls from the theme variable manifest, a live preview, theme file export and import, a shareable URL. Not yet released. |
| [theme-token-migration](archive/theme-token-migration.md) | completed; one token-driven `stellar-admin.css` with themes as short files of theme variables, Tailwind-free tag helper CSS, relative density levels and a `light` class. Merged 2026-10-11, not yet released. Open: a mobile-width sweep and ComponentPlayground dark mode. |
| [token-spec-concept](archive/token-spec-concept.md) | completed; the three-tier token concept and its `sandbox/html/token-spec/` prototype, implemented by the theme token migration |
| [upstream-references-cleanup](oss/archive/upstream-references-cleanup.md) | completed |
| [workspace-retirement](archive/workspace-retirement.md) | completed |

## Audit validation

Confirmed that all 49 records appear in this index and carry a dated assessment. Checked relative Markdown links affected by the 13 archive moves: no newly broken targets; historical links to retired checkouts remain historical. File BOM/newline conventions were preserved and `git diff --check` passed. The initial audit changed only documentation. The subsequent consumer-skill closeout added CI bundle validation and passed eight regression tests plus the real-bundle check; application tests, browser checks and external service checks were not run. The website working tree remains clean.

## Maintaining these records

Update the relevant existing record when work resumes. Record the current scope, actual source paths, remaining decisions and checks performed; keep historical results separate. Archive closed work and update incoming links. Do not treat proposals or past session permissions as authorization to implement, commit or publish.
