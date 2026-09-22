using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Fixtures;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Infrastructure;

internal static class EfCoreTestHost
{
    public static async Task<WebApplication> CreateAsync(
        Action<EfCoreResourceBuilder<CatalogDbContext, Product>>? configure = null,
        ConcurrentQueue<string>? commands = null
    )
    {
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions
            {
                ApplicationName = typeof(EfCoreTestHost).Assembly.GetName().Name,
                EnvironmentName = "Development",
            }
        );
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(_ =>
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            return connection;
        });
        builder.Services.AddDbContext<CatalogDbContext>(
            (services, options) =>
            {
                options.UseSqlite(services.GetRequiredService<SqliteConnection>());
                if (commands is not null)
                {
                    options.LogTo(commands.Enqueue, [RelationalEventId.CommandExecuted]);
                }
            }
        );
        builder
            .Services.AddStellarAdmin()
            .AddDashboard(dashboard =>
                dashboard.AddEfCoreResource<CatalogDbContext, Product>(resource =>
                {
                    resource.Index(index =>
                        index.Columns(columns =>
                        {
                            columns.Add(product => product.Number);
                            columns.Add(product => product.Name, column => column.Sortable());
                            columns.Add(product => product.Price, column => column.Sortable());
                        })
                    );
                    configure?.Invoke(resource);
                })
            );

        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            await db.Database.EnsureCreatedAsync();
            db.AddRange(
                new Product
                {
                    Number = 1,
                    Name = "Zebra",
                    Price = 10,
                },
                new Product
                {
                    Number = 2,
                    Name = "Apple",
                    Price = 20,
                },
                new Product
                {
                    Number = 3,
                    Name = "Apple",
                    Price = 30,
                },
                new Product
                {
                    Number = 4,
                    Name = "Banana",
                    Price = 40,
                },
                new Product
                {
                    Number = 5,
                    Name = "Hidden",
                    Price = 50,
                    Hidden = true,
                }
            );
            await db.SaveChangesAsync();
        }
        commands?.Clear();
        app.MapStellarAdmin();
        await app.StartAsync();

        return app;
    }
}
