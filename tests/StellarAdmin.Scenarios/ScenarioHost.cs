using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace StellarAdmin.Scenarios;

public static class ScenarioHost
{
    // Builds the same application for TestServer, browser tests and the interactive host; callers
    // choose the server through configureHost
    public static WebApplication Create(
        Scenario scenario,
        string[]? args = null,
        Action<WebApplicationBuilder>? configureHost = null
    )
    {
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions
            {
                Args = args ?? [],
                // Development serves the referenced projects' static web assets from the build output
                EnvironmentName = Environments.Development,
            }
        );
        scenario.ConfigureServices(builder.Services);
        builder.Services.AddStellarAdmin(scenario.ConfigureAdmin);
        configureHost?.Invoke(builder);

        var app = builder.Build();
        app.MapStaticAssets();
        app.MapStellarAdmin();
        app.MapGet("/", () => Results.Redirect(scenario.StartPath));

        return app;
    }
}
