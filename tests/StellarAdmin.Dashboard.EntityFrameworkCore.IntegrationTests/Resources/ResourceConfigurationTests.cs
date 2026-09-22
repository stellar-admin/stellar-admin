using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.Resources.Options;
using TUnit.Assertions;
using TUnit.Core;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceConfigurationTests
{
    [Test]
    public async Task SeparateProviders_DoNotShareSearchConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlite("Data Source=:memory:")
        );
        services
            .AddStellarAdmin()
            .AddDashboard(dashboard =>
                dashboard.AddEfCoreResource<CatalogDbContext, Product>(resource =>
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
        var firstOptions = first.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Act
        firstOptions.Index.Search!.Placeholder = "Changed";
        var secondOptions = second.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(secondOptions.Index.Search!.Placeholder).IsEqualTo("Find products");
        await Assert
            .That(secondOptions.Index.Search)
            .IsTypeOf<EfCoreResourceSearchOptions<Product>>();
    }
}
