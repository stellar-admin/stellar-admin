using AngleSharp.Dom;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Testing;
using TUnit.Assertions.Enums;
using static StellarAdmin.Testing.FormTestHelpers;

namespace StellarAdmin.IntegrationTests.Resources;

public class ResourcePageSummaryTests
{
    [Test]
    [Arguments("/stellaradmin/products/view/7")]
    [Arguments("/stellaradmin/products/edit/7")]
    public async Task ResourceSummary_RendersItemsInOrderUnderTheTitle(string url)
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new ProductState([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                ConfigurePages(resource);
                resource.Summary(summary =>
                {
                    summary.AddText("Price");
                    summary.Add(
                        product => product.Price,
                        item => item.UseDisplay<NumberDisplay>(number => number.Format = "0.00")
                    );
                    summary.AddSeparator();
                    summary.Add(product => product.Name);
                });
            }
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync(url);

        // Assert
        var summary = document.RequiredElement(
            "[data-slot='page-header'] > [data-slot='page-summary']"
        );
        await Assert
            .That(summary.PreviousElementSibling?.GetAttribute("data-slot"))
            .IsEqualTo("page-header-title");
        await Assert
            .That(Describe(summary))
            .IsEquivalentTo(
                ["text:Price", "field:8.50", "separator:·", "field:Notebook"],
                CollectionOrdering.Matching
            );
    }

    [Test]
    public async Task SeparateItems_RendersSeparatorBetweenEveryTwoItems()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new ProductState([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                ConfigurePages(resource);
                resource.Summary(summary =>
                {
                    summary.SeparateItems = true;
                    summary.Add(product => product.Id);
                    summary.Add(product => product.Name);
                    summary.AddText("in stock");
                });
            }
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/view/7");

        // Assert
        await Assert
            .That(Describe(document.RequiredElement("[data-slot='page-summary']")))
            .IsEquivalentTo(
                ["field:7", "separator:·", "field:Notebook", "separator:·", "text:in stock"],
                CollectionOrdering.Matching
            );
    }

    [Test]
    public async Task FieldWithDisplay_RendersTheCompactTemplate()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new ProductState([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                ConfigurePages(resource);
                resource.Summary(summary =>
                    summary.Add(product => product.Name, item => item.UseDisplay<BadgeDisplay>())
                );
            }
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/view/7");

        // Assert
        var badge = document.RequiredElement("[data-slot='page-summary'] [data-slot='badge']");
        await Assert.That(badge.TextContent.Trim()).IsEqualTo("Notebook");
    }

    [Test]
    public async Task EmptyValue_ShowsTheEmptyTextAndKeepsTheItem()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new ProductState([new(7, "", 8.50m)]),
            resource =>
            {
                ConfigurePages(resource);
                resource.Summary(summary =>
                {
                    summary.SeparateItems = true;
                    summary.Add(product => product.Name);
                    summary.Add(product => product.Id);
                });
            }
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/view/7");

        // Assert
        var fields = document.QuerySelectorAll("[data-slot='page-summary-field']");
        await Assert.That(fields[0].TextContent.Trim()).IsEqualTo("—");
        await Assert.That(fields[0].GetAttribute("data-empty")).IsEqualTo("true");
        await Assert.That(fields[1].TextContent.Trim()).IsEqualTo("7");
    }

    [Test]
    public async Task EditPageAfterRejectedSave_SummarizesTheStoredRecord()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new ProductState([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                ConfigurePages(resource);
                resource.Summary(summary => summary.Add(product => product.Name));
            }
        );
        using var client = sut.CreateClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/7");
        values["Entity.Name"] = "Sketchbook";
        values["Entity.Price"] = "0";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/edit/7", content);

        // Assert
        var document = await response.ReadDocumentAsync();
        await Assert
            .That(document.RequiredElement("[data-slot='page-summary-field']").TextContent.Trim())
            .IsEqualTo("Notebook");
    }

    [Test]
    public async Task PageSummary_ReplacesTheResourceSummary()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new ProductState([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.Summary(summary => summary.AddText("Resource"));
                resource.AllowView(view =>
                {
                    view.Fields(fields => fields.Add(product => product.Name));
                    view.Summary(summary => summary.AddText("View"));
                });
                resource.AllowEdit(edit =>
                {
                    edit.Fields(fields => fields.Add(product => product.Name));
                    edit.Summary(summary => summary.Clear());
                });
            }
        );
        using var client = sut.CreateClient();

        // Act
        var view = await client.GetDocumentAsync("/stellaradmin/products/view/7");
        var edit = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        await Assert
            .That(view.RequiredElement("[data-slot='page-summary']").TextContent.Trim())
            .IsEqualTo("View");
        await Assert.That(edit.QuerySelector("[data-slot='page-summary']")).IsNull();
    }

    [Test]
    public async Task PageWithItsOwnModel_DoesNotShowTheResourceSummary()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new ProductState([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.Summary(summary => summary.AddText("Resource"));
                resource.AllowEdit<EditProductModel, EditProductHandler>(edit =>
                    edit.Fields(fields => fields.Add(model => model.ProductName))
                );
            }
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        await Assert.That(document.QuerySelector("[data-slot='page-summary']")).IsNull();
    }

    [Test]
    public async Task CreatePage_HasNoSummary()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new ProductState([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                ConfigurePages(resource);
                resource.Summary(summary => summary.AddText("Resource"));
            }
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert.That(document.QuerySelector("[data-slot='page-summary']")).IsNull();
    }

    // Each item as its kind and text, such as "field:8.50"
    private static IEnumerable<string> Describe(IElement summary) =>
        summary.Children.Select(item =>
            $"{item.GetAttribute("data-slot")!["page-summary-".Length..]}:{item.TextContent.Trim()}"
        );

    private static void ConfigurePages(ResourceBuilder<Product> resource)
    {
        resource.UseKey(product => product.Id);
        resource.AllowView(view => view.Fields(fields => fields.Add(product => product.Name)));
        resource.AllowCreate(create =>
            create.Fields(fields =>
            {
                fields.Add(product => product.Name);
                fields.Add(product => product.Price);
            })
        );
        resource.AllowEdit(edit =>
            edit.Fields(fields =>
            {
                fields.Add(product => product.Name);
                fields.Add(product => product.Price);
            })
        );
    }
}
