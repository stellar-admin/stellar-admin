using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Editors;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class LookupEditorTests
{
    [Test]
    public async Task SelectedValue_DisplaysItemText()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields => fields.Add(model => model.CategoryId).UseEditor<LookupEditor>(UseCategories),
            new() { CategoryId = 2 }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var value = document.RequiredElement("[data-lookup='value']");
        await Assert.That(value.GetAttribute("type")).IsEqualTo("hidden");
        await Assert.That(value.GetAttribute("name")).IsEqualTo("Entity.CategoryId");
        await Assert.That(value.GetAttribute("value")).IsEqualTo("2");
        var display = document.RequiredElement("[data-lookup='display']");
        await Assert.That(display.GetAttribute("value")).IsEqualTo("Notebooks");
        await Assert.That(display.HasAttribute("name")).IsFalse();
        await Assert.That(display.HasAttribute("readonly")).IsTrue();
        await Assert
            .That(document.RequiredElement("label[for='Entity_CategoryId-display']").TextContent)
            .IsEqualTo("CategoryId");
        await Assert
            .That(display.GetAttribute("aria-describedby"))
            .IsEqualTo("Entity_CategoryId-error");
    }

    [Test]
    public async Task NoValue_DisplaysEmptyText()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupEditor>(lookup =>
                    {
                        lookup.EmptyText = "No category";
                        UseCategories(lookup);
                    }),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var display = document.RequiredElement("[data-lookup='display']");
        await Assert.That(display.GetAttribute("value") ?? "").IsEqualTo("");
        await Assert.That(display.GetAttribute("placeholder")).IsEqualTo("No category");
        await Assert
            .That(document.RequiredElement("[data-lookup='value']").GetAttribute("value"))
            .IsEqualTo("");
    }

    [Test]
    public async Task UnknownValue_DisplaysValue()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields => fields.Add(model => model.CategoryId).UseEditor<LookupEditor>(UseCategories),
            new() { CategoryId = 99 }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-lookup='display']").GetAttribute("value"))
            .IsEqualTo("99");
    }

    [Test]
    public async Task LookupButton_OpensSheetWithTitleAndSearch()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupEditor>(lookup =>
                    {
                        lookup.SheetTitle = "Select category";
                        lookup.SearchPlaceholder = "Search categories";
                        UseCategories(lookup);
                    }),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var open = document.RequiredElement("[data-lookup='open']");
        var sheet = document.RequiredElement("dialog[data-lookup='sheet']");
        await Assert.That(open.GetAttribute("type")).IsEqualTo("button");
        await Assert.That(open.GetAttribute("command")).IsEqualTo("show-modal");
        await Assert.That(open.GetAttribute("commandfor")).IsEqualTo(sheet.Id);
        await Assert
            .That(sheet.RequiredElement("[data-slot='sheet-title']").TextContent)
            .IsEqualTo("Select category");
        var search = sheet.RequiredElement("[data-lookup='search']");
        await Assert.That(search.GetAttribute("placeholder")).IsEqualTo("Search categories");
        await Assert.That(search.HasAttribute("name")).IsFalse();
        await Assert
            .That(sheet.RequiredElement("[data-lookup='results']").ChildElementCount)
            .IsEqualTo(0);
    }

    [Test]
    public async Task WithoutItems_FailsRendering()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields => fields.Add(model => model.CategoryId).UseEditor<LookupEditor>(),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/create");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .Contains("LookupEditor on CategoryId requires UseItems.");
    }

    private static void UseCategories(LookupEditor lookup) =>
        lookup.UseItems<CategoryLookupSource, Category, int>(
            category => category.Id,
            category => category.Name,
            items => items.DescribeWith(category => category.Code)
        );

    private static Task<WebApplication> CreateLookupFieldsHost(
        Action<ResourceFieldsBuilder<LookupFieldsModel>> configureFields,
        LookupFieldsModel model
    ) =>
        DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate<LookupFieldsModel, LookupFieldsHandler>(create =>
                {
                    create.UseFactory(() => model);
                    create.Fields(configureFields);
                }),
            configureServices: services => services.AddScoped<CategoryLookupSource>()
        );
}
