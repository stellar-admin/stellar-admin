# Dashboard and EF test migration

## Scope and ownership — 2026-09-18

Replace the final handwritten executable test runner with independently discoverable TUnit tests and remove all per-project test invocations from CI and release. Preserve the runner's coverage while distinguishing genuine unit tests from HTTP, Razor, database, and migration integration tests. No production behavior, package version, or application database changes are in scope.

| Project | Responsibility |
| --- | --- |
| `tests/StellarAdmin.Dashboard.Tests` | Pure `FormFieldsBuilder` and `FormFieldBuilder` unit tests under `Resources/Builders/`, referencing Dashboard directly. |
| `tests/StellarAdmin.Dashboard.IntegrationTests` | Dashboard editor templates, scalar model binding, and shared form views through the MVC pipeline; tests mirror `Areas/StellarAdmin/` and its shared views. |
| `tests/StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests` | `EfCoreResourceController` CRUD, authorization, antiforgery, references, relational SQL behavior, and playground compatibility. |
| `tests/StellarAdmin.Dashboard.Testing` | Shared host construction and test-only controllers/models/DbContexts for the two integration suites. This class library contains no tests or assertion replacements. |

The removed `StellarAdmin.Dashboard.EntityFrameworkCore.Tests` project mixed all of these responsibilities. Integration checks remain integration tests because replacing real SQLite, migrations, MVC binding, and Razor execution with mocks would remove the contracts they protect. `PlaygroundCompatibilityTests` explicitly covers the sample host's migration model, fluent metadata, and Identity pages; it is not presented as an EF library unit test.

All projects, NuGet references, production references, and solution membership were created or changed through the .NET CLI. The new suites use existing centrally managed TUnit 1.68.4 and AngleSharp 1.8.1. All test methods have one arrange–act–assert sequence, framework assertions, and fresh state. Former loops over independent cases became TUnit argument/data-source cases. Shared support has no handwritten Check/Require/Run test runner.

## Isolation and execution

Each hosted test owns its WebApplicationFactory, client, service scope, optional migrated SQLite database, and SQL command interceptor. DbContext configuration is replaced inside the host rather than through a process environment variable. Connections disable pooling; disposal removes the unique temporary database and sidecar files. Data protection remains ephemeral. App-level form layout is configured before host creation, with no mutation of options shared between tests. Each integration assembly permits four concurrent hosts to bound memory and I/O while retaining independent execution. No fixed TCP port, external service, or running playground is required.

Both workflows now use the same solution command for all unit and integration suites:

```bash
dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --minimum-expected-tests 1
```

The legacy EF runner step and project have been removed after the replacement suites passed. Microsoft.Testing.Platform automatically discovers future TUnit projects added to the solution. The five test assemblies are Core, TagHelpers, Dashboard unit tests, Dashboard integration tests, and EF integration tests; the support library is not a test assembly.

## Coverage inventory

Each row records the original assertions and their replacements. No existing scenarios were intentionally dropped. Related assertions remain together where they describe one outcome; cases no longer depend on earlier tests creating or modifying records.

| Legacy source / scenario | Replacement |
| --- | --- |
| `FormLayoutChecks.CheckConfiguration`: seeded field order through section/group/row, retained containers and nested read-only field | `Dashboard.Tests/Resources/Builders/FormFieldsBuilderTests.Layout.cs`: `AddSection_WhenContainersAreNested_PreservesSeededFieldOrderAndStructure`. |
| Nested Clear preserves siblings; root Clear removes containers and fields | `FormFieldsBuilderTests.Layout.cs`: the two Clear scenarios. |
| Nested template/title/editor settings, field option isolation, interleaved common/typed editor options, incompatible option error identifies field and types | `FormFieldBuilderTests.Editor.cs`: four focused tests. |
| Migration from initial Identity schema preserves existing role; current model matches migrations | `EntityFrameworkCore.IntegrationTests/PlaygroundCompatibilityTests.cs`: migration test. |
| Every Category/Product property has metadata label and help text; existing Identity user/role pages work | `PlaygroundCompatibilityTests.cs`: property data source and two Identity route cases. |
| Categories index/title and independent second EF resource routes/configuration | `EfCoreResourceControllerTests.Crud.cs`: parameterized index scenario. |
| String-key role edit and nested read-only binding protection | `EfCoreResourceControllerTests.Crud.cs`: string-key/read-only edit scenario. |
| Required fields and maximum lengths redisplay without persistence; rendered validation message | `EfCoreResourceControllerTests.Crud.cs`: invalid-input cases and required-message test. |
| Category create/edit ignore forged keys, redirect, and persist; malformed/missing edit keys return 404; delete redirects and removes row | `EfCoreResourceControllerTests.Crud.cs`: independent CRUD and key cases. |
| Create/delete enforce antiforgery | `EfCoreResourceControllerTests.Crud.cs`: separate missing-token scenarios. |
| Sorting and last-page clamping, search filtering | `EfCoreResourceControllerTests.Crud.cs`: separate index-query scenarios. |
| Policy protects index/create/edit GET and create/edit/delete POST | `EfCoreResourceControllerTests.Authorization.cs`: six route cases. |
| Product missing SKU/negative price and undefined enum rejected | `EfCoreResourceControllerTests.Products.cs`: invalid-product cases. |
| Product scalar/enum/reference values persist; read-only CreatedAt ignored | `EfCoreResourceControllerTests.Products.cs`: create persistence scenario. |
| Product SKU search and published scope; edit changes enum and clears nullable fields; delete persists | `EfCoreResourceControllerTests.Products.cs`: independent index/edit/delete scenarios. |
| `ReferenceChecks`: help, nullable prompt, filtered choices and label ordering | `EfCoreResourceControllerTests.References.cs`: nullable-reference rendering scenario. |
| Missing/filtered/malformed reference rejected; invalid redisplay reloads choices; other invalid fields retain selection | `.References.cs`: three invalid-key cases and ModelState-selection test. |
| Reference create persists; nullable reference clears or changes | `.References.cs`: create and two edit cases. |
| Index sorts by reference labels with included labels, only count/rows queries and no lookup queries | `.References.cs`: index label/order/SQL test, seeding keys in the opposite order to labels. |
| Edit selects saved key, includes assigned label and projects only choice key/label | `.References.cs`: edit selection and SQL-projection test. |
| Filtered current label stays visible; resubmitting that key rejects all edits | `.References.cs`: filtered-label and rejected-edit tests. |
| Required prompt/empty rejection/valid persistence; read-only disabled selection and forged-post protection; unused references do not load data | `.References.cs`: six independent required/read-only/unused-reference scenarios. |
| `EditorChecks`: every scalar property has styled field/control/label targets, ordinary label and styled control | `Dashboard.IntegrationTests/Areas/StellarAdmin/FormFieldsBaseViewTests.ScalarEditors.cs`: per-property data cases. |
| Integral/fractional input type/step; decimal/currency/UInt64 precision; date/time/offset formats | `.ScalarEditors.cs`: parameterized type/step and precision/format cases. |
| Null decimal; checked boolean; disabled read-only boolean/enum; metadata description and enum display/selection; empty nullable choices; true/false nullable selection; email/URL inputs; password non-echo | `.ScalarEditors.cs`: focused or parameterized scenarios. |
| Attempted invalid value, validation text and error class; description/content classes | `.ScalarEditors.cs`: invalid-redisplay and part-class tests. |
| Both radio templates: all typed parts, nullable choice, description, validation once, per-option controls; common-only options; flags fallback; incompatible typed options | `FormFieldsBaseViewTests.RadioEditors.cs`: eight cases across template/configuration variants. |
| Posted scalar precision/offset/UInt64 and nullable clearing | `FormFieldsBaseViewTests.Binding.cs`: scalar roundtrip and six nullable-property cases. |
| App/form layout cascade for create/edit inside groups, shared form/section layout and original field binding | `Areas/StellarAdmin/Views/Shared/FormPageTests.Layout.cs`: ten argument cases. |
| Product section order/description, fieldset, rows/grid markup, default split layout, no legacy column counts or unexecuted TagHelpers, every original binding prefix once | `FormPageTests.Products.cs`: markup test for create/edit/invalid redisplay. |
| Product enum choice container/cards, selection, descriptions, matching labels and help once | `.Products.cs`: condition test for create/default, edit/persisted, and invalid/attempted selection. |
| Product metadata and section help; read-only CreatedAt; Category metadata labels/help and multiline editor | `.Products.cs`: separate metadata scenarios. |
| Product and Identity user/role create/edit actions: index cancel link/style, delete absent on create, single quiet non-submit trigger before cancel on edit, destructive confirmation | `FormPageTests.Actions.cs`: create/edit/route cases. |

## Verification

Verified with the pinned SDK 10.0.400:

- Replacement suites passed individually before removing the legacy project: 7 Dashboard unit cases, 149 Dashboard integration cases, and 72 EF integration cases.
- `dotnet build StellarAdmin.slnx --configuration Release --no-restore -m:1` succeeded with zero errors. It reported 22 existing warnings from sample/generator nullable/XML documentation and the existing SQLitePCLRaw.lib.e_sqlite3 2.1.11 advisory (NU1903); no package upgrades or warning suppressions were introduced. The final Dashboard unit-project rebuild had zero warnings or errors.
- The exact shared CI/release command passed 325 tests across five assemblies, with zero failures or skips: Core 66, TagHelpers 31, Dashboard unit 7, Dashboard integration 149, EF integration 72.
- `dotnet test --solution StellarAdmin.slnx --no-build --configuration Release --list-tests` reported 325 tests in five assemblies, confirming that the support library and removed runner are not test assemblies.
- CSharpier formatted all touched C# files and preserved central package formatting. An audit confirmed the 72 new test methods have matching Arrange/Act/Assert markers and no handwritten assertion replacements. Both workflow YAML files parse and contain one solution test command and no per-project test invocation. Maintained documentation links and `git diff --check` pass.

Integration execution and solution discovery require local IPC/test-host permissions unavailable in the agent sandbox; verification used sandbox escalation. Hosted GitHub Actions, publishing, and browser visual checks were not run. No production source or playground database was changed.

Historical migration commands and results in related plans remain intact, with current-status notes linking here. Maintained development, testing, release, product, and generic-resource follow-up guidance now uses discoverable TUnit suites and documents integration isolation.
