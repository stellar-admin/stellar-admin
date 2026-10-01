using System.Net;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources.Editors;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceEditorTests
{
    [Test]
    public async Task CheckboxGroupEditor_ControlClassName_StylesChoicesContainer()
    {
        // Arrange
        var state = new RoleSelectionState();
        await using var sut = await CreateRoleSelectionHost(
            state,
            classNames => classNames.Control = "role-choices"
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement("[data-slot='checkbox-group']")
                    .ClassList.Contains("role-choices")
            )
            .IsTrue();
    }

    [Test]
    public async Task CheckboxGroupEditor_UnsupportedClassName_FailsRendering()
    {
        // Arrange
        var state = new RoleSelectionState();
        await using var sut = await CreateRoleSelectionHost(
            state,
            classNames => classNames.Label = "role-label"
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/edit/7");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .Contains("Checkbox group editors support only Control");
    }

    [Test]
    public async Task CheckboxGroupEditor_RendersSelectedChoices()
    {
        // Arrange
        var state = new RoleSelectionState();
        await using var sut = await CreateRoleSelectionHost(state);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement(
                        "input[type='checkbox'][name='Entity.RoleIds'][value='auditor']"
                    )
                    .HasAttribute("checked")
            )
            .IsTrue();
        await Assert
            .That(
                document
                    .RequiredElement(
                        "input[type='checkbox'][name='Entity.RoleIds'][value='manager']"
                    )
                    .HasAttribute("checked")
            )
            .IsFalse();
    }

    [Test]
    public async Task CheckboxGroupEditor_UncheckedSubmission_ClearsExistingSelections()
    {
        // Arrange
        var state = new RoleSelectionState();
        await using var sut = await CreateRoleSelectionHost(state);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/7");
        values["__sa_checkbox_group.Entity.RoleIds"] = "true";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(state.RoleIds).IsEmpty();
    }

    [Test]
    public async Task CheckboxGroupEditor_SelectedSubmission_ReplacesExistingSelections()
    {
        // Arrange
        var state = new RoleSelectionState();
        await using var sut = await CreateRoleSelectionHost(state);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/7");
        values["Entity.RoleIds"] = "manager";
        values["__sa_checkbox_group.Entity.RoleIds"] = "true";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(state.RoleIds).IsEquivalentTo(["manager"]);
    }

    [Test]
    public async Task CheckboxGroupEditor_RejectedSubmission_RetainsPostedSelection()
    {
        // Arrange
        var state = new RoleSelectionState
        {
            UpdateResult =
                StellarAdmin.Dashboard.Resources.ResourceOperationResult.ValidationFailed(
                    nameof(RoleSelectionModel.RoleIds),
                    "Roles cannot change."
                ),
        };
        await using var sut = await CreateRoleSelectionHost(state);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/7");
        values["Entity.RoleIds"] = "manager";
        values["__sa_checkbox_group.Entity.RoleIds"] = "true";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/edit/7", content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(state.RoleIds).IsEquivalentTo(["auditor"]);
        await Assert
            .That(
                document
                    .RequiredElement(
                        "input[type='checkbox'][name='Entity.RoleIds'][value='manager']"
                    )
                    .HasAttribute("checked")
            )
            .IsTrue();
        await Assert
            .That(document.RequiredElement("[data-slot='field-error']").TextContent)
            .Contains("Roles cannot change.");
    }

    [Test]
    public async Task SelectEditor_UsesRegisteredProviderChoices()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate(create =>
                    create.Fields(fields =>
                    {
                        fields.Clear();
                        fields
                            .Add(product => product.Name)
                            .UseEditor<SelectEditor>(options =>
                                options.UseItems<FixedChoiceItemsProvider>()
                            );
                    })
                ),
            dashboard => dashboard.Services.AddScoped<FixedChoiceItemsProvider>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement("select[name='Entity.Name'] option[value='notebook']")
                    .TextContent
            )
            .IsEqualTo("Notebook");
    }

    [Test]
    public async Task SelectEditor_UsesSnapshotOfConfiguredChoices()
    {
        // Arrange
        var choices = new List<SelectListItem> { new("Notebook", "notebook") };
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate(create =>
                    create.Fields(fields =>
                    {
                        fields.Clear();
                        fields
                            .Add(product => product.Name)
                            .UseEditor<SelectEditor>(options => options.UseItems(choices));
                        fields.Add(product => product.Price);
                    })
                )
        );
        using var client = sut.GetTestClient();
        var firstDocument = await client.GetDocumentAsync("/stellaradmin/products/create");
        choices.Add(new("Pen", "pen"));

        // Act
        var secondDocument = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(firstDocument.QuerySelector("select[name='Entity.Name'] option[value='pen']"))
            .IsNull();
        await Assert
            .That(secondDocument.QuerySelector("select[name='Entity.Name'] option[value='pen']"))
            .IsNull();
    }

    [Test]
    public async Task CustomEditor_RendersApplicationTemplateWithTypedSettings()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate<CreateProductModel, CreateProductHandler>(create =>
                {
                    create.UseFactory(() => new(10m));
                    create.Fields(fields =>
                        fields
                            .Add(model => model.ProductName)
                            .UseEditor<ProductNameEditor>(options =>
                                options.Placeholder = "Product name"
                            )
                    );
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var input = document.RequiredElement("input[data-custom-editor='product-name']");
        await Assert.That(input.GetAttribute("name")).IsEqualTo("Entity.ProductName");
        await Assert.That(input.GetAttribute("placeholder")).IsEqualTo("Product name");
        await Assert
            .That(input.GetAttribute("data-editor-context"))
            .IsEqualTo("Product name:Development");
    }

    private static Task<Microsoft.AspNetCore.Builder.WebApplication> CreateRoleSelectionHost(
        RoleSelectionState state,
        Action<EditorClassNames>? classNames = null
    ) =>
        DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit<RoleSelectionModel, RoleSelectionEditHandler>(edit =>
                    edit.Fields(fields =>
                        fields
                            .Add(model => model.RoleIds)
                            .UseEditor<CheckboxGroupEditor>(options =>
                            {
                                options.UseItems([
                                    new SelectListItem("Auditor", "auditor"),
                                    new SelectListItem("Manager", "manager"),
                                ]);
                                classNames?.Invoke(options.ClassNames);
                            })
                    )
                );
            },
            dashboard => dashboard.Services.AddSingleton(state)
        );
}
