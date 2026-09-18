# Radio and checkbox groups

Status: product implementation completed and verified 2026-09-18; authorized in the current conversation.

## Scope

Add `sa-radio-group` and `sa-checkbox-group` with string-valued child items or `asp-items`, bound and unbound selection, and separate nullable `RadioGroupVariant` / `CheckboxGroupVariant` properties (`Default`, `ChoiceCard`). Reuse the existing Input renderer and Field composition from the demos. Do not change the low-level input API or existing samples.

Support strings, enums, GUIDs, booleans and numeric scalar types; nullable radio properties; arrays and list interfaces of non-nullable elements for checkboxes. Exclude binary `byte[]`, complex objects, date/time and immutable collections from this initial contract. Use current request culture consistently for formatting and matching form values. ModelState wins over model values; bound selection cannot be overridden by explicit values.

Empty checkbox submissions use a separate group-presence marker and scoped MVC binding support registered through `AddTagHelpers`. Disabled or omitted groups do not emit a successful marker. Groups render server validation errors. Client-side validation belongs to the consuming application.

## Implementation

- Added group and item helpers, variant enums, typed selection normalization, ModelState precedence, prefix-aware names, request-unique IDs, metadata legends/descriptions, group errors, and child-item / SelectListItem source validation.
- Reused `InputTagHelper` and `FieldTagBuilder` for options and retained the existing choice-card composition. Safelisted the conventional checkbox demo's label-weight override; no new theme rules.
- Registered checkbox presence binding and a prefix-only form value provider through `AddTagHelpers`. Empty collections have explicit empty ModelState, including when a nested checkbox collection is the only field submitted. Query-bound properties ignore form markers; absent groups retain existing values.
- The initial client-side validation implementation was removed at the user’s request. No group validation JavaScript, count attributes, `Required` helper property, or automatic native radio `required` constraints remain. Server-side model validation and error rendering remain supported.
- Fixed `FieldErrorTagHelper` merging a null framework message element when client validation is disabled and no server error exists.
- Added `/RadioGroup` and `/CheckboxGroup` samples, navigation entries, generated consumer references, and maintained browser checks in `util/visual-regression/verify-choice-groups.mjs`.
- Follow-up API revision: item values and the unbound radio selection use `string?`; unbound checkbox selections use `IEnumerable<string>?`. Plain string and numeric literals use ordinary Razor attribute syntax, while bound models retain their supported scalar/collection types.

## Verification performed on 2026-09-18

- `dotnet build src/StellarAdmin.TagHelpers --no-restore`: passed, including JS and all theme bundles.
- `dotnet build tests/StellarAdmin.TagHelpers.Tests --configuration Release --no-restore -m:1`: passed without warnings.
- `dotnet run --project tests/StellarAdmin.TagHelpers.Tests --configuration Release --no-build`: all 75 tests passed. Covers both display variants, supported typed values and collection interfaces, culture, nullable scalar selection, prefixes, errors, ModelState, empty collection redisplay, omitted groups, disabled markup, IDs, and invalid configuration.
- `dotnet run --project tests/StellarAdmin.TagHelpers.Tests --configuration Release --no-build -- --list-tests`: discovered 75 tests.
- `dotnet build docs/DocsSamples --no-restore -m:1`: passed with ten existing nullable-property warnings in unrelated Field/Textarea samples.
- `node util/visual-regression/verify-choice-groups.mjs http://localhost:5206`: passed in Chromium at 390px and 1280px, Observatory and shadcn.vega, light and dark. Verified layout, ID/label associations, collection min/max validity, reset, disabled fieldsets, successful scalar POST, clearing a prepopulated nested collection via real antiforgery-protected POST, invalid enum redisplay, and exact element/class structure parity with both existing choice-card demos. Inspected mobile screenshots; added the existing samples' width wrapper to prevent compressed standalone cards. Screenshots are in `/tmp/choice-group-screenshots`, not tracked.
- TypeScript `tsc --noEmit`: passed. Theme coverage: 56 components × 15 themes passed.
- SkillsGenerator generation and `--check`: passed. Consumer bundle validation: one skill and 64 Markdown files passed.
- CSharpier check on changed/new C# files, oxfmt on the new client module and browser script, and `git diff --check`: passed.

The sandbox prevented multiprocess MSBuild and the `dotnet test` CLI's IPC listener; serial builds and direct TUnit execution/discovery passed. Local sample and Chromium listeners ran with approved sandbox escalation on agent-owned port 5206. Port 5205 was untouched. No full-solution test run, website changes, commits, or publication were performed. Website integration remains a separate repository task.

## String value API follow-up

Changed the public value APIs to strings at the user’s request. Simplified literal-valued examples and retained the enum-valued Razor expression sample to verify its compilation. Follow-up verification passed: Release TagHelpers test build and all 75 tests, DocsSamples Razor compilation (including the enum expression and plain string/numeric attributes; ten existing unrelated nullable warnings), regenerated consumer references and drift check, consumer bundle validation, CSharpier formatting, and `git diff --check`. Browser checks listed above belong to the original implementation and were not repeated for this API-only revision.

## Validation ownership follow-up

Removed library-owned group client validation, including its bundle import, metadata-derived attributes and native radio constraints. Updated consumer guidance and automated checks; model validation attributes on the samples remain server-side rules. Earlier browser validation results above describe the original implementation, not the current API. Follow-up verification passed: Release TagHelpers build and all 76 tests (including no native required constraint on non-nullable radio models), DocsSamples compilation with the same ten unrelated warnings, client bundle rebuild and inspection confirming the removed validation code is absent, generated reference drift check, consumer bundle validation, formatting, browser-script syntax check, and `git diff --check`. The revised browser script was not rerun for this removal.

## Validation display demos follow-up

Added `Validation` and `Validation Choice Cards` examples to both group sample pages. Each page seeds errors for separate validation models in `OnGet`, following the existing Radio, Checkbox and SegmentedControl demos. The checkbox validation model uses `[MinLength(1)]`; radio uses nullable enum plus `[Required]`. The examples show error rendering without client-side validation or requiring a form submission. Added both partials to the consumer reference example manifest and updated the browser checks for the additional options and visible ModelState messages.

Verification: DocsSamples built successfully with the same ten unrelated nullable warnings; reference generation and drift check, consumer skill validation, CSharpier/oxfmt, and `git diff --check` passed. Chromium checks passed for both groups at 390px/1280px in Observatory and shadcn.vega, light/dark: both validation variants display their expected message and invalid input state, and existing POST binding/empty selection/card parity checks still pass. Inspected the mobile checkbox screenshot including both error examples. The agent-owned sample server on 5206 was stopped after verification; port 5205 was untouched.

At the user’s request, narrowed validation demos to one default-variant example per group. Removed both choice-card validation partials and their extra ModelState entries, updated the browser-check expectations, and regenerated references. DocsSamples build passed with the same ten unrelated warnings; reference drift, browser-script syntax, and `git diff --check` passed. Browser checks were not repeated for this sample-only removal.

Removed the extra `Enum.IsDefined` guards from both group helpers at the user’s request. Both now resolve `effectiveVariant` with `Variant ?? <GroupVariant>.Default`, matching existing component conventions. The Release TagHelpers test build, all 76 tests, and `git diff --check` passed.

### Website documentation export

Registered all five examples per group in DocsSamplesGenerator and supplied OrderModel and ValidationModel instances through DocsStatic. Seeded errors using the exported partial's full field names (`PartialModel.DeliveryMethod` and `PartialModel.Extras`). Exported 425 demos successfully to a staging directory. Inspected all ten group exports: model-bound radio selects Standard, checkboxes select 1 and 2, and both default validation examples render empty selections, the expected error text, and aria-invalid on each input. The generator build passed with eleven existing warnings (ten DocsSamples nullable-property warnings and one generator nullable assignment warning). Website integration and verification are recorded below when completed.

Added website pages for Radio Group and Checkbox Group with binding, items, choice cards, validation, accessibility, and API documentation, plus sidebar entries next to the existing controls. Renamed the new checkbox binding partial to `_CollectionBinding` to avoid colliding with the existing `Checkbox/_GroupModelBinding` export; updated the local page, exporter, and consumer example manifest, then regenerated consumer references. The existing checkbox grouped-boolean exports remain unchanged. Copied the ten new demos and snippets and refreshed theme assets; CSS diffs are the checkbox label font utility and related Tailwind property ordering.

Website `pnpm lint`, `pnpm types:check`, and `pnpm build` passed. The formatter excludes content by repository configuration. Chromium checks against the production output confirmed both page headings, sidebar links, five working demo URLs per page, and both default validation states; screenshots were inspected. Generated bound inputs have the expected `PartialModel` names and initial selections. Consumer generator `--check`, skill validation, and both repository diff checks passed. No commits or publication performed.

### Final documentation audit

Confirmed individual-versus-group guidance in the website pages and consumer forms, Input, CheckboxGroup, and RadioGroup references. Corrected the outdated forms statement denying group tags in the preceding follow-up. Recorded the preference for full stops instead of semicolons joining prose clauses in the Markdown conventions. The maintained plan index now reflects the 76-test result and completed website integration.

Ran the docs exporter again into a fresh staging directory. All exported snippets and assets match the website byte-for-byte. All ten group HTML exports match after comparing parsed attributes independent of ordering. Broader HTML exports contain unrelated runtime and locale differences, including generated IDs, antiforgery tokens, and numeric/currency formatting, and were not rewritten. Refreshed the website's generated inline examples so their scoped theme CSS includes the exported theme update. Website lint, type checking, and production build passed after that refresh. Consumer generator check reports no drift, consumer validation passes for 64 Markdown files, theme coverage passes for 56 components across 15 themes, and both repositories pass diff checks. No further documentation changes are required for this feature. Changes remain uncommitted and unpublished.
