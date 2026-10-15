using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
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
