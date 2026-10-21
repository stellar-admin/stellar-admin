using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Options;
using TUnit.Assertions;
using TUnit.Core;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceConfigurationTests
{
    [Test]
    public async Task UnmappedEntity_MakesOnlyThatResourceUnavailable()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(configureAdmin: admin =>
            admin.AddEfCoreResource<CatalogDbContext, Supplier>()
        );
        using var client = sut.GetTestClient();

        // Act
        using var unmapped = await client.GetAsync("/stellaradmin/suppliers");
        using var products = await client.GetAsync("/stellaradmin/products");

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert.That(unmapped.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(products.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(logs.ConfigurationProblems.Single().Message)
            .Contains("Supplier is not mapped in CatalogDbContext.");
    }

    [Test]
    public async Task SeparateProviders_DoNotShareSearchConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlite("Data Source=:memory:")
        );
        services.AddStellarAdmin(admin =>
            admin.AddEfCoreResource<CatalogDbContext, Product>(resource =>
                resource.Index(index =>
                    index.EnableSearch(
                        term => product => product.Name.Contains(term),
                        search => search.Placeholder = "Find products"
                    )
                )
            )
        );
        using var first = services.BuildServiceProvider();
        using var second = services.BuildServiceProvider();

        // Act
        var firstOptions = first.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;
        var secondOptions = second.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert
            .That(secondOptions.Index.Search)
            .IsNotSameReferenceAs(firstOptions.Index.Search);
        await Assert.That(secondOptions.Index.Search!.Placeholder).IsEqualTo("Find products");
        await Assert
            .That(secondOptions.Index.Search)
            .IsTypeOf<EfCoreResourceSearchOptions<Product>>();
    }

    public sealed class Supplier
    {
        public int Id { get; set; }
    }
}
