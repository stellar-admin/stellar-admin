using System.Collections.Concurrent;
using System.Net;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceLookupTests
{
    [Test]
    public async Task Edit_DisplaysSelectionFromReferenceLoadedWithEntity()
    {
        // Arrange
        var commands = new ConcurrentQueue<string>();
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource => ConfigureLookupEdit(resource, _ => { }),
            commands
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/edit/1");
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(DisplayText(document)).IsEqualTo("Office");
        await Assert.That(commands.Count).IsEqualTo(1);
        await Assert.That(commands.Single()).Contains("JOIN \"Category\"");
    }

    [Test]
    public async Task Edit_WithReferenceFrom_DisplaysSelectionFromReferenceLoadedWithEntity()
    {
        // Arrange
        var commands = new ConcurrentQueue<string>();
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                ConfigureLookupEdit(
                    resource,
                    items => items.ReferenceFrom<Product>(product => product.Category)
                ),
            commands
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/edit/2");
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(DisplayText(document)).IsEqualTo("Technology");
        await Assert.That(commands.Count).IsEqualTo(1);
    }

    [Test]
    public async Task RejectedEdit_DisplaysChangedSelection()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            ConfigureLookupEdit(resource, _ => { })
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/1");
        values["Entity.CategoryId"] = "3";
        values["Entity.Price"] = "0";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/edit/1",
            new FormUrlEncodedContent(values)
        );
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(DisplayText(document)).IsEqualTo("Technology");
    }

    [Test]
    public async Task Create_DisplaysSelectionWithoutNavigation()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowCreate(create =>
                create.Fields(fields =>
                {
                    fields.Add(product => product.Name);
                    fields.Add(product => product.Price);
                    fields
                        .Add(product => product.CategoryId)
                        .UseEditor<LookupEditor>(options => ConfigureItems(options, _ => { }));
                })
            )
        );
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
        await Assert.That(DisplayText(document)).IsEqualTo("Beverage");
    }

    [Test]
    public async Task Search_MatchesTextIgnoringCase()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            ConfigureLookupEdit(resource, _ => { })
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/products/lookup?form=edit&field=CategoryId&term=OFF"
        );
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(ResultValues(document)).IsEquivalentTo(["1"]);
        await Assert.That(ResultTexts(document)).IsEquivalentTo(["Office"]);
    }

    [Test]
    public async Task Search_PagesItemsOrderedByText()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            ConfigureLookupEdit(resource, _ => { }, pageSize: 2)
        );
        using var client = sut.GetTestClient();
        using var first = await client.GetAsync(
            "/stellaradmin/products/lookup?form=edit&field=CategoryId"
        );
        var firstPage = await ReadDocument(first);
        var more = firstPage.QuerySelector("[data-lookup='more']")?.GetAttribute("hx-get");

        // Act
        using var second = await client.GetAsync(more);
        var secondPage = await ReadDocument(second);

        // Assert
        await Assert.That(string.Join(',', ResultTexts(firstPage))).IsEqualTo("Beverage,Office");
        await Assert.That(ResultTexts(secondPage)).IsEquivalentTo(["Technology"]);
        await Assert.That(secondPage.QuerySelector("[data-lookup='more']")).IsNull();
    }

    [Test]
    public async Task Search_UsesConfiguredPropertiesOrderAndDescription()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            ConfigureLookupEdit(
                resource,
                items =>
                    items
                        .SearchOn(category => "Code " + category.Id.ToString())
                        .UseDescription(category => "Code " + category.Id.ToString())
                        .OrderBy(category => category.Id)
            )
        );
        using var client = sut.GetTestClient();
        using var byText = await client.GetAsync(
            "/stellaradmin/products/lookup?form=edit&field=CategoryId&term=office"
        );
        var byTextPage = await ReadDocument(byText);

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/products/lookup?form=edit&field=CategoryId&term=code"
        );
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(ResultTexts(byTextPage)).IsEmpty();
        await Assert
            .That(string.Join(',', ResultTexts(document)))
            .IsEqualTo("Office,Beverage,Technology");
        await Assert
            .That(
                string.Join(
                    ',',
                    document
                        .QuerySelectorAll("[data-lookup='item-description']")
                        .Select(element => element.TextContent.Trim())
                )
            )
            .IsEqualTo("Code 1,Code 2,Code 3");
    }

    [Test]
    public async Task Search_UseImage_ShowsImagesForItemsWithUrls()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            ConfigureLookupEdit(
                resource,
                items =>
                    items
                        .OrderBy(category => category.Id)
                        .UseImage(category =>
                            category.Id == 1 ? null : "/images/" + category.Id.ToString() + ".png"
                        )
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/products/lookup?form=edit&field=CategoryId"
        );
        var document = await ReadDocument(response);

        // Assert
        await Assert
            .That(
                document
                    .QuerySelectorAll("[data-lookup='item'] > img[data-media='image']")
                    .Select(image => image.GetAttribute("src"))
            )
            .IsEquivalentTo(["/images/2.png", "/images/3.png"]);
    }

    [Test]
    public async Task Edit_UseIcon_ShowsIconOfSelection()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            ConfigureLookupEdit(resource, items => items.UseIcon(_ => "tag"))
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/edit/1");
        var document = await ReadDocument(response);

        // Assert
        await Assert
            .That(document.QuerySelector("[data-lookup='selected'] svg[data-media='icon']"))
            .IsNotNull();
    }

    [Test]
    public async Task CreateFromLookup_SelectsCreatedEntity()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                        fields
                            .Add(product => product.CategoryId)
                            .UseEditor<LookupEditor>(options =>
                            {
                                ConfigureItems(options, _ => { });
                                options.EnableCreate();
                            })
                    )
                ),
            configureDashboard: dashboard =>
                dashboard.AddEfCoreResource<CatalogDbContext, Category>(resource =>
                    resource.AllowCreate(create =>
                        create.Fields(fields => fields.Add(category => category.Name))
                    )
                )
        );
        using var client = sut.GetTestClient();
        var page = await ReadDocument(await client.GetAsync("/stellaradmin/products/edit/1"));
        var createUrl = page.QuerySelector("[data-lookup='create']")!.GetAttribute("hx-get")!;
        var selectionUrl = page.QuerySelector("dashboard-lookup-editor")!
            .GetAttribute("selection-url")!;
        var values = await PrepareForm(client, createUrl);
        values["Sheet.Name"] = "Garden";

        // Act
        using var created = await client.PostAsync(createUrl, new FormUrlEncodedContent(values));
        var key = (await ReadDocument(created))
            .QuerySelector("dashboard-lookup-created")
            ?.GetAttribute("value");
        var selection = await ReadDocument(await client.GetAsync($"{selectionUrl}&value={key}"));

        // Assert
        await Assert
            .That(createUrl)
            .IsEqualTo("/stellaradmin/categories/createsheet?for=Entity_CategoryId");
        await Assert.That(key).IsEqualTo("4");
        var template = (IHtmlTemplateElement)
            selection.QuerySelector("template[data-lookup='selection']")!;
        await Assert.That(template.GetAttribute("data-value")).IsEqualTo("4");
        await Assert
            .That(template.Content.QuerySelector("[data-lookup='title']")?.TextContent)
            .IsEqualTo("Garden");
    }

    private static void ConfigureLookupEdit(
        EfCoreResourceBuilder<CatalogDbContext, Product> resource,
        Action<EfCoreLookupItemsBuilder<Category, int>> configure,
        int pageSize = 20
    ) =>
        resource.AllowEdit(edit =>
            edit.Fields(fields =>
            {
                fields.Add(product => product.Price);
                fields
                    .Add(product => product.CategoryId)
                    .UseEditor<LookupEditor>(options =>
                    {
                        options.Sheet(sheet => sheet.PageSize = pageSize);
                        ConfigureItems(options, configure);
                    });
            })
        );

    private static void ConfigureItems(
        LookupEditor options,
        Action<EfCoreLookupItemsBuilder<Category, int>> configure
    ) =>
        options.UseItems<CatalogDbContext, Category, int>(
            category => category.Id,
            category => category.Name,
            configure
        );

    private static string? DisplayText(IDocument document) =>
        document.QuerySelector("[id$='-selected'] [data-lookup='title']")?.TextContent;

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

    private static string[] ResultTexts(IDocument document) =>
        document
            .QuerySelectorAll("[data-lookup='item-title']")
            .Select(element => element.TextContent.Trim())
            .ToArray();

    private static string[] ResultValues(IDocument document) =>
        document
            .QuerySelectorAll("[data-lookup='item']")
            .Select(element => element.GetAttribute("data-value") ?? "")
            .ToArray();
}
