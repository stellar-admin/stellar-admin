using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Tests.Resources.Builders;

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
        var resource = services.AddStellarAdmin().AddResource<Product>();

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
        var resource = services.AddStellarAdmin().AddResource<Product>();

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

    [Test]
    public async Task EnableRowDetails_WithPanels_RegistersPanelsInOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Product>();

        // Act
        resource.Index(index =>
            index.EnableRowDetails(panels =>
            {
                panels.Add("Summary", "_ProductSummary");
                panels.Add<ProductNotes, ProductNotesHandler>("Notes", "_ProductNotes");
            })
        );

        // Assert
        using var provider = services.BuildServiceProvider();
        var rowDetails = provider
            .GetRequiredService<IOptions<ResourceOptions<Product>>>()
            .Value.Index.RowDetails!;
        await Assert
            .That(rowDetails.Panels.Select(panel => (panel.Name, panel.Title, panel.LoadsOnDemand)))
            .IsEquivalentTo(
                new (string?, string?, bool)[]
                {
                    ("summary", "Summary", false),
                    ("notes", "Notes", true),
                }
            );
    }

    [Test]
    public async Task EnableRowDetails_WithoutPanels_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Product>();

        // Act
        Action act = () => resource.Index(index => index.EnableRowDetails(panels => { }));

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task EnableRowDetails_WithDuplicatePanelNames_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Product>();

        // Act
        Action act = () =>
            resource.Index(index =>
                index.EnableRowDetails(panels =>
                {
                    panels.Add("Order history", "_OrderHistory");
                    panels.Add("Orders", "_Orders").Name = "order-history";
                })
            );

        // Assert
        await Assert
            .That(act)
            .Throws<InvalidOperationException>()
            .WithMessageContaining("order-history");
    }

    [Test]
    public async Task EnableRowDetails_WithUnnamedPanel_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Product>();

        // Act
        Action act = () =>
            resource.Index(index => index.EnableRowDetails(panels => panels.Add("?!", "_Panel")));

        // Assert
        await Assert
            .That(act)
            .Throws<InvalidOperationException>()
            .WithMessageContaining("Set Name");
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
