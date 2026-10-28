using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StellarAdmin.ScenarioTesting.Hosting;

// Runs the host application on Kestrel with the scenario on top: the Voyager Travel database, sign-in, the
// scenario's services and admin panel, and a startup filter that puts authentication, the sign-in pages and
// the admin panel in front of the host's own pipeline
internal sealed class ScenarioApplicationFactory(
    Scenario scenario,
    Action<IWebHostBuilder> configureHost
) : WebApplicationFactory<Host.Program>
{
    private static readonly string HostProjectDirectory = typeof(Host.Program)
        .Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
        .Single(attribute => attribute.Key == "ProjectDirectory")
        .Value!;

    // The addresses Kestrel listens on, once the server has started
    public IReadOnlyCollection<string> Addresses =>
        [
            .. Services
                .GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!
                .Addresses,
        ];

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Development serves the referenced projects' static web assets from the build output
        builder.UseEnvironment(Environments.Development);
        builder.UseContentRoot(HostProjectDirectory);
        builder.ConfigureTestServices(services =>
        {
            services.AddVoyagerTravelDatabase();
            services.AddScenarioSignIn();
            scenario.ConfigureServices(services);
            services.AddStellarAdmin(scenario.ConfigureAdmin);
            services.AddTransient<IStartupFilter>(_ => new StartupFilter(scenario));
        });
        configureHost(builder);
    }

    private sealed class StartupFilter(Scenario scenario) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
            app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapScenarioSignIn();
                    endpoints.MapStellarAdmin();
                    endpoints.MapGet("/", () => Results.Redirect(scenario.StartPath));
                });
                next(app);
            };
    }
}
