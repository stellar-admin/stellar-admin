using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceCreateTests
{
    [Test]
    [Arguments(false, "Create Product", "Create Product")]
    [Arguments(true, "Add inventory", "Save product")]
    public async Task ConfiguredForm_RendersFieldsAndLabels(
        bool customize,
        string title,
        string submit
    )
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
            {
                if (customize)
                {
                    resource.Create(create =>
                    {
                        create.Title = "Add inventory";
                        create.SubmitLabel = "Save product";
                        create.Fields(fields =>
                        {
                            fields.Clear();
                            fields.Add(product => product.Name).Title = "Name of product";
                            fields.Add(product => product.Price);
                        });
                    });
                }
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product/Create");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-slot='page-header-title']").TextContent.Trim())
            .IsEqualTo(title);
        await Assert
            .That(document.RequiredElement("button[type='submit']").TextContent.Trim())
            .IsEqualTo(submit);
        await Assert
            .That(document.QuerySelectorAll("input[name='Entity.Name']").Length)
            .IsEqualTo(1);
        await Assert
            .That(document.QuerySelectorAll("input[name='Entity.Price']").Length)
            .IsEqualTo(1);
        await Assert.That(document.QuerySelector("input[name='Entity.Id']")).IsNull();
        await Assert
            .That(document.RequiredElement("label[for='Entity_Name']").TextContent.Trim())
            .IsEqualTo(customize ? "Name of product" : "Product name");
        await Assert
            .That(document.QuerySelector("input[name='__RequestVerificationToken']"))
            .IsNotNull();
    }

    [Test]
    public async Task FactoryWithoutParameterlessConstructor_RendersInitialValues()
    {
        // Arrange
        await using var sut = await CreateInventoryHost([], []);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/InventoryItem/Create");

        // Assert
        await Assert
            .That(document.RequiredElement("input[name='Entity.Name']").GetAttribute("value"))
            .IsEqualTo("New item");
    }

    [Test]
    [Arguments("Notebook", true)]
    [Arguments("", false)]
    public async Task FactorySubmission_BindsAndValidatesNewInstance(string name, bool valid)
    {
        // Arrange
        var items = new List<InventoryItem>();
        var created = new List<InventoryItem>();
        await using var sut = await CreateInventoryHost(items, created);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/InventoryItem/Create");
        values["Entity.Name"] = name;
        values["Entity.Sku"] = "forged";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/InventoryItem/Create", content);

        // Assert
        await Assert.That(created.Count).IsEqualTo(2);
        await Assert.That(created[0].Name).IsEqualTo("New item");
        await Assert.That(created[1].Name).IsEqualTo(valid ? name : null);
        await Assert.That(created[1].Sku).IsEqualTo("SKU-123");
        await Assert.That(items.Count).IsEqualTo(valid ? 1 : 0);
        await Assert
            .That(response.StatusCode)
            .IsEqualTo(valid ? HttpStatusCode.Redirect : HttpStatusCode.OK);
        if (valid)
        {
            await Assert.That(items[0]).IsSameReferenceAs(created[1]);
            await Assert
                .That(response.Headers.Location?.OriginalString)
                .IsEqualTo("/stellaradmin/InventoryItem");
        }
        else
        {
            var document = await response.ReadDocumentAsync();
            await Assert
                .That(
                    document.RequiredElement("input[name='Entity.Name']").GetAttribute("value")
                        ?? ""
                )
                .IsEqualTo("");
            await Assert
                .That(
                    document.RequiredElement("[data-valmsg-for='Entity.Name']").TextContent.Trim()
                )
                .IsNotNullOrEmpty();
        }
    }

    [Test]
    [Arguments("", "12.50", "Name")]
    [Arguments("Notebook", "-1", "Price")]
    [Arguments("Notebook", "not a number", "Price")]
    public async Task InvalidSubmission_RedisplaysValuesAndDoesNotPersist(
        string name,
        string price,
        string errorField
    )
    {
        // Arrange
        var state = new ProductState([]);
        await using var sut = await DashboardTestHost.CreateAsync(state);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client);
        values["Entity.Name"] = name;
        values["Entity.Price"] = price;
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/Product/Create", content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(state.Products.Count).IsEqualTo(0);
        await Assert
            .That(document.RequiredElement("input[name='Entity.Name']").GetAttribute("value") ?? "")
            .IsEqualTo(name);
        await Assert
            .That(document.RequiredElement("input[name='Entity.Price']").GetAttribute("value"))
            .IsEqualTo(price);
        await Assert
            .That(
                document
                    .RequiredElement($"[data-valmsg-for='Entity.{errorField}']")
                    .TextContent.Trim()
            )
            .IsNotNullOrEmpty();
    }

    [Test]
    public async Task MissingAntiforgeryToken_RejectsSubmission()
    {
        // Arrange
        var state = new ProductState([]);
        await using var sut = await DashboardTestHost.CreateAsync(state);
        using var client = sut.GetTestClient();
        using var content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["Entity.Name"] = "Notebook",
                ["Entity.Price"] = "12.50",
            }
        );

        // Act
        using var response = await client.PostAsync("/stellaradmin/Product/Create", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(state.Products.Count).IsEqualTo(0);
    }

    [Test]
    public async Task ValidSubmission_PersistsConfiguredFieldsAndRedirectsToIndex()
    {
        // Arrange
        var state = new ProductState([]);
        await using var sut = await DashboardTestHost.CreateAsync(state);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client);
        values["Entity.Name"] = "New notebook";
        values["Entity.Price"] = "12.50";
        values["Entity.Id"] = "999";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/Product/Create", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(response.Headers.Location?.OriginalString)
            .IsEqualTo("/stellaradmin/Product");
        await Assert.That(state.Products.Count).IsEqualTo(1);
        await Assert.That(state.SubmittedId).IsEqualTo(0);
        await Assert.That(state.Products[0].Id).IsEqualTo(1);
        await Assert.That(state.Products[0].Name).IsEqualTo("New notebook");
        await Assert.That(state.Products[0].Price).IsEqualTo(12.50m);
        var index = await client.GetStringAsync("/stellaradmin/Product");
        await Assert.That(index).Contains("New notebook");
    }

    private static Task<WebApplication> CreateInventoryHost(
        List<InventoryItem> items,
        List<InventoryItem> created
    ) =>
        DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard =>
            {
                dashboard.Services.AddSingleton(items);
                dashboard.AddResource<InventoryItem>(resource =>
                {
                    resource.UseDataSource<InventoryItemDataSource>();
                    resource.Create(create =>
                    {
                        create.UseFactory(() =>
                        {
                            var item = new InventoryItem("SKU-123");
                            created.Add(item);

                            return item;
                        });
                        create.Fields(fields => fields.Add(item => item.Name));
                    });
                });
            }
        );

    private static async Task<Dictionary<string, string>> PrepareForm(
        HttpClient client,
        string url = "/stellaradmin/Product/Create"
    )
    {
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var document = await response.ReadDocumentAsync();
        var token = document
            .RequiredElement("input[name='__RequestVerificationToken']")
            .GetAttribute("value")!;
        client.DefaultRequestHeaders.Add(
            "Cookie",
            response.Headers.GetValues("Set-Cookie").Select(cookie => cookie.Split(';')[0])
        );

        return new() { ["__RequestVerificationToken"] = token };
    }
}
