using AngleSharp.Dom;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Displays;
using StellarAdmin.UI;
using TUnit.Assertions.Enums;

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
    public async Task Index_WithComputedColumn_RendersTheComputedValue()
    {
        // Arrange
        await using var sut = await CreateAsync(columns =>
        {
            columns.Add(
                facts => facts.ProductName + " (" + facts.Code + ")",
                column => column.Title = "Product"
            );
            columns.Add(
                facts => facts.Price * 2,
                column =>
                {
                    column.Title = "Two of them";
                    column.UseDisplay<NumberDisplay>(number => number.Format = "0.00");
                }
            );
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/facts");

        // Assert
        var cells = Cells(document);
        await Assert.That(cells[0].TextContent.Trim()).IsEqualTo("Notebook (NB-01)");
        await Assert.That(cells[1].TextContent.Trim()).IsEqualTo("17.00");
        await Assert
            .That(document.QuerySelectorAll("thead th").Select(header => header.TextContent.Trim()))
            .IsEquivalentTo(["Product", "Two of them"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task Index_WithComputedColumnReadingNull_ShowsEmptyText()
    {
        // Arrange
        await using var sut = await CreateAsync(
            columns =>
                columns.Add(
                    facts => facts.Email!.ToUpperInvariant(),
                    column =>
                    {
                        column.Title = "Email";
                        column.EmptyText = "None";
                    }
                ),
            change: facts => facts.Email = null
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/facts");

        // Assert
        await Assert.That(Cells(document)[0].TextContent.Trim()).IsEqualTo("None");
    }

    [Test]
    public async Task Startup_WithComputedColumnWithoutTitle_LogsProblem()
    {
        // Arrange

        // Act
        await using var app = await CreateAsync(columns => columns.Add(facts => facts.Price * 2));

        // Assert
        var logs = app.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(
                logs.ConfigurationProblems.Any(log =>
                    log.Message.Contains("This column isn't shown because it's computed")
                )
            )
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

    [Test]
    public async Task Index_WithImageDisplay_RendersThumbnailFromResolvedUrl()
    {
        // Arrange
        await using var sut = await CreateAsync(columns =>
            columns.Add(
                facts => facts.Photo,
                column =>
                    column.UseDisplay<ImageDisplay>(image =>
                        image.ResolveUrl(value => $"https://cdn.voyager.example/{value}")
                    )
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/facts");

        // Assert
        var image = Cells(document)[0].RequiredElement(".sa-image");
        await Assert.That(image.LocalName).IsEqualTo("button");
        await Assert.That(image.GetAttribute("data-variant")).IsEqualTo("thumbnail");
        await Assert.That(image.GetAttribute("data-size")).IsEqualTo("sm");
        await Assert.That(image.GetAttribute("data-aspect-ratio")).IsEqualTo("square");
        await Assert.That(image.GetAttribute("data-interaction")).IsEqualTo("lightbox");
        await Assert.That(image.GetAttribute("aria-label")).IsEqualTo("Photo");
        await Assert
            .That(image.RequiredElement("img").GetAttribute("src"))
            .IsEqualTo("https://cdn.voyager.example/notebook.jpg");
        await Assert
            .That(document.QuerySelectorAll("[data-slot='image-lightbox-template']").Length)
            .IsEqualTo(1);
    }

    [Test]
    public async Task Index_WithImageDisplaySettings_RendersSettings()
    {
        // Arrange
        await using var sut = await CreateAsync(columns =>
            columns.Add(
                facts => facts.Photo,
                column =>
                    column.UseDisplay<ImageDisplay>(image =>
                    {
                        image.AspectRatio = ImageAspectRatio.Landscape;
                        image.Fit = ImageFit.Contain;
                        image.Size = ImageDisplaySize.Large;
                        image.Interaction = ImageDisplayInteraction.None;
                    })
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/facts");

        // Assert
        var image = Cells(document)[0].RequiredElement(".sa-image");
        await Assert.That(image.LocalName).IsEqualTo("span");
        await Assert.That(image.GetAttribute("data-size")).IsEqualTo("lg");
        await Assert.That(image.GetAttribute("data-aspect-ratio")).IsEqualTo("landscape");
        await Assert.That(image.GetAttribute("data-fit")).IsEqualTo("contain");
        await Assert.That(image.HasAttribute("data-interaction")).IsFalse();
        await Assert
            .That(image.RequiredElement("img").GetAttribute("src"))
            .IsEqualTo("notebook.jpg");
    }

    [Test]
    public async Task Index_WithAsyncImageResolver_ResolvesUrlWithServices()
    {
        // Arrange
        await using var sut = await CreateAsync(
            columns =>
                columns.Add(
                    facts => facts.Photo,
                    column =>
                        column.UseDisplay<ImageDisplay>(image =>
                            image.ResolveUrl(
                                (value, services, _) =>
                                    Task.FromResult<string?>(
                                        services.GetRequiredService<PhotoHost>().Url + value
                                    )
                            )
                        )
                ),
            services => services.AddSingleton(new PhotoHost("https://photos.voyager.example/"))
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/facts");

        // Assert
        await Assert
            .That(Cells(document)[0].RequiredElement(".sa-image img").GetAttribute("src"))
            .IsEqualTo("https://photos.voyager.example/notebook.jpg");
    }

    [Test]
    public async Task Index_WithImageDisplayAndEmptyValue_RendersPlaceholder()
    {
        // Arrange
        await using var sut = await CreateAsync(
            columns =>
                columns.Add(
                    facts => facts.Photo,
                    column =>
                    {
                        column.EmptyText = "None";
                        column.UseDisplay<ImageDisplay>();
                    }
                ),
            change: facts => facts.Photo = null
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/facts");

        // Assert
        var cell = Cells(document)[0];
        var image = cell.RequiredElement(".sa-image");
        await Assert.That(image.GetAttribute("data-state")).IsEqualTo("empty");
        await Assert.That(image.QuerySelector("img")).IsNull();
        await Assert.That(cell.TextContent.Contains("None")).IsFalse();
    }

    [Test]
    public async Task Index_EnumMemberWithoutDisplayName_ShowsReadableName()
    {
        // Arrange
        await using var sut = await CreateAsync(
            columns => columns.Add(facts => facts.Status),
            change: facts => facts.Status = ProductFactsStatus.OutOfStock
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/facts");

        // Assert
        await Assert.That(Cells(document)[0].TextContent.Trim()).IsEqualTo("Out of Stock");
    }

    private static IElement[] Cells(IDocument document) =>
        document.QuerySelectorAll("tbody tr:first-child td").ToArray();

    private static Task<WebApplication> CreateAsync(
        Action<ResourceColumnsBuilder<ProductFactsModel>> columns,
        Action<IServiceCollection>? configureServices = null,
        Action<ProductFactsModel>? change = null
    )
    {
        var facts = new ProductFactsModel
        {
            Active = true,
            Code = "NB-01",
            Email = "sales@voyager.example",
            Notes = "Ships in a sleeve.\nRecycled paper.",
            Phone = "+27 21 555 0100",
            Photo = "notebook.jpg",
            Price = 8.50m,
            ProductName = "Notebook",
            Released = new(2026, 3, 1),
            Status = ProductFactsStatus.OnSale,
            Website = "https://voyager.example/",
            Weight = 1.25m,
        };
        change?.Invoke(facts);

        return StellarAdminTestHost.CreateAsync(
            new([]),
            configureServices: services =>
            {
                configureServices?.Invoke(services);
                services.AddSingleton(new List<ProductFactsModel> { facts });
            },
            configureAdmin: admin =>
                admin
                    .AddResource<ProductFactsModel>("facts")
                    .UseDataSource<ProductFactsDataSource>()
                    .Index(index => index.Columns(columns))
        );
    }

    private sealed record PhotoHost(string Url);
}
