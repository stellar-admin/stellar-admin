using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.UI.IntegrationTests.Infrastructure;

internal static class ToastTestHost
{
    public static async Task<WebApplication> CreateAsync()
    {
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions
            {
                ApplicationName = typeof(ToastTestHost).Assembly.GetName().Name,
                EnvironmentName = "Development",
            }
        );
        builder.WebHost.UseTestServer();
        builder.Services.AddControllers();
        builder.Services.AddRazorPages();
        builder.Services.AddStellarAdminUI();

        var app = builder.Build();
        app.MapControllers();
        app.MapRazorPages();
        await app.StartAsync();

        return app;
    }
}
