using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources;

namespace StellarAdmin.Dashboard.Tests.Resources.Builders;

public class ResourceIndexBuilderBaseTests
{
    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task EnableRowDetails_WithBlankPartialName_ThrowsArgumentException(
        string partialName
    )
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddDashboard().AddResource<Product>();

        // Act
        Action act = () => resource.Index(index => index.EnableRowDetails(partialName));

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    public async Task EnableRowDetails_WithRegisteredHandler_KeepsApplicationLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ProductNotesHandler>();
        var resource = services.AddStellarAdmin().AddDashboard().AddResource<Product>();

        // Act
        resource.Index(index =>
            index.EnableRowDetails<ProductNotes, ProductNotesHandler>("_ProductNotes")
        );

        // Assert
        var registration = services.Single(service =>
            service.ServiceType == typeof(ProductNotesHandler)
        );
        await Assert.That(registration.Lifetime).IsEqualTo(ServiceLifetime.Singleton);
    }

    public sealed class Product
    {
        public int Id { get; set; }
    }

    public sealed record ProductNotes(string Notes);

    public sealed class ProductNotesHandler : IResourceRowDetailHandler<ProductNotes>
    {
        public Task<ProductNotes?> FindAsync(string id, CancellationToken cancellationToken) =>
            Task.FromResult<ProductNotes?>(new("Notes"));
    }
}
