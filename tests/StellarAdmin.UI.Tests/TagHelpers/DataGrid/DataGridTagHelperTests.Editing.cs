using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.DataGrid;

public partial class DataGridTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithEditingKeys_RendersEditTemplatesInEditingRows()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        sut.KeyField = "Id";
        sut.EditingKeys = new[] { 24817 };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, EditingChildren(context));

        // Assert
        var rows = html.QuerySelectorAll("tbody > tr");
        await Assert.That(rows[0].HasAttribute("data-editing")).IsTrue();
        await Assert
            .That(rows[0].Children.Select(cell => cell.TextContent).ToArray())
            .IsEquivalentTo(["edit:rows[24817].Reference", "24817"]);
        await Assert.That(rows[1].HasAttribute("data-editing")).IsFalse();
        await Assert
            .That(rows[1].Children.Select(cell => cell.TextContent).ToArray())
            .IsEquivalentTo(["VG-24822", "24822"]);
        await Assert.That(context.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix).IsEmpty();
    }

    [Test]
    public async Task ProcessAsync_WithEditTemplate_SwitchesOnEditingBehavior()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        sut.KeyField = "Id";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, EditingChildren(context));

        // Assert
        var element = html.QuerySelector("sel-table");
        await Assert.That(element?.HasAttribute("data-editing")).IsTrue();
        await Assert.That(element?.HasAttribute("data-selection")).IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WithEditingRow_RendersEmptySelectionCellHiddenToggleAndNoDetailRow()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        sut.KeyField = "Id";
        sut.EditingKeys = new[] { 24817 };
        var selection = new DataGridSelectionTagHelper();
        var rowDetail = new DataGridRowDetailTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            async gridContext =>
                await TagHelperRenderer.RenderChildAsync(selection, gridContext)
                + await RowDetailChildren(rowDetail)(gridContext)
        );

        // Assert
        var rows = html.QuerySelectorAll("tbody > tr");
        await Assert.That(rows.Length).IsEqualTo(3);
        await Assert.That(rows[0].HasAttribute("data-editing")).IsTrue();
        await Assert.That(rows[0].Children[0].ChildElementCount).IsEqualTo(0);
        await Assert
            .That(
                rows[0].QuerySelector("[data-slot=table-row-detail-toggle]")?.HasAttribute("hidden")
            )
            .IsTrue();
        await Assert.That(rows[1].GetAttribute("data-key")).IsEqualTo("24822");
        await Assert.That(rows[1].QuerySelector("input[data-row-select]")).IsNotNull();
        await Assert
            .That(
                rows[1].QuerySelector("[data-slot=table-row-detail-toggle]")?.HasAttribute("hidden")
            )
            .IsFalse();
        await Assert.That(rows[2].GetAttribute("data-slot")).IsEqualTo("table-row-detail");
    }

    [Test]
    public async Task ProcessAsync_WithNewItem_RendersEditingRowFirst()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        sut.KeyField = "Id";
        sut.NewItem = new GridBooking(0, "");

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, EditingChildren(context));

        // Assert
        var rows = html.QuerySelectorAll("tbody > tr");
        await Assert.That(rows.Length).IsEqualTo(3);
        await Assert.That(rows[0].GetAttribute("data-key")).IsEqualTo("new");
        await Assert.That(rows[0].HasAttribute("data-editing")).IsTrue();
        await Assert.That(rows[0].Children[0].TextContent).IsEqualTo("edit:rows[new].Reference");
        await Assert.That(rows[1].GetAttribute("data-key")).IsEqualTo("24817");
    }

    [Test]
    public async Task ProcessAsync_WithNewItemAndNoItems_RendersOnlyNewRow()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, []);
        sut.KeyField = "Id";
        sut.NewItem = new GridBooking(0, "");

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, EditingChildren(context));

        // Assert
        var rows = html.QuerySelectorAll("tbody > tr");
        await Assert.That(rows.Length).IsEqualTo(1);
        await Assert.That(rows[0].GetAttribute("data-key")).IsEqualTo("new");
    }

    [Test]
    public async Task ProcessAsync_WithRowsOnly_RendersOnlyRows()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        sut.KeyField = "Id";
        sut.EditingKeys = new[] { 24822 };
        sut.RowsOnly = true;
        var rowDetail = new DataGridRowDetailTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderInTableBodyAsync(
            sut,
            RowDetailChildren(rowDetail),
            outputAttributes: [new TagHelperAttribute("id", "test")]
        );

        // Assert
        await Assert.That(html.QuerySelector("[data-slot=data-grid]")).IsNull();
        await Assert.That(html.QuerySelectorAll("table").Length).IsEqualTo(1);
        await Assert.That(html.QuerySelector("thead")).IsNull();
        var rows = html.QuerySelectorAll("tbody > tr");
        await Assert.That(rows.Length).IsEqualTo(3);
        await Assert.That(rows[0].GetAttribute("data-key")).IsEqualTo("24817");
        await Assert.That(rows[1].Id).IsEqualTo("test-detail-24817");
        await Assert.That(rows[2].GetAttribute("data-key")).IsEqualTo("24822");
        await Assert.That(rows[2].HasAttribute("data-editing")).IsTrue();
    }

    [Test]
    public async Task ProcessAsync_WithEditingKeysWithoutKeyField_Throws()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateGrid(context, Bookings);
        sut.EditingKeys = new[] { 24817 };

        // Act
        var act = async () => await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    // A Reference column with an edit template that writes its field's full name, and an Id
    // column without one.
    private static Func<TagHelperContext, Task<string>> EditingChildren(RenderingContext context)
    {
        var viewEngine =
            context.ViewContext.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();
        var reference = new DataGridColumnTagHelper(viewEngine, context.Metadata)
        {
            ViewContext = context.ViewContext,
            Field = "Reference",
        };
        var id = new DataGridColumnTagHelper(viewEngine, context.Metadata)
        {
            ViewContext = context.ViewContext,
            Field = "Id",
        };
        var editTemplate = new DataGridEditTemplateTagHelper();

        return async gridContext =>
            await TagHelperRenderer.RenderChildAsync(
                reference,
                gridContext,
                columnContext =>
                    TagHelperRenderer.RenderChildAsync(
                        editTemplate,
                        columnContext,
                        _ =>
                            Task.FromResult(
                                "edit:"
                                    + context.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(
                                        "Reference"
                                    )
                            )
                    )
            ) + await TagHelperRenderer.RenderChildAsync(id, gridContext);
    }
}
