using Microsoft.AspNetCore.TestHost;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;
using static StellarAdmin.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.IntegrationTests.Resources;

public class ResourcePageHeaderTests
{
    [Test]
    [Arguments("/stellaradmin/products")]
    [Arguments("/stellaradmin/products/view/7")]
    [Arguments("/stellaradmin/products/create")]
    [Arguments("/stellaradmin/products/edit/7")]
    public async Task ResourcePages_RenderStickyHeader(string url)
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigurePages
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(url);

        // Assert
        await Assert
            .That(document.RequiredElement("[data-slot='page-header']").HasAttribute("data-sticky"))
            .IsTrue();
    }

    [Test]
    public async Task ViewPage_RendersRecordTitleAndOutlineDestructiveDelete()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigurePages
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/view/7");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-slot='page-header-title']").TextContent.Trim())
            .IsEqualTo("Notebook");
        await Assert.That(document.Title).StartsWith("Notebook");
        var actions = document.RequiredElement("[data-slot='page-header-actions']");
        await Assert.That(actions.QuerySelector("[data-slot='view-back']")).IsNotNull();
        await Assert
            .That(
                actions
                    .RequiredElement("button[commandfor='delete-confirm-dialog']")
                    .ClassList.Contains("sa-button-variant-outline-destructive")
            )
            .IsTrue();
    }

    [Test]
    public async Task EditPage_RendersActionsInHeaderForTheForm()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigurePages
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        var actions = document.RequiredElement("[data-slot='page-header-actions']");
        var form = document.RequiredElement("form[data-resource-form]");
        await Assert.That(form.Id).IsEqualTo("resource-form");
        await Assert.That(form.QuerySelector("button[type='submit']")).IsNull();
        await Assert
            .That(actions.RequiredElement("[data-form-submit]").GetAttribute("form"))
            .IsEqualTo("resource-form");
        await Assert.That(actions.QuerySelector("[data-slot='form-cancel']")).IsNotNull();
        await Assert
            .That(
                actions
                    .RequiredElement("button[commandfor='delete-confirm-dialog']")
                    .ClassList.Contains("sa-button-variant-outline-destructive")
            )
            .IsTrue();
    }

    [Test]
    public async Task EditPage_AfterRejectedSave_KeepsStoredRecordTitle()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigurePages
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/7");
        values["Entity.Name"] = "";
        values["Entity.Price"] = "8.50";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/edit/7", content);

        // Assert
        var document = await response.ReadDocumentAsync();
        await Assert
            .That(document.RequiredElement("[data-slot='page-header-title']").TextContent.Trim())
            .IsEqualTo("Notebook");
    }

    [Test]
    public async Task ViewAndEditPages_WithPageTitle_RenderPageTitle()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowView(view =>
                {
                    view.Title = "Product details";
                    view.Fields(fields => fields.Add(product => product.Name));
                });
                resource.AllowEdit(edit =>
                {
                    edit.Title = "Change product";
                    edit.Fields(fields => fields.Add(product => product.Name));
                });
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var view = await client.GetDocumentAsync("/stellaradmin/products/view/7");
        var edit = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        await Assert
            .That(view.RequiredElement("[data-slot='page-header-title']").TextContent.Trim())
            .IsEqualTo("Product details");
        await Assert
            .That(edit.RequiredElement("[data-slot='page-header-title']").TextContent.Trim())
            .IsEqualTo("Change product");
    }

    [Test]
    public async Task ViewPage_WhenTitleIsOnlyTheKey_RendersLabel()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
            {
                admin.AddResource<CustomProduct>(resource =>
                {
                    resource.SingularLabel = "Custom item";
                    resource.PluralLabel = "Custom inventory";
                    resource.UseDataSource<CustomProductDataSource>();
                    resource.UseKey(product => product.Code);
                    resource.AllowView(view =>
                        view.Fields(fields => fields.Add(product => product.Code))
                    );
                });
                admin.ConfigureResourceLabels(labels =>
                    labels.View(view =>
                        view.Title = resource => $"{resource.SingularLabel} details"
                    )
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/custom-products/view/item-1");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-slot='page-header-title']").TextContent.Trim())
            .IsEqualTo("Custom item details");
    }

    private static void ConfigurePages(ResourceBuilder<Product> resource)
    {
        resource.UseKey(product => product.Id);
        resource.AllowView(view => view.Fields(fields => fields.Add(product => product.Name)));
        resource.AllowCreate(create =>
            create.Fields(fields =>
            {
                fields.Add(product => product.Name);
                fields.Add(product => product.Price);
            })
        );
        resource.AllowEdit(edit =>
            edit.Fields(fields =>
            {
                fields.Add(product => product.Name);
                fields.Add(product => product.Price);
            })
        );
        resource.AllowDelete();
    }
}
