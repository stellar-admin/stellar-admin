using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.DataGrid;

public partial class DataGridTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithKeyField_RendersRowKeysAndKeyedDetailRows()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        sut.KeyField = "Id";
        var rowDetail = new DataGridRowDetailTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            RowDetailChildren(rowDetail),
            outputAttributes: [new TagHelperAttribute("id", "bookings")]
        );

        // Assert
        var rows = html.QuerySelectorAll("tbody > tr");
        await Assert.That(rows[0].GetAttribute("data-key")).IsEqualTo("24817");
        await Assert.That(rows[1].Id).IsEqualTo("bookings-detail-24817");
        await Assert.That(rows[1].HasAttribute("data-key")).IsFalse();
        await Assert
            .That(
                rows[0]
                    .QuerySelector("[data-slot=table-row-detail-toggle]")
                    ?.GetAttribute("aria-controls")
            )
            .IsEqualTo("bookings-detail-24817");
        await Assert.That(rows[2].GetAttribute("data-key")).IsEqualTo("24822");
        await Assert.That(rows[3].Id).IsEqualTo("bookings-detail-24822");
    }

    [Test]
    public async Task ProcessAsync_WithoutKeyField_RendersNoRowKeys()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        var rowDetail = new DataGridRowDetailTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, RowDetailChildren(rowDetail));

        // Assert
        await Assert.That(html.QuerySelector("tr[data-key]")).IsNull();
        await Assert.That(html.QuerySelector("tr[data-slot=table-row-detail][id]")).IsNull();
        await Assert.That(html.QuerySelector("[aria-controls]")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_WithSelectionWithoutKeyField_Throws()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        var selection = new DataGridSelectionTagHelper();

        // Act
        var act = async () =>
            await TagHelperRenderer.RenderAsync(
                sut,
                gridContext => TagHelperRenderer.RenderChildAsync(selection, gridContext)
            );

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
