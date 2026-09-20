using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceDeleteTests
{
    [Test]
    [Arguments(
        0,
        "Delete Product",
        "Are you sure you want to delete this Product?",
        "Delete Product",
        "Cancel",
        "Delete Product"
    )]
    [Arguments(
        1,
        "Remove Stock item",
        "Remove this Stock item permanently?",
        "Remove",
        "Keep",
        "Remove item"
    )]
    [Arguments(
        2,
        "Remove inventory",
        "This cannot be undone.",
        "Confirm removal",
        "Go back",
        "Remove inventory item"
    )]
    public async Task ConfiguredDeletion_RendersConfirmationAndLabelOverrides(
        int configuration,
        string title,
        string message,
        string confirm,
        string cancel,
        string button
    )
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource =>
            {
                resource.UseKey(product => product.Id);
                if (configuration > 0)
                {
                    resource.SingularLabel = "Stock item";
                }
                if (configuration == 2)
                {
                    resource.Index(index => index.DeleteLabel = "Remove inventory item");
                    resource.Delete(delete =>
                    {
                        delete.Title = "Remove inventory";
                        delete.Message = "This cannot be undone.";
                        delete.ConfirmLabel = "Confirm removal";
                        delete.CancelLabel = "Go back";
                    });
                }
            },
            dashboard =>
            {
                if (configuration > 0)
                {
                    dashboard.ConfigureResourceLabels(labels =>
                    {
                        labels.DeleteTitle = resource => $"Remove {resource.SingularLabel}";
                        labels.DeleteMessage = resource =>
                            $"Remove this {resource.SingularLabel} permanently?";
                        labels.DeleteConfirmLabel = resource => "Remove";
                        labels.DeleteCancelLabel = resource => "Keep";
                        labels.IndexDeleteLabel = resource => "Remove item";
                    });
                }
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product");

        // Assert
        var form = document.RequiredElement("form[data-resource-delete]");
        await Assert.That(form.GetAttribute("action")).IsEqualTo("/stellaradmin/Product/Delete/7");
        await Assert.That(form.GetAttribute("method")).IsEqualTo("post");
        await Assert
            .That(form.QuerySelector("input[name='__RequestVerificationToken']"))
            .IsNotNull();
        await Assert
            .That(form.QuerySelector("button")!.GetAttribute("aria-label"))
            .IsEqualTo(button);
        await Assert
            .That(document.RequiredElement("[id='--resource-delete-7']").TextContent.Trim())
            .IsEqualTo(button);
        await Assert
            .That(document.RequiredElement("[data-slot='alert-dialog-title']").TextContent.Trim())
            .IsEqualTo(title);
        await Assert
            .That(
                document
                    .RequiredElement("[data-slot='alert-dialog-description']")
                    .TextContent.Trim()
            )
            .IsEqualTo(message);
        await Assert
            .That(document.RequiredElement("[data-slot='alert-dialog-action']").TextContent.Trim())
            .IsEqualTo(confirm);
        await Assert
            .That(document.RequiredElement("[data-slot='alert-dialog-cancel']").TextContent.Trim())
            .IsEqualTo(cancel);
        await Assert.That(state.DeleteCalls).IsEqualTo(0);
        await Assert.That(state.Products.Count).IsEqualTo(1);
    }

    [Test]
    public async Task ConfirmedDeletion_UsesRouteKeyAndRedirectsToIndex()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m), new(8, "Lamp", 24m)]);
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource => resource.UseKey(product => product.Id)
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/Product");
        values["id"] = "8";
        values["Entity.Id"] = "8";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/Product/Delete/7?id=8", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(response.Headers.Location?.OriginalString)
            .IsEqualTo("/stellaradmin/Product");
        await Assert.That(state.DeleteCalls).IsEqualTo(1);
        await Assert.That(state.Products.Count).IsEqualTo(1);
        await Assert.That(state.Products[0].Id).IsEqualTo(8);
        var index = await client.GetDocumentAsync("/stellaradmin/Product");
        await Assert
            .That(index.QuerySelector("form[action='/stellaradmin/Product/Delete/7']"))
            .IsNull();
    }

    [Test]
    [Arguments("999")]
    [Arguments("invalid")]
    [Arguments("7")]
    public async Task MissingResource_ReturnsNotFound(string id)
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource => resource.UseKey(product => product.Id)
        );
        using var client = sut.GetTestClient();
        using var content = new FormUrlEncodedContent(
            await PrepareForm(client, "/stellaradmin/Product")
        );
        state.Products.Clear();

        // Act
        using var response = await client.PostAsync($"/stellaradmin/Product/Delete/{id}", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(state.DeleteCalls).IsEqualTo(1);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task UnsafeRequest_DoesNotDelete(bool get)
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource => resource.UseKey(product => product.Id)
        );
        using var client = sut.GetTestClient();
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>());

        // Act
        using var response = get
            ? await client.GetAsync("/stellaradmin/Product/Delete/7")
            : await client.PostAsync("/stellaradmin/Product/Delete/7", content);

        // Assert
        await Assert
            .That(response.StatusCode)
            .IsEqualTo(get ? HttpStatusCode.MethodNotAllowed : HttpStatusCode.BadRequest);
        await Assert.That(state.DeleteCalls).IsEqualTo(0);
        await Assert.That(state.Products.Count).IsEqualTo(1);
    }

    [Test]
    public async Task WithoutKey_HidesDeletionAndRejectsSubmission()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await DashboardTestHost.CreateAsync(state);
        using var client = sut.GetTestClient();
        using var content = new FormUrlEncodedContent(await PrepareForm(client));

        // Act
        var index = await client.GetDocumentAsync("/stellaradmin/Product");
        using var response = await client.PostAsync("/stellaradmin/Product/Delete/7", content);

        // Assert
        await Assert.That(index.QuerySelector("form[data-resource-delete]")).IsNull();
        await Assert.That(index.QuerySelector("[data-slot='alert-dialog']")).IsNull();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(state.DeleteCalls).IsEqualTo(0);
    }
}
