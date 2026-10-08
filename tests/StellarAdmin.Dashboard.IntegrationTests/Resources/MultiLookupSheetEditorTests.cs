using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Editors;
using TUnit.Assertions.Enums;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class MultiLookupSheetEditorTests
{
    private const string EditUrl = "/stellaradmin/products/edit/7";

    [Test]
    public async Task SelectedValues_PostEachValueAndTheEmptySelectionMarker()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(UseCategories),
            new() { CategoryIds = [2, 1] }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        var editor = document.RequiredElement("dashboard-multi-lookup-sheet-editor");
        await Assert
            .That(
                editor
                    .QuerySelectorAll("[data-lookup='values'] input[type='hidden']")
                    .Select(input => $"{input.GetAttribute("name")}={input.GetAttribute("value")}")
            )
            .IsEquivalentTo(
                ["Entity.CategoryIds=2", "Entity.CategoryIds=1"],
                CollectionOrdering.Matching
            );
        await Assert
            .That(
                editor
                    .RequiredElement("input[name='__sa_checkbox_group.Entity.CategoryIds']")
                    .GetAttribute("value")
            )
            .IsEqualTo("true");
    }

    [Test]
    public async Task ChipsLayout_IsTheDefaultAndShowsAChipPerItemInValueOrder()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(UseCategories),
            new() { CategoryIds = [2, 1] }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        var selected = document.RequiredElement("#Entity_CategoryIds-selected");
        await Assert.That(selected.GetAttribute("data-slot")).IsEqualTo("input-group");
        await Assert.That(selected.HasAttribute("hidden")).IsFalse();
        var chips = selected.QuerySelectorAll("[data-slot='badge'][data-lookup='item']");
        await Assert
            .That(chips.Select(chip => chip.GetAttribute("data-value")!))
            .IsEquivalentTo(["2", "1"], CollectionOrdering.Matching);
        await Assert
            .That(chips[0].RequiredElement("[data-lookup='remove'] .sr-only").TextContent)
            .IsEqualTo("Remove Notebooks");
        await Assert
            .That(selected.RequiredElement("#Entity_CategoryIds-add").TextContent.Trim())
            .IsEqualTo("Add categoryIds");
        await Assert
            .That(document.RequiredElement("#Entity_CategoryIds-empty").HasAttribute("hidden"))
            .IsTrue();
    }

    [Test]
    public async Task NoValues_ShowsChooseAndHidesSelection()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(lookup =>
                    {
                        lookup.Editor(editor => editor.EmptyText = "Pick categories");
                        UseCategories(lookup);
                    }),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        await Assert
            .That(document.QuerySelectorAll("[data-lookup='values'] input").Length)
            .IsEqualTo(0);
        await Assert
            .That(document.RequiredElement("#Entity_CategoryIds-selected").HasAttribute("hidden"))
            .IsTrue();
        await Assert
            .That(document.RequiredElement("#Entity_CategoryIds-empty").HasAttribute("hidden"))
            .IsFalse();
        var choose = document.RequiredElement("#Entity_CategoryIds-choose");
        await Assert.That(choose.TextContent.Trim()).IsEqualTo("Pick categories");
        await Assert.That(choose.GetAttribute("data-lookup")).IsEqualTo("open");
    }

    [Test]
    public async Task ListLayout_ShowsARowPerItemWithDescription()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(lookup =>
                    {
                        lookup.Editor(editor => editor.Layout = MultiLookupSheetEditorLayout.List);
                        UseCategories(lookup);
                    }),
            new() { CategoryIds = [1, 2] }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        var rows = document.QuerySelectorAll(
            "#Entity_CategoryIds-selected [role='listitem'][data-slot='item']"
        );
        await Assert
            .That(rows.Select(row => row.RequiredElement("[data-slot='item-title']").TextContent))
            .IsEquivalentTo(["Cameras", "Notebooks"], CollectionOrdering.Matching);
        await Assert
            .That(rows[0].RequiredElement("[data-slot='item-description']").TextContent)
            .IsEqualTo("CAM");
        await Assert.That(rows[0].GetAttribute("data-variant")).IsEqualTo("outline");
        await Assert.That(rows[0].QuerySelector("[data-lookup='remove']")).IsNotNull();
    }

    [Test]
    public async Task SummaryLayout_NamesTwoItemsAndCountsTheRest()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(lookup =>
                    {
                        lookup.Editor(editor =>
                            editor.Layout = MultiLookupSheetEditorLayout.Summary
                        );
                        UseCategories(lookup);
                    }),
            new() { CategoryIds = [1, 2, 3, 4] },
            categories => categories.AddRange([Category(3, "Lenses"), Category(4, "Tripods")])
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        var selected = document.RequiredElement("#Entity_CategoryIds-selected");
        await Assert.That(selected.GetAttribute("data-slot")).IsEqualTo("button-group");
        var open = selected.RequiredElement("#Entity_CategoryIds-change");
        await Assert
            .That(open.QuerySelector(".truncate")!.TextContent)
            .IsEqualTo("Cameras, Notebooks and 2 more");
        await Assert.That(open.QuerySelector(".tabular-nums")!.TextContent).IsEqualTo("4");
        await Assert.That(selected.QuerySelector("[data-lookup='clear']")).IsNotNull();
    }

    [Test]
    public async Task SummaryLayout_ConfiguredMoreText_EndsTheSummary()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(lookup =>
                    {
                        lookup.Editor(editor =>
                            editor.Layout = MultiLookupSheetEditorLayout.Summary
                        );
                        UseCategories(lookup);
                    }),
            new() { CategoryIds = [1, 2, 3] },
            categories => categories.Add(Category(3, "Lenses")),
            dashboard =>
                dashboard.ConfigureResourceLabels(labels =>
                    labels.Lookup(lookup => lookup.MoreText = context => $"+{context.Count}")
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        await Assert
            .That(document.RequiredElement("#Entity_CategoryIds-change .truncate").TextContent)
            .IsEqualTo("Cameras, Notebooks +1");
    }

    [Test]
    public async Task ReadOnlySummary_ShowsMutedListWithoutInputs()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.FixedCategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(lookup =>
                    {
                        lookup.Editor(editor =>
                            editor.Layout = MultiLookupSheetEditorLayout.Summary
                        );
                        UseCategories(lookup);
                    }),
            new() { FixedCategoryIds = [2] }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        await Assert.That(document.QuerySelector("dashboard-multi-lookup-sheet-editor")).IsNull();
        await Assert
            .That(document.QuerySelectorAll("input[name='Entity.FixedCategoryIds']").Length)
            .IsEqualTo(0);
        var row = document.RequiredElement("[role='list'] [data-slot='item']");
        await Assert.That(row.GetAttribute("data-variant")).IsEqualTo("muted");
        await Assert
            .That(row.RequiredElement("[data-slot='item-title']").TextContent)
            .IsEqualTo("Notebooks");
        await Assert.That(row.QuerySelector("[data-lookup='remove']")).IsNull();
    }

    [Test]
    public async Task ReadOnlyChips_ShowsChipsWithoutRemove()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.FixedCategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(UseCategories),
            new() { FixedCategoryIds = [1, 2] }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        var chips = document.QuerySelectorAll("[role='list'] [data-slot='badge']");
        await Assert
            .That(chips.Select(chip => chip.TextContent.Trim()))
            .IsEquivalentTo(["Cameras", "Notebooks"], CollectionOrdering.Matching);
        await Assert.That(document.QuerySelector("[data-lookup='remove']")).IsNull();
    }

    [Test]
    public async Task ReadOnlyEmpty_ShowsNone()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.FixedCategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        await Assert
            .That(document.RequiredElement("[data-slot='field'] .sa-input").TextContent.Trim())
            .IsEqualTo("None");
    }

    [Test]
    public async Task UnknownValue_DisplaysValue()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(UseCategories),
            new() { CategoryIds = [1, 99] }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        await Assert
            .That(
                document
                    .QuerySelectorAll("[data-lookup='item']")
                    .Select(chip => chip.QuerySelector("span")!.TextContent)
            )
            .IsEquivalentTo(["Cameras", "99"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task WithoutItems_FailsRendering()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields => fields.Add(model => model.CategoryIds).UseEditor<MultiLookupSheetEditor>(),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(EditUrl);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .Contains("MultiLookupSheetEditor on CategoryIds requires UseItems.");
    }

    [Test]
    public async Task PostedValues_ReplaceTheSelection()
    {
        // Arrange
        var state = new MultiLookupFieldsState { Model = new() { CategoryIds = [1] } };
        await using var sut = await CreateStateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(UseCategories),
            state
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, EditUrl);
        using var content = new FormUrlEncodedContent(
            values.Concat([
                new("__sa_checkbox_group.Entity.CategoryIds", "true"),
                new("Entity.CategoryIds", "2"),
                new("Entity.CategoryIds", "1"),
            ])
        );

        // Act
        using var response = await client.PostAsync(EditUrl, content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(state.Model.CategoryIds)
            .IsEquivalentTo([2, 1], CollectionOrdering.Matching);
    }

    [Test]
    public async Task PostedMarkerWithoutValues_ClearsTheSelection()
    {
        // Arrange
        var state = new MultiLookupFieldsState { Model = new() { CategoryIds = [1, 2] } };
        await using var sut = await CreateStateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(UseCategories),
            state
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, EditUrl);
        values["__sa_checkbox_group.Entity.CategoryIds"] = "true";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(EditUrl, content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(state.Model.CategoryIds).IsEmpty();
    }

    [Test]
    public async Task RejectedPost_ShowsPostedItemsWithTitlesAndTheError()
    {
        // Arrange
        var state = new MultiLookupFieldsState { Model = new() { LimitedCategoryIds = [1] } };
        await using var sut = await CreateStateHost(
            fields =>
                fields
                    .Add(model => model.LimitedCategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(UseCategories),
            state,
            categories => categories.Add(Category(3, "Lenses"))
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, EditUrl);
        using var content = new FormUrlEncodedContent(
            values.Concat([
                new("__sa_checkbox_group.Entity.LimitedCategoryIds", "true"),
                new("Entity.LimitedCategoryIds", "3"),
                new("Entity.LimitedCategoryIds", "1"),
                new("Entity.LimitedCategoryIds", "2"),
            ])
        );

        // Act
        using var response = await client.PostAsync(EditUrl, content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(state.Model.LimitedCategoryIds)
            .IsEquivalentTo([1], CollectionOrdering.Matching);
        await Assert
            .That(
                document
                    .QuerySelectorAll("#Entity_LimitedCategoryIds-selected [data-lookup='item']")
                    .Select(chip => chip.QuerySelector("span")!.TextContent)
            )
            .IsEquivalentTo(["Lenses", "Cameras", "Notebooks"], CollectionOrdering.Matching);
        await Assert
            .That(document.RequiredElement("#Entity_LimitedCategoryIds-error").TextContent)
            .IsNotEmpty();
        await Assert
            .That(
                document
                    .RequiredElement("#Entity_LimitedCategoryIds-selected")
                    .ClassList.Contains("border-destructive")
            )
            .IsTrue();
    }

    [Test]
    public async Task OpenButtons_OpenTheSheetAndTheEditorRendersItsItems()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(UseCategories),
            new() { CategoryIds = [1] }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        var editor = document.RequiredElement("dashboard-multi-lookup-sheet-editor");
        await Assert.That(editor.Id).IsEqualTo("Entity_CategoryIds");
        await Assert.That(editor.GetAttribute("name")).IsEqualTo("Entity.CategoryIds");
        await Assert
            .That(editor.GetAttribute("selection-url"))
            .IsEqualTo("/stellaradmin/products/multilookupselection?form=edit&field=CategoryIds");
        await Assert
            .That(
                editor
                    .QuerySelectorAll("[data-lookup='open']")
                    .Select(button => button.GetAttribute("data-sheet-open"))
            )
            .IsEquivalentTo([
                "/stellaradmin/products/lookupsheet?form=edit&field=CategoryIds&for=Entity_CategoryIds",
                "/stellaradmin/products/lookupsheet?form=edit&field=CategoryIds&for=Entity_CategoryIds",
            ]);
    }

    [Test]
    public async Task LookupSheet_ShowsAllAndSelectedViewsAndDone()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookupsheet?form=edit&field=CategoryIds&for=Entity_CategoryIds"
        );

        // Assert
        var picker = document.RequiredElement("dashboard-lookup-picker");
        await Assert.That(picker.HasAttribute("multiple")).IsTrue();
        var views = picker.QuerySelectorAll("[data-lookup='view'] input[type='radio']");
        await Assert
            .That(views.Select(view => view.GetAttribute("value")!))
            .IsEquivalentTo(["all", "selected"], CollectionOrdering.Matching);
        await Assert.That(views[0].HasAttribute("checked")).IsTrue();
        await Assert
            .That(picker.RequiredElement("[data-lookup='count']").TextContent)
            .IsEqualTo("0");
        await Assert
            .That(picker.RequiredElement("[data-lookup='clear-all']").HasAttribute("hidden"))
            .IsTrue();
        await Assert
            .That(picker.RequiredElement("[data-lookup='done']").TextContent.Trim())
            .IsEqualTo("Done");
    }

    [Test]
    public async Task Lookup_ChecksEachSelectedItem()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=edit&field=CategoryIds&selected=1&selected=2"
        );

        // Assert
        var items = document.QuerySelectorAll("[data-lookup='item']");
        await Assert
            .That(items.Select(item => item.GetAttribute("data-checked")))
            .IsEquivalentTo(["true", "true"]);
        await Assert.That(document.QuerySelector("template[data-lookup='selection']")).IsNull();
    }

    [Test]
    public async Task Lookup_SelectedOnly_ShowsSelectedItemsInValueOrder()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(lookup =>
                    {
                        lookup.Sheet(sheet => sheet.PageSize = 1);
                        UseCategories(lookup);
                    }),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=edit&field=CategoryIds&selected=2&selected=1&selectedOnly=true"
        );

        // Assert
        await Assert
            .That(document.TextContents("[data-lookup='item-title']"))
            .IsEquivalentTo(["Notebooks", "Cameras"], CollectionOrdering.Matching);
        await Assert.That(document.QuerySelector("[data-lookup='more']")).IsNull();
    }

    [Test]
    public async Task Lookup_SelectedOnlyWithTerm_ShowsMatchingSelectedItems()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=edit&field=CategoryIds&selected=2&selected=1&selectedOnly=true&term=ntb"
        );

        // Assert
        await Assert
            .That(document.TextContents("[data-lookup='item-title']"))
            .IsEquivalentTo(["Notebooks"]);
    }

    [Test]
    [Arguments("selected=1&term=lens", "No selected categoryIds match “lens”")]
    [Arguments("", "No categoryIds selected")]
    public async Task Lookup_SelectedOnlyWithoutMatches_ShowsNoSelectedTitle(
        string query,
        string title
    )
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            $"/stellaradmin/products/lookup?form=edit&field=CategoryIds&selectedOnly=true&{query}"
        );

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement("[data-lookup='message'] [data-slot='empty-title']")
                    .TextContent
            )
            .IsEqualTo(title);
        await Assert.That(document.QuerySelector("[data-lookup='item']")).IsNull();
    }

    [Test]
    public async Task MultiLookupSelection_RendersTheLayoutsItemsInValueOrder()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/multilookupselection?form=edit&field=CategoryIds&selected=2&selected=1"
        );

        // Assert
        var chips = document.QuerySelectorAll("[data-slot='badge'][data-lookup='item']");
        await Assert
            .That(chips.Select(chip => chip.GetAttribute("data-value")!))
            .IsEquivalentTo(["2", "1"], CollectionOrdering.Matching);
        await Assert
            .That(chips[0].RequiredElement("[data-lookup='remove'] .sr-only").TextContent)
            .IsEqualTo("Remove Notebooks");
    }

    [Test]
    public async Task MultiLookupSelection_SummaryLayout_RendersTheSummary()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(lookup =>
                    {
                        lookup.Editor(editor =>
                            editor.Layout = MultiLookupSheetEditorLayout.Summary
                        );
                        UseCategories(lookup);
                    }),
            new(),
            categories => categories.Add(Category(3, "Lenses"))
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/multilookupselection?form=edit&field=CategoryIds&selected=3&selected=1&selected=2"
        );

        // Assert
        await Assert
            .That(document.RequiredElement(".truncate").TextContent)
            .IsEqualTo("Lenses, Cameras and 1 more");
        await Assert.That(document.RequiredElement(".tabular-nums").TextContent).IsEqualTo("3");
    }

    [Test]
    [Arguments("form=edit&field=CategoryIds&selected=camera")]
    [Arguments("form=create&field=CategoryIds&selected=1")]
    [Arguments("form=edit&field=Unknown&selected=1")]
    public async Task MultiLookupSelection_InvalidValueOrField_ReturnsNotFound(string query)
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var response = await client.GetAsync(
            $"/stellaradmin/products/multilookupselection?{query}"
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task LookupSelection_MultiLookupField_ReturnsNotFound()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var response = await client.GetAsync(
            "/stellaradmin/products/lookupselection?form=edit&field=CategoryIds&value=1"
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    private static Category Category(int id, string name) =>
        new()
        {
            Id = id,
            Name = name,
            Code = name[..3].ToUpperInvariant(),
        };

    private static Task<WebApplication> CreateHost(
        Action<ResourceFieldsBuilder<MultiLookupFieldsModel>> configureFields,
        MultiLookupFieldsModel model,
        Action<List<Category>>? configureCategories = null,
        Action<StellarAdminDashboardBuilder>? configureDashboard = null
    ) =>
        CreateStateHost(
            configureFields,
            new MultiLookupFieldsState { Model = model },
            configureCategories,
            configureDashboard
        );

    private static Task<WebApplication> CreateStateHost(
        Action<ResourceFieldsBuilder<MultiLookupFieldsModel>> configureFields,
        MultiLookupFieldsState state,
        Action<List<Category>>? configureCategories = null,
        Action<StellarAdminDashboardBuilder>? configureDashboard = null
    )
    {
        var categories = new CategoryStore();
        configureCategories?.Invoke(categories.Categories);

        return DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit<MultiLookupFieldsModel, MultiLookupFieldsHandler>(edit =>
                    edit.Fields(configureFields)
                );
            },
            configureDashboard,
            services =>
                services
                    .AddSingleton(state)
                    .AddSingleton(categories)
                    .AddScoped<CategoryLookupSource>()
        );
    }

    private static void UseCategories(MultiLookupSheetEditor lookup) =>
        lookup.UseItems<CategoryLookupSource, Category, int>(
            category => category.Id,
            category => category.Name,
            items => items.UseDescription(category => category.Code)
        );
}
