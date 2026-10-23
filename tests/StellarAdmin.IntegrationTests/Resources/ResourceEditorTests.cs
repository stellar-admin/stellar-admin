using System.Net;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Editors;
using static StellarAdmin.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.IntegrationTests.Resources;

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
    public async Task CheckboxGroupEditor_FieldClassNames_StyleFieldParts()
    {
        // Arrange
        var state = new RoleSelectionState();
        await using var sut = await CreateRoleSelectionHost(
            state,
            classNames =>
            {
                classNames.Label = "role-label";
                classNames.Root = "role-root";
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        await Assert
            .That(document.QuerySelectorAll("[data-slot='field'].role-root").Length)
            .IsEqualTo(1);
        await Assert
            .That(document.QuerySelectorAll("[data-slot='field-label'].role-label").Length)
            .IsEqualTo(1);
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
            UpdateResult = StellarAdmin.Resources.ResourceOperationResult.ValidationFailed(
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
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate(create =>
                    create.Fields(fields =>
                    {
                        fields.Clear();
                        fields
                            .Add(product => product.Name)
                            .UseSelectEditor(options =>
                                options.UseItems<FixedChoiceItemsProvider>()
                            );
                    })
                ),
            admin => admin.Services.AddScoped<FixedChoiceItemsProvider>()
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
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate(create =>
                    create.Fields(fields =>
                    {
                        fields.Clear();
                        fields
                            .Add(product => product.Name)
                            .UseSelectEditor(options => options.UseItems(choices));
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
    [Arguments("create", new[] { "", "7", "8" }, null)]
    [Arguments("edit/7", new[] { "7", "8" }, "7")]
    [Arguments("edit/9", new[] { "", "7", "8" }, null)]
    public async Task SelectEditor_RequiredValueMatchingNoChoice_StartsOnEmptyChoice(
        string page,
        string[] expectedValues,
        string? expectedSelected
    )
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m), new(9, "Retired lamp", 24m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowCreate(create => create.Fields(ConfigureFields));
                resource.AllowEdit(edit => edit.Fields(ConfigureFields));
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync($"/stellaradmin/products/{page}");

        // Assert
        var select = document.RequiredElement("select[name='Entity.Id']");
        await Assert
            .That(select.QuerySelectorAll("option").Select(option => option.GetAttribute("value")))
            .IsEquivalentTo(expectedValues);
        await Assert
            .That(select.QuerySelector("option[selected]")?.GetAttribute("value"))
            .IsEqualTo(expectedSelected);

        static void ConfigureFields(ResourceFieldsBuilder<Product> fields)
        {
            fields.Clear();
            fields
                .Add(product => product.Id)
                .UseSelectEditor(options =>
                    options.UseItems([
                        new SelectListItem("Notebook", "7"),
                        new SelectListItem("Lamp", "8"),
                    ])
                );
        }
    }

    [Test]
    public async Task CustomEditor_RendersApplicationTemplateWithTypedSettings()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
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
            .IsEqualTo("Product name:Development:ProductName=");
    }

    [Test]
    public async Task CustomEditor_WithoutHandler_RendersTemplateWithoutEditorData()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate<CreateProductModel, CreateProductHandler>(create =>
                {
                    create.UseFactory(() => new(10m));
                    create.Fields(fields =>
                        fields
                            .Add(model => model.ProductName)
                            .UseEditor<ProductCodeEditor>(options => options.Prefix = "VT-")
                    );
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var input = document.RequiredElement("input[data-custom-editor='product-code']");
        await Assert.That(input.GetAttribute("name")).IsEqualTo("Entity.ProductName");
        await Assert.That(input.GetAttribute("data-prefix")).IsEqualTo("VT-");
        await Assert.That(input.GetAttribute("data-has-editor-data")).IsEqualTo("False");
    }

    [Test]
    public async Task CustomEditor_CreatePage_ReceivesFactoryValue()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate(create =>
                {
                    create.UseFactory(() => new() { Name = "Draft" });
                    create.Fields(fields =>
                    {
                        fields.Clear();
                        fields.Add(product => product.Name).UseEditor<ProductNameEditor>();
                    });
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement("input[name='Entity.Name']")
                    .GetAttribute("data-editor-context")
            )
            .IsEqualTo(":Development:Name=Draft");
    }

    [Test]
    public async Task CustomEditor_EditPage_ReceivesLoadedValue()
    {
        // Arrange
        await using var sut = await CreateProductNameEditHost();
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement("input[name='Entity.Name']")
                    .GetAttribute("data-editor-context")
            )
            .IsEqualTo(":Development:Name=Notebook");
    }

    [Test]
    public async Task CustomEditor_RejectedSubmission_ReceivesPostedValue()
    {
        // Arrange
        await using var sut = await CreateProductNameEditHost();
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/7");
        values["Entity.Name"] = "Changed";
        values["Entity.Price"] = "not a number";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/edit/7", content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                document
                    .RequiredElement("input[name='Entity.Name']")
                    .GetAttribute("data-editor-context")
            )
            .IsEqualTo(":Development:Name=Changed");
    }

    private static Task<Microsoft.AspNetCore.Builder.WebApplication> CreateProductNameEditHost() =>
        StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                    {
                        fields.Add(product => product.Name).UseEditor<ProductNameEditor>();
                        fields.Add(product => product.Price);
                    })
                );
            }
        );

    private static Task<Microsoft.AspNetCore.Builder.WebApplication> CreateRoleSelectionHost(
        RoleSelectionState state,
        Action<EditorClassNames>? classNames = null
    ) =>
        StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit<RoleSelectionModel, RoleSelectionEditHandler>(edit =>
                    edit.Fields(fields =>
                        fields
                            .Add(model => model.RoleIds)
                            .UseCheckboxGroupEditor(options =>
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
            admin => admin.Services.AddSingleton(state)
        );
}
