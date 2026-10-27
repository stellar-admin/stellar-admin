using Microsoft.Playwright;
using TUnit.Core.Interfaces;

namespace StellarAdmin.Scenarios.Tests.Infrastructure;

// One browser per test session; each test gets its own context
public sealed class BrowserFixture : IAsyncInitializer, IAsyncDisposable
{
    private IBrowser? _browser;
    private IPlaywright? _playwright;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(
            new() { ExecutablePath = FindChromium() }
        );
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.DisposeAsync();
        }

        _playwright?.Dispose();
    }

    // Fixed viewport, light scheme and no motion, so screenshots are repeatable
    public async Task<IPage> NewPageAsync(string baseUrl)
    {
        var context = await _browser!.NewContextAsync(
            new()
            {
                BaseURL = baseUrl,
                ColorScheme = ColorScheme.Light,
                ReducedMotion = ReducedMotion.Reduce,
                ViewportSize = new() { Width = 1280, Height = 800 },
            }
        );

        return await context.NewPageAsync();
    }

    // Uses an installed Chromium instead of downloading Playwright's; null falls back to Playwright's
    private static string? FindChromium() =>
        Environment.GetEnvironmentVariable("STELLARADMIN_CHROMIUM")
        ?? (File.Exists("/usr/bin/chromium") ? "/usr/bin/chromium" : null);
}
