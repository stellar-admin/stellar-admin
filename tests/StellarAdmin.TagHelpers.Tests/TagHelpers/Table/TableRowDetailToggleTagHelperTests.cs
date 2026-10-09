using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Table;

public class TableRowDetailToggleTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithoutContent_RendersGhostIconButtonWithChevron()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new TableRowDetailToggleTagHelper(context.Icons);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var button = html.QuerySelector("button[data-slot=table-row-detail-toggle]");
        await Assert.That(button?.GetAttribute("type")).IsEqualTo("button");
        await Assert.That(button?.GetAttribute("aria-label")).IsEqualTo("Toggle row details");
        await Assert.That(button?.GetAttribute("aria-expanded")).IsEqualTo("false");
        await Assert.That(button?.HasAttribute("data-all")).IsFalse();
        await Assert.That(button?.ClassList).Contains("sa-table-row-detail-toggle");
        await Assert.That(button?.ClassList).Contains("sa-button-variant-ghost");
        await Assert.That(button?.ClassList).Contains("sa-button-size-icon-sm");
        await Assert.That(button?.ClassList).Contains("existing-class");
        await Assert.That(button?.QuerySelector("svg")).IsNotNull();
    }

    [Test]
    public async Task ProcessAsync_WithAll_RendersToggleForEveryRow()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new TableRowDetailToggleTagHelper(context.Icons) { All = true };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var button = html.QuerySelector("[data-slot=table-row-detail-toggle]");
        await Assert.That(button?.GetAttribute("data-all")).IsEqualTo("true");
        await Assert.That(button?.GetAttribute("aria-label")).IsEqualTo("Toggle all row details");
    }

    [Test]
    public async Task ProcessAsync_WithAriaLabel_KeepsAuthorLabel()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new TableRowDetailToggleTagHelper(context.Icons);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("aria-label", "Show booking VG-24817")]
        );

        // Assert
        await Assert
            .That(
                html.QuerySelector("[data-slot=table-row-detail-toggle]")
                    ?.GetAttribute("aria-label")
            )
            .IsEqualTo("Show booking VG-24817");
    }

    [Test]
    public async Task ProcessAsync_WithContent_RendersContentInsteadOfIcon()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new TableRowDetailToggleTagHelper(context.Icons);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, _ => Task.FromResult("<i>x</i>"));

        // Assert
        var button = html.QuerySelector("[data-slot=table-row-detail-toggle]");
        await Assert.That(button?.QuerySelector("i")).IsNotNull();
        await Assert.That(button?.QuerySelector("svg")).IsNull();
    }
}
