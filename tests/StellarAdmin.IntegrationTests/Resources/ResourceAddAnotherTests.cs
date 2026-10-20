using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using static StellarAdmin.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.IntegrationTests.Resources;

public class ResourceAddAnotherTests
{
    [Test]
    public async Task CreatePage_WithoutAddAnother_RendersOnlySubmitButton()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert.That(document.QuerySelector("[data-form-add-another]")).IsNull();
        await Assert
            .That(document.QuerySelectorAll("form button[type='submit']").Length)
            .IsEqualTo(1);
    }

    [Test]
    [Arguments(null, "Save and add another")]
    [Arguments("Save, then the next product", "Save, then the next product")]
    public async Task CreatePage_WithAddAnother_RendersButtonBeforeSubmit(
        string? label,
        string expected
    )
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            resource => ConfigureAddAnother(resource, another => another.Label = label)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var buttons = document.QuerySelectorAll("form button[type='submit']");
        await Assert.That(buttons.Length).IsEqualTo(2);
        await Assert.That(buttons[0].HasAttribute("data-form-add-another")).IsTrue();
        await Assert.That(buttons[0].TextContent.Trim()).IsEqualTo(expected);
        await Assert.That(buttons[0].GetAttribute("name")).IsEqualTo("after");
        await Assert.That(buttons[0].GetAttribute("value")).IsEqualTo("add-another");
        await Assert.That(buttons[1].HasAttribute("data-form-submit")).IsTrue();
        await Assert
            .That(
                document
                    .RequiredElement("form[data-resource-form]")
                    .GetAttribute("data-focus-first-field")
            )
            .IsEqualTo("false");
    }

    [Test]
    public async Task CreatePost_AddAnother_ReopensCreatePageWithKeptValues()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            resource => ConfigureAddAnother(resource)
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client);
        values["Entity.Name"] = "Fountain pen";
        values["Entity.Price"] = "12.00";
        values["after"] = "add-another";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var post = await client.PostAsync("/stellaradmin/products/create", content);
        using var request = new HttpRequestMessage(HttpMethod.Get, post.Headers.Location);
        request.Headers.Add(
            "Cookie",
            post.Headers.GetValues("Set-Cookie").Select(cookie => cookie.Split(';')[0])
        );
        using var next = await client.SendAsync(request);

        // Assert
        await Assert.That(post.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(post.Headers.Location!.OriginalString)
            .IsEqualTo("/stellaradmin/products/create");
        await Assert.That(state.Products.Count).IsEqualTo(2);
        await Assert.That(state.Products[1].Name).IsEqualTo("Fountain pen");
        var document = await next.ReadDocumentAsync();
        await Assert
            .That(document.RequiredElement("input[name='Entity.Name']").GetAttribute("value"))
            .IsEqualTo("");
        await Assert
            .That(document.RequiredElement("input[name='Entity.Price']").GetAttribute("value"))
            .IsEqualTo("12.00");
        await Assert
            .That(
                document
                    .RequiredElement("form[data-resource-form]")
                    .GetAttribute("data-focus-first-field")
            )
            .IsEqualTo("true");
        await Assert.That(document.Body!.InnerHtml).Contains("Product added");
    }

    [Test]
    public async Task CreatePost_Save_RedirectsToIndex()
    {
        // Arrange
        var state = new ProductState([]);
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            resource => ConfigureAddAnother(resource)
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client);
        values["Entity.Name"] = "Fountain pen";
        values["Entity.Price"] = "12.00";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/create", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(response.Headers.Location!.OriginalString)
            .IsEqualTo("/stellaradmin/products");
        await Assert.That(state.Products.Count).IsEqualTo(1);
    }

    [Test]
    public async Task CreatePost_AddAnotherWithInvalidValue_RedisplaysForm()
    {
        // Arrange
        var state = new ProductState([]);
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            resource => ConfigureAddAnother(resource)
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client);
        values["Entity.Name"] = "";
        values["Entity.Price"] = "12.00";
        values["after"] = "add-another";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/create", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var document = await response.ReadDocumentAsync();
        await Assert
            .That(document.QuerySelector("[data-valmsg-for='Entity.Name'].field-validation-error"))
            .IsNotNull();
        await Assert.That(state.Products.Count).IsEqualTo(0);
    }

    [Test]
    public async Task CreateRow_WithAddAnother_RendersAddAnotherButton()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            resource => ConfigureInlineAddAnother(resource)
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/createrow");

        // Assert
        var row = (await response.ReadRowsAsync()).RequiredElement("tr[data-key='new']");
        var another = row.RequiredElement("[data-row-save-another]");
        await Assert.That(another.GetAttribute("aria-label")).IsEqualTo("Save and add another");
        await Assert
            .That(another.GetAttribute("hx-post"))
            .IsEqualTo("/stellaradmin/products/createrow?another=True&added=0");
    }

    [Test]
    public async Task IndexPage_WithoutAddAnother_LeavesAddAnotherMarkerEmpty()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowCreate(create =>
                {
                    create.Fields(fields =>
                    {
                        fields.Add(product => product.Name);
                        fields.Add(product => product.Price);
                    });
                    create.UseInlineRow();
                });
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert
            .That(document.RequiredElement("#index-page-grid").GetAttribute("data-add-another"))
            .IsNullOrEmpty();
    }

    [Test]
    public async Task IndexPage_WithInlineAddAnother_NamesWhatEndsTheRun()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            resource =>
                ConfigureInlineAddAnother(
                    resource,
                    inline => inline.AfterSave = InlineRowAfterSave.KeepInPlace
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert
            .That(document.RequiredElement("#index-page-grid").GetAttribute("data-add-another"))
            .IsEqualTo("keep");
    }

    [Test]
    [Arguments(InlineCreatePosition.Top, new[] { "new", "2" })]
    [Arguments(InlineCreatePosition.Bottom, new[] { "2", "new" })]
    public async Task CreateRowPost_AddAnother_RendersSavedRowAndNewRowWithKeptValues(
        InlineCreatePosition position,
        string[] keys
    )
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            resource => ConfigureInlineAddAnother(resource, inline => inline.Position = position)
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/createrow");
        values["rows[new].Name"] = "Fountain pen";
        values["rows[new].Price"] = "12.00";

        // Act
        using var response = await PostRowAsync(
            client,
            "/stellaradmin/products/createrow?another=True&added=0",
            values
        );

        // Assert
        var document = await response.ReadRowsAsync();
        var rowKeys = document
            .QuerySelectorAll("tbody > tr[data-key]")
            .Select(row => row.GetAttribute("data-key"))
            .ToArray();
        await Assert
            .That(rowKeys)
            .IsEquivalentTo(keys, TUnit.Assertions.Enums.CollectionOrdering.Matching);
        await Assert.That(state.Products.Count).IsEqualTo(2);
        await Assert.That(response.Headers.GetValues("SA-Saved-Row").Single()).IsEqualTo("2");
        await Assert
            .That(response.Headers.GetValues("SA-Toasts").Single())
            .IsEqualTo("""[{"title":"Product added","type":"success"}]""");
        var newRow = document.RequiredElement("tr[data-key='new']");
        await Assert
            .That(newRow.RequiredElement("input[name='rows[new].Name']").GetAttribute("value"))
            .IsEqualTo("");
        await Assert
            .That(newRow.RequiredElement("input[name='rows[new].Price']").GetAttribute("value"))
            .IsEqualTo("12.00");
        await Assert
            .That(newRow.RequiredElement("[data-row-save-another]").GetAttribute("hx-post"))
            .IsEqualTo("/stellaradmin/products/createrow?another=True&added=1");
        await Assert
            .That(document.RequiredElement(".sa-add-another-status").TextContent)
            .Contains("1 added");
    }

    [Test]
    public async Task CreateRowPost_AddAnotherWithInvalidValue_KeepsCount()
    {
        // Arrange
        var state = new ProductState([]);
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            resource => ConfigureInlineAddAnother(resource)
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/createrow");
        values["rows[new].Name"] = "";
        values["rows[new].Price"] = "12.00";

        // Act
        using var response = await PostRowAsync(
            client,
            "/stellaradmin/products/createrow?another=True&added=2",
            values
        );

        // Assert
        var document = await response.ReadRowsAsync();
        await Assert.That(document.QuerySelectorAll("tbody > tr[data-key]").Length).IsEqualTo(1);
        await Assert
            .That(document.RequiredElement("tr[data-key='new'] .sa-cell-error").Id)
            .IsEqualTo("rows_new__Name-error");
        await Assert
            .That(document.RequiredElement(".sa-add-another-status").TextContent)
            .Contains("2 added");
        await Assert.That(state.Products.Count).IsEqualTo(0);
    }

    [Test]
    public async Task CreateSheet_WithAddAnother_RendersNoAddAnotherButton()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            resource => ConfigureAddAnother(resource)
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/products/createsheet?for=product-id&level=1"
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var document = await response.ReadDocumentAsync();
        await Assert.That(document.QuerySelector("[data-form-add-another]")).IsNull();
        await Assert.That(document.Body!.TextContent).DoesNotContain("Save and add another");
    }

    private static async Task<HttpResponseMessage> PostRowAsync(
        HttpClient client,
        string url,
        Dictionary<string, string> values
    )
    {
        // A script's request, whose toasts come back in a header
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(values),
        };
        request.Headers.Add("Sec-Fetch-Mode", "cors");

        return await client.SendAsync(request);
    }

    private static void ConfigureAddAnother(
        ResourceBuilder<Product> resource,
        Action<ResourceAddAnotherBuilder<Product>>? configure = null
    ) =>
        resource.AllowCreate(create =>
        {
            create.Fields(fields =>
            {
                fields.Add(product => product.Name);
                fields.Add(product => product.Price);
            });
            create.EnableAddAnother(another =>
            {
                another.KeepForNext(product => product.Price);
                configure?.Invoke(another);
            });
        });

    private static void ConfigureInlineAddAnother(
        ResourceBuilder<Product> resource,
        Action<ResourceInlineCreateBuilder>? configure = null
    )
    {
        resource.UseKey(product => product.Id);
        resource.AllowCreate(create =>
        {
            create.Fields(fields =>
            {
                fields.Add(product => product.Name);
                fields.Add(product => product.Price);
            });
            create.UseInlineRow(inline => configure?.Invoke(inline));
            create.EnableAddAnother(another => another.KeepForNext(product => product.Price));
        });
    }
}
