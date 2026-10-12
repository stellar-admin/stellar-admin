using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.TagHelpers;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceDisplayFieldTests
{
    [Test]
    public async Task Create_WithDisplayField_ShowsEmptyText()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate(create =>
                    create.Fields(fields =>
                    {
                        fields.Add(product => product.Price);
                        fields.AddDisplayField(
                            product => product.Name,
                            field => field.EmptyText = "Not named yet"
                        );
                    })
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var field = document.RequiredElement("[data-slot='display-field']");
        await Assert
            .That(field.RequiredElement("[data-slot='display-field-label']").TextContent.Trim())
            .IsEqualTo("Product name");
        await Assert
            .That(field.RequiredElement("[data-slot='display-field-value']").TextContent.Trim())
            .IsEqualTo("Not named yet");
        await Assert.That(document.QuerySelector("input[name='Entity.Name']")).IsNull();
    }

    [Test]
    public async Task CustomEdit_WithDisplayField_ReadsValueFromModel()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit<EditProductModel, EditProductHandler>(edit =>
                    edit.Fields(fields =>
                    {
                        fields.Add(model => model.ProductName);
                        fields.AddDisplayField(
                            model => model.Price,
                            field => field.Format = "{0:0.00}"
                        );
                    })
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-slot='display-field-value']").TextContent.Trim())
            .IsEqualTo("8.50");
    }

    [Test]
    public async Task Edit_WithDetails_RendersListWithLayoutAndPlacement()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                    {
                        fields.Add(product => product.Name);
                        fields.AddDetails(
                            "Audit",
                            details =>
                            {
                                details.Description = "Read-only values.";
                                details.Orientation = FieldOrientation.Horizontal;
                                details.Variant = DetailsVariant.Divided;
                                details.Columns(3);
                                details.Add(
                                    product => product.Id,
                                    field => field.Title = "Reference"
                                );
                                details.Add(product => product.Price).ColumnSpan(2).ColumnStart(2);
                            }
                        );
                    })
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        var details = document.RequiredElement("[data-slot='details']");
        var section = details.Closest("[data-slot='form-section']")!;
        await Assert
            .That(section.RequiredElement("[data-slot='form-section-title']").TextContent.Trim())
            .IsEqualTo("Audit");
        await Assert.That(section.TextContent).Contains("Read-only values.");
        await Assert.That(details.GetAttribute("data-variant")).IsEqualTo("divided");
        await Assert
            .That(details.RequiredElement("dl").GetAttribute("style"))
            .IsEqualTo("--sa-cols-md:3;--sa-cols-lg:3");
        var fields = details.QuerySelectorAll("dl > [data-slot='display-field']");
        await Assert.That(fields.Length).IsEqualTo(2);
        await Assert.That(fields[0].GetAttribute("data-orientation")).IsEqualTo("horizontal");
        await Assert
            .That(fields[0].RequiredElement("dt").TextContent.Trim())
            .IsEqualTo("Reference");
        await Assert.That(fields[0].RequiredElement("dd").TextContent.Trim()).IsEqualTo("7");
        await Assert
            .That(fields[1].GetAttribute("style"))
            .IsEqualTo("--sa-span-md:2;--sa-span-lg:2;--sa-start-md:2;--sa-start-lg:2");
    }

    [Test]
    [Arguments(false, "EUR 8.50")]
    [Arguments(true, "USD 8.50")]
    public async Task Edit_WithDisplay_RendersDisplayTemplateWithSettings(
        bool configure,
        string expected
    )
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                    {
                        var price = fields.AddDisplayField(product => product.Price);
                        if (configure)
                        {
                            price.UseDisplay<PriceDisplay>(display => display.Currency = "USD");
                        }
                        else
                        {
                            price.UseDisplay<PriceDisplay>();
                        }

                        fields.AddDetails(
                            "Audit",
                            details =>
                                details
                                    .Add(product => product.Price)
                                    .UseDisplay<PriceDisplay>(display => display.Currency = "GBP")
                        );
                    })
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        await Assert
            .That(document.TextContents("[data-custom-display='price']"))
            .IsEquivalentTo([expected, "GBP 8.50"]);
        await Assert.That(document.QuerySelector("dd > [data-custom-display='price']")).IsNotNull();
    }

    [Test]
    public async Task Edit_WithDisplayFieldAmongInputs_RendersInGridItem()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                    {
                        fields.Columns(2);
                        fields.Add(product => product.Name);
                        fields.AddDisplayField(product => product.Price).ColumnSpanFull();
                        fields.AddSection(
                            "Pricing",
                            section =>
                                section.AddDisplayField(
                                    product => product.Id,
                                    field => field.Title = "Reference"
                                )
                        );
                    })
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        var item = document.RequiredElement("[data-slot='display-field']").ParentElement!;
        await Assert.That(item.GetAttribute("data-slot")).IsEqualTo("grid-item");
        await Assert.That(item.GetAttribute("style")).IsEqualTo("--sa-span-md:2;--sa-span-lg:2");
        await Assert
            .That(item.RequiredElement("[data-slot='display-field-value']").TextContent.Trim())
            .IsEqualTo("8.50");
        await Assert.That(item.QuerySelector("dt")).IsNull();
        await Assert
            .That(
                document
                    .RequiredElement("[data-slot='form-section'] [data-slot='display-field-label']")
                    .TextContent.Trim()
            )
            .IsEqualTo("Reference");
        await Assert.That(document.QuerySelector("input[name='Entity.Price']")).IsNull();
    }

    [Test]
    public async Task InvalidSubmission_RedisplaysDisplayFieldValues()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigureEdit
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/7");
        values["Entity.Name"] = "";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var document = await response.ReadDocumentAsync();
        await Assert
            .That(document.RequiredElement("[data-slot='display-field-value']").TextContent.Trim())
            .IsEqualTo("8.50");
    }

    [Test]
    public async Task Submission_IgnoresPostedDisplayFieldValue()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await DashboardTestHost.CreateAsync(state, ConfigureEdit);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/7");
        values["Entity.Name"] = "Updated notebook";
        values["Entity.Price"] = "999";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(state.Products[0].Name).IsEqualTo("Updated notebook");
        await Assert.That(state.Products[0].Price).IsEqualTo(8.50m);
    }

    private static void ConfigureEdit(ResourceBuilder<Product> resource)
    {
        resource.UseKey(product => product.Id);
        resource.AllowEdit(edit =>
            edit.Fields(fields =>
            {
                fields.Add(product => product.Name);
                fields.AddDisplayField(
                    product => product.Price,
                    field => field.Format = "{0:0.00}"
                );
            })
        );
    }
}
