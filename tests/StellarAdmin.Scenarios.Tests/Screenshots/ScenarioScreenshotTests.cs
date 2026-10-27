using Microsoft.Playwright;
using StellarAdmin.Scenarios.Tests.Infrastructure;

namespace StellarAdmin.Scenarios.Tests.Screenshots;

// Explicit, so ordinary test runs skip it; run with
// dotnet run --project tests/StellarAdmin.Scenarios.Tests -- --treenode-filter "/*/*/ScenarioScreenshotTests/*"
[Explicit]
public class ScenarioScreenshotTests
{
    [ClassDataSource<BrowserFixture>(Shared = SharedType.PerTestSession)]
    public required BrowserFixture Browser { get; init; }

    public static IEnumerable<Func<Scenario>> Scenarios() =>
        ScenarioCatalog.All.Select(scenario => (Func<Scenario>)(() => scenario));

    [Test]
    [MethodDataSource(nameof(Scenarios))]
    public async Task StartPage_SavesScreenshot(Scenario scenario)
    {
        // Arrange
        var (app, baseUrl) = await ScenarioApp.StartKestrelAsync(scenario);
        await using var _ = app;
        var page = await Browser.NewPageAsync(baseUrl);
        var path = Path.Combine(ScreenshotDirectory(), $"{scenario.Name}.png");
        await page.GotoAsync(scenario.StartPath);

        // Act
        await page.ScreenshotAsync(
            new() { Path = path, Animations = ScreenshotAnimations.Disabled }
        );

        // Assert
        await Assert.That(File.Exists(path)).IsTrue();
    }

    // artifacts/screenshots at the repository root unless STELLARADMIN_SCREENSHOT_DIR says otherwise
    private static string ScreenshotDirectory()
    {
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
