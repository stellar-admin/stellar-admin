using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Scenarios;

namespace StellarAdmin.Scenarios.Tests.Infrastructure;

internal static class ScenarioApp
{
    // In-memory server for HTTP tests
    public static async Task<WebApplication> StartTestServerAsync(Scenario scenario)
    {
        var app = ScenarioHost.Create(
            scenario,
            configureHost: builder => builder.WebHost.UseTestServer()
        );
        await app.StartAsync();

        return app;
    }

    // Real server on a free loopback port, for the browser
    public static async Task<(WebApplication App, string BaseUrl)> StartKestrelAsync(
        Scenario scenario
    )
    {
        var app = ScenarioHost.Create(
            scenario,
            configureHost: builder => builder.WebHost.UseUrls("http://127.0.0.1:0")
        );
        await app.StartAsync();

        return (app, app.Urls.First());
    }
}
