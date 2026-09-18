# Unit testing

Conventions for new and migrated .NET tests in this repository. Existing executable checks are migration inputs, not templates for new tests. Apply these conventions as each SUT (system under test) is migrated; do not expand a focused change into an unrelated test-suite rewrite.

## Project and folder ownership

Place unit tests in `tests/<ProductionProject>.Tests/` and mirror the SUT's directory within its production project. Use the test project and folder for the namespace, even when the production namespace differs from the project name.

For example, `src/StellarAdmin.Core/Icons/IconOptions.cs` maps to `tests/StellarAdmin.Core.Tests/Icons/`, with namespace `StellarAdmin.Core.Tests.Icons`. Reference the owning production project directly. Core unit tests must not depend on TagHelpers just to reach Core transitively.

Place each test according to the behavior it verifies. Icon registration and mapping belong with `IconOptions`; builder and DI registration behavior belongs with the relevant Core registration type; rendered pagination icons belong with `PaginationEllipsisTagHelper` in the corresponding TagHelpers folder. A feature spanning projects does not justify a single feature-wide test class.

Tests requiring an application host, database, or HTTP boundary belong in explicitly named `<ProductionProject>.IntegrationTests` projects. Document their environment and execution requirements separately. Using a small, in-memory service collection to test registration does not by itself require a hosted integration test.

## Class, file, and method names

Name the test class `<SutType>Tests`. Start with `<SutType>Tests.cs`; when behavioral subdivisions help navigation, use a partial class and dot-separated file names:

```text
tests/StellarAdmin.Core.Tests/
  Icons/
    IconOptionsTests.cs
    IconOptionsTests.IconPacks.cs
    IconOptionsTests.SemanticIcons.cs
```

All these files declare `public partial class IconOptionsTests`. Create only files that contain useful tests or support; a subdivided class does not require an otherwise empty `IconOptionsTests.cs`. Keep the same namespace across all parts. Use behavior names for subdivisions, not numbered files. Do not introduce nested test classes or regions solely for categorization. Partial files are source organization; they remain one test class in the test explorer.

Name test methods `Member_Scenario_ExpectedOutcome`, using `Constructor` for construction behavior. Examples:

- `AddIconPack_WhenMappingTargetIsMissing_PreservesExistingRegistrations`
- `RemoveIcon_WhenNameCasingDiffers_RemovesAssociatedMappings`
- `MapSemanticIcon_WhenIconIsUnregistered_ThrowsArgumentException`

Follow [C# file organization](csharp-file-organization.md) within each partial file. Keep test methods before private helpers and nested fake types.

## Framework

Use TUnit for new and migrated .NET tests, with individually discoverable `[Test]` methods and its built-in assertions. Manage package versions in `Directory.Packages.props`. Await TUnit assertions; see its [assertion guidance](https://tunit.dev/docs/assertions/getting-started/).

Do not introduce handwritten test runners, console success messages, or assertion replacements such as `Run()`, `Require()`, and `Reject()`. Use framework discovery, assertions, exception assertions, and parameterized cases. Keep framework analyzers enabled and resolve diagnostics in changed tests.

## Arrange, act, assert

Every test method must contain one arrange–act–assert sequence with the exact section comments `// Arrange`, `// Act`, and `// Assert`. Separate the sections with blank lines. Arrange establishes the scenario; Act performs the operation under test; Assert checks the observable outcome. Keep all three markers even when a section needs no statements.

Keep the operation under test visible in the method. Setup may call other SUT operations to establish the scenario. Observing the resulting state in Assert is appropriate; do not continue mutating the SUT through unrelated scenarios there. Use `sut` for the primary object under test and descriptive names for inputs and results. Explicit locals that clarify these sections take precedence over the general preference to inline single-use locals.

```csharp
using StellarAdmin.Icons;
using TUnit.Assertions;
using TUnit.Core;

namespace StellarAdmin.Core.Tests.Icons;

public partial class IconOptionsTests
{
    [Test]
    public async Task ClearIcons_WhenSemanticIconsAreMapped_RemovesMapping()
    {
        // Arrange
        var sut = new IconOptions();

        // Act
        sut.ClearIcons();

        // Assert
        await Assert.That(sut.GetSemanticIconName(SemanticIconRole.Close)).IsNull();
    }
}
```

For an expected exception, define an `Action` or `Func<Task>` in Act and execute it through the framework's exception assertion in Assert. This is the explicit exception to executing the operation directly in Act. Do not invoke it twice or wrap setup in the exception assertion. See TUnit's [exception assertions](https://tunit.dev/docs/assertions/exceptions/).

```csharp
[Test]
public async Task MapSemanticIcon_WhenIconIsUnregistered_ThrowsArgumentException()
{
    // Arrange
    var sut = new IconOptions();
    sut.ClearIcons();

    // Act
    Action act = () => sut.MapSemanticIcon(SemanticIconRole.Close, "unregistered");

    // Assert
    await Assert.That(act).Throws<ArgumentException>();
}
```

## Independent, focused scenarios

Test one behavior per method. Multiple assertions are appropriate when they describe one outcome: rejecting an invalid icon pack may need assertions that both icons and semantic mappings remain unchanged. Separate unrelated behaviors into different methods so they run and report failures independently.

Use parameterized tests when the scenario and operation are identical and only input or expected values change. Do not put independent cases in a loop inside one test. A collection assertion verifying a single outcome is appropriate.

Create fresh mutable SUT instances and data for each test. Tests must pass alone, in any order, and alongside other tests. Avoid shared mutable fixtures, execution dependencies, sleeps, and dependence on machine-specific data. TUnit [runs tests in parallel by default](https://tunit.dev/docs/execution/parallelism/); fix isolation instead of disabling parallelism to accommodate shared unit-test state. Dispose resources owned by the test.

Keep setup understandable within the method. Small private helpers and nested fake types are appropriate; extract shared support only when multiple classes need it, and keep it within the owning test project. Avoid a general-purpose test base class or helper that hides the behavior being exercised. Prefer simple real values and small fakes over unnecessary mocking infrastructure.

## Assertions and coverage

Assert observable contracts, including relevant boundaries, rejected inputs, and preservation of existing state after failure. Do not assert private implementation details or merely reproduce the implementation's algorithm to calculate an expected result.

Use specific assertions on values and collections rather than compound Boolean expressions that hide which expectation failed. For rendering, assert the relevant element, attribute, content, or fallback behavior without coupling the test to irrelevant HTML formatting. Verify exception types and other documented exception details; avoid incidental message wording.

## Migration and verification

Before replacing an existing runner or large test method, inventory its checks and map each to a replacement test under the appropriate SUT. Preserve distinct cases even when the old runner intertwined their state. Record intentional coverage changes explicitly.

Add new test projects to the solution and the repository's test execution paths. Verify the selected TUnit version and runner configuration against the pinned SDK, and document the actual commands in [development and verification](../development.md). Update affected project READMEs and CI/release test steps as part of migration. Do not assume a successful build or process exit proves that tests were discovered.

Run the affected tests, verify discovery and the expected cases, and ensure the CI command executes the migrated suite. Remove replaced runner code only after its replacement coverage passes. During incremental migration, keep the remaining legacy checks executable. Record checks actually performed and any outstanding environment or CI verification; do not describe planned checks as completed.

## Review checklist

- Does the project, folder, namespace, and class identify the owning SUT, with dot-separated partial files only where useful?
- Is every case discoverable through TUnit and named `Member_Scenario_ExpectedOutcome`?
- Does every test have one clear `// Arrange`, `// Act`, and `// Assert` sequence?
- Can each test run independently with fresh mutable state and useful failure diagnostics?
- Are assertions focused on one observable behavior, including relevant failure cases?
- For migrations, are existing checks accounted for and solution, documentation, and CI execution paths updated and verified?
