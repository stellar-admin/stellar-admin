using AngleSharp.Dom;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Editors;
using StellarAdmin.TagHelpers;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class SliderEditorTests
{
    [Test]
    public async Task UseEditor_WithoutSettings_ShowsValueAndLabelsBounds()
    {
        // Arrange
        await using var sut = await CreateSliderFieldsHost(fields =>
            fields.Add(model => model.Passengers).UseEditor<SliderEditor>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var field = document.RequiredElement("[data-slot='field']:has([data-slot='slider'])");
        var label = field.RequiredElement("[data-slot='field-label']");
        await Assert.That(field.QuerySelector("output[data-slot='slider-value']")).IsNotNull();
        await Assert
            .That(field.RequiredElement("[role='slider']").GetAttribute("aria-labelledby"))
            .IsEqualTo(label.Id);
        await Assert.That(MarkLabels(field)).IsEqualTo("1 9");
        await Assert
            .That(field.QuerySelectorAll("[data-slot='slider-mark-tick']").Length)
            .IsEqualTo(0);
    }

    [Test]
    public async Task UseEditor_WhenShowValueIsFalse_OmitsValue()
    {
        // Arrange
        await using var sut = await CreateSliderFieldsHost(fields =>
            fields
                .Add(model => model.Passengers)
                .UseEditor<SliderEditor>(slider => slider.ShowValue = false)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert.That(document.QuerySelector("[data-slot='slider-value']")).IsNull();
        await Assert.That(document.QuerySelector("[data-slot='field-label']")).IsNotNull();
    }

    [Test]
    public async Task UseEditor_WithValueFormat_FormatsValueTextAndMarkLabels()
    {
        // Arrange
        await using var sut = await CreateSliderFieldsHost(fields =>
            fields
                .Add(model => model.Passengers)
                .UseEditor<SliderEditor>(slider => slider.ValueFormat = "{0} seats")
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var slider = document.RequiredElement("[data-slot='slider']");
        await Assert.That(slider.GetAttribute("data-value-format")).IsEqualTo("{0} seats");
        await Assert
            .That(slider.RequiredElement("[role='slider']").GetAttribute("aria-valuetext"))
            .IsEqualTo("2 seats");
        await Assert.That(MarkLabels(slider)).IsEqualTo("1 seats 9 seats");
    }

    [Test]
    public async Task UseEditor_WithMarkInterval_RendersTicks()
    {
        // Arrange
        await using var sut = await CreateSliderFieldsHost(fields =>
            fields
                .Add(model => model.Passengers)
                .UseEditor<SliderEditor>(slider =>
                {
                    slider.MarkInterval = 2;
                    slider.MarkLabels = SliderMarkLabels.None;
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var marks = document
            .QuerySelectorAll("[data-slot='slider-mark']")
            .Select(mark => mark.GetAttribute("data-value"));
        await Assert.That(string.Join(" ", marks)).IsEqualTo("1 3 5 7 9");
        await Assert
            .That(document.QuerySelectorAll("[data-slot='slider-mark-tick']").Length)
            .IsEqualTo(5);
        await Assert
            .That(document.QuerySelectorAll("[data-slot='slider-mark-label']").Length)
            .IsEqualTo(0);
    }

    [Test]
    public async Task UseEditor_WhenMarkLabelsIsNone_RendersNoMarks()
    {
        // Arrange
        await using var sut = await CreateSliderFieldsHost(fields =>
            fields
                .Add(model => model.Passengers)
                .UseEditor<SliderEditor>(slider => slider.MarkLabels = SliderMarkLabels.None)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert.That(document.QuerySelector("[data-slot='slider-marks']")).IsNull();
    }

    [Test]
    public async Task UseEditor_WithAddedMarks_ReplacesGeneratedMarks()
    {
        // Arrange
        await using var sut = await CreateSliderFieldsHost(fields =>
            fields
                .Add(model => model.Passengers)
                .UseEditor<SliderEditor>(slider =>
                {
                    slider.MarkInterval = 1;
                    slider.AddMark(1, "Solo");
                    slider.AddMark(4);
                    slider.AddMark(9, "Group");
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var marks = document
            .QuerySelectorAll("[data-slot='slider-mark']")
            .Select(mark => mark.GetAttribute("data-value"));
        await Assert.That(string.Join(" ", marks)).IsEqualTo("1 4 9");
        await Assert
            .That(document.QuerySelectorAll("[data-slot='slider-mark-tick']").Length)
            .IsEqualTo(3);
        await Assert.That(MarkLabels(document.DocumentElement)).IsEqualTo("Solo Group");
    }

    [Test]
    public async Task UseEditor_WithDescription_DescribesThumb()
    {
        // Arrange
        await using var sut = await CreateSliderFieldsHost(fields =>
            fields.Add(model => model.Passengers).UseEditor<SliderEditor>().Description =
                "Including children."
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(document.RequiredElement("[role='slider']").GetAttribute("aria-describedby"))
            .IsEqualTo("Entity_Passengers-description Entity_Passengers-error");
        await Assert
            .That(document.RequiredElement("#Entity_Passengers-description").TextContent.Trim())
            .IsEqualTo("Including children.");
    }

    [Test]
    public async Task UseEditor_WithoutDescription_DescribesThumbByError()
    {
        // Arrange
        await using var sut = await CreateSliderFieldsHost(fields =>
            fields.Add(model => model.Passengers).UseEditor<SliderEditor>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(document.RequiredElement("[role='slider']").GetAttribute("aria-describedby"))
            .IsEqualTo("Entity_Passengers-error");
        await Assert.That(document.QuerySelector("#Entity_Passengers-error")).IsNotNull();
    }

    [Test]
    public async Task UseEditor_WithInvalidSubmission_MarksThumbInvalid()
    {
        // Arrange
        await using var sut = await CreateSliderFieldsHost(fields =>
            fields.Add(model => model.Passengers).UseEditor<SliderEditor>()
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.Passengers"] = "12";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/create", content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert
            .That(document.RequiredElement("[role='slider']").GetAttribute("aria-invalid"))
            .IsEqualTo("true");
        await Assert
            .That(document.RequiredElement("#Entity_Passengers-error").TextContent.Trim())
            .IsNotEmpty();
    }

    [Test]
    public async Task UseEditor_WithClassNames_AppliesThemToParts()
    {
        // Arrange
        await using var sut = await CreateSliderFieldsHost(fields =>
            fields
                .Add(model => model.Passengers)
                .UseEditor<SliderEditor>(slider =>
                {
                    slider.ClassNames.Root = "root-class";
                    slider.ClassNames.Label = "label-class";
                    slider.ClassNames.Control = "control-class";
                    slider.ClassNames.Value = "value-class";
                    slider.ClassNames.Marks = "marks-class";
                    slider.ClassNames.Mark = "mark-class";
                    slider.ClassNames.MarkLabel = "mark-label-class";
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert.That(document.QuerySelector("[data-slot='field'].root-class")).IsNotNull();
        await Assert
            .That(document.QuerySelector("[data-slot='field-label'].label-class"))
            .IsNotNull();
        await Assert.That(document.QuerySelector("[data-slot='slider'].control-class")).IsNotNull();
        await Assert
            .That(document.QuerySelector("[data-slot='slider-value'].value-class"))
            .IsNotNull();
        await Assert
            .That(document.QuerySelector("[data-slot='slider-marks'].marks-class"))
            .IsNotNull();
        await Assert
            .That(document.QuerySelectorAll("[data-slot='slider-mark'].mark-class").Length)
            .IsEqualTo(2);
        await Assert
            .That(
                document.QuerySelectorAll("[data-slot='slider-mark-label'].mark-label-class").Length
            )
            .IsEqualTo(2);
    }

    private static string MarkLabels(IElement element) =>
        string.Join(
            " ",
            element
                .QuerySelectorAll("[data-slot='slider-mark-label']")
                .Select(label => label.TextContent.Trim())
        );

    private static Task<WebApplication> CreateSliderFieldsHost(
        Action<ResourceFieldsBuilder<InputFieldsModel>> configureFields
    ) =>
        DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate<InputFieldsModel, InputFieldsHandler>(create =>
                {
                    create.UseFactory(() => new() { Passengers = 2 });
                    create.Fields(configureFields);
                })
        );
}
