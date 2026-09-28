using System.ComponentModel.DataAnnotations;
using System.Net;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Editors;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceReferenceTests
{
    [Test]
    public async Task CheckboxGroupEditor_LoadsOrderedEntityChoices()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync();
        await using var scope = sut.Services.CreateAsyncScope();
        var options = new CheckboxGroupEditorOptions();
        options.UseItems<CatalogDbContext, Category, int>(
            category => category.Id,
            category => category.Name,
            items => items.OrderBy(category => category.Name)
        );
        var editor = new CheckboxGroupEditor(options, scope.ServiceProvider);

        // Act
        var choices =
            (IReadOnlyList<SelectListItem>)(await editor.PrepareAsync(CancellationToken.None))!;

        // Assert
        await Assert
            .That(string.Join(',', choices.Select(choice => choice.Text)))
            .IsEqualTo("Beverage,Office,Technology");
        await Assert
            .That(choices.Select(choice => choice.Value).ToArray())
            .IsEquivalentTo(["2", "1", "3"]);
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
        using var response = await client.GetAsync("/stellaradmin/products/Create");
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
    public async Task Create_OmitsEmptyOptionByDefault()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowCreate(create =>
                create.Fields(fields =>
                    fields
                        .Add(product => product.CategoryId)
                        .UseEditor<SelectListEditorOptions>(options =>
                            options.UseItems<CatalogDbContext, Category, int>(
                                category => category.Id,
                                category => category.Name
                            )
                        )
                )
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/Create");
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
        var values = await PrepareForm(client, "/stellaradmin/products/Create");
        values["Entity.Name"] = "New product";
        values["Entity.Price"] = "25";
        values["Entity.CategoryId"] = "2";
        values["Entity.Hidden"] = "true";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/Create",
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
        using var response = await client.GetAsync("/stellaradmin/products/Edit/1");
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
        var values = await PrepareForm(client, "/stellaradmin/products/Edit/1");
        values["Entity.CategoryId"] = "3";
        values["Entity.Name"] = "Forged";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/Edit/1",
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
        using var form = await client.GetAsync("/stellaradmin/products/Create");
        var document = await ReadDocument(form);
        var values = await PrepareForm(client, "/stellaradmin/products/Create");
        values["Entity.Name"] = "Custom product";
        values["Entity.CategoryId"] = "2";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/Create",
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
        var values = await PrepareForm(client, "/stellaradmin/products/Create");
        values["Entity.CategoryId"] = "2";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/Create",
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
                    .UseEditor<SelectListEditorOptions>(ConfigureCategoryItems);
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
                    .UseEditor<SelectListEditorOptions>(ConfigureCategoryItems);
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
                    .UseEditor<SelectListEditorOptions>(ConfigureCategoryItems)
            )
        );
    }

    private static void ConfigureCategoryItems(SelectListEditorOptions options) =>
        options.UseItems<CatalogDbContext, Category, int>(
            category => category.Id,
            category => category.Name,
            items =>
            {
                items.OrderBy(category => category.Name);
                items.IncludeEmptyOption("Not specified");
            }
        );

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
