using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.DataGrid;

public partial class DataGridTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithAddRow_RendersItFirstAcrossTheColumns()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        sut.KeyField = "Id";
        var addRow = new DataGridAddRowTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, AddRowChildren(context, addRow));

        // Assert
        var rows = html.QuerySelectorAll("tbody > tr");
        await Assert.That(rows.Length).IsEqualTo(3);
        await Assert.That(rows[0].GetAttribute("data-slot")).IsEqualTo("table-add-row");
        await Assert.That(rows[0].ClassList.Contains("sa-table-add-row")).IsTrue();
        await Assert.That(rows[0].Children[0].GetAttribute("colspan")).IsEqualTo("2");
        await Assert.That(rows[0].QuerySelector("button")?.TextContent).IsEqualTo("Add");
        await Assert.That(rows[1].GetAttribute("data-key")).IsEqualTo("24817");
    }

    [Test]
    public async Task ProcessAsync_WithAddRowAtBottom_RendersItLast()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        sut.KeyField = "Id";
        var addRow = new DataGridAddRowTagHelper { Position = DataGridAddRowPosition.Bottom };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, AddRowChildren(context, addRow));

        // Assert
        var rows = html.QuerySelectorAll("tbody > tr");
        await Assert.That(rows[0].GetAttribute("data-key")).IsEqualTo("24817");
        await Assert.That(rows[^1].GetAttribute("data-slot")).IsEqualTo("table-add-row");
    }

    [Test]
    public async Task ProcessAsync_WithAddRowAndNoItems_RendersItWithTheMarkedEmptyRow()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, []);
        sut.KeyField = "Id";
        var addRow = new DataGridAddRowTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, AddRowChildren(context, addRow));

        // Assert
        var rows = html.QuerySelectorAll("tbody > tr");
        await Assert.That(rows.Length).IsEqualTo(2);
        await Assert.That(rows[0].GetAttribute("data-slot")).IsEqualTo("table-add-row");
        await Assert.That(rows[1].HasAttribute("data-empty")).IsTrue();
    }

    [Test]
    public async Task ProcessAsync_WithAddRowAndRowsOnly_LeavesItOut()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        sut.KeyField = "Id";
        sut.RowsOnly = true;
        var addRow = new DataGridAddRowTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderInTableBodyAsync(
            sut,
            AddRowChildren(context, addRow)
        );

        // Assert
        await Assert.That(html.QuerySelector("[data-slot=table-add-row]")).IsNull();
        await Assert.That(html.QuerySelectorAll("tbody > tr").Length).IsEqualTo(2);
    }

    private static Func<TagHelperContext, Task<string>> AddRowChildren(
        RenderingContext context,
        DataGridAddRowTagHelper addRow
    )
    {
        var columns = EditingChildren(context);

        return async gridContext =>
            await columns(gridContext)
            + await TagHelperRenderer.RenderChildAsync(
                addRow,
                gridContext,
                _ => Task.FromResult("<button>Add</button>")
            );
    }
}
