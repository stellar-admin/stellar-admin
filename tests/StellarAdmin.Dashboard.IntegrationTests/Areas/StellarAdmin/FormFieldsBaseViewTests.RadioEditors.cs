using System.Net;
using System.Text.Json;
using StellarAdmin.Dashboard.Testing;

namespace StellarAdmin.Dashboard.IntegrationTests.Areas.StellarAdmin;

public partial class FormFieldsBaseViewTests
{
    [Test]
    [Arguments("EnumRadioGroup")]
    [Arguments("EnumRadioChoiceCards")]
    public async Task ExecuteAsync_WhenFlagsUseCommonOptions_FallbackPreservesControlClass(
        string template
    )
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var html = await app.GetHtmlAsync(
            $"/test-editors/radio?template={template}&typed=false&flags=true"
        );

        // Assert
        await Assert.That(html.QuerySelector("input.radio-options")).IsNotNull();
    }

    [Test]
    [Arguments("EnumRadioGroup")]
    [Arguments("EnumRadioChoiceCards")]
    public async Task ExecuteAsync_WhenRadioEditorHasOnlyCommonOptions_PreservesClasses(
        string template
    )
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var html = await app.GetHtmlAsync(
            $"/test-editors/radio?template={template}&typed=false"
        );

        // Assert
        await Assert.That(html.QuerySelector(".radio-root")).IsNotNull();
        await Assert.That(html.QuerySelector(".radio-options")).IsNotNull();
    }

    [Test]
    [Arguments("EnumRadioGroup")]
    [Arguments("EnumRadioChoiceCards")]
    public async Task ExecuteAsync_WhenRadioEditorHasTypedOptions_AppliesAllPartClasses(
        string template
    )
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var html = await app.GetHtmlAsync($"/test-editors/radio?template={template}");

        // Assert
        await Assert.That(html.QuerySelector("fieldset.radio-root")).IsNotNull();
        await Assert.That(html.QuerySelector("div.radio-options")).IsNotNull();
        await Assert.That(html.QuerySelector("legend.radio-label")).IsNotNull();
        await Assert.That(html.QuerySelector("p.radio-description")).IsNotNull();
        await Assert.That(html.QuerySelector("div.radio-error")).IsNotNull();
        var card = template == "EnumRadioChoiceCards";
        await Assert
            .That(html.QuerySelector(card ? "label.option-root" : "div.option-root"))
            .IsNotNull();
        await Assert
            .That(html.QuerySelector(card ? "div.option-label" : "label.option-label"))
            .IsNotNull();
        await Assert.That(html.QuerySelectorAll("span.option-control").Length).IsEqualTo(3);
        await Assert.That(html.QuerySelector("div.option-content")).IsNotNull();
        await Assert.That(html.QuerySelector("p.option-description")).IsNotNull();
        await Assert.That(html.Body!.TextContent).Contains("Not set");
        await Assert.That(html.Body.TextContent).Contains("Available for purchase.");
        await Assert.That(html.QuerySelectorAll(".radio-error").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelector(".radio-error")!.TextContent.Trim())
            .IsEqualTo("Choose a condition.");
    }

    [Test]
    [Arguments("/test-editors/radio?template=Enum")]
    [Arguments("/test-editors/radio?flags=true")]
    public async Task ExecuteAsync_WhenTypedOptionsAreIncompatible_ReportsConfigurationError(
        string url
    )
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var response = await app.Client.GetAsync(url);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .Contains("RadioEditorOptions");
    }
}
