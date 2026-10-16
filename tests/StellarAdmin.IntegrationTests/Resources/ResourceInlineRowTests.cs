using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using static StellarAdmin.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.IntegrationTests.Resources;

public class ResourceInlineRowTests
{
    [Test]
    public async Task InlineEdit_HidesEditLinks()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigureInlineRows
        );
        using var client = sut.GetTestClient();

        // Act
        var index = await client.GetDocumentAsync("/stellaradmin/products");
        var view = await client.GetDocumentAsync("/stellaradmin/products/view/7");

        // Assert
        await Assert.That(index.QuerySelector("a[href*='/view/7']")).IsNotNull();
        await Assert.That(index.QuerySelector("a[href*='/edit/']")).IsNull();
        await Assert.That(view.QuerySelector("a[href*='/edit/']")).IsNull();
    }

    [Test]
    public async Task InlineCreate_HidesCreateLink()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigureInlineRows
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert.That(document.Body!.TextContent).Contains("Notebook");
        await Assert.That(document.QuerySelector("a[href$='/create']")).IsNull();
    }

    [Test]
    [Arguments("create")]
    [Arguments("edit/7")]
    public async Task InlineOperation_RejectsPageRequests(string path)
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigureInlineRows
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products");
        values["Model.Name"] = "Changed";
        values["Model.Price"] = "9.00";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var get = await client.GetAsync($"/stellaradmin/products/{path}");
        using var post = await client.PostAsync($"/stellaradmin/products/{path}", content);

        // Assert
        await Assert.That(get.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(post.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task EditRow_RendersRowInEditMode()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigureInlineRows
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/editrow/7");

        // Assert
        var rows = (await response.ReadRowsAsync()).QuerySelectorAll("tbody > tr");
        await Assert.That(rows.Length).IsEqualTo(1);
        await Assert.That(rows[0].GetAttribute("data-key")).IsEqualTo("7");
        await Assert.That(rows[0].HasAttribute("data-editing")).IsTrue();
        await Assert
            .That(rows[0].RequiredElement("input[name='rows[7].Name']").GetAttribute("value"))
            .IsEqualTo("Notebook");
        await Assert.That(rows[0].QuerySelector("input[name='rows[7].Price']")).IsNotNull();
        await Assert
            .That(rows[0].RequiredElement("[data-row-save]").GetAttribute("hx-post"))
            .IsEqualTo("/stellaradmin/products/editrow/7");
        await Assert
            .That(rows[0].QuerySelector("input[name='__RequestVerificationToken']"))
            .IsNotNull();
        await Assert.That(rows[0].QuerySelector("[data-row-cancel]")).IsNotNull();
    }

    [Test]
    public async Task EditRow_Save_UpdatesAndRendersDisplayRow()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await StellarAdminTestHost.CreateAsync(state, ConfigureInlineRows);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products");
        values["rows[7].Name"] = "Fountain pen";
        values["rows[7].Price"] = "12.00";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/editrow/7", content);

        // Assert
        var rows = (await response.ReadRowsAsync()).QuerySelectorAll("tbody > tr");
        await Assert.That(state.Products[0].Name).IsEqualTo("Fountain pen");
        await Assert.That(rows.Length).IsEqualTo(1);
        await Assert.That(rows[0].GetAttribute("data-key")).IsEqualTo("7");
        await Assert.That(rows[0].HasAttribute("data-editing")).IsFalse();
        await Assert.That(rows[0].TextContent).Contains("Fountain pen");
        await Assert.That(state.ListRequests[^1].Keys).IsEquivalentTo(["7"]);
        await Assert.That(state.ListRequests[^1].Paging).IsNull();
    }

    [Test]
    public async Task EditRow_SaveWithInvalidValue_RendersErrorUnderCell()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await StellarAdminTestHost.CreateAsync(state, ConfigureInlineRows);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products");
        values["rows[7].Name"] = "Notebook";
        values["rows[7].Price"] = "0";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/editrow/7", content);

        // Assert
        var document = await response.ReadRowsAsync();
        var row = document.RequiredElement("tr[data-key='7']");
        await Assert.That(row.HasAttribute("data-editing")).IsTrue();
        await Assert
            .That(row.RequiredElement(".sa-cell-error").Id)
            .IsEqualTo("rows_7__Price-error");
        await Assert.That(document.QuerySelector(".sa-row-message")).IsNull();
        await Assert.That(state.UpdateCalls).IsEqualTo(0);
    }

    [Test]
    public async Task EditRow_SaveRejectedByHandler_RendersRowMessage()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)])
        {
            UpdateResult = ResourceOperationResult.ValidationFailed(null, "Notebooks are locked."),
        };
        await using var sut = await StellarAdminTestHost.CreateAsync(state, ConfigureInlineRows);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products");
        values["rows[7].Name"] = "Notebook";
        values["rows[7].Price"] = "9.00";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/editrow/7", content);

        // Assert
        var rows = (await response.ReadRowsAsync()).QuerySelectorAll("tbody > tr");
        await Assert.That(rows.Length).IsEqualTo(2);
        await Assert.That(rows[0].HasAttribute("data-editing")).IsTrue();
        await Assert.That(rows[1].ClassList.Contains("sa-row-message")).IsTrue();
        await Assert
            .That(rows[1].RequiredElement("[role='alert']").TextContent.Trim())
            .IsEqualTo("Notebooks are locked.");
        await Assert.That(rows[1].RequiredElement("td").GetAttribute("colspan")).IsEqualTo("3");
    }

    [Test]
    public async Task EditRow_Save_BindsOnlyEditFields()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                {
                    edit.Fields(fields => fields.Add(product => product.Name));
                    edit.UseInlineRow();
                });
            }
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["rows[7].Name"] = "Fountain pen";
        values["rows[7].Price"] = "99.00";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/editrow/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(state.Products[0].Name).IsEqualTo("Fountain pen");
        await Assert.That(state.Products[0].Price).IsEqualTo(8.50m);
    }

    [Test]
    public async Task EditRow_WithUnknownKey_ReturnsNotFound()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigureInlineRows
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products");
        values["rows[99].Name"] = "Fountain pen";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var get = await client.GetAsync("/stellaradmin/products/editrow/99");
        using var post = await client.PostAsync("/stellaradmin/products/editrow/99", content);

        // Assert
        await Assert.That(get.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(post.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    [Arguments("editrow/7")]
    [Arguments("createrow")]
    public async Task RowRequests_WithoutInlineRows_ReturnNotFound(string path)
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                    edit.Fields(fields => fields.Add(product => product.Name))
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync($"/stellaradmin/products/{path}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task CreateRow_RendersNewRowInEditMode()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigureInlineRows
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/createrow");

        // Assert
        var rows = (await response.ReadRowsAsync()).QuerySelectorAll("tbody > tr");
        await Assert.That(rows.Length).IsEqualTo(1);
        await Assert.That(rows[0].GetAttribute("data-key")).IsEqualTo("new");
        await Assert.That(rows[0].HasAttribute("data-editing")).IsTrue();
        await Assert.That(rows[0].QuerySelector("input[name='rows[new].Name']")).IsNotNull();
        await Assert
            .That(rows[0].RequiredElement("[data-row-save]").GetAttribute("hx-post"))
            .IsEqualTo("/stellaradmin/products/createrow");
    }

    [Test]
    public async Task CreateRow_Save_CreatesAndRendersDisplayRow()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await StellarAdminTestHost.CreateAsync(state, ConfigureInlineRows);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products");
        values["rows[new].Name"] = "Fountain pen";
        values["rows[new].Price"] = "12.00";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/createrow", content);

        // Assert
        var rows = (await response.ReadRowsAsync()).QuerySelectorAll("tbody > tr");
        await Assert.That(state.Products.Count).IsEqualTo(2);
        await Assert.That(rows.Length).IsEqualTo(1);
        await Assert.That(rows[0].GetAttribute("data-key")).IsEqualTo("2");
        await Assert.That(rows[0].HasAttribute("data-editing")).IsFalse();
        await Assert.That(rows[0].TextContent).Contains("Fountain pen");
    }

    [Test]
    public async Task CreateRow_SaveWithInvalidValue_RendersNewRowWithError()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await StellarAdminTestHost.CreateAsync(state, ConfigureInlineRows);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products");
        values["rows[new].Name"] = "";
        values["rows[new].Price"] = "12.00";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/createrow", content);

        // Assert
        var row = (await response.ReadRowsAsync()).RequiredElement("tr[data-key='new']");
        await Assert.That(row.HasAttribute("data-editing")).IsTrue();
        await Assert
            .That(row.RequiredElement(".sa-cell-error").Id)
            .IsEqualTo("rows_new__Name-error");
        await Assert
            .That(row.RequiredElement("input[name='rows[new].Price']").GetAttribute("value"))
            .IsEqualTo("12.00");
        await Assert.That(state.Products.Count).IsEqualTo(1);
    }

    [Test]
    public async Task CreateRow_SavedRowNotFound_RefreshesPage()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)])
        {
            CreateResult = ResourceOperationResult.Success(),
        };
        await using var sut = await StellarAdminTestHost.CreateAsync(state, ConfigureInlineRows);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products");
        values["rows[new].Name"] = "Fountain pen";
        values["rows[new].Price"] = "12.00";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/createrow", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(response.Headers.GetValues("HX-Refresh")).IsEquivalentTo(["true"]);
    }

    private static void ConfigureInlineRows(ResourceBuilder<Product> resource)
    {
        resource.UseKey(product => product.Id);
        resource.AllowView(view => view.Fields(fields => fields.Add(product => product.Name)));
        resource.AllowDelete();
        resource.AllowEdit(edit =>
        {
            edit.Fields(fields =>
            {
                fields.Add(product => product.Name);
                fields.Add(product => product.Price);
            });
            edit.UseInlineRow();
        });
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
}
