using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Editors;
using TUnit.Assertions.Enums;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ChoiceEditorTests
{
    private static readonly ChoiceItem[] SeatChoices =
    [
        new("aisle", "Aisle"),
        new("window", "Window"),
    ];

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
    public async Task CheckboxGroupEditor_CardsAppearance_RendersChoiceCards()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields
                .Add(model => model.Cabins)
                .UseEditor<CheckboxGroupEditor>(editor =>
                    editor.Appearance = CheckboxGroupAppearance.Cards
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var cards = document.QuerySelectorAll(
            "[data-slot='checkbox-group'] > label[data-slot='field-label'] > [data-slot='field']"
        );
        await Assert.That(cards.Length).IsEqualTo(3);
    }

    [Test]
    public async Task CheckboxGroupEditor_Columns_WritesColumnsAndRowsForDownFlow()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields
                .Add(model => model.Cabins)
                .UseEditor<CheckboxGroupEditor>(editor => editor.Columns(2))
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var group = document.RequiredElement("[data-slot='checkbox-group']");
        await Assert.That(group.ClassList.Contains("sa-choice-columns-down")).IsTrue();
        await Assert
            .That(group.GetAttribute("style"))
            .IsEqualTo(
                "--sa-cols-md:2;--sa-cols-lg:2;--sa-rows:3;--sa-rows-sm:3;--sa-rows-md:2;--sa-rows-lg:2"
            );
    }

    [Test]
    public async Task CheckboxGroupEditor_ColumnsAcross_WritesOnlyColumns()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields
                .Add(model => model.Cabins)
                .UseEditor<CheckboxGroupEditor>(editor =>
                {
                    editor.Columns(columns => columns.Small(2).Large(3));
                    editor.Flow = CheckboxGroupFlow.Across;
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var group = document.RequiredElement("[data-slot='checkbox-group']");
        await Assert.That(group.ClassList.Contains("sa-choice-columns")).IsTrue();
        await Assert.That(group.ClassList.Contains("sa-choice-columns-down")).IsFalse();
        await Assert
            .That(group.GetAttribute("style"))
            .IsEqualTo("--sa-cols-sm:2;--sa-cols-md:2;--sa-cols-lg:3");
    }

    [Test]
    public async Task CheckboxGroupEditor_WithoutColumns_LeavesOutColumnAttributes()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields.Add(model => model.Cabins).UseEditor<CheckboxGroupEditor>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var group = document.RequiredElement("[data-slot='checkbox-group']");
        await Assert.That(group.ClassList.Contains("sa-choice-columns")).IsFalse();
        await Assert.That(group.HasAttribute("style")).IsFalse();
    }

    [Test]
    public async Task SegmentedControlEditor_RendersSegmentedControlWithChoices()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
        {
            fields.Add(model => model.Cabin).UseEditor<SegmentedControlEditor>();
            fields
                .Add(model => model.Seat)
                .UseEditor<SegmentedControlEditor>(toggle =>
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
    public async Task ToggleGroupEditor_CollectionProperty_RendersMultipleChips()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields.Add(model => model.Cabins).UseEditor<ToggleGroupEditor>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var group = document.RequiredElement("[data-slot='toggle-group']");
        await Assert.That(group.ClassList.Contains("sa-toggle-chips")).IsTrue();
        var checkboxes = group.QuerySelectorAll("input[type='checkbox'][name='Entity.Cabins']");
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
        await Assert.That(group.QuerySelectorAll(".sa-toggle-chip-check").Length).IsEqualTo(3);
        await Assert
            .That(group.QuerySelector("input[type='hidden']")?.GetAttribute("name"))
            .IsEqualTo("__sa_checkbox_group.Entity.Cabins");
    }

    [Test]
    public async Task ToggleGroupEditor_JoinedAppearance_RendersSingleJoinedGroup()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields
                .Add(model => model.Cabin)
                .UseEditor<ToggleGroupEditor>(editor =>
                    editor.Appearance = ToggleGroupAppearance.Joined
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var group = document.RequiredElement("[data-slot='toggle-group']");
        await Assert.That(group.GetAttribute("data-spacing")).IsEqualTo("0");
        await Assert.That(group.ClassList.Contains("sa-toggle-group-wrap")).IsFalse();
        var radios = group.QuerySelectorAll("input[type='radio'][name='Entity.Cabin']");
        await Assert.That(radios.Length).IsEqualTo(3);
        await Assert
            .That(radios.Single(radio => radio.HasAttribute("checked")).GetAttribute("value"))
            .IsEqualTo("Business");
        await Assert.That(group.QuerySelectorAll("input[type='hidden']").Length).IsEqualTo(0);
    }

    [Test]
    public async Task ToggleGroupEditor_NullNullableBoolean_SelectsEmptyChoice()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields
                .Add(model => model.Insured)
                .UseEditor<ToggleGroupEditor>(editor =>
                    editor.Appearance = ToggleGroupAppearance.Buttons
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var group = document.RequiredElement("[data-slot='toggle-group']");
        await Assert.That(group.ClassList.Contains("sa-toggle-group-wrap")).IsTrue();
        await Assert.That(group.ClassList.Contains("sa-toggle-chips")).IsFalse();
        var radios = group.QuerySelectorAll("input[type='radio'][name='Entity.Insured']");
        await Assert
            .That(radios.Select(radio => radio.GetAttribute("value")))
            .IsEquivalentTo(["", "true", "false"]);
        await Assert
            .That(radios.Single(radio => radio.HasAttribute("checked")).GetAttribute("value"))
            .IsEqualTo("");
    }

    [Test]
    public async Task ToggleGroupEditor_RejectedSubmissionWithNoChips_KeepsEmptySelection()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
        {
            fields.Add(model => model.Cabins).UseEditor<ToggleGroupEditor>();
            fields.Add(model => model.Seat);
        });
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        // Turning every chip off posts only the marker the group renders
        values["__sa_checkbox_group.Entity.Cabins"] = "true";
        values["Entity.Seat"] = "";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/create", content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.QuerySelectorAll("input[name='Entity.Cabins'][checked]").Length)
            .IsEqualTo(0);
    }

    [Test]
    public async Task SelectEditor_GroupedChoices_RendersOptionGroups()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields
                .Add(model => model.Seat)
                .UseEditor<SelectEditor>(select =>
                    select.UseItems([
                        new ChoiceItem("1a", "1A") { Group = new("Front") },
                        new ChoiceItem("any", "Any seat"),
                        new ChoiceItem("30c", "30C") { Group = new("Back") { Disabled = true } },
                        new ChoiceItem("2b", "2B") { Group = new("Front") },
                    ])
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var select = document.RequiredElement("select[name='Entity.Seat']");
        await Assert
            .That(OptionValues(select))
            .IsEquivalentTo(["", "1a", "2b", "any", "30c"], CollectionOrdering.Matching);
        await Assert
            .That(select.QuerySelectorAll("optgroup").Select(group => group.GetAttribute("label")))
            .IsEquivalentTo(["Front", "Back"], CollectionOrdering.Matching);
        await Assert
            .That(select.QuerySelectorAll("optgroup[label='Front'] option").Length)
            .IsEqualTo(2);
        await Assert
            .That(select.RequiredElement("optgroup[label='Back']").HasAttribute("disabled"))
            .IsTrue();
    }

    [Test]
    public async Task SelectEditor_EnumGroupNames_RendersOptionGroups()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields.Add(model => model.Meal)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var select = document.RequiredElement("select[name='Entity.Meal']");
        await Assert
            .That(
                select
                    .QuerySelectorAll("optgroup[label='Hot'] option")
                    .Select(option => option.GetAttribute("value"))
            )
            .IsEquivalentTo(["Pasta", "Curry"], CollectionOrdering.Matching);
        await Assert
            .That(select.QuerySelectorAll("optgroup[label='Cold'] option").Length)
            .IsEqualTo(1);
    }

    [Test]
    public async Task RadioGroupEditor_SuppliedDescriptions_RendersDescriptions()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields
                .Add(model => model.Seat)
                .UseEditor<RadioGroupEditor>(radio =>
                    radio.UseItems([
                        new ChoiceItem("aisle", "Aisle") { Description = "Easy to stretch." },
                        new ChoiceItem("window", "Window"),
                    ])
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(
                document
                    .QuerySelectorAll("[data-slot='field-description']")
                    .Select(description => description.TextContent.Trim())
            )
            .Contains("Easy to stretch.");
        await Assert
            .That(document.QuerySelectorAll("input[type=radio][name='Entity.Seat']").Length)
            .IsEqualTo(2);
    }

    [Test]
    public async Task SelectEditor_RequiredUnsetValue_AddsEmptyChoice()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields
                .Add(model => model.Seat)
                .UseEditor<SelectEditor>(select => select.UseItems(SeatChoices))
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var select = document.RequiredElement("select[name='Entity.Seat']");
        await Assert
            .That(OptionValues(select))
            .IsEquivalentTo(["", "aisle", "window"], CollectionOrdering.Matching);
        await Assert
            .That(select.RequiredElement("option[value='']").TextContent)
            .IsEqualTo("Not set");
    }

    [Test]
    public async Task SelectEditor_RequiredValueSet_LeavesOutEmptyChoice()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
        {
            fields
                .Add(model => model.Seat)
                .UseEditor<SelectEditor>(select => select.UseItems(SeatChoices));
            fields.Add(model => model.RequiredCabin);
        });
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.Seat"] = "aisle";
        values["Entity.RequiredCabin"] = "";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/create", content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(OptionValues(document.RequiredElement("select[name='Entity.Seat']")))
            .IsEquivalentTo(["aisle", "window"], CollectionOrdering.Matching);
        await Assert
            .That(
                document
                    .RequiredElement("select[name='Entity.RequiredCabin'] option[selected]")
                    .GetAttribute("value")
            )
            .IsEqualTo("");
    }

    [Test]
    public async Task RadioGroupEditor_RequiredNullableEnum_LeavesOutEmptyChoice()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields.Add(model => model.RequiredCabin).UseEditor<RadioGroupEditor>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var radios = document.QuerySelectorAll("input[type=radio][name='Entity.RequiredCabin']");
        await Assert.That(radios.Length).IsEqualTo(3);
        await Assert.That(radios.Any(radio => radio.HasAttribute("checked"))).IsFalse();
    }

    [Test]
    public async Task RadioGroupEditor_OptionalSuppliedChoices_ChecksAddedEmptyChoice()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields
                .Add(model => model.Preference)
                .UseEditor<RadioGroupEditor>(radio =>
                {
                    radio.UseItems(SeatChoices);
                    radio.EmptyChoiceText = "No preference";
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var empty = document.RequiredElement(
            "input[type=radio][name='Entity.Preference'][value='']"
        );
        await Assert.That(empty.HasAttribute("checked")).IsTrue();
        await Assert
            .That(document.RequiredElement($"label[for='{empty.Id}']").TextContent.Trim())
            .IsEqualTo("No preference");
    }

    [Test]
    public async Task SelectEditor_SuppliedEmptyChoice_IsNotDuplicated()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields
                .Add(model => model.Preference)
                .UseEditor<SelectEditor>(select =>
                    select.UseItems([new ChoiceItem("", "Any seat"), .. SeatChoices])
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var select = document.RequiredElement("select[name='Entity.Preference']");
        await Assert
            .That(OptionValues(select))
            .IsEquivalentTo(["", "aisle", "window"], CollectionOrdering.Matching);
        await Assert
            .That(select.RequiredElement("option[value='']").TextContent)
            .IsEqualTo("Any seat");
    }

    [Test]
    public async Task ChoiceEditor_EmptyChoiceSetting_OverridesAuto()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
        {
            fields
                .Add(model => model.RequiredCabin)
                .UseEditor<RadioGroupEditor>(radio => radio.EmptyChoice = EmptyChoice.Include);
            fields
                .Add(model => model.OptionalCabin)
                .UseEditor<SelectEditor>(select => select.EmptyChoice = EmptyChoice.Omit);
            fields
                .Add(model => model.Cabins)
                .UseEditor<CheckboxGroupEditor>(group => group.EmptyChoice = EmptyChoice.Include);
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(document.QuerySelector("input[name='Entity.RequiredCabin'][value='']"))
            .IsNotNull();
        await Assert
            .That(document.QuerySelector("select[name='Entity.OptionalCabin'] option[value='']"))
            .IsNull();
        await Assert.That(document.QuerySelector("input[name='Entity.Cabins'][value='']")).IsNull();
    }

    [Test]
    public async Task SegmentedControlEditor_NullNullableEnum_SelectsEmptyChoice()
    {
        // Arrange
        await using var sut = await CreateChoiceFieldsHost(fields =>
            fields.Add(model => model.OptionalCabin).UseEditor<SegmentedControlEditor>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement("input[name='Entity.OptionalCabin']:checked")
                    .GetAttribute("value")
            )
            .IsEqualTo("");
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
