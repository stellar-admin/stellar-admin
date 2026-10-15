using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Displays;

namespace StellarAdmin.IntegrationTests.Infrastructure;

internal static class StellarAdminTestHost
{
    public static async Task<WebApplication> CreateAsync(
        ProductState state,
        Action<ResourceBuilder<Product>>? configure = null,
        Action<StellarAdminBuilder>? configureAdmin = null,
        Action<IServiceCollection>? configureServices = null,
        Action<ControllerActionEndpointConventionBuilder>? configureRoute = null,
        Action<WebApplication>? configureApp = null,
        string environmentName = "Development"
    )
    {
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions
            {
                ApplicationName = typeof(StellarAdminTestHost).Assembly.GetName().Name,
                EnvironmentName = environmentName,
            }
        );
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(state);
        configureServices?.Invoke(builder.Services);
        builder.Services.AddStellarAdmin(admin =>
        {
            admin.AddResource<Product>(resource =>
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
                                column.UseDisplay<NumberDisplay>(number => number.Format = "0.00");
                            }
                        );
                    })
                );
                resource.AllowCreate(create =>
                    create.Fields(fields =>
                    {
                        fields.Add(product => product.Name);
                        fields.Add(product => product.Price);
                    })
                );
                configure?.Invoke(resource);
            });
            configureAdmin?.Invoke(admin);
        });

        var app = builder.Build();
        var route = app.MapStellarAdmin();
        configureRoute?.Invoke(route);
        configureApp?.Invoke(app);
        await app.StartAsync();

        return app;
    }
}
