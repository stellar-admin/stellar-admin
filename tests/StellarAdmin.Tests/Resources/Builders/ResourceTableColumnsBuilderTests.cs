using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Tests.Resources.Builders;

public class ResourceTableColumnsBuilderTests
{
    [Test]
    public async Task AddGroup_AddsGroupColumnsToTableInOrder()
    {
        // Arrange
        var table = new ResourceTableDataLayoutOptions("table");
        var sut = new ResourceTableColumnsBuilder<Product>(configure => configure(table));

        // Act
        sut.Add(product => product.Name);
        sut.AddGroup(
            "Stock",
            stock =>
            {
                stock.Add(product => product.Price);
                stock.Add(product => product.Quantity);
            }
        );
        sut.Add(product => product.Id);

        // Assert
        await Assert
            .That(table.Columns.Select(column => column.FieldName).ToArray())
            .IsEquivalentTo(["Name", "Price", "Quantity", "Id"]);
        await Assert.That(table.Columns[0].Group).IsNull();
        await Assert.That(table.Columns[1].Group?.Title).IsEqualTo("Stock");
        await Assert.That(table.Columns[2].Group).IsSameReferenceAs(table.Columns[1].Group);
        await Assert.That(table.Columns[3].Group).IsNull();
    }

    [Test]
    public async Task AddGroup_InGroup_NestsGroupInParent()
    {
        // Arrange
        var table = new ResourceTableDataLayoutOptions("table");
        var sut = new ResourceTableColumnsBuilder<Product>(configure => configure(table));

        // Act
        sut.AddGroup(
            "Product",
            product =>
            {
                product.Add(item => item.Name);
                product.AddGroup("Stock", stock => stock.Add(item => item.Quantity));
            }
        );

        // Assert
        var outer = table.Columns[0].Group;
        var inner = table.Columns[1].Group;
        await Assert.That(outer?.Title).IsEqualTo("Product");
        await Assert.That(outer?.Parent).IsNull();
        await Assert.That(inner?.Title).IsEqualTo("Stock");
        await Assert.That(inner?.Parent).IsSameReferenceAs(outer);
    }

    [Test]
    public async Task AddGroup_WithoutAlign_CentersGroup()
    {
        // Arrange
        var table = new ResourceTableDataLayoutOptions("table");
        var sut = new ResourceTableColumnsBuilder<Product>(configure => configure(table));

        // Act
        sut.AddGroup("Stock", stock => stock.Add(product => product.Quantity));

        // Assert
        await Assert.That(table.Columns[0].Group?.Align).IsEqualTo(DataGridColumnGroupAlign.Center);
    }

    [Test]
    public async Task AddGroup_WithAlign_SetsGroupAlign()
    {
        // Arrange
        var table = new ResourceTableDataLayoutOptions("table");
        var sut = new ResourceTableColumnsBuilder<Product>(configure => configure(table));

        // Act
        sut.AddGroup(
            "Stock",
            stock =>
            {
                stock.Add(product => product.Quantity);
                stock.Align = DataGridColumnGroupAlign.End;
            }
        );

        // Assert
        await Assert.That(table.Columns[0].Group?.Align).IsEqualTo(DataGridColumnGroupAlign.End);
    }

    [Test]
    public async Task Clear_WithGroups_RemovesGroupColumns()
    {
        // Arrange
        var table = new ResourceTableDataLayoutOptions("table");
        var sut = new ResourceTableColumnsBuilder<Product>(configure => configure(table));
        sut.AddGroup("Stock", stock => stock.Add(product => product.Quantity));

        // Act
        sut.Clear();

        // Assert
        await Assert.That(table.Columns).IsEmpty();
    }

    public sealed class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public decimal Price { get; set; }

        public int Quantity { get; set; }
    }
}
