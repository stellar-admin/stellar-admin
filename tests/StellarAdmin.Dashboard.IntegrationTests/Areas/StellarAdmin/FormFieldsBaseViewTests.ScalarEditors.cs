using System.Net;
using System.Text.Json;
using StellarAdmin.Dashboard.Testing;

namespace StellarAdmin.Dashboard.IntegrationTests.Areas.StellarAdmin;

public partial class FormFieldsBaseViewTests
{
    public static IEnumerable<(string Name, string Tag)> ScalarProperties() =>
        typeof(EditorTestModel)
            .GetProperties()
            .Select(property =>
                (
                    property.Name,
                    property.PropertyType == typeof(bool) ? "span"
                    : property.PropertyType == typeof(bool?)
                    || property.PropertyType.IsEnum
                    || Nullable.GetUnderlyingType(property.PropertyType)?.IsEnum == true
                        ? "div"
                    : property.Name == "Multiline" ? "textarea"
                    : "input"
                )
            );

    [Test]
    public async Task ExecuteAsync_WhenBooleanIsTrue_RendersCheckedInput()
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var html = await app.GetHtmlAsync("/test-editors");

        // Assert
        await Assert.That(html.QuerySelector("[name='Boolean']")!.HasAttribute("checked")).IsTrue();
    }

    [Test]
    public async Task ExecuteAsync_WhenDecimalIsNull_RendersEmptyValue()
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var html = await app.GetHtmlAsync("/test-editors");

        // Assert
        await Assert
            .That(html.QuerySelector("[name='NullableDecimal']")?.GetAttribute("value"))
            .IsEqualTo("");
    }

    [Test]
    public async Task ExecuteAsync_WhenFieldHasDescriptionOrContent_AppliesPartClasses()
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var html = await app.GetHtmlAsync("/test-editors?styled=true");

        // Assert
        await Assert
            .That(html.QuerySelector(".test-description-Decimal")?.TextContent)
            .Contains("A fractional amount");
        await Assert.That(html.QuerySelector(".test-content-Boolean")).IsNotNull();
    }

    [Test]
    [Arguments("ReadOnlyBoolean")]
    [Arguments("ReadOnlyEnum")]
    public async Task ExecuteAsync_WhenFieldIsReadOnly_DisablesControl(string name)
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var html = await app.GetHtmlAsync("/test-editors");

        // Assert
        await Assert
            .That(html.QuerySelector($"[name='{name}']")!.HasAttribute("disabled"))
            .IsTrue();
    }

    [Test]
    public async Task ExecuteAsync_WhenMetadataIsPresent_RendersDescriptionAndEnumLabel()
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var html = await app.GetHtmlAsync("/test-editors");

        // Assert
        await Assert.That(html.Body!.TextContent).Contains("A fractional amount");
        await Assert
            .That(html.QuerySelector("select[name=Enum] option[selected]")?.GetAttribute("value"))
            .IsEqualTo("1");
        await Assert
            .That(html.QuerySelector("select[name=Enum] option[selected]")?.TextContent)
            .IsEqualTo("Ready to sell");
    }

    [Test]
    public async Task ExecuteAsync_WhenModelStateIsInvalid_PreservesAttemptedValueAndStyledError()
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var html = await app.GetHtmlAsync("/test-editors?styled=true&invalid=true");

        // Assert
        await Assert
            .That(html.QuerySelector("[name='Decimal']")?.GetAttribute("value"))
            .IsEqualTo("bad-number");
        await Assert
            .That(html.QuerySelector("div.test-error-Decimal")?.TextContent)
            .Contains("Enter a valid number.");
    }

    [Test]
    [Arguments("true")]
    [Arguments("false")]
    public async Task ExecuteAsync_WhenNullableBooleanIsSet_SelectsValue(string value)
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var html = await app.GetHtmlAsync($"/test-editors?choice={value}");

        // Assert
        await Assert
            .That(
                html.QuerySelector("select[name=NullableBoolean] option[selected]")
                    ?.GetAttribute("value")
            )
            .IsEqualTo(value);
    }

    [Test]
    [Arguments("NullableBoolean")]
    [Arguments("NullableEnum")]
    public async Task ExecuteAsync_WhenNullableChoiceIsEmpty_RendersUnselectedEmptyOption(
        string name
    )
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var html = await app.GetHtmlAsync("/test-editors");

        // Assert
        await Assert
            .That(html.QuerySelector($"select[name='{name}'] option[value='']")?.TextContent)
            .IsEqualTo("Not set");
        await Assert.That(html.QuerySelector($"select[name='{name}'] option[selected]")).IsNull();
    }

    [Test]
    [Arguments("Byte", "1")]
    [Arguments("SByte", "1")]
    [Arguments("Int16", "1")]
    [Arguments("UInt16", "1")]
    [Arguments("Int32", "1")]
    [Arguments("UInt32", "1")]
    [Arguments("Int64", "1")]
    [Arguments("UInt64", "1")]
    [Arguments("Currency", "any")]
    [Arguments("Decimal", "any")]
    [Arguments("Single", "any")]
    [Arguments("Double", "any")]
    [Arguments("NullableDecimal", "any")]
    public async Task ExecuteAsync_WhenNumericEditorIsRendered_UsesExpectedStep(
        string name,
        string step
    )
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var html = await app.GetHtmlAsync("/test-editors");

        // Assert
        await Assert
            .That(html.QuerySelector($"input[name='{name}']")?.GetAttribute("type"))
            .IsEqualTo("number");
        await Assert
            .That(html.QuerySelector($"input[name='{name}']")?.GetAttribute("step"))
            .IsEqualTo(step);
    }

    [Test]
    public async Task ExecuteAsync_WhenPasswordHasValue_DoesNotEchoSecret()
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var html = await app.GetHtmlAsync("/test-editors");

        // Assert
        await Assert
            .That(html.QuerySelector("[name='Password']")!.OuterHtml)
            .DoesNotContain("secret");
    }

    [Test]
    [MethodDataSource(nameof(ScalarProperties))]
    public async Task ExecuteAsync_WhenScalarEditorIsRendered_HasLabelAndStyledControl(
        string name,
        string tag
    )
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var html = await app.GetHtmlAsync("/test-editors");

        // Assert
        await Assert.That(html.QuerySelector($"label[for='{name}']")).IsNotNull();
        await Assert.That(html.QuerySelector($"[name='{name}']")!.ClassName).Contains("sa-");
    }

    [Test]
    [MethodDataSource(nameof(ScalarProperties))]
    public async Task ExecuteAsync_WhenScalarEditorIsStyled_AppliesClassesToCorrectParts(
        string name,
        string tag
    )
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var html = await app.GetHtmlAsync("/test-editors?styled=true");

        // Assert
        await Assert.That(html.QuerySelector($"div.sa-field.test-editor-{name}")).IsNotNull();
        await Assert
            .That(html.QuerySelector($"[name='{name}']")!.ClassList)
            .DoesNotContain($"test-editor-{name}");
        await Assert.That(html.QuerySelector($"{tag}.test-control-{name}")).IsNotNull();
        await Assert.That(html.QuerySelector($".test-label-{name}")).IsNotNull();
    }

    [Test]
    [Arguments("Decimal", "123.456")]
    [Arguments("Currency", "123.456")]
    [Arguments("UInt64", "18446744073709551615")]
    [Arguments("DateOnly", "2026-09-09")]
    [Arguments("TimeOnly", "14:30:15")]
    [Arguments("DateTime", "2026-09-09T14:30:15")]
    [Arguments("DateTimeOffset", "+07:00")]
    public async Task ExecuteAsync_WhenScalarValueIsRendered_PreservesPrecisionAndBrowserFormat(
        string name,
        string expected
    )
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var html = await app.GetHtmlAsync("/test-editors");

        // Assert
        await Assert
            .That(html.QuerySelector($"[name='{name}']")?.GetAttribute("value"))
            .Contains(expected);
    }

    [Test]
    [Arguments("Email", "email")]
    [Arguments("Url", "url")]
    public async Task ExecuteAsync_WhenSpecializedTextEditorIsRendered_UsesInputType(
        string name,
        string type
    )
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var html = await app.GetHtmlAsync("/test-editors");

        // Assert
        await Assert
            .That(html.QuerySelector($"input[name='{name}']")?.GetAttribute("type"))
            .IsEqualTo(type);
    }
}
