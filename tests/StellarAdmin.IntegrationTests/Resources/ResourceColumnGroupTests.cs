using AngleSharp.Dom;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Testing;
using StellarAdmin.UI;

namespace StellarAdmin.IntegrationTests.Resources;

public class ResourceColumnGroupTests
{
    [Test]
    public async Task Index_WithNestedColumnGroups_RendersHeaderRowPerGroupLevel()
    {
        // Arrange
        await using var sut = await CreateAsync(columns =>
        {
            columns.Add(facts => facts.ProductName);
            columns.AddGroup(
                "Details",
                details =>
                {
                    details.Add(facts => facts.Code);
                    details.AddGroup(
                        "Contact",
                        contact =>
                        {
                            contact.Align = DataGridColumnGroupAlign.Start;
                            contact.Add(facts => facts.Email);
                            contact.Add(facts => facts.Phone);
                        }
                    );
                }
            );
            columns.Add(facts => facts.Price);
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/facts");

        // Assert
        var rows = document.QuerySelectorAll("thead > tr");
        await Assert.That(rows.Length).IsEqualTo(3);
        await Assert
            .That(rows[0].Children.Take(3).Select(head => head.TextContent.Trim()).ToArray())
            .IsEquivalentTo(["Product Name", "Details", "Price"]);

        var details = rows[0].Children[1];
        await Assert.That(details.GetAttribute("scope")).IsEqualTo("colgroup");
        await Assert.That(details.GetAttribute("colspan")).IsEqualTo("3");
        await Assert.That(details.HasAttribute("data-align")).IsFalse();

        var contact = rows[1].Children[1];
        await Assert.That(contact.TextContent.Trim()).IsEqualTo("Contact");
        await Assert.That(contact.GetAttribute("colspan")).IsEqualTo("2");
        await Assert.That(contact.GetAttribute("data-align")).IsEqualTo("start");

        await Assert
            .That(rows[2].Children.Select(head => head.TextContent.Trim()).ToArray())
            .IsEquivalentTo(["Email", "Phone"]);
    }

    [Test]
    public async Task Index_WithColumnGroups_RendersCellsInColumnOrder()
    {
        // Arrange
        await using var sut = await CreateAsync(columns =>
        {
            columns.AddGroup(
                "Product",
                product =>
                {
                    product.Add(facts => facts.ProductName);
                    product.Add(facts => facts.Code);
                }
            );
            columns.Add(facts => facts.Email);
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/facts");

        // Assert
        var cells = document.QuerySelectorAll("tbody tr:first-child td");
        await Assert.That(cells[0].TextContent.Trim()).IsEqualTo("Notebook");
        await Assert.That(cells[1].TextContent.Trim()).IsEqualTo("NB-01");
        await Assert.That(cells[2].TextContent.Trim()).IsEqualTo("sales@voyager.example");
    }

    [Test]
    public async Task Index_WithSortableColumnInGroup_RendersSortLink()
    {
        // Arrange
        await using var sut = await CreateAsync(columns =>
            columns.AddGroup(
                "Product",
                product => product.Add(facts => facts.ProductName).Sortable()
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/facts");

        // Assert
        var link = document.QuerySelector("thead a[href*='sortBy=ProductName']");
        await Assert.That(link).IsNotNull();
    }

    [Test]
    public async Task Index_WithGroupWhoseColumnsAreRemoved_RendersNoGroup()
    {
        // Arrange
        await using var sut = await CreateAsync(columns =>
        {
            columns.Add(facts => facts.ProductName);
            columns.AddGroup(
                "Computed",
                computed => computed.Add(facts => facts.ProductName + facts.Code)
            );
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/facts");

        // Assert
        await Assert.That(document.QuerySelectorAll("thead > tr").Length).IsEqualTo(1);
        await Assert.That(document.QuerySelector("th[scope=colgroup]")).IsNull();
    }

    private static Task<WebApplication> CreateAsync(
        Action<ResourceTableColumnsBuilder<ProductFactsModel>> columns
    )
    {
        var facts = new ProductFactsModel
        {
            Code = "NB-01",
            Email = "sales@voyager.example",
            Phone = "+27 21 555 0100",
            Price = 8.50m,
            ProductName = "Notebook",
        };

        return StellarAdminTestHost.CreateAsync(
            configureServices: services =>
                services.AddSingleton(new List<ProductFactsModel> { facts }),
            configureAdmin: admin =>
                admin
                    .AddResource<ProductFactsModel>("facts")
                    .UseDataSource<ProductFactsDataSource>()
                    .Index(index => index.Columns(columns))
        );
    }
}
