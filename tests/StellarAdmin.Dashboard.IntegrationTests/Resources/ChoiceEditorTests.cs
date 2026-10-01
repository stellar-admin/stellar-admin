using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Editors;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ChoiceEditorTests
{
    [Test]
    public async Task EnumDataTypeTemplate_RendersSelectWithMemberChoices()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
        {
            fields.Add(model => model.Cabin);
            fields.Add(model => model.OptionalCabin);
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var cabin = document.RequiredElement("select[name='Entity.Cabin']");
        await Assert
            .That(OptionValues(cabin))
            .IsEquivalentTo(["Economy", "PremiumEconomy", "Business"]);
        await Assert
            .That(cabin.QuerySelector("option[value='PremiumEconomy']")!.TextContent)
            .IsEqualTo("Premium economy");
        await Assert
            .That(cabin.QuerySelector("option[selected]")!.GetAttribute("value"))
            .IsEqualTo("Business");
        var optionalCabin = document.RequiredElement("select[name='Entity.OptionalCabin']");
        await Assert
            .That(OptionValues(optionalCabin))
            .IsEquivalentTo(["", "Economy", "PremiumEconomy", "Business"]);
        await Assert
            .That(optionalCabin.QuerySelector("option[value='']")!.TextContent)
            .IsEqualTo("Not set");
    }

    [Test]
    public async Task FlagsEnumDataTypeTemplate_RendersTextInput()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields.Add(model => model.Extras)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var input = document.RequiredElement("input[name='Entity.Extras']");
        await Assert.That(input.GetAttribute("type")).IsEqualTo("text");
    }

    [Test]
    public async Task NullableBooleanDataTypeTemplate_RendersYesNoSelect()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields.Add(model => model.Insured)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var select = document.RequiredElement("select[name='Entity.Insured']");
        await Assert.That(OptionValues(select)).IsEquivalentTo(["", "true", "false"]);
        await Assert
            .That(select.QuerySelectorAll("option").Select(option => option.TextContent))
            .IsEquivalentTo(["Not set", "Yes", "No"]);
    }

    [Test]
    public async Task SelectEditor_EmptyChoiceText_ReplacesNotSet()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields
                .Add(model => model.OptionalCabin)
                .UseEditor<SelectEditor>(select => select.EmptyChoiceText = "Any cabin")
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement("select[name='Entity.OptionalCabin'] option[value='']")
                    .TextContent
            )
            .IsEqualTo("Any cabin");
    }

    [Test]
    [Arguments(RadioGroupAppearance.Default)]
    [Arguments(RadioGroupAppearance.Cards)]
    public async Task RadioGroupEditor_EnumProperty_RendersChoicesWithDescriptions(
        RadioGroupAppearance appearance
    )
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields
                .Add(model => model.Cabin)
                .UseEditor<RadioGroupEditor>(radio => radio.Appearance = appearance)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var radios = document.QuerySelectorAll("input[type='radio'][name='Entity.Cabin']");
        await Assert
            .That(radios.Select(radio => radio.GetAttribute("value")))
            .IsEquivalentTo(["Economy", "PremiumEconomy", "Business"]);
        await Assert
            .That(document.RequiredElement("#Entity_Cabin_Business").HasAttribute("checked"))
            .IsTrue();
        var premium = document
            .RequiredElement("label[for='Entity_Cabin_PremiumEconomy']")
            .Closest(appearance == RadioGroupAppearance.Cards ? "label" : "[data-slot='field']")!;
        await Assert
            .That(premium.RequiredElement("[data-slot='field-description']").TextContent.Trim())
            .IsEqualTo("Extra legroom.");
        // Cards wrap the whole choice, radio included, in its label
        await Assert
            .That(document.RequiredElement("#Entity_Cabin_Economy").Closest("label") is not null)
            .IsEqualTo(appearance == RadioGroupAppearance.Cards);
    }

    [Test]
    public async Task RadioGroupEditor_NullNullableEnum_ChecksEmptyChoice()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields.Add(model => model.OptionalCabin).UseEditor<RadioGroupEditor>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var radios = document.QuerySelectorAll("input[type='radio'][name='Entity.OptionalCabin']");
        await Assert
            .That(
                radios
                    .Where(radio => radio.HasAttribute("checked"))
                    .Select(radio => radio.GetAttribute("value"))
            )
            .IsEquivalentTo([""]);
        await Assert
            .That(document.RequiredElement("label[for='Entity_OptionalCabin_']").TextContent.Trim())
            .IsEqualTo("Not set");
    }

    [Test]
    public async Task CheckboxGroupEditor_EnumCollection_RendersMemberChoices()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields.Add(model => model.Cabins).UseEditor<CheckboxGroupEditor>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var checkboxes = document.QuerySelectorAll("input[type='checkbox'][name='Entity.Cabins']");
        await Assert
            .That(checkboxes.Select(checkbox => checkbox.GetAttribute("value")))
            .IsEquivalentTo(["Economy", "PremiumEconomy", "Business"]);
        await Assert
            .That(
                checkboxes
                    .Where(checkbox => checkbox.HasAttribute("checked"))
                    .Select(checkbox => checkbox.GetAttribute("value"))
            )
            .IsEquivalentTo(["Economy", "Business"]);
        await Assert
            .That(
                document
                    .QuerySelectorAll(
                        "[data-slot='checkbox-group'] [data-slot='field-description']"
                    )
                    .Select(description => description.TextContent.Trim())
            )
            .IsEquivalentTo(["Extra legroom."]);
    }

    [Test]
    public async Task ToggleButtonsEditor_RendersSegmentedControlWithChoices()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
        {
            fields.Add(model => model.Cabin).UseEditor<ToggleButtonsEditor>();
            fields
                .Add(model => model.Seat)
                .UseEditor<ToggleButtonsEditor>(toggle =>
                {
                    toggle.ClassNames.Control = "seat-buttons";
                    toggle.UseItems([
                        new SelectListItem("Aisle", "aisle"),
                        new SelectListItem("Window", "window"),
                    ]);
                });
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var cabins = document.QuerySelectorAll("input[type='radio'][name='Entity.Cabin']");
        await Assert
            .That(cabins.Select(radio => radio.GetAttribute("value")))
            .IsEquivalentTo(["Economy", "PremiumEconomy", "Business"]);
        await Assert
            .That(cabins.Single(radio => radio.HasAttribute("checked")).GetAttribute("value"))
            .IsEqualTo("Business");
        var seats = document
            .RequiredElement("input[name='Entity.Seat']")
            .Closest("[data-slot='segmented-control']")!;
        await Assert.That(seats.ClassList.Contains("seat-buttons")).IsTrue();
        await Assert.That(seats.QuerySelectorAll("input[required]").Length).IsEqualTo(2);
    }

    [Test]
    public async Task ChoiceEditor_RejectedSubmission_RetainsPostedChoice()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
        {
            fields.Add(model => model.OptionalCabin).UseEditor<RadioGroupEditor>();
            fields.Add(model => model.Insured);
            fields.Add(model => model.Seat);
        });
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.OptionalCabin"] = "PremiumEconomy";
        values["Entity.Insured"] = "false";
        values["Entity.Seat"] = "";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/create", content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                document
                    .RequiredElement("#Entity_OptionalCabin_PremiumEconomy")
                    .HasAttribute("checked")
            )
            .IsTrue();
        await Assert
            .That(
                document
                    .RequiredElement("select[name='Entity.Insured'] option[selected]")
                    .GetAttribute("value")
            )
            .IsEqualTo("false");
    }

    [Test]
    public async Task ChoiceEditor_WithoutItemsOnStringProperty_FailsRendering()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields.Add(model => model.Seat).UseEditor<SelectEditor>()
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/create");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .Contains("SelectEditor on Seat requires UseItems");
    }

    private static IEnumerable<string?> OptionValues(AngleSharp.Dom.IElement select) =>
        select.QuerySelectorAll("option").Select(option => option.GetAttribute("value"));

    private static Task<WebApplication> CreateChoiceFieldsHost(
        Action<ResourceFieldsBuilder<ChoiceFieldsModel>> configureFields
    ) =>
        DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate<ChoiceFieldsModel, ChoiceFieldsHandler>(create =>
                {
                    create.UseFactory(() =>
                        new() { Cabin = Cabin.Business, Cabins = [Cabin.Economy, Cabin.Business] }
                    );
                    create.Fields(configureFields);
                })
        );
}
