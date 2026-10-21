using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Builders;

namespace StellarAdmin.Tests.Resources.Builders;

public class ResourceRowDetailPanelBuilderTests
{
    [Test]
    public async Task Name_WhenSet_ReplacesNameFromTitle()
    {
        // Arrange
        var panels = new DefaultRowDetailPanelsBuilder<Product>(new ServiceCollection());
        var sut = panels.Add("Order history", "_OrderHistory");

        // Act
        sut.Name = "orders";

        // Assert
        await Assert.That(panels.Panels.Single().Name).IsEqualTo("orders");
    }

    public sealed class Product
    {
        public int Id { get; set; }
    }
}
