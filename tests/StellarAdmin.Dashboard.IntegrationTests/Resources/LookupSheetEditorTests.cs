using System.Net;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Editors;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class LookupSheetEditorTests
{
    [Test]
    public async Task SelectedValue_ShowsItemAndHidesEmptyButtons()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories),
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
        var selected = document.RequiredElement("#Entity_CategoryId-selected");
        await Assert.That(selected.HasAttribute("hidden")).IsFalse();
        await Assert
            .That(selected.RequiredElement("[data-slot='item-title']").TextContent.Trim())
            .IsEqualTo("Notebooks");
        await Assert
            .That(selected.RequiredElement("[data-slot='item-description']").TextContent)
            .IsEqualTo("NTB");
        await Assert
            .That(selected.GetAttribute("aria-labelledby"))
            .IsEqualTo("Entity_CategoryId-label");
        await Assert
            .That(selected.GetAttribute("aria-describedby"))
            .IsEqualTo("Entity_CategoryId-error");
        await Assert
            .That(document.RequiredElement("#Entity_CategoryId-label").TextContent)
            .IsEqualTo("CategoryId");
        await Assert
            .That(document.RequiredElement("#Entity_CategoryId-empty").HasAttribute("hidden"))
            .IsTrue();
    }

    [Test]
    public async Task NoValue_ShowsChooseButtonAndHidesSelection()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-lookup='value']").GetAttribute("value"))
            .IsEqualTo("");
        await Assert
            .That(document.RequiredElement("#Entity_CategoryId-selected").HasAttribute("hidden"))
            .IsTrue();
        await Assert
            .That(document.RequiredElement("#Entity_CategoryId-empty").HasAttribute("hidden"))
            .IsFalse();
        var choose = document.RequiredElement("#Entity_CategoryId-choose");
        await Assert.That(choose.TextContent.Trim()).IsEqualTo("Choose categoryId");
        await Assert.That(choose.GetAttribute("data-lookup")).IsEqualTo("open");
        await Assert.That(document.QuerySelector("[data-lookup='create']")).IsNull();
    }

    [Test]
    public async Task EmptyText_ReplacesChooseText()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupSheetEditor>(lookup =>
                    {
                        lookup.Editor(editor => editor.EmptyText = "Pick a category");
                        UseCategories(lookup);
                    }),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(document.RequiredElement("#Entity_CategoryId-choose").TextContent.Trim())
            .IsEqualTo("Pick a category");
    }

    [Test]
    public async Task ConfiguredLabels_ReplaceEditorText()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupSheetEditor>(lookup =>
                    {
                        lookup.EnableCreate();
                        UseCategories(lookup);
                    }),
            new(),
            dashboard =>
                dashboard
                    .AddCategoryResource()
                    .ConfigureResourceLabels(labels =>
                        labels.Lookup(lookup =>
                        {
                            lookup.ChooseLabel = context => $"Find a {context.FieldLabel}";
                            lookup.CreateLabel = context => $"Add {context.FieldLabel}";
                        })
                    )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(document.RequiredElement("#Entity_CategoryId-choose").TextContent.Trim())
            .IsEqualTo("Find a CategoryId");
        await Assert
            .That(document.RequiredElement("[data-lookup='create']").TextContent.Trim())
            .IsEqualTo("Add CategoryId");
    }

    [Test]
    public async Task EnableCreate_ShowsNewButtonWhileEmpty()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupSheetEditor>(lookup =>
                    {
                        lookup.EnableCreate();
                        UseCategories(lookup);
                    }),
            new(),
            dashboard => dashboard.AddCategoryResource()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var create = document.RequiredElement("#Entity_CategoryId-empty [data-lookup='create']");
        await Assert.That(create.GetAttribute("type")).IsEqualTo("button");
        await Assert.That(create.TextContent.Trim()).IsEqualTo("New");
    }

    [Test]
    public async Task ItemsWithDescription_DefaultToCardLayout()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories),
            new() { CategoryId = 2 }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(document.RequiredElement("#Entity_CategoryId-selected").GetAttribute("data-slot"))
            .IsEqualTo("item");
    }

    [Test]
    public async Task ItemsWithoutDescription_DefaultToInputLayout()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupSheetEditor>(lookup =>
                        lookup.UseItems<CategoryLookupSource, Category, int>(
                            category => category.Id,
                            category => category.Name
                        )
                    ),
            new() { CategoryId = 2 }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var selected = document.RequiredElement("#Entity_CategoryId-selected");
        await Assert.That(selected.GetAttribute("data-slot")).IsEqualTo("button-group");
        await Assert
            .That(selected.RequiredElement("[data-lookup='text']").TextContent.Trim())
            .IsEqualTo("Notebooks");
    }

    [Test]
    public async Task InputLayout_HidesDescription()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupSheetEditor>(lookup =>
                    {
                        lookup.Editor(editor => editor.Layout = LookupSheetEditorLayout.Input);
                        UseCategories(lookup);
                    }),
            new() { CategoryId = 2 }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var selected = document.RequiredElement("#Entity_CategoryId-selected");
        await Assert.That(selected.GetAttribute("data-slot")).IsEqualTo("button-group");
        await Assert.That(selected.TextContent).DoesNotContain("NTB");
    }

    [Test]
    public async Task UseCode_ShowsCode()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupSheetEditor>(lookup =>
                        lookup.UseItems<CategoryLookupSource, Category, int>(
                            category => category.Id,
                            category => category.Name,
                            items => items.UseCode(category => category.Code)
                        )
                    ),
            new() { CategoryId = 2 }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var media = document.RequiredElement("#Entity_CategoryId-selected [data-lookup='media']");
        await Assert.That(media.HasAttribute("hidden")).IsFalse();
        await Assert.That(media.TextContent.Trim()).IsEqualTo("NTB");
    }

    [Test]
    public async Task UseAvatar_ShowsImageOrInitials()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
            {
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupSheetEditor>(lookup =>
                        lookup.UseItems<CategoryLookupSource, Category, int>(
                            category => category.Id,
                            category => category.Name,
                            items => items.UseAvatar(category => $"/images/{category.Code}.png")
                        )
                    );
                fields
                    .Add(model => model.PrimaryCategoryId)
                    .UseEditor<LookupSheetEditor>(lookup =>
                        lookup.UseItems<CategoryLookupSource, Category, int>(
                            category => category.Id,
                            category => category.Name,
                            items => items.UseAvatar(_ => null)
                        )
                    );
            },
            new() { CategoryId = 2, PrimaryCategoryId = 1 }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement("#Entity_CategoryId-selected [data-slot='avatar-image']")
                    .GetAttribute("src")
            )
            .IsEqualTo("/images/NTB.png");
        await Assert
            .That(
                document
                    .RequiredElement(
                        "#Entity_PrimaryCategoryId-selected [data-slot='avatar-fallback']"
                    )
                    .TextContent
            )
            .IsEqualTo("CA");
    }

    [Test]
    public async Task UseImageAndUseIcon_ShowMediaWithMediaClasses()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
            {
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupSheetEditor>(lookup =>
                    {
                        lookup.ClassNames.Media = "size-7";
                        lookup.UseItems<CategoryLookupSource, Category, int>(
                            category => category.Id,
                            category => category.Name,
                            items => items.UseImage(category => $"/images/{category.Code}.png")
                        );
                    });
                fields
                    .Add(model => model.PrimaryCategoryId)
                    .UseEditor<LookupSheetEditor>(lookup =>
                        lookup.UseItems<CategoryLookupSource, Category, int>(
                            category => category.Id,
                            category => category.Name,
                            items => items.UseIcon(_ => "tag")
                        )
                    );
            },
            new() { CategoryId = 2, PrimaryCategoryId = 1 }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var image = document.RequiredElement("#Entity_CategoryId-selected img[data-media='image']");
        await Assert.That(image.GetAttribute("src")).IsEqualTo("/images/NTB.png");
        await Assert.That(image.GetAttribute("data-placement")).IsEqualTo("lookup");
        await Assert.That(image.ClassList.Contains("size-7")).IsTrue();
        var icon = document.RequiredElement(
            "#Entity_PrimaryCategoryId-selected svg[data-media='icon']"
        );
        await Assert.That(icon.ClassList.Contains("sa-choice-media")).IsTrue();
    }

    [Test]
    public async Task ShowMediaFalse_HidesMedia()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupSheetEditor>(lookup =>
                    {
                        lookup.UseItems<CategoryLookupSource, Category, int>(
                            category => category.Id,
                            category => category.Name,
                            items => items.UseCode(category => category.Code)
                        );
                        lookup.Editor(editor => editor.ShowMedia = false);
                    }),
            new() { CategoryId = 2 }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var media = document.RequiredElement("#Entity_CategoryId-selected [data-lookup='media']");
        await Assert.That(media.HasAttribute("hidden")).IsTrue();
        await Assert.That(media.TextContent.Trim()).IsEqualTo("");
    }

    [Test]
    public async Task ReadOnlySelected_ShowsItemWithoutButtons()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.FixedCategoryId)
                    .UseEditor<LookupSheetEditor>(UseCategories),
            new() { FixedCategoryId = 2 }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var field = document.RequiredElement("[data-slot='field']");
        await Assert
            .That(field.RequiredElement("[data-slot='item']").GetAttribute("data-variant"))
            .IsEqualTo("muted");
        await Assert
            .That(field.RequiredElement("[data-slot='item-title']").TextContent.Trim())
            .IsEqualTo("Notebooks");
        await Assert.That(field.QuerySelector("button")).IsNull();
    }

    [Test]
    public async Task ReadOnlyEmpty_ShowsNone()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.FixedCategoryId)
                    .UseEditor<LookupSheetEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var field = document.RequiredElement("[data-slot='field']");
        await Assert
            .That(field.RequiredElement("[data-slot='item']").TextContent.Trim())
            .IsEqualTo("None");
        await Assert.That(field.QuerySelector("button")).IsNull();
    }

    [Test]
    public async Task InvalidValue_MarksEmptyButtonsInvalid()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.PrimaryCategoryId)
                    .UseEditor<LookupSheetEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client);
        values["Entity.PrimaryCategoryId"] = "";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/create", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var document = await response.ReadDocumentAsync();
        await Assert
            .That(
                document
                    .RequiredElement("#Entity_PrimaryCategoryId-empty [data-slot='input-group']")
                    .ClassList
            )
            .Contains("border-destructive");
    }

    [Test]
    public async Task UnknownValue_DisplaysValue()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories),
            new() { CategoryId = 99 }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement("#Entity_CategoryId-selected [data-slot='item-title']")
                    .TextContent.Trim()
            )
            .IsEqualTo("99");
    }

    [Test]
    public async Task LookupButtons_OpenSheet()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var openers = document.QuerySelectorAll("[data-lookup='open']");
        await Assert.That(openers.Length).IsEqualTo(2);
        foreach (var open in openers)
        {
            await Assert.That(open.GetAttribute("type")).IsEqualTo("button");
            await Assert
                .That(open.GetAttribute("data-sheet-open"))
                .IsEqualTo(
                    "/stellaradmin/products/lookupsheet?form=create&field=CategoryId&for=Entity_CategoryId"
                );
            await Assert.That(open.HasAttribute("hx-get")).IsFalse();
        }
    }

    [Test]
    public async Task WithoutItems_FailsRendering()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields => fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/create");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .Contains("LookupSheetEditor on CategoryId requires UseItems.");
    }

    [Test]
    public async Task OptionalField_ShowsClearButton()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories),
            new() { CategoryId = 2 }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var clear = document.RequiredElement("#Entity_CategoryId-selected [data-lookup='clear']");
        await Assert.That(clear.GetAttribute("type")).IsEqualTo("button");
        await Assert.That(clear.Closest("dashboard-lookup-sheet-editor")).IsNotNull();
    }

    [Test]
    public async Task RequiredField_HasNoClearButton()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.PrimaryCategoryId)
                    .UseEditor<LookupSheetEditor>(UseCategories),
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
                    .UseEditor<LookupSheetEditor>(lookup =>
                    {
                        lookup.Editor(editor => editor.AllowClear = true);
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
            fields =>
                fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=create&field=CategoryId"
        );

        // Assert
        var options = document.QuerySelectorAll("[data-lookup='item']");
        await Assert
            .That(options.Select(option => option.GetAttribute("data-value")))
            .IsEquivalentTo(["1", "2"]);
        await Assert
            .That(options.Select(option => option.GetAttribute("role")))
            .IsEquivalentTo(["option", "option"]);
        await Assert
            .That(document.TextContents("[data-lookup='item-title']"))
            .IsEquivalentTo(["Cameras", "Notebooks"]);
        await Assert
            .That(document.TextContents("[data-lookup='item-description']"))
            .IsEquivalentTo(["CAM", "NTB"]);
        await Assert.That(document.QuerySelector("[data-checked]")).IsNull();
        await Assert.That(document.QuerySelector("[data-lookup='more']")).IsNull();
        await Assert.That(document.QuerySelector("[data-lookup='message']")).IsNull();
    }

    [Test]
    public async Task Lookup_ResultsCarryTheEditorDisplayOfEachItem()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupSheetEditor>(lookup =>
                        lookup.UseItems<CategoryLookupSource, Category, int>(
                            category => category.Id,
                            category => category.Name,
                            items => items.UseCode(category => category.Code)
                        )
                    ),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=create&field=CategoryId"
        );

        // Assert
        var selection = (IHtmlTemplateElement)
            document.RequiredElement("[data-value='2'] template[data-lookup='selection']");
        await Assert
            .That(selection.Content.QuerySelector("[data-lookup='media']")!.TextContent.Trim())
            .IsEqualTo("NTB");
        await Assert
            .That(selection.Content.QuerySelector("[data-lookup='text']")!.TextContent.Trim())
            .IsEqualTo("Notebooks");
    }

    [Test]
    public async Task Lookup_ChecksSelectedValue()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=create&field=CategoryId&selected=2"
        );

        // Assert
        await Assert
            .That(
                document
                    .QuerySelectorAll("[data-lookup='item']")
                    .Select(item => item.GetAttribute("data-checked"))
            )
            .IsEquivalentTo([null, "true"]);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Lookup_ShowsMediaUnlessSheetHidesIt(bool showMedia)
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupSheetEditor>(lookup =>
                    {
                        lookup.UseItems<CategoryLookupSource, Category, int>(
                            category => category.Id,
                            category => category.Name,
                            items => items.UseCode(category => category.Code)
                        );
                        lookup.Sheet(sheet => sheet.ShowMedia = showMedia);
                    }),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=create&field=CategoryId"
        );

        // Assert
        var item = document.RequiredElement("[data-value='2']");
        await Assert
            .That(item.Children.Any(child => child.TextContent.Trim() == "NTB"))
            .IsEqualTo(showMedia);
    }

    [Test]
    public async Task LookupSheet_LoadingRowsTakeTheShapeOfTheResults()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupSheetEditor>(lookup =>
                        lookup.UseItems<CategoryLookupSource, Category, int>(
                            category => category.Id,
                            category => category.Name,
                            items =>
                            {
                                items.UseDescription(category => category.Name);
                                items.UseCode(category => category.Code);
                            }
                        )
                    ),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookupsheet?form=create&field=CategoryId&for=Entity_CategoryId"
        );

        // Assert
        var loading = (IHtmlTemplateElement)
            document.RequiredElement("template[data-lookup='loading']");
        await Assert.That(loading.Content.QuerySelectorAll(".sa-command-item").Length).IsEqualTo(4);
        await Assert
            .That(loading.Content.QuerySelectorAll("[data-slot='skeleton'].size-7").Length)
            .IsEqualTo(4);
        await Assert
            .That(loading.Content.QuerySelectorAll("[data-slot='skeleton'].h-3").Length)
            .IsEqualTo(4);
    }

    [Test]
    public async Task LookupSheet_ImageResults_LoadingRowsTakeTheImageShape()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupSheetEditor>(lookup =>
                        lookup.UseItems<CategoryLookupSource, Category, int>(
                            category => category.Id,
                            category => category.Name,
                            items => items.UseImage(category => $"/images/{category.Code}.png")
                        )
                    ),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookupsheet?form=create&field=CategoryId&for=Entity_CategoryId"
        );

        // Assert
        var loading = (IHtmlTemplateElement)
            document.RequiredElement("template[data-lookup='loading']");
        await Assert
            .That(
                loading.Content.QuerySelectorAll("[data-slot='skeleton'].size-6.rounded-sm").Length
            )
            .IsEqualTo(4);
    }

    [Test]
    public async Task Lookup_FiltersByTerm()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=create&field=CategoryId&term=%20note%20"
        );

        // Assert
        await Assert
            .That(document.TextContents("[data-lookup='item-title']"))
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
                    .UseEditor<LookupSheetEditor>(lookup =>
                    {
                        lookup.Sheet(sheet => sheet.PageSize = 1);
                        UseCategories(lookup);
                    }),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var first = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=create&field=CategoryId&selected=2"
        );
        var more = first.RequiredElement("[data-lookup='more']");
        // The picker sends the selected value with every request
        var second = await client.GetDocumentAsync($"{more.GetAttribute("hx-get")}&selected=2");

        // Assert
        await Assert
            .That(first.TextContents("[data-lookup='item-title']"))
            .IsEquivalentTo(["Cameras"]);
        await Assert.That(more.GetAttribute("role")).IsEqualTo("option");
        await Assert.That(more.GetAttribute("hx-swap")).IsEqualTo("outerHTML");
        await Assert
            .That(more.GetAttribute("hx-get"))
            .IsEqualTo("/stellaradmin/products/lookup?form=create&field=CategoryId&skip=1");
        await Assert
            .That(second.TextContents("[data-lookup='item-title']"))
            .IsEquivalentTo(["Notebooks"]);
        await Assert
            .That(second.RequiredElement("[data-lookup='item']").GetAttribute("data-checked"))
            .IsEqualTo("true");
        await Assert.That(second.QuerySelector("[data-lookup='more']")).IsNull();
        await Assert.That(second.QuerySelector("[data-lookup='message']")).IsNull();
    }

    [Test]
    public async Task Lookup_NoMatches_ShowsNoResults()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories),
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=create&field=CategoryId&term=tents"
        );

        // Assert
        await Assert.That(document.QuerySelector("[data-lookup='item']")).IsNull();
        var message = document.RequiredElement("[data-lookup='message']");
        await Assert
            .That(message.RequiredElement("[data-slot='empty-title']").TextContent)
            .IsEqualTo("No results found");
        await Assert
            .That(message.RequiredElement("[data-slot='empty-description']").TextContent)
            .IsEqualTo("Nothing matches “tents”.");
    }

    [Test]
    public async Task Lookup_ConfiguredLabels_ReplaceNoResultsText()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories),
            new(),
            dashboard =>
                dashboard.ConfigureResourceLabels(labels =>
                    labels.Lookup(lookup =>
                        lookup.NoResultsDescription = context =>
                            $"No {context.FieldLabel} matches {context.Term}"
                    )
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=create&field=CategoryId&term=tents"
        );

        // Assert
        await Assert
            .That(document.RequiredElement("[data-slot='empty-description']").TextContent)
            .IsEqualTo("No CategoryId matches tents");
    }

    [Test]
    public async Task Lookup_TermShorterThanMinimum_AsksForMoreCharacters()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupSheetEditor>(lookup =>
                    {
                        lookup.Sheet(sheet => sheet.MinimumSearchLength = 2);
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
        await Assert.That(document.QuerySelector("[data-lookup='item']")).IsNull();
        await Assert
            .That(document.RequiredElement("[data-lookup='message']").TextContent.Trim())
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
                fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories);
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

    [Test]
    public async Task LookupSheet_RendersSearchPanelForTheOpeningEditor()
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<LookupSheetEditor>(lookup =>
                    {
                        lookup.Sheet(sheet =>
                        {
                            sheet.Title = "Select category";
                            sheet.SearchPlaceholder = "Search categories";
                        });
                        UseCategories(lookup);
                    }),
            new()
        );
        using var client = sut.GetTestClient();
        var page = await client.GetDocumentAsync("/stellaradmin/products/create");
        var open = page.RequiredElement("[data-lookup='open']");

        // Act
        var panel = await client.GetDocumentAsync(open.GetAttribute("data-sheet-open")!);

        // Assert
        var root = panel.RequiredElement("dashboard-lookup-picker");
        await Assert
            .That(root.GetAttribute("for"))
            .IsEqualTo(page.RequiredElement("[data-lookup='value']").Id);
        await Assert
            .That(root.RequiredElement("[data-slot='sheet-title']").TextContent)
            .IsEqualTo("Select category");
        var search = root.RequiredElement("[data-lookup='search']");
        await Assert.That(search.GetAttribute("placeholder")).IsEqualTo("Search categories");
        await Assert.That(search.GetAttribute("name")).IsEqualTo("term");
        await Assert
            .That(search.GetAttribute("hx-get"))
            .IsEqualTo("/stellaradmin/products/lookup?form=create&field=CategoryId");
        await Assert.That(search.GetAttribute("hx-target")).IsEqualTo("#Entity_CategoryId-results");
        await Assert.That(search.GetAttribute("hx-trigger")).StartsWith("load");
        await Assert
            .That(root.RequiredElement("#Entity_CategoryId-results").ChildElementCount)
            .IsEqualTo(4);
    }

    [Test]
    [Arguments("form=create&field=CategoryId")]
    [Arguments("form=create&field=PrimaryCategoryId&for=Entity_PrimaryCategoryId")]
    [Arguments("form=edit&field=CategoryId&for=Entity_CategoryId")]
    public async Task LookupSheet_UnknownLookupFieldOrNoEditor_ReturnsNotFound(string query)
    {
        // Arrange
        await using var sut = await CreateLookupFieldsHost(
            fields =>
            {
                fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories);
                fields.Add(model => model.PrimaryCategoryId);
            },
            new()
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync($"/stellaradmin/products/lookupsheet?{query}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    private static void UseCategories(LookupSheetEditor lookup) =>
        lookup.UseItems<CategoryLookupSource, Category, int>(
            category => category.Id,
            category => category.Name,
            items => items.UseDescription(category => category.Code)
        );

    private static Task<WebApplication> CreateLookupFieldsHost(
        Action<ResourceFieldsBuilder<LookupFieldsModel>> configureFields,
        LookupFieldsModel model,
        Action<StellarAdminDashboardBuilder>? configureDashboard = null
    ) =>
        DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate<LookupFieldsModel, LookupFieldsHandler>(create =>
                {
                    create.UseFactory(() => model);
                    create.Fields(configureFields);
                }),
            configureDashboard,
            services => services.AddSingleton<CategoryStore>().AddScoped<CategoryLookupSource>()
        );
}
