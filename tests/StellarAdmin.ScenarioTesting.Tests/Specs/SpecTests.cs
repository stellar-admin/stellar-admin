using System.Reflection;
using StellarAdmin.ScenarioTesting.Runner;
using StellarAdmin.ScenarioTesting.Tests.Infrastructure;

namespace StellarAdmin.ScenarioTesting.Tests.Specs;

// One test per Specs/*.spec.yaml file, read from this project's source folder. Screenshot steps save only when
// STELLARADMIN_SCREENSHOTS is set:
// STELLARADMIN_SCREENSHOTS=1 dotnet run --project tests/StellarAdmin.Scenarios.Tests
public class SpecTests
{
    [ClassDataSource<BrowserFixture>(Shared = SharedType.PerTestSession)]
    public required BrowserFixture Browser { get; init; }

    // The source folder, so edits to a spec take effect without a build
    private static readonly string ProjectDirectory = typeof(SpecTests)
        .Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
        .Single(attribute => attribute.Key == "ProjectDirectory")
        .Value!;

    // Paths relative to the project, e.g. Specs/branding.spec.yaml
    public static IEnumerable<TestDataRow<string>> Specs() =>
        SpecFiles
            .Find(Path.Combine(ProjectDirectory, "Specs"))
            .Select(file => Path.GetRelativePath(ProjectDirectory, file))
            .Select(file => new TestDataRow<string>(file, DisplayName: SpecFiles.Name(file)));

    [Test]
    [MethodDataSource(nameof(Specs))]
    public async Task Spec_StepsPass(string file)
    {
        // Arrange
        var spec = SpecLoader.Load(Path.Combine(ProjectDirectory, file), file);

        // Act
        var completed = await SpecRunner.RunAsync(spec, Browser.Browser, ScreenshotDirectory());

        // Assert
        await Assert.That(completed).IsEqualTo(spec.Steps.Count);
    }

    // artifacts/screenshots at the repository root unless STELLARADMIN_SCREENSHOT_DIR says otherwise
    private static string? ScreenshotDirectory()
    {
        if (Environment.GetEnvironmentVariable("STELLARADMIN_SCREENSHOTS") is null)
        {
            return null;
        }

        if (Environment.GetEnvironmentVariable("STELLARADMIN_SCREENSHOT_DIR") is { } configured)
        {
            return configured;
        }

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (
            directory is not null && !File.Exists(Path.Combine(directory.FullName, "global.json"))
        )
        {
            directory = directory.Parent;
        }

        return Path.Combine(
            directory?.FullName ?? AppContext.BaseDirectory,
            "artifacts",
            "screenshots"
        );
    }
}
