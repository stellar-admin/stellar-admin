using StellarAdmin.Scenarios.Tests.Infrastructure;

namespace StellarAdmin.Scenarios.Tests.Specs;

// One test per Specs/*.spec.yaml file. Screenshot steps save only when STELLARADMIN_SCREENSHOTS is set:
// STELLARADMIN_SCREENSHOTS=1 dotnet run --project tests/StellarAdmin.Scenarios.Tests
public class SpecTests
{
    [ClassDataSource<BrowserFixture>(Shared = SharedType.PerTestSession)]
    public required BrowserFixture Browser { get; init; }

    public static IEnumerable<TestDataRow<string>> Specs() =>
        Directory
            .EnumerateFiles(
                Path.Combine(AppContext.BaseDirectory, "Specs"),
                "*.spec.yaml",
                SearchOption.AllDirectories
            )
            .Order()
            .Select(file => Path.GetRelativePath(AppContext.BaseDirectory, file))
            .Select(file => new TestDataRow<string>(
                file,
                DisplayName: Path.GetFileName(file).Replace(".spec.yaml", "")
            ));

    [Test]
    [MethodDataSource(nameof(Specs))]
    public async Task Spec_StepsPass(string file)
    {
        // Arrange
        var spec = SpecLoader.Load(file);
        var (app, baseUrl) = await ScenarioApp.StartKestrelAsync(spec.Scenario);
        await using var _ = app;
        var run = new SpecRun(await Browser.NewPageAsync(baseUrl), ScreenshotDirectory());

        // Act
        var completed = 0;
        foreach (var step in spec.Steps)
        {
            try
            {
                await step.RunAsync(run);
            }
            catch (Exception exception) when (exception is not SpecException)
            {
                throw new SpecException(
                    spec.Path,
                    step.Line,
                    $"{step.Kind}: {exception.Message}",
                    exception
                );
            }

            completed++;
        }

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
