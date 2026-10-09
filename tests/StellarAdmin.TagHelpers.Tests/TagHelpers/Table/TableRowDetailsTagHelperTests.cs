using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Table;

public class TableRowDetailsTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithDefaults_RendersComponentWithDefaultSettings()
    {
        // Arrange
        var sut = new TableRowDetailsTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var details = html.QuerySelector("sel-table-row-details[data-slot=table-row-details]");
        await Assert.That(details?.GetAttribute("data-expand-mode")).IsEqualTo("multiple");
        await Assert.That(details?.GetAttribute("data-emphasis")).IsEqualTo("band");
        await Assert.That(details?.GetAttribute("data-inset")).IsEqualTo("aligned");
        await Assert.That(details?.GetAttribute("data-animate")).IsEqualTo("true");
        await Assert.That(details?.GetAttribute("data-row-click")).IsEqualTo("false");
        await Assert.That(details?.ClassList).Contains("sa-table-row-details");
        await Assert.That(details?.ClassList).Contains("existing-class");
    }

    [Test]
    public async Task ProcessAsync_WithSettings_RendersSettingsAsDataAttributes()
    {
        // Arrange
        var sut = new TableRowDetailsTagHelper
        {
            ExpandMode = TableRowDetailExpandMode.Single,
            Emphasis = TableRowDetailEmphasis.Rail,
            Inset = TableRowDetailInset.Bleed,
            Animate = false,
            RowClick = true,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var details = html.QuerySelector("[data-slot=table-row-details]");
        await Assert.That(details?.GetAttribute("data-expand-mode")).IsEqualTo("single");
        await Assert.That(details?.GetAttribute("data-emphasis")).IsEqualTo("rail");
        await Assert.That(details?.GetAttribute("data-inset")).IsEqualTo("bleed");
        await Assert.That(details?.GetAttribute("data-animate")).IsEqualTo("false");
        await Assert.That(details?.GetAttribute("data-row-click")).IsEqualTo("true");
    }

    [Test]
    public async Task ProcessAsync_WithNoEmphasis_RendersNone()
    {
        // Arrange
        var sut = new TableRowDetailsTagHelper { Emphasis = TableRowDetailEmphasis.None };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(
                html.QuerySelector("[data-slot=table-row-details]")?.GetAttribute("data-emphasis")
            )
            .IsEqualTo("none");
    }
}
