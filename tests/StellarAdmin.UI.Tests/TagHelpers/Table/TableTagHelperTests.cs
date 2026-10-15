using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Table;

public class TableTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithDefaults_RendersContainerWithoutComponent()
    {
        // Arrange
        var sut = new TableTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("id", "bookings")]
        );

        // Assert
        await Assert.That(html.QuerySelector("sel-table")).IsNull();
        var container = html.QuerySelector("div[data-slot=table-container]");
        await Assert.That(container?.Id).IsEqualTo("bookings");
        await Assert
            .That(container?.QuerySelector("table")?.ClassName)
            .IsEqualTo("sa-table existing-class");
    }

    [Test]
    public async Task ProcessAsync_WithSelection_RendersComponentWithSelection()
    {
        // Arrange
        var sut = new TableTagHelper { Selection = true };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("id", "booking-selection")]
        );

        // Assert
        var table = html.QuerySelector("sel-table[data-slot=table-interactivity]");
        await Assert.That(table?.Id).IsEqualTo("booking-selection");
        await Assert.That(table?.HasAttribute("data-selection")).IsTrue();
        await Assert.That(table?.HasAttribute("data-row-details")).IsFalse();
        await Assert.That(table?.HasAttribute("class")).IsFalse();
        await Assert
            .That(html.QuerySelector("sel-table > [data-slot=table-container] > table")?.ClassName)
            .IsEqualTo("sa-table existing-class");
    }

    [Test]
    public async Task ProcessAsync_WithRowDetails_RendersComponentWithDefaultSettings()
    {
        // Arrange
        var sut = new TableTagHelper { RowDetails = true };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var table = html.QuerySelector("sel-table[data-row-details]");
        await Assert.That(table?.HasAttribute("data-selection")).IsFalse();
        await Assert.That(table?.GetAttribute("data-expand-mode")).IsEqualTo("multiple");
        await Assert.That(table?.GetAttribute("data-emphasis")).IsEqualTo("band");
        await Assert.That(table?.GetAttribute("data-divider")).IsEqualTo("inset");
        await Assert.That(table?.GetAttribute("data-inset")).IsEqualTo("aligned");
        await Assert.That(table?.GetAttribute("data-animate")).IsEqualTo("true");
        await Assert.That(table?.GetAttribute("data-row-click")).IsEqualTo("false");
        await Assert.That(table?.ClassName).IsEqualTo("sa-table-row-details");
    }

    [Test]
    public async Task ProcessAsync_WithRowDetailSettings_RendersSettingsAsDataAttributes()
    {
        // Arrange
        var sut = new TableTagHelper
        {
            RowDetails = true,
            RowClick = true,
            RowDetailAnimate = false,
            RowDetailDivider = TableRowDetailDivider.FullWidth,
            RowDetailEmphasis = TableRowDetailEmphasis.Rail,
            RowDetailExpandMode = TableRowDetailExpandMode.Single,
            RowDetailInset = TableRowDetailInset.Bleed,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var table = html.QuerySelector("sel-table[data-row-details]");
        await Assert.That(table?.GetAttribute("data-expand-mode")).IsEqualTo("single");
        await Assert.That(table?.GetAttribute("data-emphasis")).IsEqualTo("rail");
        await Assert.That(table?.GetAttribute("data-divider")).IsEqualTo("full-width");
        await Assert.That(table?.GetAttribute("data-inset")).IsEqualTo("bleed");
        await Assert.That(table?.GetAttribute("data-animate")).IsEqualTo("false");
        await Assert.That(table?.GetAttribute("data-row-click")).IsEqualTo("true");
    }

    [Test]
    public async Task ProcessAsync_WithNoEmphasisAndNoDivider_RendersNone()
    {
        // Arrange
        var sut = new TableTagHelper
        {
            RowDetails = true,
            RowDetailDivider = TableRowDetailDivider.None,
            RowDetailEmphasis = TableRowDetailEmphasis.None,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var table = html.QuerySelector("sel-table[data-row-details]");
        await Assert.That(table?.GetAttribute("data-emphasis")).IsEqualTo("none");
        await Assert.That(table?.GetAttribute("data-divider")).IsEqualTo("none");
    }

    [Test]
    public async Task ProcessAsync_WithSelectionAndRowDetails_RendersOneComponentWithBoth()
    {
        // Arrange
        var sut = new TableTagHelper { Selection = true, RowDetails = true };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll("sel-table").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelector("sel-table[data-selection][data-row-details]"))
            .IsNotNull();
    }
}
