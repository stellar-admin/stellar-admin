using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Editors;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class InputEditorTests
{
    [Test]
    [Arguments("Booked", "text", "2026-10-01T09:30:00.0000000+02:00", null)]
    [Arguments("Boarding", "datetime-local", "2026-10-01T09:30:00.000", "any")]
    [Arguments("Departure", "date", "2026-10-01", null)]
    [Arguments("DepartureDate", "date", "2026-10-01", null)]
    [Arguments("Gate", "time", "09:30:00.000", "any")]
    public async Task DataTypeTemplate_TemporalProperty_RendersInputTypeAndFormat(
        string property,
        string expectedType,
        string expectedValue,
        string? expectedStep
    )
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(fields =>
        {
            fields.Add(model => model.Booked);
            fields.Add(model => model.Boarding);
            fields.Add(model => model.Departure);
            fields.Add(model => model.DepartureDate);
            fields.Add(model => model.Gate);
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var input = document.RequiredElement($"input[name='Entity.{property}']");
        await Assert.That(input.GetAttribute("type")).IsEqualTo(expectedType);
        await Assert.That(input.GetAttribute("value")).IsEqualTo(expectedValue);
        await Assert.That(input.GetAttribute("step")).IsEqualTo(expectedStep);
        await Assert.That(input.HasAttribute("min")).IsFalse();
    }

    [Test]
    public async Task UseEditor_TemporalSettings_RenderBoundsAndStep()
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(fields =>
        {
            fields
                .Add(model => model.Departure)
                .UseEditor<DateInputEditor>(date =>
                {
                    date.Max = new DateOnly(2026, 12, 31);
                    date.Min = new DateOnly(2026, 1, 1);
                    date.Step = 7;
                });
            fields
                .Add(model => model.Boarding)
                .UseEditor<DateTimeInputEditor>(dateTime =>
                {
                    dateTime.Min = new DateTime(2026, 1, 1, 6, 0, 0);
                    dateTime.Step = TimeSpan.FromMinutes(15);
                });
            fields
                .Add(model => model.Gate)
                .UseEditor<TimeInputEditor>(time =>
                {
                    time.Max = new TimeOnly(22, 0);
                    time.Step = TimeSpan.FromMinutes(5);
                });
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var departure = document.RequiredElement("input[name='Entity.Departure']");
        await Assert.That(departure.GetAttribute("min")).IsEqualTo("2026-01-01");
        await Assert.That(departure.GetAttribute("max")).IsEqualTo("2026-12-31");
        await Assert.That(departure.GetAttribute("step")).IsEqualTo("7");
        var boarding = document.RequiredElement("input[name='Entity.Boarding']");
        await Assert.That(boarding.GetAttribute("min")).IsEqualTo("2026-01-01T06:00:00");
        await Assert.That(boarding.GetAttribute("step")).IsEqualTo("900");
        var gate = document.RequiredElement("input[name='Entity.Gate']");
        await Assert.That(gate.GetAttribute("max")).IsEqualTo("22:00:00");
        await Assert.That(gate.GetAttribute("step")).IsEqualTo("300");
    }

    [Test]
    public async Task DataTypeTemplate_Boolean_RendersCheckboxWithFieldDescription()
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(fields =>
            fields.Add(model => model.Insured).Description = "Covers lost luggage."
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var checkbox = document.RequiredElement("input[name='Entity.Insured'][type='checkbox']");
        await Assert.That(checkbox.HasAttribute("checked")).IsTrue();
        await Assert.That(checkbox.HasAttribute("role")).IsFalse();
        await Assert
            .That(
                checkbox
                    .Closest("[data-slot='field']")!
                    .RequiredElement("[data-slot='field-description']")
                    .TextContent.Trim()
            )
            .IsEqualTo("Covers lost luggage.");
    }

    [Test]
    public async Task UseEditor_Toggle_RendersSwitch()
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(fields =>
            fields
                .Add(model => model.Insured)
                .UseEditor<ToggleEditor>(toggle => toggle.ClassNames.Control = "insured-switch")
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var input = document.RequiredElement("input[name='Entity.Insured'][type='checkbox']");
        await Assert.That(input.GetAttribute("role")).IsEqualTo("switch");
        await Assert.That(input.HasAttribute("checked")).IsTrue();
        await Assert
            .That(input.Closest("[data-slot='switch']")!.ClassList.Contains("insured-switch"))
            .IsTrue();
    }

    [Test]
    [Arguments(typeof(CheckboxEditor))]
    [Arguments(typeof(ToggleEditor))]
    public async Task UseEditor_BooleanEditorOnNullableBoolean_FailsRendering(Type editorType)
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(fields =>
        {
            var field = fields.Add(model => model.OptionalInsured);
            if (editorType == typeof(CheckboxEditor))
            {
                field.UseEditor<CheckboxEditor>();
            }
            else
            {
                field.UseEditor<ToggleEditor>();
            }
        });
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/create");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .Contains($"{editorType.Name} on OptionalInsured requires a non-nullable Boolean");
    }

    [Test]
    public async Task DataTypeTemplate_MultilineText_RendersTextarea()
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(fields =>
        {
            fields.Add(model => model.Notes);
            fields
                .Add(model => model.PromoCode)
                .UseEditor<TextareaEditor>(textarea =>
                {
                    textarea.Placeholder = "Optional";
                    textarea.Rows = 6;
                });
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var notes = document.RequiredElement("textarea[name='Entity.Notes']");
        await Assert.That(notes.TextContent.Trim()).IsEqualTo("Window seat");
        await Assert.That(notes.HasAttribute("rows")).IsFalse();
        await Assert.That(notes.HasAttribute("placeholder")).IsFalse();
        var promoCode = document.RequiredElement("textarea[name='Entity.PromoCode']");
        await Assert.That(promoCode.GetAttribute("rows")).IsEqualTo("6");
        await Assert.That(promoCode.GetAttribute("placeholder")).IsEqualTo("Optional");
        await Assert.That(promoCode.GetAttribute("style")).IsEqualTo("field-sizing: fixed");
        await Assert.That(notes.HasAttribute("style")).IsFalse();
    }

    [Test]
    public async Task UseEditor_Slider_InfersBoundsFromRange()
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(fields =>
            fields.Add(model => model.Passengers).UseEditor<SliderEditor>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var slider = document.RequiredElement("[data-slot='slider']");
        await Assert.That(slider.GetAttribute("min")).IsEqualTo("1");
        await Assert.That(slider.GetAttribute("max")).IsEqualTo("9");
        await Assert.That(slider.GetAttribute("step")).IsEqualTo("1");
        await Assert
            .That(slider.RequiredElement("input[name='Entity.Passengers']").GetAttribute("value"))
            .IsEqualTo("2");
    }

    [Test]
    public async Task UseEditor_SliderSettings_OverrideRange()
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(fields =>
            fields
                .Add(model => model.Passengers)
                .UseEditor<SliderEditor>(slider =>
                {
                    slider.Max = 8;
                    slider.Step = 2;
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var slider = document.RequiredElement("[data-slot='slider']");
        await Assert.That(slider.GetAttribute("min")).IsEqualTo("1");
        await Assert.That(slider.GetAttribute("max")).IsEqualTo("8");
        await Assert.That(slider.GetAttribute("step")).IsEqualTo("2");
    }

    [Test]
    public async Task UseEditor_OneTimeCode_InfersLengthFromStringLength()
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(fields =>
        {
            fields.Add(model => model.BookingCode).UseEditor<OneTimeCodeEditor>();
            fields
                .Add(model => model.PromoCode)
                .UseEditor<OneTimeCodeEditor>(code => code.Length = 8);
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var bookingCode = document.RequiredElement("input[name='Entity.BookingCode']");
        await Assert.That(bookingCode.GetAttribute("maxlength")).IsEqualTo("4");
        await Assert.That(bookingCode.GetAttribute("value")).IsEqualTo("1234");
        await Assert
            .That(
                document.RequiredElement("input[name='Entity.PromoCode']").GetAttribute("maxlength")
            )
            .IsEqualTo("8");
    }

    [Test]
    public async Task UseEditor_OneTimeCodeWithInvalidSubmission_MarksInputInvalid()
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(fields =>
            fields.Add(model => model.BookingCode).UseEditor<OneTimeCodeEditor>()
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.BookingCode"] = "12345";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/create", content);
        var document = await response.ReadDocumentAsync();

        // Assert
        var input = document.RequiredElement("input[name='Entity.BookingCode']");
        await Assert.That(input.GetAttribute("aria-invalid")).IsEqualTo("true");
        await Assert.That(input.GetAttribute("value")).IsEqualTo("12345");
        await Assert
            .That(
                document
                    .RequiredElement("[data-valmsg-for='Entity.BookingCode']")
                    .TextContent.Trim()
            )
            .IsNotEmpty();
    }

    [Test]
    public async Task UseEditor_SliderSubmission_BindsValue()
    {
        // Arrange
        await using var sut = await CreateInputFieldsHost(fields =>
        {
            fields.Add(model => model.BookingCode);
            fields.Add(model => model.Passengers).UseEditor<SliderEditor>();
        });
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.BookingCode"] = "12345";
        values["Entity.Passengers"] = "7";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/create", content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert
            .That(document.RequiredElement("input[name='Entity.Passengers']").GetAttribute("value"))
            .IsEqualTo("7");
    }

    private static Task<WebApplication> CreateInputFieldsHost(
        Action<ResourceFieldsBuilder<InputFieldsModel>> configureFields
    ) =>
        DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate<InputFieldsModel, InputFieldsHandler>(create =>
                {
                    create.UseFactory(() =>
                        new()
                        {
                            BookingCode = "1234",
                            Booked = new DateTimeOffset(
                                2026,
                                10,
                                1,
                                9,
                                30,
                                0,
                                TimeSpan.FromHours(2)
                            ),
                            Boarding = new DateTime(2026, 10, 1, 9, 30, 0),
                            Departure = new DateOnly(2026, 10, 1),
                            DepartureDate = new DateTime(2026, 10, 1),
                            Gate = new TimeOnly(9, 30),
                            Insured = true,
                            Notes = "Window seat",
                            Passengers = 2,
                        }
                    );
                    create.Fields(configureFields);
                })
        );
}
