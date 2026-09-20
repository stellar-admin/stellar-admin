using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Tests;

public class StellarAdminDashboardBuilderExtensionsTests
{
    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task AddResource_WithBlankPluralLabel_RejectsConfiguration(string value)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();
        sut.AddResource<Product>(resource => resource.PluralLabel = value);
        using var provider = services.BuildServiceProvider();

        // Act
        Action act = () =>
            _ = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task AddResource_WithBlankSingularLabel_RejectsConfiguration(string value)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();
        sut.AddResource<Product>(resource => resource.SingularLabel = value);
        using var provider = services.BuildServiceProvider();

        // Act
        Action act = () =>
            _ = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    private sealed class Product;
}
