using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.DataGrid;

public partial class DataGridTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithoutColumnGroups_RendersOneHeaderRowWithEdgeHeads()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        var reference = CreateColumn(context, "Reference");
        var id = CreateColumn(context, "Id");

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            async gridContext =>
                await TagHelperRenderer.RenderChildAsync(reference, gridContext)
                + await TagHelperRenderer.RenderChildAsync(id, gridContext)
        );

        // Assert
        var heads = html.QuerySelectorAll("thead > tr > th");
        await Assert.That(html.QuerySelectorAll("thead > tr").Length).IsEqualTo(1);
        await Assert.That(heads[0].HasAttribute("data-edge-start")).IsTrue();
        await Assert.That(heads[0].HasAttribute("data-edge-end")).IsFalse();
        await Assert.That(heads[1].HasAttribute("data-edge-start")).IsFalse();
        await Assert.That(heads[1].HasAttribute("data-edge-end")).IsTrue();
        await Assert.That(html.QuerySelector("th[rowspan], th[colspan]")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_WithNestedColumnGroups_RendersHeaderRowPerGroupLevel()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, NestedGroupChildren(context));

        // Assert
        var headerRows = html.QuerySelectorAll("thead > tr");
        await Assert.That(headerRows.Length).IsEqualTo(3);
        await Assert
            .That(headerRows.Select(row => row.ChildElementCount).ToArray())
            .IsEquivalentTo([2, 2, 2]);

        var booking = headerRows[0].Children[0];
        await Assert.That(booking.TextContent).IsEqualTo("Booking");
        await Assert.That(booking.GetAttribute("rowspan")).IsEqualTo("3");
        await Assert.That(booking.HasAttribute("data-edge-start")).IsTrue();

        var trip = headerRows[0].Children[1];
        await Assert.That(trip.TextContent).IsEqualTo("Trip");
        await Assert.That(trip.GetAttribute("scope")).IsEqualTo("colgroup");
        await Assert.That(trip.GetAttribute("colspan")).IsEqualTo("3");
        await Assert.That(trip.HasAttribute("data-edge-end")).IsTrue();

        var id = headerRows[1].Children[0];
        await Assert.That(id.TextContent).IsEqualTo("Id");
        await Assert.That(id.GetAttribute("rowspan")).IsEqualTo("2");
        await Assert.That(id.HasAttribute("data-edge-start")).IsFalse();

        var dates = headerRows[1].Children[1];
        await Assert.That(dates.TextContent).IsEqualTo("Dates");
        await Assert.That(dates.GetAttribute("colspan")).IsEqualTo("2");

        await Assert
            .That(headerRows[2].Children.Select(head => head.TextContent).ToArray())
            .IsEquivalentTo(["Reference", "Id"]);
        await Assert.That(headerRows[2].Children[0].HasAttribute("rowspan")).IsFalse();
        await Assert.That(headerRows[2].Children[1].HasAttribute("data-edge-end")).IsTrue();
    }

    [Test]
    public async Task ProcessAsync_WithColumnGroups_RendersBodyCellsInColumnOrder()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, NestedGroupChildren(context));

        // Assert
        var rows = html.QuerySelectorAll("tbody > tr");
        await Assert.That(rows.Length).IsEqualTo(2);
        await Assert
            .That(rows[0].Children.Select(cell => cell.TextContent).ToArray())
            .IsEquivalentTo(["VG-24817", "24817", "VG-24817", "24817"]);
    }

    [Test]
    public async Task ProcessAsync_WithColumnGroupsAndSelection_SpansSelectionHeadAcrossHeaderRows()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        sut.KeyField = "Id";
        var selection = new DataGridSelectionTagHelper();
        var groupChildren = NestedGroupChildren(context);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            async gridContext =>
                await TagHelperRenderer.RenderChildAsync(selection, gridContext)
                + await groupChildren(gridContext)
        );

        // Assert
        var heads = html.QuerySelectorAll("thead > tr:first-child > th");
        await Assert.That(heads[0].GetAttribute("rowspan")).IsEqualTo("3");
        await Assert.That(heads[0].HasAttribute("data-edge-start")).IsTrue();
        await Assert.That(heads[1].HasAttribute("data-edge-start")).IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WithColumnAlign_RendersAlignmentOnHeadAndCells()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        var reference = CreateColumn(context, "Reference");
        var id = CreateColumn(context, "Id");
        id.Align = DataGridColumnAlign.End;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            async gridContext =>
                await TagHelperRenderer.RenderChildAsync(reference, gridContext)
                + await TagHelperRenderer.RenderChildAsync(id, gridContext)
        );

        // Assert
        var heads = html.QuerySelectorAll("thead th");
        await Assert.That(heads[0].HasAttribute("data-align")).IsFalse();
        await Assert.That(heads[1].GetAttribute("data-align")).IsEqualTo("end");
        var cells = html.QuerySelectorAll("tbody > tr:first-child > td");
        await Assert.That(cells[0].HasAttribute("data-align")).IsFalse();
        await Assert.That(cells[1].GetAttribute("data-align")).IsEqualTo("end");
    }

    [Test]
    public async Task ProcessAsync_WithColumnGroupAlignAndClass_RendersThemOnGroupHead()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        var group = new DataGridColumnGroupTagHelper
        {
            Title = "Trip",
            Align = DataGridColumnGroupAlign.Start,
        };
        var reference = CreateColumn(context, "Reference");

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            gridContext =>
                TagHelperRenderer.RenderChildAsync(
                    group,
                    gridContext,
                    groupContext => TagHelperRenderer.RenderChildAsync(reference, groupContext)
                )
        );

        // Assert
        var head = html.QuerySelector("th[scope=colgroup]");
        await Assert.That(head?.GetAttribute("data-align")).IsEqualTo("start");
        await Assert.That(head?.ClassList.Contains("existing-class")).IsTrue();
        await Assert.That(head?.HasAttribute("colspan")).IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WithCenteredColumnGroup_RendersNoAlignment()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, NestedGroupChildren(context));

        // Assert
        await Assert.That(html.QuerySelector("th[scope=colgroup][data-align]")).IsNull();
    }

    private static DataGridColumnTagHelper CreateColumn(RenderingContext context, string field) =>
        new(
            context.ViewContext.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>(),
            context.Metadata
        )
        {
            ViewContext = context.ViewContext,
            Field = field,
        };

    // Booking, then Trip holding Id and Dates, and Dates holding Reference and Id.
    private static Func<TagHelperContext, Task<string>> NestedGroupChildren(
        RenderingContext context
    )
    {
        var booking = CreateColumn(context, "Reference");
        booking.Title = "Booking";
        var trip = new DataGridColumnGroupTagHelper { Title = "Trip" };
        var tripId = CreateColumn(context, "Id");
        var dates = new DataGridColumnGroupTagHelper { Title = "Dates" };
        var datesReference = CreateColumn(context, "Reference");
        var datesId = CreateColumn(context, "Id");

        return async gridContext =>
            await TagHelperRenderer.RenderChildAsync(booking, gridContext)
            + await TagHelperRenderer.RenderChildAsync(
                trip,
                gridContext,
                async tripContext =>
                    await TagHelperRenderer.RenderChildAsync(tripId, tripContext)
                    + await TagHelperRenderer.RenderChildAsync(
                        dates,
                        tripContext,
                        async datesContext =>
                            await TagHelperRenderer.RenderChildAsync(datesReference, datesContext)
                            + await TagHelperRenderer.RenderChildAsync(datesId, datesContext)
                    )
            );
    }
}
