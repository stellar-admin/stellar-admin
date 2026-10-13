using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Displays;
using StellarAdmin.TagHelpers;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceViewPageTests
{
    [Test]
    public async Task View_WithSectionsAndGroups_RendersNestedDisplayFields()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowView(view =>
                    view.Fields(fields =>
                    {
                        fields.Columns(2);
                        fields.Add(product => product.Name);
                        fields.AddSection(
                            "Pricing",
                            section =>
                            {
                                section.Columns(2);
                                section.ColumnSpanFull();
                                section
                                    .Add(product => product.Price)
                                    .UseDisplay<NumberDisplay>(number => number.Format = "0.00");
                                section.AddGroup(group => group.Add(product => product.Id));
                            }
                        );
                    })
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/view/7");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-slot='page-header-title']").TextContent.Trim())
            .IsEqualTo("View Product");
        await Assert.That(document.QuerySelector(".sa-resource-view")).IsNotNull();
        var section = document.RequiredElement("[data-slot='form-section']");
        await Assert.That(section.TextContent).Contains("Pricing");
        await Assert
            .That(
                section
                    .QuerySelectorAll("[data-slot='display-field-value']")
                    .Select(value => value.TextContent.Trim())
            )
            .IsEquivalentTo(["8.50", "7"]);
        var group = section.QuerySelectorAll("[data-slot='field-group']").Last();
        await Assert
            .That(
                group
                    .QuerySelectorAll("[data-slot='display-field-value']")
                    .Select(value => value.TextContent.Trim())
            )
            .IsEquivalentTo(["7"]);
        await Assert
            .That(document.RequiredElement("[data-slot='display-field-value']").TextContent.Trim())
            .IsEqualTo("Notebook");
        await Assert.That(document.QuerySelector("input[name^='Entity.']")).IsNull();
    }

    [Test]
    public async Task View_WithSectionOrientation_InheritsUnlessFieldOverrides()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowView(view =>
                    view.Fields(fields =>
                        fields.AddSection(
                            "Details",
                            section =>
                            {
                                section.Orientation = FieldOrientation.Horizontal;
                                section.AddGroup(group => group.Add(product => product.Name));
                                section.Add(product => product.Price).Orientation =
                                    FieldOrientation.Vertical;
                            }
                        )
                    )
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/view/7");

        // Assert
        var fields = document.QuerySelectorAll("[data-slot='display-field']");
        await Assert.That(fields[0].GetAttribute("data-orientation")).IsEqualTo("horizontal");
        await Assert.That(fields[1].GetAttribute("data-orientation")).IsEqualTo("vertical");
    }

    [Test]
    public async Task View_WithEditAndDelete_RendersPageActions()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowView(view =>
                    view.Fields(fields => fields.Add(product => product.Name))
                );
                resource.AllowEdit(edit =>
                    edit.Fields(fields => fields.Add(product => product.Name))
                );
                resource.AllowDelete();
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/view/7");

        // Assert
        var actions = document.RequiredElement("[data-slot='page-header-actions']");
        var edit = actions.RequiredElement("a[href='/stellaradmin/products/edit/7']");
        await Assert.That(edit.TextContent.Trim()).IsEqualTo("Edit Product");
        await Assert
            .That(actions.RequiredElement("[data-slot='view-back']").GetAttribute("href"))
            .IsEqualTo("/stellaradmin/products");
        await Assert
            .That(actions.QuerySelector("button[commandfor='delete-confirm-dialog']"))
            .IsNotNull();
        var deleteForm = document.RequiredElement(
            "dialog[id='delete-confirm-dialog'] form[action='/stellaradmin/products/delete/7']"
        );
        await Assert
            .That(deleteForm.RequiredElement("input[name='origin']").GetAttribute("value"))
            .IsEqualTo("view");
    }

    [Test]
    public async Task View_WithoutEditOrDelete_RendersOnlyBackLink()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowView(view =>
                    view.Fields(fields => fields.Add(product => product.Name))
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/view/7");

        // Assert
        var actions = document.RequiredElement("[data-slot='page-header-actions']");
        await Assert.That(actions.QuerySelectorAll("a, button").Length).IsEqualTo(1);
        await Assert.That(document.QuerySelector("dialog[id='delete-confirm-dialog']")).IsNull();
    }

    [Test]
    public async Task RejectedViewPageDeletion_RedirectsToViewWithErrors()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)])
        {
            DeleteResult = ResourceOperationResult.ValidationFailed(
                null,
                "This product is still in use."
            ),
        };
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowView(view =>
                    view.Fields(fields => fields.Add(product => product.Name))
                );
                resource.AllowDelete();
            }
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/view/7");
        values["origin"] = "view";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/delete/7",
            new FormUrlEncodedContent(values)
        );
        client.DefaultRequestHeaders.Remove("Cookie");
        client.DefaultRequestHeaders.Add(
            "Cookie",
            response.Headers.GetValues("Set-Cookie").Select(cookie => cookie.Split(';')[0])
        );
        var document = await client.GetDocumentAsync(response.Headers.Location!.OriginalString);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(response.Headers.Location?.OriginalString)
            .IsEqualTo("/stellaradmin/products/view/7");
        await Assert
            .That(document.RequiredElement(".validation-summary-errors").TextContent)
            .Contains("This product is still in use.");
        await Assert.That(state.Products.Count).IsEqualTo(1);
    }

    [Test]
    public async Task CustomView_WithHandler_ReadsValuesFromModel()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowView<EditProductModel, EditProductHandler>(view =>
                    view.Fields(fields => fields.Add(model => model.ProductName))
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/view/7");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-slot='display-field-value']").TextContent.Trim())
            .IsEqualTo("Notebook");
    }

    [Test]
    public async Task View_WithDisplayHandler_PassesPreparedDataToTemplate()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(2, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowView(view =>
                    view.Fields(fields =>
                        fields.Add(product => product.Id).UseDisplay<CategoryNameDisplay>()
                    )
                );
            },
            configureServices: services => services.AddSingleton<CategoryStore>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/view/2");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-custom-display='category']").TextContent)
            .IsEqualTo("Notebooks");
    }

    [Test]
    [Arguments("view/99")]
    [Arguments("view/7")]
    public async Task View_WhenMissingOrDisabled_ReturnsNotFound(string path)
    {
        // Arrange
        var enableView = path == "view/99";
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                if (enableView)
                {
                    resource.AllowView(view =>
                        view.Fields(fields => fields.Add(product => product.Name))
                    );
                }
            }
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync($"/stellaradmin/products/{path}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Index_WithView_RendersViewRowAction()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowView(view =>
                    view.Fields(fields => fields.Add(product => product.Name))
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        var link = document.RequiredElement("tbody a[href='/stellaradmin/products/view/7']");
        await Assert.That(link.GetAttribute("aria-label")).IsEqualTo("View Product");
    }

    [Test]
    public async Task Labels_WithOverrides_RenderConfiguredText()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.Index(index => index.ViewLabel = "Open");
                resource.AllowView(view =>
                    view.Fields(fields => fields.Add(product => product.Name))
                );
            },
            dashboard =>
                dashboard.ConfigureResourceLabels(labels =>
                    labels.View(view =>
                        view.Title = resource => $"{resource.SingularLabel} details"
                    )
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var index = await client.GetDocumentAsync("/stellaradmin/products");
        var page = await client.GetDocumentAsync("/stellaradmin/products/view/7");

        // Assert
        await Assert
            .That(
                index
                    .RequiredElement("tbody a[href='/stellaradmin/products/view/7']")
                    .GetAttribute("aria-label")
            )
            .IsEqualTo("Open");
        await Assert
            .That(page.RequiredElement("[data-slot='page-header-title']").TextContent.Trim())
            .IsEqualTo("Product details");
    }
}
