using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.DataGrid;

public partial class DataGridTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithRowDetail_RendersLeadingToggleColumnAndCollapsedDetailRows()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        var rowDetail = new DataGridRowDetailTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, RowDetailChildren(rowDetail));

        // Assert
        var details = html.QuerySelector("sel-table-row-details[data-slot=table-row-details]");
        await Assert.That(details?.QuerySelector("[data-slot=table-container]")).IsNotNull();
        await Assert
            .That(
                html.QuerySelector("thead th [data-slot=table-row-detail-toggle]")
                    ?.GetAttribute("data-all")
            )
            .IsEqualTo("true");
        var rows = html.QuerySelectorAll("tbody > tr");
        await Assert.That(rows.Length).IsEqualTo(4);
        await Assert
            .That(
                rows[0]
                    .Children[0]
                    .QuerySelector("[data-slot=table-row-detail-toggle]")
                    ?.GetAttribute("aria-expanded")
            )
            .IsEqualTo("false");
        await Assert.That(rows[1].GetAttribute("data-slot")).IsEqualTo("table-row-detail");
        await Assert.That(rows[1].HasAttribute("hidden")).IsTrue();
        await Assert.That(rows[1].GetAttribute("data-state")).IsEqualTo("closed");
        await Assert.That(rows[1].QuerySelector("td")?.GetAttribute("colspan")).IsEqualTo("1");
        await Assert
            .That(rows[1].QuerySelector("[data-slot=table-row-detail-content]")?.TextContent)
            .IsEqualTo("Details 1");
        await Assert
            .That(rows[3].QuerySelector("[data-slot=table-row-detail-content]")?.TextContent)
            .IsEqualTo("Details 2");
    }

    [Test]
    public async Task ProcessAsync_WithRowDetailExpandedKeys_RendersMatchingRowExpanded()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        var rowDetail = new DataGridRowDetailTagHelper
        {
            KeyField = "Id",
            ExpandedKeys = new[] { 24822 },
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, RowDetailChildren(rowDetail));

        // Assert
        var rows = html.QuerySelectorAll("tbody > tr");
        await Assert.That(rows[1].HasAttribute("hidden")).IsTrue();
        await Assert.That(rows[3].HasAttribute("hidden")).IsFalse();
        await Assert.That(rows[3].GetAttribute("data-state")).IsEqualTo("open");
        await Assert
            .That(
                rows[2]
                    .QuerySelector("[data-slot=table-row-detail-toggle]")
                    ?.GetAttribute("aria-expanded")
            )
            .IsEqualTo("true");
    }

    [Test]
    public async Task ProcessAsync_WithRowDetailTrailingToggle_RendersToggleColumnLast()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        var rowDetail = new DataGridRowDetailTagHelper
        {
            Toggle = DataGridRowDetailToggle.Trailing,
        };
        var selection = new DataGridSelectionTagHelper { KeyField = "Id" };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            async gridContext =>
                await TagHelperRenderer.RenderChildAsync(selection, gridContext)
                + await RowDetailChildren(rowDetail)(gridContext)
        );

        // Assert
        var firstRow = html.QuerySelector("tbody > tr")!;
        await Assert.That(firstRow.Children[0].QuerySelector("input[type=checkbox]")).IsNotNull();
        await Assert
            .That(firstRow.Children[^1].QuerySelector("[data-slot=table-row-detail-toggle]"))
            .IsNotNull();
        await Assert
            .That(
                html.QuerySelector("tbody > tr[data-slot=table-row-detail] > td")
                    ?.GetAttribute("colspan")
            )
            .IsEqualTo("2");
        await Assert
            .That(html.QuerySelector("sel-table-selection > sel-table-row-details"))
            .IsNotNull();
    }

    [Test]
    public async Task ProcessAsync_WithRowDetailWithoutToggle_RendersNoToggleColumn()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        var rowDetail = new DataGridRowDetailTagHelper { Toggle = DataGridRowDetailToggle.None };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, RowDetailChildren(rowDetail));

        // Assert
        await Assert.That(html.QuerySelector("[data-slot=table-row-detail-toggle]")).IsNull();
        await Assert
            .That(html.QuerySelectorAll("tbody > tr[data-slot=table-row-detail]").Length)
            .IsEqualTo(2);
        await Assert.That(html.QuerySelector("thead th")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_WithRowDetailSettings_RendersSettingsOnWrapper()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        var rowDetail = new DataGridRowDetailTagHelper
        {
            RowClick = true,
            ExpandMode = TableRowDetailExpandMode.Single,
            Emphasis = TableRowDetailEmphasis.Rail,
            Inset = TableRowDetailInset.Bleed,
            Animate = false,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, RowDetailChildren(rowDetail));

        // Assert
        var details = html.QuerySelector("sel-table-row-details");
        await Assert.That(details?.GetAttribute("data-row-click")).IsEqualTo("true");
        await Assert.That(details?.GetAttribute("data-expand-mode")).IsEqualTo("single");
        await Assert.That(details?.GetAttribute("data-emphasis")).IsEqualTo("rail");
        await Assert.That(details?.GetAttribute("data-inset")).IsEqualTo("bleed");
        await Assert.That(details?.GetAttribute("data-animate")).IsEqualTo("false");
    }

    [Test]
    public async Task ProcessAsync_WithRowDetailAndNoItems_SpansEmptyStateAcrossToggleColumn()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, []);
        var rowDetail = new DataGridRowDetailTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, RowDetailChildren(rowDetail));

        // Assert
        await Assert
            .That(html.QuerySelector("tbody > tr > td")?.GetAttribute("colspan"))
            .IsEqualTo("1");
        await Assert.That(html.QuerySelector("tbody > tr[data-slot=table-row-detail]")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_WithRowDetailExpandedKeysWithoutKeyField_Throws()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        var rowDetail = new DataGridRowDetailTagHelper { ExpandedKeys = new[] { 24822 } };

        // Act
        var act = async () =>
            await TagHelperRenderer.RenderAsync(sut, RowDetailChildren(rowDetail));

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    private static readonly GridBooking[] Bookings =
    [
        new(24817, "VG-24817"),
        new(24822, "VG-24822"),
    ];

    private static DataGridTagHelper CreateGrid(RenderingContext context, GridBooking[] items) =>
        new(context.Generator, context.Icons) { ViewContext = context.ViewContext, Items = items };

    // The row detail template only runs in row passes, so each call renders the next row's details.
    private static Func<TagHelperContext, Task<string>> RowDetailChildren(
        DataGridRowDetailTagHelper rowDetail
    )
    {
        var row = 0;
        return gridContext =>
            TagHelperRenderer.RenderChildAsync(
                rowDetail,
                gridContext,
                _ => Task.FromResult($"Details {++row}")
            );
    }

    private sealed record GridBooking(int Id, string Reference);
}
