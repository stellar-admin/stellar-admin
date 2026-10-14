using AngleSharp.Dom;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Displays;

namespace StellarAdmin.IntegrationTests.Resources;

public class ResourceColumnDisplayTests
{
    [Test]
    public async Task Index_WithoutDisplays_RendersDefaultDisplayForEachType()
    {
        // Arrange
        await using var sut = await CreateAsync(columns =>
        {
            columns.Add(facts => facts.ProductName);
            columns.Add(facts => facts.Active);
            columns.Add(facts => facts.Status);
            columns.Add(facts => facts.Released);
            columns.Add(facts => facts.Weight);
            columns.Add(facts => facts.Email);
            columns.Add(facts => facts.Website);
            columns.Add(facts => facts.Phone);
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/facts");

        // Assert
        var cells = Cells(document);
        await Assert
            .That(cells[0].QuerySelector(".sa-text-display")?.TextContent)
            .IsEqualTo("Notebook");
        await Assert
            .That(cells[1].QuerySelector(".sa-boolean-display")?.GetAttribute("data-value"))
            .IsEqualTo("true");
        await Assert.That(cells[2].TextContent.Trim()).IsEqualTo("On sale");
        await Assert
            .That(cells[3].QuerySelector("time")?.GetAttribute("datetime"))
            .IsEqualTo("2026-03-01");
        await Assert
            .That(cells[4].QuerySelector(".sa-number-display")?.TextContent)
            .IsEqualTo("1.250");
        await Assert
            .That(cells[5].QuerySelector("a")?.GetAttribute("href"))
            .IsEqualTo("mailto:sales@voyager.example");
        await Assert
            .That(cells[6].QuerySelector("a")?.GetAttribute("href"))
            .IsEqualTo("https://voyager.example/");
        await Assert
            .That(cells[7].QuerySelector("a")?.GetAttribute("href"))
            .IsEqualTo("tel:+27215550100");
    }

    [Test]
    public async Task Index_WithNumberDisplay_FormatsAndRightAlignsColumn()
    {
        // Arrange
        await using var sut = await CreateAsync(columns =>
            columns.Add(
                facts => facts.Price,
                column => column.UseDisplay<NumberDisplay>(number => number.Format = "0.00")
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/facts");

        // Assert
        var cell = Cells(document)[0];
        await Assert.That(cell.TextContent.Trim()).IsEqualTo("8.50");
        await Assert.That(cell.ClassList.Contains("text-right")).IsTrue();
        await Assert
            .That(document.RequiredElement("thead th").ClassList.Contains("text-right"))
            .IsTrue();
    }

    [Test]
    public async Task Index_WithColumnsSharingDisplayTemplate_RendersEachColumnsDisplay()
    {
        // Arrange
        await using var sut = await CreateAsync(columns =>
        {
            columns.Add(facts => facts.ProductName);
            columns.Add(
                facts => facts.Notes,
                column => column.UseDisplay<TextDisplay>(text => text.LineClamp = 2)
            );
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/facts");

        // Assert
        var cells = Cells(document);
        await Assert
            .That(cells[0].RequiredElement(".sa-text-display").HasAttribute("data-line-clamp"))
            .IsFalse();
        await Assert
            .That(cells[1].RequiredElement(".sa-text-display").GetAttribute("data-line-clamp"))
            .IsEqualTo("2");
    }

    [Test]
    public async Task Index_WithCustomDisplay_RendersGridTemplateWithDisplay()
    {
        // Arrange
        await using var sut = await CreateAsync(columns =>
            columns.Add(
                facts => facts.Price,
                column => column.UseDisplay<PriceDisplay>(price => price.Currency = "ZAR")
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/facts");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-custom-display='grid-price']").TextContent)
            .IsEqualTo("ZAR 8.50");
    }

    [Test]
    public async Task Index_WithUIHint_RendersHintedGridTemplate()
    {
        // Arrange
        await using var sut = await CreateAsync(columns => columns.Add(facts => facts.Code));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/facts");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-custom-display='grid-code']").TextContent)
            .IsEqualTo("NB-01");
    }

    [Test]
    public async Task Index_WithBuiltInRendering_RendersValueWithoutDisplay()
    {
        // Arrange
        await using var sut = await CreateAsync(columns =>
            columns.Add(facts => facts.Weight, column => column.UseBuiltInRendering())
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/facts");

        // Assert
        var cell = Cells(document)[0];
        await Assert.That(cell.TextContent.Trim()).IsEqualTo("1.250");
        await Assert.That(cell.QuerySelector(".sa-number-display")).IsNull();
        await Assert.That(cell.ClassList.Contains("text-right")).IsFalse();
    }

    [Test]
    public async Task Index_WithEmptyValue_ShowsEmptyText()
    {
        // Arrange
        await using var sut = await CreateAsync(columns =>
            columns.Add(facts => facts.Discontinued, column => column.EmptyText = "Unknown")
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/facts");

        // Assert
        var cell = Cells(document)[0];
        await Assert.That(cell.TextContent.Trim()).IsEqualTo("Unknown");
        await Assert.That(cell.QuerySelector(".sa-boolean-display")).IsNull();
    }

    private static IElement[] Cells(IDocument document) =>
        document.QuerySelectorAll("tbody tr:first-child td").ToArray();

    private static Task<WebApplication> CreateAsync(
        Action<ResourceColumnsBuilder<ProductFactsModel>> columns
    ) =>
        StellarAdminTestHost.CreateAsync(
            new([]),
            configureServices: services =>
                services.AddSingleton(
                    new List<ProductFactsModel>
                    {
                        new()
                        {
                            Active = true,
                            Code = "NB-01",
                            Email = "sales@voyager.example",
                            Notes = "Ships in a sleeve.\nRecycled paper.",
                            Phone = "+27 21 555 0100",
                            Price = 8.50m,
                            ProductName = "Notebook",
                            Released = new(2026, 3, 1),
                            Status = ProductFactsStatus.OnSale,
                            Website = "https://voyager.example/",
                            Weight = 1.25m,
                        },
                    }
                ),
            configureAdmin: admin =>
                admin
                    .AddResource<ProductFactsModel>("facts")
                    .UseDataSource<ProductFactsDataSource>()
                    .Index(index => index.Columns(columns))
        );
}
