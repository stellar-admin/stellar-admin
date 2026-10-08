using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Editors;
using StellarAdmin.Dashboard.Resources.Options;
using TUnit.Assertions.Enums;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceReferenceTests
{
    [Test]
    public async Task CheckboxGroupEditor_LoadsOrderedEntityChoices()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync();
        await using var scope = sut.Services.CreateAsyncScope();
        var editor = new CheckboxGroupEditor();
        editor.UseItems<CatalogDbContext, Category, int>(
            category => category.Id,
            category => category.Name,
            items => items.OrderBy(category => category.Name)
        );
        var handler = new CheckboxGroupEditorHandler(editor, scope.ServiceProvider);
        var context = new FieldEditorContext(nameof(Product.CategoryId), new Product(), null);

        // Act
        var choices =
            (IReadOnlyList<ChoiceItem>)
                (await handler.PrepareAsync(context, CancellationToken.None))!;

        // Assert
        await Assert
            .That(string.Join(',', choices.Select(choice => choice.Text)))
            .IsEqualTo("Beverage,Office,Technology");
        await Assert
            .That(choices.Select(choice => choice.Value).ToArray())
            .IsEquivalentTo(["2", "1", "3"]);
    }

    [Test]
    public async Task RadioGroupEditor_LoadsEntityChoicesWithDescriptionsAndGroups()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync();
        await using var scope = sut.Services.CreateAsyncScope();
        var editor = new RadioGroupEditor();
        editor.UseItems<CatalogDbContext, Category, int>(
            category => category.Id,
            category => category.Name,
            items =>
                items
                    .OrderBy(category => category.Name)
                    .UseDescription(category => category.Name + " supplies")
                    .UseGroup(category => category.Id == 2 ? "Food" : null)
        );
        var handler = new RadioGroupEditorHandler(editor, scope.ServiceProvider);
        var context = new FieldEditorContext(nameof(Product.CategoryId), new Product(), null);

        // Act
        var choices =
            (IReadOnlyList<ChoiceItem>)
                (await handler.PrepareAsync(context, CancellationToken.None))!;

        // Assert
        await Assert
            .That(choices)
            .IsEquivalentTo(
                [
                    new ChoiceItem("2", "Beverage")
                    {
                        Description = "Beverage supplies",
                        Group = new("Food"),
                    },
                    new ChoiceItem("1", "Office") { Description = "Office supplies" },
                    new ChoiceItem("3", "Technology") { Description = "Technology supplies" },
                ],
                CollectionOrdering.Matching
            );
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ChoiceEditor_UseCodeOrUseAvatar_LoadsEntityMedia(bool avatar)
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync();
        await using var scope = sut.Services.CreateAsyncScope();
        var editor = new RadioGroupEditor();
        editor.UseItems<CatalogDbContext, Category, int>(
            category => category.Id,
            category => category.Name,
            items =>
            {
                items.OrderBy(category => category.Name);
                if (avatar)
                {
                    items.UseAvatar(category =>
                        category.Id == 2 ? null : "/images/" + category.Name
                    );
                }
                else
                {
                    items.UseCode(category =>
                        category.Id == 2 ? "" : category.Name.Substring(0, 3)
                    );
                }
            }
        );
        var handler = new RadioGroupEditorHandler(editor, scope.ServiceProvider);
        var context = new FieldEditorContext(nameof(Product.CategoryId), new Product(), null);

        // Act
        var choices =
            (IReadOnlyList<ChoiceItem>)
                (await handler.PrepareAsync(context, CancellationToken.None))!;

        // Assert
        ItemMedia?[] expected = avatar
            ?
            [
                new ItemMedia.Avatar(null),
                new ItemMedia.Avatar("/images/Office"),
                new ItemMedia.Avatar("/images/Technology"),
            ]
            : [null, new ItemMedia.Code("Off"), new ItemMedia.Code("Tec")];
        await Assert
            .That(choices.Select(choice => choice.Media).ToArray())
            .IsEquivalentTo(expected, CollectionOrdering.Matching);
    }

    [Test]
    public async Task SelectEditor_FractionalValues_FormatsInCurrentCulture()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync();
        await using var scope = sut.Services.CreateAsyncScope();
        var editor = new SelectEditor();
        editor.UseItems<CatalogDbContext, Category, double>(
            category => category.Id + 0.5,
            category => category.Name,
            items => items.OrderBy(category => category.Id)
        );
        var handler = new SelectEditorHandler(editor, scope.ServiceProvider);
        var context = new FieldEditorContext(nameof(Product.CategoryId), new Product(), null);
        var previousCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
        IReadOnlyList<ChoiceItem> choices;

        // Act
        try
        {
            choices =
                (IReadOnlyList<ChoiceItem>)
                    (await handler.PrepareAsync(context, CancellationToken.None))!;
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }

        // Assert
        await Assert
            .That(choices.Select(choice => choice.Value))
            .IsEquivalentTo(["1,5", "2,5", "3,5"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task Index_DisplaysAndSortsByForeignKey()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
        {
            resource.Index(index =>
                index.Columns(columns =>
                    columns.Add(product => product.CategoryId, column => column.Sortable())
                )
            );
        });
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/products?sortBy=CategoryId&sortDirection=asc"
        );
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                string.Join(
                    ',',
                    document
                        .QuerySelectorAll("tbody tr td:last-child")
                        .Select(cell => cell.TextContent.Trim())
                )
            )
            .IsEqualTo("1,2,3,3");
    }

    [Test]
    public async Task Create_RendersSortedCategories()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(ConfigureReferenceCreate);
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/create");
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                string.Join(
                    ',',
                    document
                        .QuerySelectorAll("select[name='Entity.CategoryId'] option")
                        .Select(option => option.TextContent.Trim())
                )
            )
            .IsEqualTo("Not specified,Beverage,Office,Technology");
    }

    [Test]
    public async Task Create_EmptyChoiceOmit_LeavesOutEmptyChoice()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowCreate(create =>
                create.Fields(fields =>
                    fields
                        .Add(product => product.CategoryId)
                        .UseEditor<SelectEditor>(options =>
                        {
                            options.UseItems<CatalogDbContext, Category, int>(
                                category => category.Id,
                                category => category.Name
                            );
                            options.EmptyChoice = EmptyChoice.Omit;
                        })
                )
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/create");
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.QuerySelectorAll("select[name='Entity.CategoryId'] option").Length)
            .IsEqualTo(3);
        await Assert
            .That(document.QuerySelector("select[name='Entity.CategoryId'] option[value='']"))
            .IsNull();
    }

    [Test]
    public async Task Create_SavesSelectedForeignKey()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(ConfigureReferenceCreate);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.Name"] = "New product";
        values["Entity.Price"] = "25";
        values["Entity.CategoryId"] = "2";
        values["Entity.Hidden"] = "true";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/create",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var created = await db.Set<Product>().SingleAsync(product => product.Name == "New product");
        await Assert.That(created.CategoryId).IsEqualTo(2);
        await Assert.That(created.Hidden).IsFalse();
    }

    [Test]
    public async Task Edit_RendersCurrentSelection()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(ConfigureReferenceEdit);
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/edit/1");
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                document
                    .QuerySelector("select[name='Entity.CategoryId'] option[selected]")
                    ?.GetAttribute("value")
            )
            .IsEqualTo("1");
    }

    [Test]
    public async Task Edit_UpdatesOnlyConfiguredForeignKey()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(ConfigureReferenceEdit);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/1");
        values["Entity.CategoryId"] = "3";
        values["Entity.Name"] = "Forged";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/edit/1",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var updated = await db.Set<Product>().SingleAsync(product => product.Number == 1);
        await Assert.That(updated.CategoryId).IsEqualTo(3);
        await Assert.That(updated.Name).IsEqualTo("Zebra");
    }

    [Test]
    public async Task CustomCreateModel_RendersLookupAndSavesSelectedReference()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(ConfigureCustomCreate);
        using var client = sut.GetTestClient();
        using var form = await client.GetAsync("/stellaradmin/products/create");
        var document = await ReadDocument(form);
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.Name"] = "Custom product";
        values["Entity.CategoryId"] = "2";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/create",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(form.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.QuerySelectorAll("select[name='Entity.CategoryId'] option").Length)
            .IsEqualTo(4);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var product = await db.Set<Product>().SingleAsync(p => p.Name == "Custom product");
        await Assert.That(product.CategoryId).IsEqualTo(2);
    }

    [Test]
    public async Task CustomCreateModel_RejectedSubmissionReloadsLookup()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(ConfigureCustomCreate);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.CategoryId"] = "2";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/create",
            new FormUrlEncodedContent(values)
        );
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.QuerySelectorAll("select[name='Entity.CategoryId'] option").Length)
            .IsEqualTo(4);
        await Assert
            .That(document.QuerySelector("[data-valmsg-for='Entity.Name']")?.TextContent.Trim())
            .IsNotNullOrEmpty();
    }

    private static void ConfigureReferenceCreate(
        EfCoreResourceBuilder<CatalogDbContext, Product> resource
    )
    {
        resource.AllowCreate(create =>
            create.Fields(fields =>
            {
                fields.Add(product => product.Name);
                fields.Add(product => product.Price);
                fields
                    .Add(product => product.CategoryId)
                    .UseEditor<SelectEditor>(ConfigureCategoryItems);
            })
        );
    }

    private static void ConfigureCustomCreate(
        EfCoreResourceBuilder<CatalogDbContext, Product> resource
    ) =>
        resource.AllowCreate<CreateProductModel, CreateProductHandler>(create =>
            create.Fields(fields =>
            {
                fields.Add(model => model.Name);
                fields
                    .Add(model => model.CategoryId)
                    .UseEditor<SelectEditor>(ConfigureCategoryItems);
            })
        );

    private static void ConfigureReferenceEdit(
        EfCoreResourceBuilder<CatalogDbContext, Product> resource
    )
    {
        resource.AllowEdit(edit =>
            edit.Fields(fields =>
                fields
                    .Add(product => product.CategoryId)
                    .UseEditor<SelectEditor>(ConfigureCategoryItems)
            )
        );
    }

    private static void ConfigureCategoryItems(SelectEditor options)
    {
        options.UseItems<CatalogDbContext, Category, int>(
            category => category.Id,
            category => category.Name,
            items => items.OrderBy(category => category.Name)
        );
        options.EmptyChoiceText = "Not specified";
    }

    private static async Task<Dictionary<string, string>> PrepareForm(HttpClient client, string url)
    {
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var document = await ReadDocument(response);
        var token = document
            .QuerySelector("input[name='__RequestVerificationToken']")!
            .GetAttribute("value")!;
        client.DefaultRequestHeaders.Add(
            "Cookie",
            response.Headers.GetValues("Set-Cookie").Select(cookie => cookie.Split(';')[0])
        );

        return new() { ["__RequestVerificationToken"] = token };
    }

    private static async Task<IDocument> ReadDocument(HttpResponseMessage response) =>
        await new HtmlParser().ParseDocumentAsync(await response.Content.ReadAsStringAsync());

    public sealed class CreateProductModel
    {
        [Required]
        public string Name { get; set; } = "";

        public int? CategoryId { get; set; }
    }

    public sealed class CreateProductHandler(CatalogDbContext db)
        : IResourceCreateHandler<CreateProductModel>
    {
        public async Task<ResourceOperationResult> CreateAsync(
            CreateProductModel model,
            CancellationToken cancellationToken
        )
        {
            db.Add(
                new Product
                {
                    Name = model.Name,
                    CategoryId = model.CategoryId,
                    Price = 1,
                }
            );
            await db.SaveChangesAsync(cancellationToken);
            return ResourceOperationResult.Success();
        }
    }
}
