using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.TagHelpers;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceEditTests
{
    [Test]
    public async Task PersistenceRejection_RedisplaysValuesAndFieldAndSummaryErrors()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)])
        {
            UpdateResult = ResourceOperationResult.ValidationFailed([
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
        var values = await PrepareForm(client, "/stellaradmin/Product/Edit/7");
        values["Entity.Name"] = "Attempted name";
        values["Entity.Price"] = "12.50";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/Product/Edit/7", content);

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
    [Arguments(0, "Edit Product", "Save Product", "Edit Product")]
    [Arguments(1, "Change Stock item", "Save changes", "Change")]
    [Arguments(2, "Update inventory", "Apply", "Update")]
    public async Task ConfiguredEdit_RendersExistingValuesLayoutAndLabels(
        int configuration,
        string title,
        string submit,
        string link
    )
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 12.50m)]),
            resource =>
            {
                var edit = ConfigureEdit(resource);
                resource.AllowCreate(create =>
                    create.UseFactory(() =>
                        throw new InvalidOperationException("Edit must not use the create factory.")
                    )
                );
                if (configuration > 0)
                {
                    resource.SingularLabel = "Stock item";
                }
                if (configuration == 2)
                {
                    resource.Index(index => index.EditLabel = "Update");
                    edit.Title = "Update inventory";
                    edit.SubmitLabel = "Apply";
                }
            },
            dashboard =>
            {
                if (configuration > 0)
                {
                    dashboard.ConfigureResourceLabels(labels =>
                    {
                        labels.EditTitle = resource => $"Change {resource.SingularLabel}";
                        labels.EditSubmitLabel = resource => "Save changes";
                        labels.IndexEditLabel = resource => "Change";
                    });
                }
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var index = await client.GetDocumentAsync("/stellaradmin/Product");
        var editLink = index.RequiredElement("a[href='/stellaradmin/Product/Edit/7']");
        var document = await client.GetDocumentAsync(editLink.GetAttribute("href")!);

        // Assert
        await Assert.That(editLink.GetAttribute("aria-label")).IsEqualTo(link);
        await Assert.That(editLink.GetAttribute("title")).IsNull();
        var tooltipId = editLink.GetAttribute("interestfor");
        await Assert
            .That(
                index
                    .RequiredElement($"[data-slot='tooltip-content'][id='{tooltipId}']")
                    .TextContent.Trim()
            )
            .IsEqualTo(link);
        await Assert
            .That(document.RequiredElement("[data-slot='page-header-title']").TextContent.Trim())
            .IsEqualTo(title);
        await Assert
            .That(document.RequiredElement("button[type='submit']").TextContent.Trim())
            .IsEqualTo(submit);
        await Assert
            .That(document.RequiredElement("input[name='Entity.Name']").GetAttribute("value"))
            .IsEqualTo("Notebook");
        await Assert
            .That(document.RequiredElement("input[name='Entity.Price']").GetAttribute("value"))
            .IsEqualTo("12.50");
        await Assert
            .That(
                document.RequiredElement("[data-slot='form-section']").GetAttribute("data-layout")
            )
            .IsEqualTo("card");
        await Assert
            .That(
                document
                    .RequiredElement("[data-slot='form-row']")
                    .QuerySelectorAll("input[name^='Entity.']")
                    .Length
            )
            .IsEqualTo(2);
        await Assert
            .That(document.RequiredElement("[data-slot='form-cancel']").GetAttribute("href"))
            .IsEqualTo("/stellaradmin/Product");
    }

    [Test]
    [Arguments("", "12.50", "Name")]
    [Arguments("Changed", "-1", "Price")]
    [Arguments("Changed", "not a number", "Price")]
    public async Task InvalidSubmission_RedisplaysAttemptedValuesWithoutUpdating(
        string name,
        string price,
        string errorField
    )
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource => ConfigureEdit(resource)
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/Product/Edit/7");
        values["Entity.Name"] = name;
        values["Entity.Price"] = price;
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/Product/Edit/7", content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(state.UpdateCalls).IsEqualTo(0);
        await Assert.That(state.Products[0].Name).IsEqualTo("Notebook");
        await Assert.That(state.Products[0].Price).IsEqualTo(8.50m);
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
        await Assert
            .That(
                document.RequiredElement("[data-slot='form-section']").GetAttribute("data-layout")
            )
            .IsEqualTo("card");
    }

    [Test]
    public async Task MissingAntiforgeryToken_RejectsUpdate()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource => ConfigureEdit(resource)
        );
        using var client = sut.GetTestClient();
        using var content = new FormUrlEncodedContent(
            new Dictionary<string, string> { ["Entity.Name"] = "Changed" }
        );

        // Act
        using var response = await client.PostAsync("/stellaradmin/Product/Edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(state.UpdateCalls).IsEqualTo(0);
        await Assert.That(state.Products[0].Name).IsEqualTo("Notebook");
    }

    [Test]
    [Arguments("999", false)]
    [Arguments("invalid", false)]
    [Arguments("999", true)]
    [Arguments("invalid", true)]
    public async Task MissingResource_ReturnsNotFound(string id, bool post)
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource => ConfigureEdit(resource)
        );
        using var client = sut.GetTestClient();
        using var content = new FormUrlEncodedContent(
            await PrepareForm(client, "/stellaradmin/Product/Edit/7")
        );

        // Act
        using var response = post
            ? await client.PostAsync($"/stellaradmin/Product/Edit/{id}", content)
            : await client.GetAsync($"/stellaradmin/Product/Edit/{id}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(state.UpdateCalls).IsEqualTo(0);
    }

    [Test]
    public async Task ResourceDisappearsBeforeSave_ReturnsNotFound()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]) { DisappearOnUpdate = true };
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource => ConfigureEdit(resource)
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/Product/Edit/7");
        values["Entity.Name"] = "Changed";
        values["Entity.Price"] = "12.50";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/Product/Edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(state.UpdateCalls).IsEqualTo(1);
    }

    [Test]
    [Arguments(false, "999", 8.50)]
    [Arguments(true, "12.50", 12.50)]
    public async Task ValidSubmission_UsesRouteKeyAndOnlyUpdatesConfiguredFields(
        bool includePrice,
        string price,
        double expectedPrice
    )
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m), new(8, "Lamp", 24m)]);
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                    {
                        fields.Add(product => product.Id);
                        fields.Add(product => product.Name);
                        if (includePrice)
                        {
                            fields.Add(product => product.Price);
                        }
                    })
                );
            }
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/Product/Edit/7");
        values["id"] = "8";
        values["Entity.Id"] = "8";
        values["Entity.Name"] = "Updated notebook";
        values["Entity.Price"] = price;
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/Product/Edit/7?id=8", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(response.Headers.Location?.OriginalString)
            .IsEqualTo("/stellaradmin/Product");
        await Assert.That(state.UpdateCalls).IsEqualTo(1);
        await Assert.That(state.Products[0].Id).IsEqualTo(7);
        await Assert.That(state.SubmittedId).IsEqualTo(7);
        await Assert.That(state.Products[0].Name).IsEqualTo("Updated notebook");
        await Assert.That(state.Products[0].Price).IsEqualTo((decimal)expectedPrice);
        await Assert.That(state.Products[1].Name).IsEqualTo("Lamp");
        var index = await client.GetDocumentAsync("/stellaradmin/Product");
        await Assert.That(index.Body!.TextContent).Contains("Updated notebook");
    }

    private static ResourceEditBuilder<Product> ConfigureEdit(ResourceBuilder<Product> resource)
    {
        resource.UseKey(product => product.Id);
        var edit = resource.AllowEdit();
        edit.SectionLayout = FormSectionLayout.Card;
        edit.Fields(fields =>
            fields.AddSection(
                "Product details",
                section =>
                    section.AddRow(row =>
                    {
                        row.Add(product => product.Name);
                        row.Add(product => product.Price);
                    })
            )
        );

        return edit;
    }
}
