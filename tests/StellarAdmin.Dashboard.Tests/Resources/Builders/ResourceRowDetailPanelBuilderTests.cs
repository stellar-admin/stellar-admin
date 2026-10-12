using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Builders;

namespace StellarAdmin.Dashboard.Tests.Resources.Builders;

public class ResourceRowDetailPanelBuilderTests
{
    [Test]
    public async Task Name_WhenSet_ReplacesNameFromTitle()
    {
        // Arrange
        var panels = new ResourceRowDetailPanelsBuilder<Product>(new ServiceCollection());
        var sut = panels.Add("Order history", "_OrderHistory");

        // Act
        sut.Name = "orders";

        // Assert
        await Assert.That(panels.Panels.Single().Name).IsEqualTo("orders");
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task Name_WhenBlank_ThrowsArgumentException(string name)
    {
        // Arrange
        var panels = new ResourceRowDetailPanelsBuilder<Product>(new ServiceCollection());
        var sut = panels.Add("Orders", "_Orders");

        // Act
        Action act = () => sut.Name = name;

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    public sealed class Product
    {
        public int Id { get; set; }
    }
}
