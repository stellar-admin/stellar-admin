using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Builders;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceCustomEditTests
{
    [Test]
    public async Task CustomModel_LoadsTypedFieldsAndResourceLabels()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            Configure
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product/Edit/7");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-slot='page-header-title']").TextContent.Trim())
            .IsEqualTo("Edit Product");
        await Assert
            .That(document.RequiredElement("[data-slot='form-section-title']").TextContent.Trim())
            .IsEqualTo("Details");
        await Assert
            .That(
                document.RequiredElement("input[name='Entity.ProductName']").GetAttribute("value")
            )
            .IsEqualTo("Notebook");
        await Assert.That(document.QuerySelector("input[name='Entity.Name']")).IsNull();
        await Assert.That(document.QuerySelector("input[name='Entity.Id']")).IsNull();
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task CustomSubmission_UsesHandlerAndBindsOnlyConfiguredFields(bool sourceLast)
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource =>
            {
                resource.UseDataSource<CreateOnlyDataSource>();
                Configure(resource);
                if (sourceLast)
                {
                    resource.UseDataSource<ProductDataSource>();
                }
            }
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/Product/Edit/7");
        values["Entity.ProductName"] = "Updated notebook";
        values["Entity.Price"] = "999";
        values["Entity.Id"] = "999";
        values["id"] = "999";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/Product/Edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(response.Headers.Location!.OriginalString)
            .IsEqualTo("/stellaradmin/Product");
        await Assert.That(state.Products.Single().Name).IsEqualTo("Updated notebook");
        await Assert.That(state.Products.Single().Id).IsEqualTo(7);
        await Assert.That(state.Products.Single().Price).IsEqualTo(8.50m);
        await Assert.That(state.SubmittedId).IsEqualTo(7);
        await Assert.That(state.UpdateCalls).IsEqualTo(1);
    }

    [Test]
    public async Task MissingAntiforgeryToken_RejectsCustomSubmission()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await DashboardTestHost.CreateAsync(state, Configure);
        using var client = sut.GetTestClient();
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>());

        // Act
        using var response = await client.PostAsync("/stellaradmin/Product/Edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(state.UpdateCalls).IsEqualTo(0);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task MissingRecord_ReturnsNotFound(bool post)
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await DashboardTestHost.CreateAsync(state, Configure);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/Product/Edit/7");
        values["Entity.ProductName"] = "Updated";
        state.Products.Clear();
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = post
            ? await client.PostAsync("/stellaradmin/Product/Edit/7", content)
            : await client.GetAsync("/stellaradmin/Product/Edit/7");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(state.UpdateCalls).IsEqualTo(0);
    }

    [Test]
    public async Task RecordDisappearsDuringUpdate_ReturnsNotFound()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]) { DisappearOnUpdate = true };
        await using var sut = await DashboardTestHost.CreateAsync(state, Configure);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/Product/Edit/7");
        values["Entity.ProductName"] = "Updated";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/Product/Edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(state.UpdateCalls).IsEqualTo(1);
        await Assert.That(state.Products.Single().Name).IsEqualTo("Notebook");
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task RejectedSubmission_ShowsErrorsWithoutPersisting(bool handlerRejection)
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)])
        {
            UpdateResult = handlerRejection
                ? ResourceOperationResult.ValidationFailed([
                    new(nameof(EditProductModel.ProductName), "This name is taken."),
                    new(null, "Update rejected."),
                    new(nameof(EditProductModel.Price), "Price cannot change."),
                ])
                : null,
        };
        await using var sut = await DashboardTestHost.CreateAsync(state, Configure);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/Product/Edit/7");
        values["Entity.ProductName"] = handlerRejection ? "Attempted name" : "";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/Product/Edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(state.Products.Single().Name).IsEqualTo("Notebook");
        await Assert.That(state.UpdateCalls).IsEqualTo(handlerRejection ? 1 : 0);
        var document = await response.ReadDocumentAsync();
        await Assert
            .That(
                document.RequiredElement("input[name='Entity.ProductName']").GetAttribute("value")
            )
            .IsEqualTo(values["Entity.ProductName"]);
        await Assert
            .That(
                document
                    .RequiredElement("[data-valmsg-for='Entity.ProductName']")
                    .TextContent.Trim()
            )
            .IsNotNullOrEmpty();
        if (handlerRejection)
        {
            await Assert
                .That(document.RequiredElement(".validation-summary-errors").TextContent)
                .Contains("Update rejected.");
            await Assert
                .That(document.RequiredElement(".validation-summary-errors").TextContent)
                .Contains("Price cannot change.");
        }
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task RepeatedEdit_UsesOnlyLatestRegistration(bool customModel)
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource =>
            {
                Configure(resource);
                resource.AllowEdit<EditProductModel, EditProductHandler>(edit =>
                {
                    edit.Title = "Discarded title";
                    edit.SubmitLabel = "Discarded submit";
                    edit.Fields(fields => fields.Add(model => model.Price));
                });
                if (customModel)
                {
                    resource
                        .AllowEdit<EditProductModel, EditProductHandler>()
                        .Fields(fields => fields.Add(model => model.ProductName));
                }
                else
                {
                    resource.AllowEdit().Fields(fields => fields.Add(model => model.Name));
                }
            }
        );
        using var client = sut.GetTestClient();
        var field = customModel ? "ProductName" : "Name";
        var values = await PrepareForm(client, "/stellaradmin/Product/Edit/7");
        values[$"Entity.{field}"] = "Updated";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/Product/Edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(state.Products.Single().Name).IsEqualTo("Updated");
        var document = await client.GetDocumentAsync("/stellaradmin/Product/Edit/7");
        await Assert
            .That(document.RequiredElement("[data-slot='page-header-title']").TextContent.Trim())
            .IsEqualTo("Edit Product");
        await Assert.That(document.QuerySelectorAll("input[name^='Entity.']").Length).IsEqualTo(1);
        await Assert.That(document.QuerySelector($"input[name='Entity.{field}']")).IsNotNull();
        await Assert.That(document.QuerySelector("[data-slot='form-section-title']")).IsNull();
    }

    private static void Configure(ResourceBuilder<Product> resource)
    {
        resource.UseKey(product => product.Id);
        resource.AllowEdit<EditProductModel, EditProductHandler>(edit =>
            edit.Fields(fields =>
                fields.AddSection(
                    "Details",
                    section => section.AddRow(row => row.Add(model => model.ProductName))
                )
            )
        );
    }

    public sealed class CreateOnlyDataSource(ProductDataSource source)
        : IResourceDataSource<Product>,
            IResourceCreateHandler<Product>
    {
        public Task<ResourceOperationResult> CreateAsync(
            Product model,
            CancellationToken cancellationToken
        ) => source.CreateAsync(model, cancellationToken);

        public Task<ResourceListResult<Product>> ListAsync(
            ResourceListRequest request,
            CancellationToken cancellationToken
        ) => source.ListAsync(request, cancellationToken);
    }
}
