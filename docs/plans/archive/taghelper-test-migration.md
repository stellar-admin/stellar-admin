# TagHelper runner migration

## Scope — 2026-09-18

Replace `tests/StellarAdmin.TagHelpers.Tests/Program.cs` with discoverable TUnit tests, preserving its checks while moving Core behavior into the owning test project. Use SUT-based classes, matching production folders, behavior-specific partial files, independent mutable state, and explicit arrange–act–assert sections. No production changes or migration of the EntityFrameworkCore executable are in scope.

## Coverage inventory

Paths below are relative to the named test project. Each row accounts for checks in the removed runner; related assertions remain together when they describe one outcome.

| Old runner checks | Replacement in StellarAdmin.Core.Tests |
| --- | --- |
| Case-insensitive custom lookup and icon-name listing | `Icons/IconOptionsTests.Registration.cs`: `TryGetIcon_WhenNameCasingDiffers_ReturnsRegisteredIcon`, `GetIconNames_WhenCustomIconIsRegistered_IncludesName`. |
| Default Lucide shapes and duplicate default/custom names | `Icons/IconOptionsTests.Registration.cs`: constructor test and two duplicate-name cases. |
| Same options within a provider; distinct options between providers; no custom icons or pack overrides leaking between independent collections | `StellarAdminExtensionsTests.IconOptions.cs`: resolution, shared-collection provider, and independent-collection tests. |
| Repeated registration preserves overrides; direct Configure removes activity | `StellarAdminExtensionsTests.IconOptions.cs`: repeated registration and direct configuration tests. |
| Builder custom registration; later registration does not mutate resolved options; new provider includes later registration | `StellarAdminBuilderTests.Icons.cs`: registration and two late-registration scenarios. |
| Later pack wins; duplicate names throw at options resolution | `StellarAdminBuilderTests.Icons.cs`: pack ordering and two duplicate-name cases, resolving options directly without an IconTagHelper. |

| Old runner checks | Replacement in StellarAdmin.TagHelpers.Tests |
| --- | --- |
| AddTagHelpers preserves configured icons and overrides | `StellarAdminTagHelpersExtensionsTests.cs`. |
| Constructor resolves options once; rendering reuses options for registered/missing names; SVG shapes and fallback glyph | `TagHelpers/Icon/IconTagHelperTests.cs`. |
| Root, label, description, error, control, and content classes; explicit input classes; suppressed fields for text/checkbox/radio | `TagHelpers/Input/InputTagHelperTests.ClassNames.cs`: four parameterized methods, retaining the text versus checkbox/radio element differences. |
| Submitted value, custom bound-control/error classes, MVC validation class, and ClassAttributeHtmlContent regression | `TagHelpers/Input/InputTagHelperTests.ModelBinding.cs`: submitted-value, validation, and class-content tests. |
| Select custom and explicit wrapper classes; textarea and switch control classes; slider control/track/range/thumb classes | Each component's `TagHelpers/<Component>/<Component>TagHelperTests.ClassNames.cs`. |
| Generated OTP classes and counts; composed OTP classes and counts; caret and caret-line attributes in both modes | `TagHelpers/InputOtp/InputOtpTagHelperTests.ClassNames.cs`, `.Composition.cs`, and `.cs`. |
| Toggle container class, inherited item class plus explicit class, selected item | `TagHelpers/ToggleGroup/ToggleGroupTagHelperTests.ClassNames.cs`, `ToggleGroupItemTagHelperTests.ClassNames.cs`, and `ToggleGroupItemTagHelperTests.cs`. |

No old scenarios are intentionally dropped. Direct IconOptions duplicate tests and explicit builder registration tests additionally separate the underlying contracts from DI behavior. HTML assertions inspect parsed elements and class tokens instead of string order or regex. Existing IconOptions pack replacement coverage remains; the builder case separately exercises deferred registration ordering.

## Implementation and verification

TUnit 1.68.4 and AngleSharp 1.8.1 were added to the existing TagHelpers project with the .NET CLI and central package management. Core tests gained Microsoft.Extensions.DependencyInjection 10.0.10 through the CLI, matching the existing abstraction package. No new project was needed. The old entry point and handwritten assertions were removed; TUnit supplies discovery and the entry point. Shared support contains rendering/MVC setup only, and each test owns and disposes its state. Documentation and the release step identify both TUnit suites.

Verified locally with the pinned .NET SDK 10.0.400:

- Release builds of both test projects with `dotnet build tests/<Project> --configuration Release --no-restore -m:1`: zero warnings and errors.
- `dotnet run --project tests/StellarAdmin.Core.Tests --no-build --configuration Release`: 66 passed, zero failed or skipped (50 existing cases plus 16 new cases).
- `dotnet run --project tests/StellarAdmin.TagHelpers.Tests --no-build --configuration Release`: 31 passed, zero failed or skipped.
- Both commands with `-- --list-tests`: verified discovery of 66 Core and 31 TagHelpers cases. The run commands match the release workflow.
- CSharpier formatting of touched C# and package files; AAA marker audit; affected Markdown links and task diff whitespace checks.

The first build identified the missing concrete DI package in Core; the first rendering run identified unnecessary Razor dependencies from AddMvc. The final fixture uses AddMvcCore().AddViews() and all rendering tests pass without a host. One TUnit analyzer recommendation was corrected; no analyzers were suppressed.

Hosted CI and the unrelated EntityFrameworkCore suite were not run. The pre-existing user edit in `StellarAdmin.sln.DotSettings` remains untouched and excluded from the task's whitespace check.
