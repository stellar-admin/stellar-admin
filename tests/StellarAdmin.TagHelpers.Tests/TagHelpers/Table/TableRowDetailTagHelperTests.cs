using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Table;

public class TableRowDetailTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithDefaults_RendersHiddenClosedRowAroundContent()
    {
        // Arrange
        var sut = new TableRowDetailTagHelper();

        // Act
        using var html = await RenderInTableAsync(sut, "<p>Kyoto itinerary</p>");

        // Assert
        var row = html.QuerySelector("tr[data-slot=table-row-detail]");
        await Assert.That(row?.HasAttribute("hidden")).IsTrue();
        await Assert.That(row?.GetAttribute("data-state")).IsEqualTo("closed");
        await Assert.That(row?.ClassList).Contains("sa-table-row-detail");
        await Assert.That(row?.ClassList).Contains("existing-class");
        var cell = row?.QuerySelector("td[data-slot=table-row-detail-cell]");
        await Assert.That(cell?.HasAttribute("colspan")).IsFalse();
        await Assert
            .That(cell?.QuerySelector("[data-slot=table-row-detail-content] > p")?.TextContent)
            .IsEqualTo("Kyoto itinerary");
    }

    [Test]
    public async Task ProcessAsync_WhenExpanded_RendersVisibleOpenRow()
    {
        // Arrange
        var sut = new TableRowDetailTagHelper { Expanded = true };

        // Act
        using var html = await RenderInTableAsync(sut, "<p>Kyoto itinerary</p>");

        // Assert
        var row = html.QuerySelector("tr[data-slot=table-row-detail]");
        await Assert.That(row?.HasAttribute("hidden")).IsFalse();
        await Assert.That(row?.GetAttribute("data-state")).IsEqualTo("open");
    }

    [Test]
    public async Task ProcessAsync_WithColspan_SpansCellAcrossColumns()
    {
        // Arrange
        var sut = new TableRowDetailTagHelper { Colspan = 7 };

        // Act
        using var html = await RenderInTableAsync(sut, "<p>Kyoto itinerary</p>");

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=table-row-detail-cell]")?.GetAttribute("colspan"))
            .IsEqualTo("7");
    }

    // A <tr> only parses inside a table, so the rendered row is parsed in one.
    private static async Task<IHtmlDocument> RenderInTableAsync(TagHelper sut, string content)
    {
        var parent = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString()
        );
        var row = await TagHelperRenderer.RenderChildAsync(
            sut,
            parent,
            _ => Task.FromResult(content)
        );

        return new HtmlParser().ParseDocument($"<table><tbody>{row}</tbody></table>");
    }
}
