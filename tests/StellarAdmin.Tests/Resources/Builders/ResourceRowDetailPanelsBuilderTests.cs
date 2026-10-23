using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;

namespace StellarAdmin.Tests.Resources.Builders;

public class ResourceRowDetailPanelsBuilderTests
{
    [Test]
    [Arguments("Pricing", "pricing")]
    [Arguments("Order history", "order-history")]
    [Arguments("  Notes & files: 2026 ", "notes-files-2026")]
    [Arguments("?!", "")]
    public async Task Add_WithTitle_NamesPanelFromTitle(string title, string expected)
    {
        // Arrange
        var sut = new DefaultRowDetailPanelsBuilder<Product>(new ServiceCollection());

        // Act
        sut.Add(title, "_Panel");

        // Assert
        await Assert.That(sut.Panels.Single().Name).IsEqualTo(expected);
        await Assert.That(sut.Panels.Single().Title).IsEqualTo(title);
    }

    [Test]
    public async Task Add_WithoutHandler_RendersWithIndex()
    {
        // Arrange
        var sut = new DefaultRowDetailPanelsBuilder<Product>(new ServiceCollection());

        // Act
        sut.Add("Summary", "_ProductSummary");

        // Assert
        var panel = sut.Panels.Single();
        await Assert.That(panel.PartialName).IsEqualTo("_ProductSummary");
        await Assert.That(panel.LoadsOnDemand).IsFalse();
    }

    [Test]
    public async Task Add_WithHandler_LoadsOnDemandAndRegistersHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = new DefaultRowDetailPanelsBuilder<Product>(services);

        // Act
        sut.Add<ProductNotesHandler>("Notes", "_ProductNotes");

        // Assert
        await Assert.That(sut.Panels.Single().LoadsOnDemand).IsTrue();
        var registration = services.Single(service =>
            service.ServiceType == typeof(ProductNotesHandler)
        );
        await Assert.That(registration.Lifetime).IsEqualTo(ServiceLifetime.Scoped);
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
