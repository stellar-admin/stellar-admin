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

    [Test]
    public async Task SearchInput_RequestsFieldLookupWhenSheetOpensOrTermChanges()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields => fields.Add(model => model.CategoryId).UseEditor<LookupEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var search = document.RequiredElement("[data-lookup='search']");
        await Assert.That(search.GetAttribute("name")).IsEqualTo("term");
        await Assert.That(search.GetAttribute("form")).IsEqualTo("Entity_CategoryId-lookup-search");
        await Assert
            .That(search.GetAttribute("hx-get"))
            .IsEqualTo("/stellaradmin/products/lookup?form=create&field=CategoryId");
        await Assert
            .That(search.GetAttribute("hx-trigger"))
            .IsEqualTo("input changed delay:300ms, search, click from:#Entity_CategoryId-open");
        await Assert.That(search.GetAttribute("hx-target")).IsEqualTo("#Entity_CategoryId-results");
        await Assert
            .That(document.RequiredElement("[data-lookup='open']").Id)
            .IsEqualTo("Entity_CategoryId-open");
        await Assert
            .That(document.RequiredElement("[data-lookup='results']").Id)
            .IsEqualTo("Entity_CategoryId-results");
        await Assert
            .That(document.RequiredElement("[data-lookup='sheet']").GetAttribute("data-lookup-for"))
            .IsEqualTo("Entity_CategoryId");
    }

    [Test]
    public async Task OptionalFieldWithValue_ShowsClearButton()
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
        var clear = document.RequiredElement("[data-lookup='clear']");
        await Assert.That(clear.Id).IsEqualTo("Entity_CategoryId-clear");
        await Assert.That(clear.GetAttribute("type")).IsEqualTo("button");
        await Assert.That(clear.GetAttribute("data-lookup-for")).IsEqualTo("Entity_CategoryId");
        await Assert.That(clear.HasAttribute("hidden")).IsFalse();
    }

    [Test]
    public async Task OptionalFieldWithoutValue_HidesClearButton()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields => fields.Add(model => model.CategoryId).UseEditor<LookupEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-lookup='clear']").HasAttribute("hidden"))
            .IsTrue();
    }

    [Test]
    public async Task RequiredField_HasNoClearButton()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields.Add(model => model.PrimaryCategoryId).UseEditor<LookupEditor>(UseCategories),
            new() { PrimaryCategoryId = 1 }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert.That(document.QuerySelector("[data-lookup='clear']")).IsNull();
    }

    [Test]
    public async Task AllowClear_OverridesNullability()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.PrimaryCategoryId)
                    .UseEditor<LookupEditor>(lookup =>
                    {
                        lookup.AllowClear = true;
                        UseCategories(lookup);
                    }),
            new() { PrimaryCategoryId = 1 }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert.That(document.QuerySelector("[data-lookup='clear']")).IsNotNull();
    }

    [Test]
    public async Task Lookup_ListsItemValuesTextAndDescriptions()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields => fields.Add(model => model.CategoryId).UseEditor<LookupEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=create&field=CategoryId"
        );

        // Assert
        var options = document.QuerySelectorAll("[data-lookup-value]");
        await Assert
            .That(options.Select(option => option.GetAttribute("data-lookup-value")))
            .IsEquivalentTo(["1", "2"]);
        await Assert
            .That(options.Select(option => option.GetAttribute("type")))
            .IsEquivalentTo(["button", "button"]);
        await Assert
            .That(document.TextContents("[data-lookup-value]"))
            .IsEquivalentTo(["Cameras", "Notebooks"]);
        await Assert
            .That(document.TextContents("[data-slot='item-description']"))
            .IsEquivalentTo(["CAM", "NTB"]);
        await Assert.That(document.QuerySelector("[data-lookup='more']")).IsNull();
        await Assert.That(document.QuerySelector("[data-lookup='message']")).IsNull();
    }

    [Test]
    public async Task Lookup_FiltersByTerm()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields => fields.Add(model => model.CategoryId).UseEditor<LookupEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=create&field=CategoryId&term=%20note%20"
        );

        // Assert
        await Assert
            .That(document.TextContents("[data-lookup-value]"))
            .IsEquivalentTo(["Notebooks"]);
    }

    [Test]
    public async Task Lookup_MorePages_LoadsNextPageInPlaceOfButton()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupEditor>(lookup =>
                    {
                        lookup.PageSize = 1;
                        UseCategories(lookup);
                    }),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var first = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=create&field=CategoryId"
        );
        var more = first.RequiredElement("[data-lookup='more']");
        var second = await client.GetDocumentAsync(more.GetAttribute("hx-get")!);

        // Assert
        await Assert.That(first.TextContents("[data-lookup-value]")).IsEquivalentTo(["Cameras"]);
        await Assert.That(more.GetAttribute("type")).IsEqualTo("button");
        await Assert.That(more.GetAttribute("hx-swap")).IsEqualTo("outerHTML");
        await Assert
            .That(more.GetAttribute("hx-get"))
            .IsEqualTo("/stellaradmin/products/lookup?form=create&field=CategoryId&skip=1");
        await Assert.That(second.TextContents("[data-lookup-value]")).IsEquivalentTo(["Notebooks"]);
        await Assert.That(second.QuerySelector("[data-lookup='more']")).IsNull();
        await Assert.That(second.QuerySelector("[data-lookup='message']")).IsNull();
    }

    [Test]
    public async Task Lookup_NoMatches_ShowsNoResults()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields => fields.Add(model => model.CategoryId).UseEditor<LookupEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=create&field=CategoryId&term=tents"
        );

        // Assert
        await Assert.That(document.QuerySelector("[data-lookup-value]")).IsNull();
        await Assert
            .That(document.RequiredElement("[data-lookup='message']").TextContent)
            .IsEqualTo("No results found.");
    }

    [Test]
    public async Task Lookup_TermShorterThanMinimum_AsksForMoreCharacters()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupEditor>(lookup =>
                    {
                        lookup.MinimumSearchLength = 2;
                        UseCategories(lookup);
                    }),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=create&field=CategoryId&term=n"
        );

        // Assert
        await Assert.That(document.QuerySelector("[data-lookup-value]")).IsNull();
        await Assert
            .That(document.RequiredElement("[data-lookup='message']").TextContent)
            .IsEqualTo("Type at least 2 characters to search.");
    }

    [Test]
    [Arguments("form=create&field=PrimaryCategoryId")]
    [Arguments("form=edit&field=CategoryId")]
    [Arguments("form=index&field=CategoryId")]
    [Arguments("field=CategoryId")]
    [Arguments("form=create&field=CategoryId&skip=-1")]
    public async Task Lookup_UnknownLookupField_ReturnsNotFound(string query)
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
            {
                fields.Add(model => model.CategoryId).UseEditor<LookupEditor>(UseCategories);
                fields.Add(model => model.PrimaryCategoryId);
            },
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync($"/stellaradmin/products/lookup?{query}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
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
