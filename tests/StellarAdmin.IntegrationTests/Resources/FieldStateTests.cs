using System.Net;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;
using static StellarAdmin.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.IntegrationTests.Resources;

// Read-only fields render an editor that can't change and posts its value; disabled fields render a disabled editor
// that doesn't post. The server binds neither.
public class FieldStateTests
{
    [Test]
    public async Task ReadOnlyField_EditSubmission_KeepsStoredValue()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await CreateProductsHost(
            state,
            fields =>
            {
                fields.Add(product => product.Name).ReadOnly();
                fields.Add(product => product.Price);
            }
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/7");
        values["Entity.Name"] = "Fountain pen";
        values["Entity.Price"] = "12.00";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(state.Products[0].Name).IsEqualTo("Notebook");
        await Assert.That(state.Products[0].Price).IsEqualTo(12.00m);
    }

    [Test]
    public async Task DisabledField_EditSubmission_KeepsStoredValue()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await CreateProductsHost(
            state,
            fields =>
            {
                fields.Add(product => product.Name).Disabled();
                fields.Add(product => product.Price);
            }
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/7");
        values["Entity.Name"] = "Fountain pen";
        values["Entity.Price"] = "12.00";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(state.Products[0].Name).IsEqualTo("Notebook");
        await Assert.That(state.Products[0].Price).IsEqualTo(12.00m);
    }

    [Test]
    [Arguments(24, "Notebook")]
    [Arguments(8.50, "Fountain pen")]
    public async Task ReadOnlyPredicate_EditSubmission_DecidesAgainstStoredRecord(
        double storedPrice,
        string expectedName
    )
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", (decimal)storedPrice)]);
        await using var sut = await CreateProductsHost(
            state,
            fields =>
            {
                fields.Add(product => product.Name).ReadOnly(product => product.Price > 10);
                fields.Add(product => product.Price);
            }
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/7");
        values["Entity.Name"] = "Fountain pen";
        values["Entity.Price"] = "5.00";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(state.Products[0].Name).IsEqualTo(expectedName);
        await Assert.That(state.Products[0].Price).IsEqualTo(5.00m);
    }

    [Test]
    public async Task ReadOnlyField_CreateSubmission_KeepsFactoryValue()
    {
        // Arrange
        var state = new ProductState([]);
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowCreate(create =>
                {
                    create.UseFactory(() => new() { Price = 5.00m });
                    create.Fields(fields =>
                    {
                        fields.Add(product => product.Name);
                        fields.Add(product => product.Price).ReadOnly();
                    });
                });
            }
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client);
        values["Entity.Name"] = "Fountain pen";
        values["Entity.Price"] = "99.00";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/create", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(state.Products.Single().Name).IsEqualTo("Fountain pen");
        await Assert.That(state.Products.Single().Price).IsEqualTo(5.00m);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ReadOnlyListField_EditSubmission_KeepsStoredItems(bool fromMetadata)
    {
        // Arrange
        var store = new TourStore();
        store.Tours.Add(
            new()
            {
                Id = 7,
                Name = "Lisbon",
                Guides = ["Ana"],
                Highlights = ["Alfama"],
            }
        );
        var name = fromMetadata ? "Guides" : "Highlights";
        await using var sut = await CreateToursHost(
            store,
            fields =>
            {
                fields.Add(tour => tour.Name);
                if (fromMetadata)
                {
                    fields.Add(tour => tour.Guides);
                }
                else
                {
                    fields.Add(tour => tour.Highlights).ReadOnly();
                }
            }
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/tours/edit/7");
        values["Entity.Name"] = "Lisbon by tram";
        values[$"__sa_checkbox_group.Entity.{name}"] = "true";
        values[$"Entity.{name}[0]"] = "Belém";
        values[$"Entity.{name}[1]"] = "Sintra";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/tours/edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(store.Saved!.Name).IsEqualTo("Lisbon by tram");
        await Assert.That(store.Saved.Guides).IsEquivalentTo(["Ana"]);
        await Assert.That(store.Saved.Highlights).IsEquivalentTo(["Alfama"]);
    }

    [Test]
    [Arguments("checkbox", "input[type='checkbox'][name='Entity.Insured']")]
    [Arguments("toggle", "input[type='checkbox'][name='Entity.Insured']")]
    [Arguments("text", "input[name='Entity.PromoCode']")]
    [Arguments("textarea", "textarea[name='Entity.Notes']")]
    [Arguments("date", "input[name='Entity.Departure']")]
    [Arguments("one-time-code", "input[name='Entity.BookingCode']")]
    public async Task ReadOnlyInputField_RendersReadOnlyControl(string editor, string control)
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(fields =>
            AddInputField(fields, editor).ReadOnly()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var element = document.RequiredElement(control);
        await Assert.That(element.HasAttribute("readonly")).IsTrue();
        await Assert.That(element.HasAttribute("disabled")).IsFalse();
    }

    [Test]
    [Arguments("checkbox", "input[type='checkbox'][name='Entity.Insured']")]
    [Arguments("toggle", "input[type='checkbox'][name='Entity.Insured']")]
    [Arguments("text", "input[name='Entity.PromoCode']")]
    [Arguments("textarea", "textarea[name='Entity.Notes']")]
    [Arguments("date", "input[name='Entity.Departure']")]
    [Arguments("one-time-code", "input[name='Entity.BookingCode']")]
    public async Task DisabledInputField_RendersDisabledControl(string editor, string control)
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(fields =>
            AddInputField(fields, editor).Disabled()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var element = document.RequiredElement(control);
        await Assert.That(element.HasAttribute("disabled")).IsTrue();
        await Assert.That(element.HasAttribute("readonly")).IsFalse();
    }

    [Test]
    [Arguments("select", "select[name='Entity.Cabin']")]
    [Arguments("radio-group", "input[type='radio'][name='Entity.Cabin']")]
    [Arguments("segmented-control", "input[type='radio'][name='Entity.Cabin']")]
    [Arguments("toggle-group", "input[type='radio'][name='Entity.Cabin']")]
    [Arguments("checkbox-group", "input[type='checkbox'][name='Entity.Cabins']")]
    public async Task ReadOnlyChoiceField_RendersReadOnlyChoices(string editor, string control)
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            AddChoiceField(fields, editor).ReadOnly()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var elements = document.QuerySelectorAll(control);
        await Assert.That(elements.Length).IsGreaterThan(0);
        await Assert.That(elements.All(element => element.HasAttribute("readonly"))).IsTrue();
        await Assert.That(elements.Any(element => element.HasAttribute("disabled"))).IsFalse();
    }

    [Test]
    [Arguments("select", "select[name='Entity.Cabin']")]
    [Arguments("radio-group", "input[type='radio'][name='Entity.Cabin']")]
    [Arguments("segmented-control", "input[type='radio'][name='Entity.Cabin']")]
    [Arguments("toggle-group", "input[type='radio'][name='Entity.Cabin']")]
    [Arguments("checkbox-group", "input[type='checkbox'][name='Entity.Cabins']")]
    public async Task DisabledChoiceField_RendersDisabledChoices(string editor, string control)
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            AddChoiceField(fields, editor).Disabled()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var elements = document.QuerySelectorAll(control);
        await Assert.That(elements.Length).IsGreaterThan(0);
        await Assert.That(elements.All(element => element.HasAttribute("disabled"))).IsTrue();
        await Assert.That(elements.Any(element => element.HasAttribute("readonly"))).IsFalse();
    }

    [Test]
    public async Task ReadOnlyRadioGroup_MarksGroupReadOnly()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields.Add(model => model.Cabin).UseRadioGroupEditor().ReadOnly()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var group = document.RequiredElement("[data-slot='radio-group']");
        await Assert.That(group.GetAttribute("role")).IsEqualTo("radiogroup");
        await Assert.That(group.GetAttribute("aria-readonly")).IsEqualTo("true");
        var legend = document.RequiredElement($"#{group.GetAttribute("aria-labelledby")}");
        await Assert.That(legend.TextContent.Trim()).IsEqualTo("Cabin");
    }

    [Test]
    public async Task ReadOnlyAndDisabledField_RendersDisabledControl()
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(fields =>
            fields.Add(model => model.Insured).ReadOnly().Disabled()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var checkbox = document.RequiredElement("input[type='checkbox'][name='Entity.Insured']");
        await Assert.That(checkbox.HasAttribute("disabled")).IsTrue();
        await Assert.That(checkbox.HasAttribute("readonly")).IsFalse();
    }

    [Test]
    public async Task DisabledLookupField_DisablesItsControlsAndValue()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate<LookupFieldsModel, LookupFieldsHandler>(create =>
                {
                    create.UseFactory(() => new() { CategoryId = 2 });
                    create.Fields(fields =>
                        fields
                            .Add(model => model.CategoryId)
                            .UseLookupSheetEditor(lookup =>
                                lookup.UseSource<CategoryLookupSource>()
                            )
                            .Disabled()
                    );
                }),
            configureServices: services =>
                services.AddSingleton<CategoryStore>().AddScoped<CategoryLookupSource>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var fieldset = document.RequiredElement("admin-lookup-sheet-editor").ParentElement!;
        await Assert.That(fieldset.LocalName).IsEqualTo("fieldset");
        await Assert.That(fieldset.HasAttribute("disabled")).IsTrue();
        await Assert
            .That(fieldset.RequiredElement("[data-lookup='value']").GetAttribute("value"))
            .IsEqualTo("2");
        await Assert.That(fieldset.QuerySelector("[data-lookup='open']")).IsNotNull();
    }

    [Test]
    [Arguments(24, false)]
    [Arguments(8.50, true)]
    public async Task ReadOnlyPredicate_EditRow_ShowsDisplayValueForReadOnlyRecord(
        double price,
        bool editable
    )
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", (decimal)price)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                {
                    edit.Fields(fields =>
                    {
                        fields.Add(product => product.Name).ReadOnly(product => product.Price > 10);
                        fields.Add(product => product.Price);
                    });
                    edit.UseInlineRow();
                });
            }
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/editrow/7");

        // Assert
        var row = (await response.ReadRowsAsync()).RequiredElement("tbody > tr");
        await Assert
            .That(row.QuerySelector("input[name='rows[7].Name']") is not null)
            .IsEqualTo(editable);
        await Assert.That(row.QuerySelector("input[name='rows[7].Price']")).IsNotNull();
        await Assert.That(row.TextContent.Contains("Notebook")).IsEqualTo(!editable);
    }

    [Test]
    public async Task DisabledField_EditRow_RendersDisabledEditor()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                {
                    edit.Fields(fields =>
                    {
                        fields.Add(product => product.Name).Disabled();
                        fields.Add(product => product.Price);
                    });
                    edit.UseInlineRow();
                });
            }
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/editrow/7");

        // Assert
        var row = (await response.ReadRowsAsync()).RequiredElement("tbody > tr");
        await Assert
            .That(row.RequiredElement("input[name='rows[7].Name']").HasAttribute("disabled"))
            .IsTrue();
    }

    private static ResourceFieldBuilder<ChoiceFieldsModel> AddChoiceField(
        ResourceFieldsBuilder<ChoiceFieldsModel> fields,
        string editor
    ) =>
        editor switch
        {
            "select" => fields.Add(model => model.Cabin).UseSelectEditor(),
            "radio-group" => fields.Add(model => model.Cabin).UseRadioGroupEditor(),
            "segmented-control" => fields.Add(model => model.Cabin).UseSegmentedControlEditor(),
            "toggle-group" => fields.Add(model => model.Cabin).UseToggleGroupEditor(),
            "checkbox-group" => fields.Add(model => model.Cabins).UseCheckboxGroupEditor(),
            _ => throw new ArgumentOutOfRangeException(nameof(editor)),
        };

    private static ResourceFieldBuilder<InputFieldsModel> AddInputField(
        ResourceFieldsBuilder<InputFieldsModel> fields,
        string editor
    ) =>
        editor switch
        {
            "checkbox" => fields.Add(model => model.Insured).UseCheckboxEditor(),
            "toggle" => fields.Add(model => model.Insured).UseToggleEditor(),
            "text" => fields.Add(model => model.PromoCode),
            "textarea" => fields.Add(model => model.Notes),
            "date" => fields.Add(model => model.Departure),
            "one-time-code" => fields.Add(model => model.BookingCode).UseOneTimeCodeEditor(),
            _ => throw new ArgumentOutOfRangeException(nameof(editor)),
        };

    private static Task<WebApplication> CreateChoiceFieldsHost(
        Action<ResourceFieldsBuilder<ChoiceFieldsModel>> configureFields
    ) =>
        StellarAdminTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate<ChoiceFieldsModel, ChoiceFieldsHandler>(create =>
                    create.Fields(configureFields)
                )
        );

    private static Task<WebApplication> CreateInputFieldsHost(
        Action<ResourceFieldsBuilder<InputFieldsModel>> configureFields
    ) =>
        StellarAdminTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate<InputFieldsModel, InputFieldsHandler>(create =>
                    create.Fields(configureFields)
                )
        );

    private static Task<WebApplication> CreateProductsHost(
        ProductState state,
        Action<ResourceFieldsBuilder<Product>> configureFields
    ) =>
        StellarAdminTestHost.CreateAsync(
            state,
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit => edit.Fields(configureFields));
            }
        );

    private static Task<WebApplication> CreateToursHost(
        TourStore store,
        Action<ResourceFieldsBuilder<Tour>> configureFields
    ) =>
        StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
            {
                admin.Services.AddSingleton(store);
                admin.AddResource<Tour>(
                    "tours",
                    resource =>
                    {
                        resource.UseDataSource<TourDataSource>();
                        resource.UseKey(tour => tour.Id);
                        resource.Index(index =>
                            index.Columns(columns => columns.Add(tour => tour.Name))
                        );
                        resource.AllowEdit(builder => builder.Fields(configureFields));
                    }
                );
            }
        );
}
