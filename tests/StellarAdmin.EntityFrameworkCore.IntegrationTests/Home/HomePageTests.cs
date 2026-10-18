using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Home;

public class HomePageTests
{
    [Test]
    public async Task Development_SuggestsEntityThatIsNotAResource()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(configureAdmin: admin =>
            admin.Services.AddDbContext<StoreDbContext>(options =>
                options.UseSqlite("Data Source=:memory:")
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var html = await client.GetStringAsync("/stellaradmin");

        // Assert
        await Assert.That(html).Contains("AddEfCoreResource&lt;StoreDbContext, Category&gt;");
        await Assert.That(html).Contains("columns.Add(category =&gt; category.Name)");
        await Assert.That(html).DoesNotContain("dotnet add package");
    }
}
