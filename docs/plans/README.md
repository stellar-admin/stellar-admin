# Product and cross-repo plans

Product and cross-repo plans and research are tracked here. These records were imported from workspace commit `0c80b64`; historical paths and commands refer to their original checkout layout. Follow current `AGENTS.md` and `docs/development.md` for execution. The website remains a separate repository. Statuses below were reconciled from the existing records on 2026-09-05; historical verification has not been rerun. There is no implicitly selected active implementation task. The component backlog lives in [the OSS backlog](oss/README.md).

Completed records remain useful for rationale and commit references. Their old paths, commands, session permissions, and review checkpoints describe the original task, not current instructions. References to private memory or session-only artifacts are historical evidence, not required inputs; if needed for a new task, reconstruct the facts from code and record them in the plan or current design.

| Plan | Status |
| --- | --- |
| [Consumer skill consolidation](consumer-skill-consolidation.md) | active — product-specific layout and installation documentation complete; installation tests and CI checks remain |
| [Workspace and skills consolidation](workspace-retirement.md) | completed — product 0.3.0 release verified; old repositories archived and NuGet trust removed; website stays separate |
| [Icon encapsulation and semantic icons](semantic-icons.md) | encapsulation completed; semantic API proposed |
| [Repository consolidation](archive/repository-consolidation.md) | completed — clean import committed; push and retirement remain separate |
| [Homepage component embeds](homepage-component-embeds.md) | reference — experiment accepted; reusable code retained, example route removed; homepage integration proposed |
| [Observatory recommendation and website palette](archive/observatory-default-theme.md) | completed — website palette, sample defaults, and consumer guidance aligned |
| [Optional shadcn-theme fonts](archive/shadcn-theme-fonts.md) | completed — per-style optional font defaults and native fallbacks |
| [Optional custom-theme fonts](archive/optional-theme-fonts.md) | completed — original fonts retained with native fallbacks; demos load only selected families |
| [Component showcase examples](component-showcases.md) | Final selection: Masonry and ThemeShowcase; both exported, ThemeShowcase embedded in theming docs |
| [Shadcn theme namespace](archive/shadcn-theme-namespace.md) | completed — implemented and verified locally |
| [Parallax theme](archive/parallax-theme.md) | completed — implemented and verified locally; visual refinement follows |
| [Aurora theme](archive/aurora-theme.md) | completed — implemented and verified locally |
| [Segmented control](oss/archive/segmented-control.md) | completed |
| [Ice theme](archive/ice-theme.md) | completed — implemented and verified locally |
| [Concourse theme](archive/concourse-theme.md) | completed — implemented and verified locally |
| [Observatory theme](archive/observatory-theme.md) | completed — implemented and verified locally |
| [Meridian theme](archive/meridian-theme.md) | completed — implemented, verified and accepted |
| [Ledger theme](ledger-theme.md) | active — foundational visual checkpoint |
| [Native scrolling carousel](oss/archive/carousel.md) | completed |
| [Form section](oss/form-section.md) | OSS components, docs, and Pro form-definition integration implemented |
| [Resource form header and actions](archive/resource-form-header-actions.md) | completed |
| [crud-screen-tag-helpers](crud-screen-tag-helpers.md) | proposed |
| [demo-theme-selector](archive/demo-theme-selector.md) | completed |
| [grid-field-expression-binding](grid-field-expression-binding.md) | reference |
| [grid-nested-field-binding](archive/grid-nested-field-binding.md) | completed |
| [generic-resources-brainstorming](archive/generic-resources-brainstorming.md) | completed; session closed 2026-09-10 |
| [generic-resources-follow-ups](generic-resources-follow-ups.md) | parked; deferred work, no active implementation |
| [identity-entity-constraint-research](identity-entity-constraint-research.md) | reference |
| [identity-options-builder](archive/identity-options-builder.md) | completed |
| [identity-resource-layer](archive/identity-resource-layer.md) | completed |
| [identity-user-forms](identity-user-forms.md) | needs-reconciliation |
| [identity-user-forms-approaches](identity-user-forms-approaches.md) | reference |
| [identity-view-override-hatches](identity-view-override-hatches.md) | reference |
| [oss-bug-sweep](archive/oss-bug-sweep.md) | completed |
| [oss-docs-content-sweep](archive/oss-docs-content-sweep.md) | completed |
| [oss-docs-new-pages](archive/oss-docs-new-pages.md) | completed |
| [oss-release](archive/oss-release.md) | completed |
| [pr-vrt](pr-vrt.md) | needs-reconciliation |
| [pro-builder-collapse](archive/pro-builder-collapse.md) | completed |
| [pro-release](pro-release.md) | parked |
| [project-reorganization](archive/project-reorganization.md) | completed |
| [release-pipeline](archive/release-pipeline.md) | completed |
| [resource-index-page-view-model](archive/resource-index-page-view-model.md) | completed |
| [resource-consumer-sketches](resource-consumer-sketches.md) | proposed |
| [resources-project-extraction](archive/resources-project-extraction.md) | completed |
| [templated-view-slots](archive/templated-view-slots.md) | completed |

## Starting or handing off work

Use a plan for substantial work that needs continuity across sessions or agents; do not create one for every small edit. Record status (proposed, active, blocked, parked, completed, superseded, reference, or needs-reconciliation), last update, affected repos, current design links, scope, decisions, remaining work, verification results, and the next concrete step. Include branch/commit references per repo when useful. Distinguish agreed design from authorization to execute or publish.

At handoff, update the relevant plan rather than creating a competing global session log. Preserve existing decisions, identify any unknowns, and report uncommitted changes per repo. When closing a plan, move it to `archive/` and update incoming links and this index. Promote durable conventions and current API design into their maintained documents.
