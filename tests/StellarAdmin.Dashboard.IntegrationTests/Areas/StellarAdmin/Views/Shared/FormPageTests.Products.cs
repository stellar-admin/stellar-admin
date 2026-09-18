using AngleSharp.Html.Parser;
using IdentitySimplePlayground.Data;
using StellarAdmin.Dashboard.Testing;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Areas.StellarAdmin.Views.Shared;

public partial class FormPageTests
{
    [Test]
    public async Task ExecuteAsync_WhenCategoryMetadataIsConfigured_RendersLabelsHelpAndMultilineEditor()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();

        // Act
        using var html = await app.GetHtmlAsync("/admin/categories/Create");

        // Assert
        await Assert.That(html.Body!.TextContent).Contains("Category name");
        await Assert
            .That(html.Body.TextContent)
            .Contains("Explain what kinds of products belong in this category.");
        await Assert.That(html.QuerySelector("textarea[name='Entity.Description']")).IsNotNull();
    }

    [Test]
    [Arguments("create")]
    [Arguments("edit")]
    [Arguments("invalid")]
    public async Task ExecuteAsync_WhenConditionIsRendered_PreservesChoiceCardsAndSelection(
        string mode
    )
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var product = new Product
        {
            Name = "Lamp",
            Sku = "LAMP",
            Condition = ProductCondition.Refurbished,
        };
        app.Db.Products.Add(product);
        await app.Db.SaveChangesAsync();
        var path = mode == "edit" ? $"/admin/products/Edit/{product.Id}" : "/admin/products/Create";
        var token = await app.GetTokenAsync(path);

        // Act
        using var response =
            mode == "invalid"
                ? await app.PostAsync(
                    path,
                    token,
                    ("Entity.Name", "Invalid"),
                    ("Entity.Sku", ""),
                    ("Entity.Price", "-1"),
                    ("Entity.Condition", "Used")
                )
                : await app.Client.GetAsync(path);
        using var html = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync());

        // Assert
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(html.QuerySelector("div.sa-field-group.grid-cols-1")).IsNotNull();
        await Assert
            .That(html.QuerySelectorAll("input[name='Entity.Condition']").Length)
            .IsEqualTo(3);
        await Assert
            .That(html.QuerySelectorAll("input[type=radio][name='Entity.Condition']").Length)
            .IsEqualTo(3);
        await Assert
            .That(html.QuerySelectorAll("input[name='Entity.Condition'][checked]").Length)
            .IsEqualTo(1);
        await Assert
            .That(
                html.QuerySelector("input[name='Entity.Condition'][checked]")?.GetAttribute("value")
            )
            .IsEqualTo(
                mode == "edit" ? "Refurbished"
                : mode == "invalid" ? "Used"
                : "New"
            );
        await Assert
            .That(
                html.QuerySelectorAll(
                    "label[for^=Entity_Condition_] > div[data-orientation=horizontal]"
                ).Length
            )
            .IsEqualTo(3);
        await Assert
            .That(html.Body!.TextContent)
            .Contains("An unused item in its original condition.");
        await Assert
            .That(html.Body.TextContent)
            .Contains("A previously owned item restored and tested for resale.");
        await Assert
            .That(html.Body.TextContent)
            .Contains("A previously owned item that may show signs of wear.");
        await Assert.That(html.QuerySelector("label[for=Entity_Condition_New]")).IsNotNull();
        await Assert
            .That(html.QuerySelector("label[for=Entity_Condition_Refurbished]"))
            .IsNotNull();
        await Assert.That(html.QuerySelector("label[for=Entity_Condition_Used]")).IsNotNull();
        await Assert
            .That(
                html.Body.TextContent.Split("Select the condition of the item being sold.").Length
                    - 1
            )
            .IsEqualTo(1);
    }

    [Test]
    [Arguments("create")]
    [Arguments("edit")]
    [Arguments("invalid")]
    public async Task ExecuteAsync_WhenProductFormRenders_PreservesSectionsRowsAndBindings(
        string mode
    )
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var product = new Product
        {
            Name = "Lamp",
            Sku = "LAMP",
            Condition = ProductCondition.Refurbished,
        };
        app.Db.Products.Add(product);
        await app.Db.SaveChangesAsync();
        var path = mode == "edit" ? $"/admin/products/Edit/{product.Id}" : "/admin/products/Create";
        var token = await app.GetTokenAsync(path);

        // Act
        using var response =
            mode == "invalid"
                ? await app.PostAsync(
                    path,
                    token,
                    ("Entity.Name", "Invalid"),
                    ("Entity.Sku", ""),
                    ("Entity.Price", "-1"),
                    ("Entity.Condition", "Used")
                )
                : await app.Client.GetAsync(path);
        using var html = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync());

        // Assert
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        var headings = html.QuerySelectorAll("h2[data-slot=form-section-title]")
            .Select(element => element.TextContent.Trim());
        await Assert
            .That(string.Join(",", headings))
            .IsEqualTo("Details,Pricing,Inventory,Shipping,Publishing");
        await Assert.That(html.QuerySelectorAll("fieldset").Length).IsEqualTo(1);
        await Assert
            .That(html.Body!.TextContent)
            .Contains("How this product appears in the catalog.");
        await Assert.That(html.QuerySelectorAll("[data-slot=form-row]").Length).IsEqualTo(3);
        await Assert
            .That(html.QuerySelectorAll("[data-slot=form-row-content]").Length)
            .IsEqualTo(3);
        await Assert.That(html.QuerySelectorAll("[data-layout=split]").Length).IsEqualTo(5);
        await Assert.That(html.DocumentElement.OuterHtml).DoesNotContain("--sa-form-row-columns");
        await Assert.That(html.DocumentElement.OuterHtml).DoesNotContain("<sa-");
        var expectedFields = new[]
        {
            "Name",
            "Sku",
            "Description",
            "Price",
            "CompareAtPrice",
            "CostPrice",
            "StockQuantity",
            "WeightKg",
            "WidthCm",
            "HeightCm",
            "DepthCm",
            "PublishedAt",
            "CreatedAt",
        };
        var fieldCounts = expectedFields
            .Select(name =>
                html.QuerySelectorAll(
                    $"input[name='Entity.{name}'], textarea[name='Entity.{name}']"
                ).Length
            )
            .ToArray();
        await Assert
            .That(fieldCounts)
            .IsEquivalentTo(Enumerable.Repeat(1, expectedFields.Length).ToArray());
    }

    [Test]
    public async Task ExecuteAsync_WhenProductMetadataIsConfigured_RendersDescriptionsAndReadOnlyDate()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();

        // Act
        using var html = await app.GetHtmlAsync("/admin/products/Create");

        // Assert
        await Assert.That(html.Body!.TextContent).Contains("Compare-at price");
        await Assert
            .That(html.Body!.TextContent)
            .Contains("The cost to purchase or produce one unit.");
        await Assert
            .That(html.Body!.TextContent)
            .Contains("Record stock levels and inventory preferences.");
        await Assert
            .That(html.Body!.TextContent)
            .Contains("Describe shipping requirements and physical measurements.");
        await Assert
            .That(html.Body!.TextContent)
            .Contains("Record the publication status and date.");
        await Assert
            .That(html.QuerySelector("input[name='Entity.CreatedAt']")!.HasAttribute("readonly"))
            .IsTrue();
    }
}
