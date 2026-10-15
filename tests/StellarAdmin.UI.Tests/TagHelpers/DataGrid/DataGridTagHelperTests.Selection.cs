using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.DataGrid;

public partial class DataGridTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithSelection_RendersComponentWithMarkedRowCheckboxes()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        var selection = new DataGridSelectionTagHelper { KeyField = "Id", Name = "selected" };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            gridContext =>
                TagHelperRenderer.RenderChildAsync(
                    selection,
                    gridContext,
                    outputAttributes: [new TagHelperAttribute("id", "booking-selection")]
                )
        );

        // Assert
        var table = html.QuerySelector("sel-table[data-slot=table-interactivity]");
        await Assert.That(table?.HasAttribute("data-selection")).IsTrue();
        await Assert.That(table?.HasAttribute("data-row-details")).IsFalse();
        await Assert.That(table?.Id).IsEqualTo("booking-selection");
        await Assert.That(table?.ClassName).IsEqualTo("existing-class");
        var rowCheckboxes = html.QuerySelectorAll("tbody input[type=checkbox]");
        await Assert.That(rowCheckboxes.Length).IsEqualTo(2);
        await Assert.That(rowCheckboxes.All(c => c.HasAttribute("data-row-select"))).IsTrue();
        await Assert
            .That(html.QuerySelector("thead input[type=checkbox]")?.HasAttribute("data-row-select"))
            .IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WithoutSelectionOrRowDetail_RendersNoComponent()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelector("sel-table")).IsNull();
        await Assert.That(html.QuerySelector("[data-slot=table-container]")).IsNotNull();
    }
}
