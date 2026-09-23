using System.Net;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Infrastructure;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceOwnedObjectTests
{
    [Test]
    public async Task Index_RendersAndSortsOwnedProperty()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.Index(index =>
            {
                index.Columns(columns =>
                    columns.Add(product => product.Details.Sku, column => column.Sortable())
                );
                index.DefaultSortBy(product => product.Details.Sku);
            })
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/Product?sortBy=Details.Sku&sortDirection=desc"
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
            .IsEqualTo("Z-1,B-4,A-3,A-2");
        await Assert
            .That(document.QuerySelector("thead a[href*='sortBy=Details.Sku']"))
            .IsNotNull();
    }

    [Test]
    public async Task Index_DefaultSortByOwnedProperty()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.Index(index =>
            {
                index.Columns(columns =>
                    columns.Add(product => product.Details.Sku, column => column.Sortable())
                );
                index.DefaultSortBy(product => product.Details.Sku);
            })
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product");
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
            .IsEqualTo("A-2,A-3,B-4,Z-1");
    }

    [Test]
    public async Task Create_BindsOwnedPropertyAndIgnoresOtherPostedProperties()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowCreate(create =>
                create.Fields(fields =>
                {
                    fields.Add(product => product.Name);
                    fields.Add(product => product.Price);
                    fields.Add(product => product.Details.Sku);
                })
            )
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/Product/Create");
        values["Entity.Name"] = "New product";
        values["Entity.Price"] = "25";
        values["Entity.Details.Sku"] = "N-6";
        values["Entity.Details.InternalNote"] = "Forged";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/Product/Create",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var created = await db.Set<Product>().SingleAsync(product => product.Name == "New product");
        await Assert.That(created.Details.Sku).IsEqualTo("N-6");
        await Assert.That(created.Details.InternalNote).IsEqualTo("");
    }

    [Test]
    public async Task Edit_UpdatesOnlyConfiguredOwnedProperty()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                    fields.AddSection(
                        "Details",
                        section => section.Add(product => product.Details.Sku)
                    )
                )
            )
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/Product/Edit/2");
        values["Entity.Details.Sku"] = "NEW-2";
        values["Entity.Details.InternalNote"] = "Forged";
        values["Entity.Name"] = "Forged";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/Product/Edit/2",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var updated = await db.Set<Product>().SingleAsync(product => product.Number == 2);
        await Assert.That(updated.Details.Sku).IsEqualTo("NEW-2");
        await Assert.That(updated.Details.InternalNote).IsEqualTo("Keep apple note");
        await Assert.That(updated.Name).IsEqualTo("Apple");
    }

    [Test]
    public async Task Edit_RendersOwnedPropertyValue()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit =>
                edit.Fields(fields => fields.Add(product => product.Details.Sku))
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product/Edit/2");
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.QuerySelector("input[name='Entity.Details.Sku']")!.GetAttribute("value"))
            .IsEqualTo("A-2");
    }

    [Test]
    public async Task InvalidOwnedProperty_RedisplaysErrorWithoutPersisting()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit =>
                edit.Fields(fields => fields.Add(product => product.Details.Sku))
            )
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/Product/Edit/2");
        values["Entity.Details.Sku"] = "SKU-TOO-LONG";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/Product/Edit/2",
            new FormUrlEncodedContent(values)
        );
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.QuerySelector("[data-valmsg-for='Entity.Details.Sku']")!.TextContent)
            .IsNotNullOrEmpty();
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var unchanged = await db.Set<Product>().SingleAsync(product => product.Number == 2);
        await Assert.That(unchanged.Details.Sku).IsEqualTo("A-2");
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
}
