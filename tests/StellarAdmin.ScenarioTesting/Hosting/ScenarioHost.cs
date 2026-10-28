using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StellarAdmin.ScenarioTesting.Hosting;

public static class ScenarioHost
{
    // Builds the same application for browser tests and the interactive host; callers
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
        builder.Services.AddVoyagerTravelDatabase();
        builder.Services.AddScenarioSignIn();
        scenario.ConfigureServices(builder.Services);
        builder.Services.AddStellarAdmin(scenario.ConfigureAdmin);
        configureHost?.Invoke(builder);

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapStaticAssets();
        app.MapScenarioSignIn();
        app.MapStellarAdmin();
        app.MapGet("/", () => Results.Redirect(scenario.StartPath));

        return app;
    }

    // Real server on a free loopback port for the browser; logs only warnings and errors, to the
    // providers configureLogging leaves
    public static async Task<(WebApplication App, string BaseUrl)> StartLoopbackAsync(
        Scenario scenario,
        Action<ILoggingBuilder>? configureLogging = null
    )
    {
        var app = Create(
            scenario,
            configureHost: builder =>
            {
                builder.WebHost.UseUrls("http://127.0.0.1:0");
                builder.Logging.SetMinimumLevel(LogLevel.Warning);
                configureLogging?.Invoke(builder.Logging);
            }
        );
        await app.StartAsync();

        return (app, app.Urls.First());
    }
}
