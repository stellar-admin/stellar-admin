using System.Net;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Editors;
using TUnit.Assertions.Enums;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Resources;

// The catalog's context and a reporting context both map products and categories; only the reporting context maps
// warehouses. The reporting context is registered after AddStellarAdmin.
public class ResourceDbContextTests
{
    [Test]
    public async Task EntityOfOneContext_RegisteredAfterAddStellarAdmin_UsesThatContext()
    {
        // Arrange
        await using var sut = await CreateAsync(configureAdmin: admin =>
            admin.AddEfCoreResource<Warehouse>(resource =>
                resource.Index(index =>
                    index.Columns(columns => columns.Add(warehouse => warehouse.Name))
                )
            )
        );
        using (var scope = sut.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
            await db.Database.ExecuteSqlRawAsync(
                "CREATE TABLE Warehouse (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL); INSERT INTO Warehouse VALUES (1, 'North');"
            );
        }
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/warehouses");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(await response.Content.ReadAsStringAsync()).Contains("North");
    }

    [Test]
    public async Task EntityOfTwoContexts_MakesTheResourceUnavailable()
    {
        // Arrange
        await using var sut = await CreateAsync();
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products");

        // Assert
        var problem = sut
            .Services.GetRequiredService<RecordingLoggerProvider>()
            .ConfigurationProblems.Single();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(problem.EventId.Id).IsEqualTo(1088);
        await Assert
            .That(problem.Message)
            .Contains(
                "because more than one DbContext maps Product: CatalogDbContext and ReportingDbContext."
            );
    }

    [Test]
    public async Task EntityOfTwoContexts_WithUseDbContext_UsesThatContext()
    {
        // Arrange
        await using var sut = await CreateAsync(resource =>
            resource.UseDbContext<ReportingDbContext>()
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products");

        // Assert
        var html = await response.Content.ReadAsStringAsync();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(html).Contains("Banana");
        await Assert.That(html).DoesNotContain("Zebra");
    }

    [Test]
    public async Task UseDbContext_WithContextThatDoesNotMapTheEntity_MakesTheResourceUnavailable()
    {
        // Arrange
        await using var sut = await CreateAsync(
            resource => resource.UseDbContext<AuditDbContext>(),
            configureServices: services =>
                services.AddDbContext<AuditDbContext>(
                    (provider, options) =>
                        options.UseSqlite(provider.GetRequiredService<SqliteConnection>())
                )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products");

        // Assert
        var problem = sut
            .Services.GetRequiredService<RecordingLoggerProvider>()
            .ConfigurationProblems.Single();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(problem.EventId.Id).IsEqualTo(1080);
        await Assert
            .That(problem.Message)
            .Contains("because AuditDbContext has no Product entity.");
    }

    [Test]
    public async Task UseItems_OfEntityOfTwoContexts_WithUseDbContext_LoadsFromThatContext()
    {
        // Arrange
        await using var sut = await CreateAsync(resource =>
        {
            resource.UseDbContext<CatalogDbContext>();
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                    fields
                        .Add(product => product.CategoryId)
                        .UseSelectEditor(select =>
                            select.UseItems<Category>(
                                category => category.Id,
                                category => category.Name,
                                items => items.UseDbContext<ReportingDbContext>()
                            )
                        )
                )
            );
        });
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/edit/1");
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                document
                    .QuerySelectorAll("select[name='Entity.CategoryId'] option")
                    .Select(option => option.TextContent.Trim())
            )
            .IsEquivalentTo(["Not set", "Office", "Technology"], CollectionOrdering.Any);
    }

    [Test]
    public async Task UseItems_OfEntityOfTwoContexts_FailsTheRequestNamingTheFix()
    {
        // Arrange
        await using var sut = await CreateAsync(resource =>
        {
            resource.UseDbContext<CatalogDbContext>();
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                    fields
                        .Add(product => product.CategoryId)
                        .UseSelectEditor(select =>
                            select.UseItems<Category>(
                                category => category.Id,
                                category => category.Name
                            )
                        )
                )
            );
        });
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/edit/1");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .Contains(
                "Category items can't load because more than one DbContext maps Category: CatalogDbContext and ReportingDbContext. Call UseDbContext<TContext>() on the items to pick one."
            );
    }

    private static Task<Microsoft.AspNetCore.Builder.WebApplication> CreateAsync(
        Action<EfCoreResourceBuilder<Product>>? configure = null,
        Action<StellarAdminBuilder>? configureAdmin = null,
        Action<IServiceCollection>? configureServices = null
    ) =>
        EfCoreTestHost.CreateAsync(
            configure,
            configureAdmin: configureAdmin,
            configureServices: services =>
            {
                services.AddDbContext<ReportingDbContext>(
                    (provider, options) =>
                        options.UseSqlite(provider.GetRequiredService<SqliteConnection>())
                );
                configureServices?.Invoke(services);
            }
        );

    private static async Task<IDocument> ReadDocument(HttpResponseMessage response) =>
        await new HtmlParser().ParseDocumentAsync(await response.Content.ReadAsStringAsync());

    public sealed class AuditDbContext(DbContextOptions<AuditDbContext> options)
        : DbContext(options);
}
