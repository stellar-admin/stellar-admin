using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Editors;
using static StellarAdmin.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.IntegrationTests.Resources;

// The grid-cells resource's Create view is overridden to render its fields as grid cells
public class GridEditorTests
{
    [Test]
    public async Task FieldsWithoutEditor_RenderGridTemplatesForTheirTypes()
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(fields =>
        {
            fields.Add(model => model.Passengers);
            fields.Add(model => model.Insured);
            fields.Add(model => model.OptionalInsured);
            fields.Add(model => model.Departure);
            fields.Add(model => model.Gate);
            fields.Add(model => model.Notes);
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/grid-cells/create");

        // Assert
        await Assert.That(document.QuerySelector("label, [data-slot='field']")).IsNull();
        var passengers = Cell(document, "Passengers").RequiredElement("input");
        await Assert.That(passengers.GetAttribute("name")).IsEqualTo("Entity.Passengers");
        await Assert.That(passengers.GetAttribute("type")).IsEqualTo("number");
        await Assert.That(passengers.GetAttribute("aria-label")).IsEqualTo("Passengers");
        await Assert.That(passengers.ClassList.Contains("sa-cell-editor")).IsTrue();
        await Assert
            .That(
                Cell(document, "Insured")
                    .RequiredElement(".sa-cell-check input[type='checkbox']")
                    .HasAttribute("checked")
            )
            .IsTrue();
        var optional = Cell(document, "OptionalInsured").RequiredElement("select");
        await Assert.That(optional.GetAttribute("data-size")).IsEqualTo("sm");
        await Assert.That(optional.ParentElement!.ClassList.Contains("sa-cell-editor")).IsTrue();
        await Assert
            .That(Cell(document, "Departure").RequiredElement("input").GetAttribute("type"))
            .IsEqualTo("date");
        await Assert
            .That(Cell(document, "Gate").RequiredElement("input").GetAttribute("type"))
            .IsEqualTo("time");
        // A multiline text property falls back to the String template, since textareas have no grid template
        await Assert
            .That(Cell(document, "Notes").RequiredElement("input").GetAttribute("name"))
            .IsEqualTo("Entity.Notes");
    }

    [Test]
    public async Task ToggleEditor_RendersSmallSwitch()
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(fields =>
            fields.Add(model => model.Insured).UseEditor<ToggleEditor>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/grid-cells/create");

        // Assert
        var input = Cell(document, "Insured").RequiredElement("input[type='checkbox']");
        await Assert.That(input.GetAttribute("role")).IsEqualTo("switch");
        await Assert.That(input.GetAttribute("aria-label")).IsEqualTo("Insured");
        await Assert
            .That(input.Closest("[data-slot='switch']")!.GetAttribute("data-size"))
            .IsEqualTo("sm");
    }

    [Test]
    public async Task DateTimeAndSegmentedControlEditors_RenderInCells()
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(fields =>
        {
            fields.Add(model => model.Booked);
            fields.Add(model => model.Boarding).UseEditor<DateTimeInputEditor>();
            fields.Add(model => model.OptionalInsured).UseEditor<SegmentedControlEditor>();
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/grid-cells/create");

        // Assert
        await Assert
            .That(Cell(document, "Booked").RequiredElement("input").GetAttribute("name"))
            .IsEqualTo("Entity.Booked");
        await Assert
            .That(Cell(document, "Boarding").RequiredElement("input").GetAttribute("type"))
            .IsEqualTo("datetime-local");
        var segmented = Cell(document, "OptionalInsured")
            .RequiredElement("[data-slot='segmented-control']");
        await Assert.That(segmented.GetAttribute("aria-label")).IsEqualTo("OptionalInsured");
        await Assert
            .That(segmented.QuerySelectorAll("input[type='radio']").Length)
            .IsGreaterThan(1);
    }

    [Test]
    [Arguments("Development")]
    [Arguments("Production")]
    public async Task EditorWithoutGridTemplate_ShowsErrorOnlyInDevelopment(string environment)
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(
            fields => fields.Add(model => model.Notes).UseEditor<TextareaEditor>(),
            environment
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/grid-cells/create");

        // Assert
        var cell = Cell(document, "Notes");
        if (environment == "Development")
        {
            await Assert
                .That(cell.RequiredElement("[data-slot='cell-editor-missing']").TextContent)
                .Contains("GridEditorTemplates/Editors/Textarea");
        }
        else
        {
            await Assert.That(cell.InnerHtml.Trim()).IsEmpty();
        }
    }

    [Test]
    public async Task InvalidValue_ShowsErrorUnderCell()
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(fields =>
            fields.Add(model => model.Passengers)
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/grid-cells/create");
        values["Entity.Passengers"] = "12";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/grid-cells/create", content);

        // Assert
        var document = await response.ReadDocumentAsync();
        var cell = Cell(document, "Passengers");
        var input = cell.RequiredElement("input");
        var error = cell.RequiredElement(".sa-cell-error");
        await Assert.That(input.GetAttribute("aria-invalid")).IsEqualTo("true");
        await Assert.That(input.GetAttribute("value")).IsEqualTo("12");
        await Assert.That(error.Id).IsEqualTo("Entity_Passengers-error");
        await Assert.That(input.GetAttribute("aria-describedby")).IsEqualTo(error.Id);
    }

    [Test]
    public async Task LookupSheetEditor_RendersTriggerThatOpensTheFormsSheet()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                AddGridCells(
                    admin,
                    resource =>
                        resource.AllowCreate<LookupFieldsModel, LookupFieldsHandler>(create =>
                        {
                            create.UseFactory(() => new() { CategoryId = 2 });
                            create.Fields(fields =>
                                fields
                                    .Add(model => model.CategoryId)
                                    .UseEditor<LookupSheetEditor>(lookup =>
                                        lookup.UseItems<CategoryLookupSource, Category, int>(
                                            category => category.Id,
                                            category => category.Name
                                        )
                                    )
                            );
                        })
                ),
            configureServices: services =>
                services.AddSingleton<CategoryStore>().AddScoped<CategoryLookupSource>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/grid-cells/create");

        // Assert
        var cell = Cell(document, "CategoryId");
        await Assert
            .That(cell.RequiredElement("[data-lookup='value']").GetAttribute("value"))
            .IsEqualTo("2");
        var selected = cell.RequiredElement("[data-lookup='selected']");
        await Assert.That(selected.HasAttribute("hidden")).IsFalse();
        await Assert
            .That(selected.RequiredElement("[data-lookup='title']").TextContent)
            .IsEqualTo("Notebooks");
        var open = selected.RequiredElement("[data-lookup='open']");
        await Assert.That(open.ClassList.Contains("sa-lookup-trigger")).IsTrue();
        await Assert
            .That(open.GetAttribute("data-sheet-open"))
            .Contains("form=create&field=CategoryId");
        await Assert
            .That(cell.RequiredElement("[data-lookup='empty']").HasAttribute("hidden"))
            .IsTrue();
    }

    private static void AddGridCells(
        StellarAdminBuilder admin,
        Action<ResourceBuilder<InventoryItem>> configure
    )
    {
        admin.Services.AddSingleton(new List<InventoryItem>());
        admin.AddResource<InventoryItem>(
            "grid-cells",
            resource =>
            {
                resource.UseDataSource<InventoryItemDataSource>();
                resource.UseKey(item => item.Sku);
                resource.Index(index => index.Columns(columns => columns.Add(item => item.Name)));
                configure(resource);
            }
        );
    }

    private static AngleSharp.Dom.IElement Cell(
        AngleSharp.Dom.IParentNode document,
        string field
    ) => document.RequiredElement($"td[data-field='{field}']");

    private static Task<WebApplication> CreateInputFieldsHost(
        Action<ResourceFieldsBuilder<InputFieldsModel>> configureFields,
        string environment = "Development"
    ) =>
        StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                AddGridCells(
                    admin,
                    resource =>
                        resource.AllowCreate<InputFieldsModel, InputFieldsHandler>(create =>
                        {
                            create.UseFactory(() =>
                                new()
                                {
                                    Departure = new DateOnly(2026, 10, 1),
                                    Insured = true,
                                    Passengers = 2,
                                }
                            );
                            create.Fields(configureFields);
                        })
                ),
            environmentName: environment
        );
}
