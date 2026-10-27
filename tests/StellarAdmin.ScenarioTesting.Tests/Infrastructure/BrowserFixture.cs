using StellarAdmin.ScenarioTesting.Runner;
using TUnit.Core.Interfaces;

namespace StellarAdmin.ScenarioTesting.Tests.Infrastructure;

// One browser per test session
public sealed class BrowserFixture : IAsyncInitializer, IAsyncDisposable
{
    private SpecBrowser? _browser;

    public SpecBrowser Browser =>
        _browser ?? throw new InvalidOperationException("The browser has not started.");

    public async Task InitializeAsync() => _browser = await SpecBrowser.LaunchAsync();

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.DisposeAsync();
        }
    }
}
