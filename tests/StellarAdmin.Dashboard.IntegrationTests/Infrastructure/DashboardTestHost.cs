using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.Resources.Builders;

namespace StellarAdmin.Dashboard.IntegrationTests.Infrastructure;

internal static class DashboardTestHost
{
    public static async Task<WebApplication> CreateAsync(
        ProductState state,
        Action<ResourceBuilder<Product>>? configure = null,
        Action<StellarAdminDashboardBuilder>? configureDashboard = null
    )
    {
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions
            {
                ApplicationName = typeof(DashboardTestHost).Assembly.GetName().Name,
                EnvironmentName = "Development",
            }
        );
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(state);
        builder
            .Services.AddStellarAdmin()
            .AddDashboard(dashboard =>
            {
                dashboard.AddResource<Product>(resource =>
                {
                    resource.UseDataSource<ProductDataSource>();
                    resource.Index(index =>
                        index.Columns(columns =>
                        {
                            columns.Add(product => product.Name);
                            columns.Add(
                                product => product.Price,
                                column =>
                                {
                                    column.Title = "Unit price";
                                    column.Format = "{0:0.00}";
                                }
                            );
                        })
                    );
                    resource.Create(create =>
                        create.Fields(fields =>
                        {
                            fields.Add(product => product.Name);
                            fields.Add(product => product.Price);
                        })
                    );
                    configure?.Invoke(resource);
                });
                configureDashboard?.Invoke(dashboard);
            });

        var app = builder.Build();
        app.MapStellarAdmin();
        await app.StartAsync();

        return app;
    }
}
