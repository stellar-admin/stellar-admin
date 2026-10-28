using Microsoft.Playwright;

namespace StellarAdmin.ScenarioTesting.Runner;

// One Chromium shared by every spec run; each run gets its own context
public sealed class SpecBrowser : IAsyncDisposable
{
    private readonly IBrowser _browser;
    private readonly IPlaywright _playwright;

    private SpecBrowser(IPlaywright playwright, IBrowser browser)
    {
        _playwright = playwright;
        _browser = browser;
    }

    public static async Task<SpecBrowser> LaunchAsync()
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(
            new() { ExecutablePath = FindChromium() }
        );

        return new(playwright, browser);
    }

    public async ValueTask DisposeAsync()
    {
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    // Fixed viewport, light scheme and no motion, so screenshots are repeatable; a video directory records
    // the whole context at the viewport size
    public Task<IBrowserContext> NewContextAsync(string baseUrl, string? videoDirectory = null) =>
        _browser.NewContextAsync(
            new()
            {
                BaseURL = baseUrl,
                ColorScheme = ColorScheme.Light,
                RecordVideoDir = videoDirectory,
                RecordVideoSize = videoDirectory is null
                    ? null
                    : new() { Width = 1280, Height = 800 },
                ReducedMotion = ReducedMotion.Reduce,
                ViewportSize = new() { Width = 1280, Height = 800 },
            }
        );

    // Uses an installed Chromium instead of downloading Playwright's; null falls back to Playwright's
    private static string? FindChromium() =>
        Environment.GetEnvironmentVariable("STELLARADMIN_CHROMIUM")
        ?? (File.Exists("/usr/bin/chromium") ? "/usr/bin/chromium" : null);
}
