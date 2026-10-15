using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Table;

public class TableSelectionHeadTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithDefaults_RendersHeadWithSelectAllCheckbox()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new TableSelectionHeadTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
        };

        // Act
        using var html = await TagHelperRenderer.RenderInTableRowAsync(sut);

        // Assert
        var head = html.QuerySelector("th[data-slot=table-head]");
        await Assert.That(head?.ClassList).Contains("sa-table-head");
        await Assert.That(head?.ClassList).Contains("existing-class");
        await Assert.That(head?.GetAttribute("style")).IsEqualTo("width: 3rem");
        var checkbox = head?.QuerySelector("input[type=checkbox]");
        await Assert.That(checkbox?.ClassList).Contains("sa-checkbox");
        await Assert.That(checkbox?.GetAttribute("aria-label")).IsEqualTo("Select all");
        await Assert.That(checkbox?.HasAttribute("data-row-select")).IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WithLabelAndStyle_KeepsThem()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new TableSelectionHeadTagHelper(context.Generator, context.Icons)
        {
            Label = "Select all bookings",
            ViewContext = context.ViewContext,
        };

        // Act
        using var html = await TagHelperRenderer.RenderInTableRowAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("style", "width: 4rem")]
        );

        // Assert
        var head = html.QuerySelector("th[data-slot=table-head]");
        await Assert.That(head?.GetAttribute("style")).IsEqualTo("width: 4rem");
        await Assert
            .That(head?.QuerySelector("input")?.GetAttribute("aria-label"))
            .IsEqualTo("Select all bookings");
    }
}
