using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.TagHelpers;
using TUnit.Assertions.Enums;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceCreateTests
{
    [Test]
    public async Task PersistenceRejection_RedisplaysValuesAndFieldAndSummaryErrors()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)])
        {
            CreateResult = ResourceOperationResult.ValidationFailed([
                new(nameof(Product.Name), "This name is already used."),
                new(nameof(Product.Name), "Choose another name."),
                new(null, "The operation was rejected."),
                new("UnrenderedProperty", "An additional requirement was not met."),
            ]),
        };
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                    {
                        fields.Add(product => product.Name);
                        fields.Add(product => product.Price);
                    })
                );
            }
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.Name"] = "Attempted name";
        values["Entity.Price"] = "12.50";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/create", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var document = await response.ReadDocumentAsync();
        await Assert
            .That(document.RequiredElement("input[name='Entity.Name']").GetAttribute("value"))
            .IsEqualTo("Attempted name");
        await Assert
            .That(document.RequiredElement("input[name='Entity.Price']").GetAttribute("value"))
            .IsEqualTo("12.50");
        await Assert
            .That(document.RequiredElement("[data-valmsg-for='Entity.Name']").TextContent)
            .Contains("This name is already used.");
        await Assert
            .That(document.RequiredElement(".validation-summary-errors").TextContent)
            .Contains("The operation was rejected.");
        await Assert
            .That(document.RequiredElement(".validation-summary-errors").TextContent)
            .Contains("An additional requirement was not met.");
        await Assert.That(state.Products.Count).IsEqualTo(1);
        await Assert.That(state.Products[0].Name).IsEqualTo("Notebook");
        await Assert.That(state.Products[0].Price).IsEqualTo(8.50m);
    }

    [Test]
    public async Task ClearedLayout_RendersNoFieldsOrContainers()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
            {
                var create = ConfigureLayout(resource);
                create.Fields(fields => fields.Clear());
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert.That(document.QuerySelectorAll("input[name^='Entity.']").Length).IsEqualTo(0);
        await Assert.That(document.QuerySelector("[data-slot='form-section']")).IsNull();
        await Assert.That(document.QuerySelector("[data-slot='form-grid-cell']")).IsNull();
    }

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
                    resource.AllowCreate(create =>
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
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

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
        var document = await client.GetDocumentAsync("/stellaradmin/inventory-items/create");

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
        var values = await PrepareForm(client, "/stellaradmin/inventory-items/create");
        values["Entity.Name"] = name;
        values["Entity.Sku"] = "forged";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/inventory-items/create",
            content
        );

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
                .IsEqualTo("/stellaradmin/inventory-items");
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
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource => ConfigureLayout(resource),
            configureDashboard: dashboard =>
                dashboard.ConfigureResourceLabels(labels =>
                    labels.Create(create =>
                    {
                        create.Title = resource => $"Add {resource.SingularLabel}";
                        create.SubmitLabel = resource => "Save";
                    })
                )
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client);
        values["Entity.Name"] = name;
        values["Entity.Price"] = price;
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/create", content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(state.Products.Count).IsEqualTo(0);
        await Assert
            .That(
                document
                    .RequiredElement("[data-slot='form-section']")
                    .QuerySelectorAll("[data-slot='form-grid-content'][data-columns='multiple']")
                    .Length
            )
            .IsEqualTo(1);
        await Assert
            .That(document.RequiredElement("[data-slot='page-header-title']").TextContent.Trim())
            .IsEqualTo("Add Product");
        await Assert
            .That(document.RequiredElement("button[type='submit']").TextContent.Trim())
            .IsEqualTo("Save");
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
    public async Task GridLayout_WritesResolvedColumnsAndSpans()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource
                    .AllowCreate()
                    .Fields(fields =>
                    {
                        fields.Clear();
                        fields.Columns(columns => columns.Small(2).Large(3));
                        fields.Add(product => product.Name).ColumnSpanFull();
                        fields.Add(product => product.Price).ColumnSpan(2);
                        fields.AddGroup(group =>
                        {
                            group.ColumnSpan(span => span.Medium(2));
                            group.Add(product => product.Id);
                        });
                    })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var grid = document.RequiredElement("[data-slot='form-grid-content']");
        await Assert
            .That(grid.GetAttribute("style"))
            .IsEqualTo("--sa-cols-sm:2;--sa-cols-md:2;--sa-cols-lg:3");
        await Assert.That(grid.GetAttribute("data-columns")).IsEqualTo("multiple");
        var items = grid.Children;
        await Assert.That(items.Length).IsEqualTo(3);
        await Assert
            .That(items[0].GetAttribute("style"))
            .IsEqualTo("--sa-span-sm:2;--sa-span-md:2;--sa-span-lg:3");
        await Assert.That(items[0].QuerySelector("input[name='Entity.Name']")).IsNotNull();
        await Assert
            .That(items[1].GetAttribute("style"))
            .IsEqualTo("--sa-span-sm:2;--sa-span-md:2;--sa-span-lg:2");
        await Assert.That(items[2].GetAttribute("data-slot")).IsEqualTo("field-group");
        await Assert
            .That(items[2].GetAttribute("style"))
            .IsEqualTo("--sa-span-md:2;--sa-span-lg:2");
        var groupGrid = items[2].RequiredElement("[data-slot='form-grid-content']");
        await Assert.That(groupGrid.HasAttribute("style")).IsFalse();
        await Assert.That(groupGrid.GetAttribute("data-columns")).IsEqualTo("single");
        await Assert
            .That(groupGrid.RequiredElement("[data-slot='form-grid-cell']").HasAttribute("style"))
            .IsFalse();
    }

    [Test]
    public async Task SectionLayout_OverridesFormSectionLayout()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource
                    .AllowCreate()
                    .Fields(fields =>
                    {
                        fields.Clear();
                        fields.AddSection(
                            "Details",
                            section =>
                            {
                                section.Layout = FormSectionLayout.Card;
                                section.Add(product => product.Name);
                            }
                        );
                        fields.AddSection(
                            "Pricing",
                            section => section.Add(product => product.Price)
                        );
                    }),
            dashboard =>
                dashboard
                    .Services.AddStellarAdmin()
                    .ConfigureForms(forms => forms.SectionLayout = FormSectionLayout.Split)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(
                document
                    .QuerySelectorAll("[data-slot='form-section']")
                    .Select(section => section.GetAttribute("data-layout"))
                    .ToArray()
            )
            .IsEquivalentTo(["card", "split"], CollectionOrdering.Matching);
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
        using var response = await client.PostAsync("/stellaradmin/products/create", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(state.Products.Count).IsEqualTo(0);
    }

    [Test]
    [Arguments(false, "split")]
    [Arguments(true, "card")]
    public async Task NestedLayout_RendersContainersAndFieldsInOrder(
        bool overrideLayout,
        string layout
    )
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
            {
                var create = ConfigureLayout(resource);
                if (overrideLayout)
                {
                    create.SectionLayout = FormSectionLayout.Card;
                }

                create.Fields(fields => fields.Add(product => product.Id));
            },
            dashboard =>
                dashboard
                    .Services.AddStellarAdmin()
                    .ConfigureForms(forms => forms.SectionLayout = FormSectionLayout.Split)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var section = document.RequiredElement("[data-slot='form-section']");
        await Assert.That(section.GetAttribute("data-layout")).IsEqualTo(layout);
        await Assert
            .That(section.RequiredElement("[data-slot='form-section-title']").TextContent.Trim())
            .IsEqualTo("Product <details>");
        await Assert
            .That(
                section.RequiredElement("[data-slot='form-section-description']").TextContent.Trim()
            )
            .IsEqualTo("Catalog <information>");
        await Assert.That(section.QuerySelector("details")).IsNull();
        var grid = section.RequiredElement(
            "[data-slot='form-grid-content'][data-columns='multiple']"
        );
        await Assert.That(grid.GetAttribute("style")).IsEqualTo("--sa-cols-md:2;--sa-cols-lg:2");
        await Assert
            .That(grid.QuerySelectorAll(":scope > [data-slot='field-group']").Length)
            .IsEqualTo(2);
        await Assert
            .That(grid.RequiredElement("label[for='Entity_Name']").TextContent.Trim())
            .IsEqualTo("Item name");
        await Assert
            .That(
                document
                    .QuerySelectorAll("input[name^='Entity.']")
                    .Select(input => input.GetAttribute("name")!)
                    .ToArray()
            )
            .IsEquivalentTo(
                ["Entity.Name", "Entity.Price", "Entity.Id"],
                CollectionOrdering.Matching
            );
        await Assert.That(section.QuerySelector("input[name='Entity.Id']")).IsNull();
    }

    [Test]
    public async Task ValidSubmission_PersistsConfiguredFieldsAndRedirectsToIndex()
    {
        // Arrange
        var state = new ProductState([]);
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource => ConfigureLayout(resource)
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client);
        values["Entity.Name"] = "New notebook";
        values["Entity.Price"] = "12.50";
        values["Entity.Id"] = "999";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/create", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(response.Headers.Location?.OriginalString)
            .IsEqualTo("/stellaradmin/products");
        await Assert.That(state.Products.Count).IsEqualTo(1);
        await Assert.That(state.SubmittedId).IsEqualTo(0);
        await Assert.That(state.Products[0].Id).IsEqualTo(1);
        await Assert.That(state.Products[0].Name).IsEqualTo("New notebook");
        await Assert.That(state.Products[0].Price).IsEqualTo(12.50m);
        var index = await client.GetStringAsync("/stellaradmin/products");
        await Assert.That(index).Contains("New notebook");
    }

    private static ResourceCreateBuilder<Product> ConfigureLayout(
        ResourceBuilder<Product> resource
    ) =>
        resource
            .AllowCreate()
            .Fields(fields =>
            {
                fields.Clear();
                var section = fields.AddSection("Product <details>");
                section.Description = "Catalog <information>";
                var columns = section.AddGroup();
                columns.Columns(2);
                columns.AddGroup(group =>
                {
                    group.Add(product => product.Id);
                    group.Clear();
                    group.Add(product => product.Name).Title = "Item name";
                });
                columns.AddGroup().Add(product => product.Price);
            });

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
                    resource.AllowCreate(create =>
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
}
