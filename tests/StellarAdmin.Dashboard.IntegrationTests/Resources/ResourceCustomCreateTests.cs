using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Builders;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceCustomCreateTests
{
    [Test]
    public async Task CustomModel_RendersTypedLayoutFactoryValuesAndResourceLabels()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(new([]), Configure);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product/Create");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-slot='page-header-title']").TextContent.Trim())
            .IsEqualTo("Create Product");
        await Assert
            .That(document.RequiredElement("[data-slot='form-section-title']").TextContent.Trim())
            .IsEqualTo("Registration");
        await Assert
            .That(document.RequiredElement("input[name='Entity.Price']").GetAttribute("value"))
            .IsEqualTo("10");
        await Assert
            .That(document.RequiredElement("input[name='Entity.Password']").GetAttribute("type"))
            .IsEqualTo("password");
        await Assert.That(document.QuerySelector("input[name='Entity.Name']")).IsNull();
        await Assert.That(document.QuerySelector("input[name='Entity.Id']")).IsNull();
        await Assert
            .That(document.RequiredElement("button[type='submit']").TextContent.Trim())
            .IsEqualTo("Register product");
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task CustomSubmission_UsesHandlerRegardlessOfDataSourceRegistrationOrder(
        bool sourceLast
    )
    {
        // Arrange
        var state = new ProductState([]);
        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource =>
            {
                Configure(resource);
                if (sourceLast)
                {
                    resource.UseDataSource<ProductDataSource>();
                }
            }
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client);
        values["Entity.ProductName"] = "Custom notebook";
        values["Entity.Price"] = "12.50";
        values["Entity.Password"] = "secret-value";
        values["Entity.PasswordConfirmation"] = "secret-value";
        values["Entity.Id"] = "999";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/Product/Create", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(state.CreateCalls).IsEqualTo(1);
        await Assert.That(state.SubmittedId).IsEqualTo(0);
        await Assert.That(state.Products.Single().Name).IsEqualTo("Custom notebook");
        await Assert.That(state.Products.Single().Price).IsEqualTo(12.50m);
        var edit = await client.GetDocumentAsync("/stellaradmin/Product/Edit/1");
        await Assert
            .That(edit.RequiredElement("input[name='Entity.Name']").GetAttribute("value"))
            .IsEqualTo("Custom notebook");
        await Assert.That(edit.QuerySelector("input[name='Entity.Password']")).IsNull();
    }

    [Test]
    public async Task MissingAntiforgeryToken_RejectsCustomSubmission()
    {
        // Arrange
        var state = new ProductState([]);
        await using var sut = await DashboardTestHost.CreateAsync(state, Configure);
        using var client = sut.GetTestClient();
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>());

        // Act
        using var response = await client.PostAsync("/stellaradmin/Product/Create", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(state.CreateCalls).IsEqualTo(0);
    }

    [Test]
    public async Task OrdinaryEditWithCustomCreate_PersistsThroughDataSource()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await DashboardTestHost.CreateAsync(state, Configure);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/Product/Edit/7");
        values["Entity.Name"] = "Edited notebook";
        values["Entity.Price"] = "15";
        values["Entity.Id"] = "999";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/Product/Edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(state.Products.Single().Id).IsEqualTo(7);
        await Assert.That(state.Products.Single().Name).IsEqualTo("Edited notebook");
        await Assert.That(state.UpdateCalls).IsEqualTo(1);
        await Assert.That(state.CreateCalls).IsEqualTo(0);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task RejectedSubmission_RedisplaysModelErrorsWithoutPasswords(
        bool handlerRejection
    )
    {
        // Arrange
        var state = new ProductState([])
        {
            CreateResult = handlerRejection
                ? ResourceOperationResult.ValidationFailed([
                    new(nameof(CreateProductModel.ProductName), "This name is taken."),
                    new(null, "Registration rejected."),
                ])
                : null,
        };
        await using var sut = await DashboardTestHost.CreateAsync(state, Configure);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client);
        values["Entity.ProductName"] = "Attempted name";
        values["Entity.Price"] = "12.50";
        values["Entity.Password"] = "secret-value";
        values["Entity.PasswordConfirmation"] = handlerRejection
            ? "secret-value"
            : "different-secret";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/Product/Create", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(state.Products.Count).IsEqualTo(0);
        await Assert.That(state.CreateCalls).IsEqualTo(handlerRejection ? 1 : 0);
        var document = await response.ReadDocumentAsync();
        await Assert
            .That(
                document.RequiredElement("input[name='Entity.ProductName']").GetAttribute("value")
            )
            .IsEqualTo("Attempted name");
        await Assert
            .That(
                document.RequiredElement("input[name='Entity.Password']").GetAttribute("value")
                    ?? ""
            )
            .IsEqualTo("");
        await Assert
            .That(
                document
                    .RequiredElement("input[name='Entity.PasswordConfirmation']")
                    .GetAttribute("value")
                    ?? ""
            )
            .IsEqualTo("");
        var field = handlerRejection ? "ProductName" : "PasswordConfirmation";
        await Assert
            .That(
                document.RequiredElement($"[data-valmsg-for='Entity.{field}']").TextContent.Trim()
            )
            .IsNotNullOrEmpty();
        if (handlerRejection)
        {
            await Assert
                .That(document.RequiredElement(".validation-summary-errors").TextContent)
                .Contains("Registration rejected.");
        }
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task RepeatedCreate_UsesOnlyLatestRegistration(bool customModel)
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
            {
                resource.Create<CreateProductModel, CreateProductHandler>(create =>
                {
                    create.Title = "Discarded title";
                    create.SubmitLabel = "Discarded submit";
                    create.UseFactory(() =>
                        throw new InvalidOperationException("Discarded factory")
                    );
                    create.Fields(fields => fields.Add(model => model.Password));
                });
                if (customModel)
                {
                    resource.Create<CreateProductModel, CreateProductHandler>(create =>
                    {
                        create.UseFactory(() => new(25m));
                        create.Fields(fields => fields.Add(model => model.ProductName));
                    });
                }
                else
                {
                    resource.Create().Fields(fields => fields.Add(product => product.Name));
                }
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product/Create");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-slot='page-header-title']").TextContent.Trim())
            .IsEqualTo("Create Product");
        await Assert
            .That(document.RequiredElement("button[type='submit']").TextContent.Trim())
            .IsEqualTo("Create Product");
        await Assert.That(document.QuerySelectorAll("input[name^='Entity.']").Length).IsEqualTo(1);
        var name = customModel ? "ProductName" : "Name";
        await Assert.That(document.QuerySelector($"input[name='Entity.{name}']")).IsNotNull();
        await Assert.That(document.QuerySelector("input[name='Entity.Password']")).IsNull();
    }

    private static void Configure(ResourceBuilder<Product> resource)
    {
        resource.UseDataSource<ProductMaintenanceDataSource>();
        resource.UseKey(product => product.Id);
        resource.Create<CreateProductModel, CreateProductHandler>(create =>
        {
            create.SubmitLabel = "Register product";
            create.UseFactory(() => new(10m));
            create.Fields(fields =>
                fields.AddSection(
                    "Registration",
                    section =>
                        section.AddRow(row =>
                        {
                            row.Add(model => model.ProductName);
                            row.Add(model => model.Price);
                            row.Add(model => model.Password);
                            row.Add(model => model.PasswordConfirmation);
                        })
                )
            );
        });
        resource.Edit(edit =>
            edit.Fields(fields =>
            {
                fields.Add(product => product.Name);
                fields.Add(product => product.Price);
            })
        );
    }
}
