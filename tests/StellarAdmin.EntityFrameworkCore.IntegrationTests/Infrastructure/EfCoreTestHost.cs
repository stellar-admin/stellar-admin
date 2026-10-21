using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;

internal static class EfCoreTestHost
{
    public static async Task<WebApplication> CreateAsync(
        Action<EfCoreResourceBuilder<CatalogDbContext, Product>>? configure = null,
        ConcurrentQueue<string>? commands = null,
        Action<StellarAdminBuilder>? configureAdmin = null
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
        var logs = new RecordingLoggerProvider();
        builder.Logging.AddProvider(logs);
        builder.Services.AddSingleton(logs);
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
        builder.Services.AddStellarAdmin(admin =>
        {
            admin.AddEfCoreResource<CatalogDbContext, Product>(resource =>
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
            });
            configureAdmin?.Invoke(admin);
        });

        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            await db.Database.EnsureCreatedAsync();
            db.AddRange(
                new Category { Id = 1, Name = "Office" },
                new Category { Id = 2, Name = "Beverage" },
                new Category
                {
                    Id = 3,
                    Name = "Technology",
                    ParentId = 1,
                }
            );
            var sale = new Tag { Id = 1, Name = "Sale" };
            var seasonal = new Tag { Id = 2, Name = "Seasonal" };
            db.AddRange(sale, seasonal, new Tag { Id = 3, Name = "Organic" });
            db.AddRange(
                new Product
                {
                    Number = 1,
                    CategoryId = 1,
                    Name = "Zebra",
                    Price = 10,
                    Details = new() { Sku = "Z-1", InternalNote = "Keep zebra note" },
                },
                new Product
                {
                    Number = 2,
                    CategoryId = 3,
                    Name = "Apple",
                    Price = 20,
                    Details = new() { Sku = "A-2", InternalNote = "Keep apple note" },
                    Tags = { sale, seasonal },
                },
                new Product
                {
                    Number = 3,
                    CategoryId = 3,
                    Name = "Apple",
                    Price = 30,
                    Details = new() { Sku = "A-3" },
                },
                new Product
                {
                    Number = 4,
                    CategoryId = 2,
                    Name = "Banana",
                    Price = 40,
                    Details = new() { Sku = "B-4" },
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
