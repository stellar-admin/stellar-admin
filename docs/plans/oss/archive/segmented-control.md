# Segmented control

Status: completed. Last updated: 2026-09-11. Affected repos: workspace, OSS, pro (sample exporter only), website, consumer skills. Implementation is complete. The user subsequently authorized committing and pushing all affected repositories.

## Approved scope and design

The user approved end-to-end implementation of `sa-segmented-control` and `sa-segmented-control-item`, and explicitly waived the visual prototype. The parent owns `asp-for` or unbound `name`/`value`, optional `disabled`/`required`, and the standard bubbling `onchange` handler. Items render native radios inside labels. ASP.NET Core's HTML generator resolves bound values, ModelState, validation metadata, and field prefixes. A request-local sequence keeps IDs unique across loops and repeated partials.

The sole appearance matches horizontal default tabs. `CreateSegmentedControlStyles()` derives eight upstream themes from tab tokens, removing vertical/line-variant rules and mapping active/focus/disabled states to radio selectors. Shared styles live in `components.css`; Ledger is handwritten and its maintained specification was updated. No client web component is needed.

## Delivered changes and Git status

All five repos were clean when work began. The changes below are recorded in the commits listed in the final delivery section.

| Repo | Changes |
| --- | --- |
| Workspace | Curated skill examples, Ledger specification, plan and index. |
| OSS | Two public tag helpers and internal context; structural CSS and nine themes; theme processor and coverage; DocsSamples page, five partials, navigation/static model registrations; reusable browser checks in `util/segmented-control/`. |
| Pro | Five export registrations and a small exporter fix removing the samples-only appearance script, which referenced a theme link replaced by website export. |
| Website | Documentation/navigation, five rendered demos and snippets, and nine updated theme assets. |
| Consumer skills | Segmented-control reference and index; generation also filled existing missing FormRow/FormSection references so the drift check passes. |

The full website generator exported 411 demos. Its existing-demo output also picked up unrelated layout changes from previously edited source (new Ledger default, font link, and attribute ordering). The 406 existing demos were returned to their clean pre-task contents, with the generated versions preserved at `/tmp/segmented-export-existing-demos`; the new demos and theme assets were retained. No existing demo source or shared layout was changed by this task.

## Verification

- Library build passed with zero warnings/errors. DocsSamples and the exporter built successfully; the samples/exporter report existing nullable warnings.
- Theme generator completed; all eight generated source diffs add only segmented-control rules. Direct CSS build passed for all nine themes. Coverage passed: 55 components × 9 themes.
- `node stellar-admin/util/segmented-control/check.mjs http://localhost:5206` passed. It checks selected/unselected appearance against actual default tabs across all nine themes in light/dark modes at 1280px and 390px, labels, unique IDs, independent groups, disabled options, keyboard navigation/focus, event counts, programmatic assignment, reset, and prefixed nullable-enum form submissions including required/invalid errors.
- Screenshots were inspected in Ledger light and Nova dark/mobile. Captures remain in `/tmp/stellar-admin-segmented-control`.
- An additional temporary .NET harness at `/tmp/segmented-binding-check` passed string/non-nullable-enum/nullable binding, collection prefixes, `asp-for` precedence, ModelState-over-model redisplay, invalid state, repeated-source IDs, sanitized-value collisions, and HTML encoding.
- All five exported demos were smoke-tested in Chromium. The change handler works with library JavaScript blocked; Ledger/dark preferences apply; no JavaScript exceptions occurred.
- Website `pnpm lint`, `pnpm types:check`, and `pnpm build` passed.
- Skills regeneration and `--check` passed with no drift. Formatting and all five repos' `git diff --check` passed.

The installed SDK is .NET 10.0.400; builds were run from the workspace using that SDK because the OSS-pinned 10.0.100 is unavailable. No SDK pins changed. Tailwind's sandboxed build wrote empty bundles and pnpm could not access its database, so the successful builds/checks ran outside the sandbox. Upstream generation also required network access.

## Handoff

No implementation or validation work remains. Review the DocsSamples `/SegmentedControl` page after starting or rebuilding the samples app. The agent's servers on ports 5206 and 8322 were stopped; Jerrie's port 5205 was left alone. At the initial handoff, no commits, pushes, or deployments had been made; the user subsequently authorized commits and pushes.

## Review adjustment — 2026-09-11

Moved the default-tabs comparison to the second DocsSamples example, alongside Intro. Validation now draws a single 2px destructive ring around the outer control; individual items retain their normal borders and keyboard focus styling. Rebuilt all nine CSS bundles and synchronized the website demo assets. Browser checks passed for the outer ring, unchanged item styling, and example order across all nine themes in light/dark modes; the Ledger screenshot was inspected. The agent’s port 5206 instance was stopped after verification.

## Shared field integration — 2026-09-11

The user required inheritance from the standard field-input base. `SegmentedControlTagHelper` now derives from `FieldInputBaseTagHelper`, uses its inherited binding/field properties and rendering pipeline, and returns the standard vertical field configuration. The context records the first radio ID so the generated field label targets a native control. Explicit `error` also marks the group invalid. ModelBinding and Validation examples now demonstrate automatic labels, metadata descriptions, and error messages without handwritten validation markup. The single outer validation ring and second-position tabs comparison remain intact.

The reusable browser check now asserts the automatic field wrapper, label-to-radio association, metadata description, and exactly one standard validation message on initial error display and failed submission. All browser checks passed across nine themes and both modes/widths. The temporary .NET harness also passed binding, ModelState precedence, loop IDs, automatic errors, wrapper opt-out, no duplicate wrapper inside an explicit field, and explicit field text. Screenshots were inspected. Samples/exporter builds, website lint/typecheck/build, and skills drift checks passed. Website demo/snippet exports and consumer documentation were updated; unrelated generated outputs were restored from the pre-export snapshot at `/tmp/segmented-field-export-before`. The agent’s 5206 process was stopped after verification.

## Icons example and final delivery — 2026-09-11

Added an Icons example immediately after the default-tabs comparison, using decorative plane, bed, and car icons with text labels. Registered and exported the demo, added website documentation, and included it in the consumer reference. Browser verification passed for icon size, decorative semantics, and radio selection in Nova, Vega, and Ledger, light/dark. Samples/exporter build, website lint/typecheck/build, and skills drift validation passed. Existing generated files were preserved from `/tmp/segmented-icons-export-before`; generated HTML retains the exporter's whitespace-only blank lines. The agent's port 8322 server was stopped.

The user authorized committing and pushing to the current `master` branches. Product commits:

| Repo | Main implementation | Icons follow-up |
| --- | --- | --- |
| OSS | `e932b25` | `319d4be` |
| Pro | `f71f08a` | `51f3a46` |
| Website | `df12fe3a` | `addefc68` |
| Skills | `3d2a8d2` | `92bfa62` |

The workspace commit contains this record, the curated example choices, the Ledger design addition, and the plan index entry.
