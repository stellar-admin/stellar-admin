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
}
