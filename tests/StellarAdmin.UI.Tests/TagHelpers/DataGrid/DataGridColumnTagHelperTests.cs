using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using StellarAdmin.UI.Tests.Support;
using TUnit.Assertions.Enums;

namespace StellarAdmin.UI.Tests.TagHelpers.DataGrid;

public class DataGridColumnTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithEmptyValueAndEmptyText_ShowsEmptyText()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateColumn(context, new TemplateViewEngine());
        sut.Field = nameof(GridProduct.Name);
        sut.EmptyText = "Unnamed";

        // Act
        var cells = await RenderCellsAsync(context, sut, new GridProduct { Name = " " });

        // Assert
        await Assert.That(cells[0]).IsEqualTo("Unnamed");
    }

    [Test]
    public async Task ProcessAsync_WithEmptyValueAndNullDisplayText_ShowsNullDisplayText()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateColumn(context, new TemplateViewEngine());
        sut.Field = nameof(GridProduct.Notes);

        // Act
        var cells = await RenderCellsAsync(context, sut, new GridProduct());

        // Assert
        await Assert.That(cells[0]).IsEqualTo("No notes");
    }

    [Test]
    public async Task ProcessAsync_WithTemplate_RendersTemplateWithFieldMetadata()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateColumn(context, new TemplateViewEngine());
        sut.Field = nameof(GridProduct.Notes);
        sut.Template = "Notes";

        // Act
        var cells = await RenderCellsAsync(context, sut, new GridProduct { Notes = "Fragile" });

        // Assert
        await Assert.That(cells[0]).IsEqualTo("Notes:No notes:Fragile");
    }

    [Test]
    public async Task ProcessAsync_WithTemplateAndEmptyValue_SkipsTemplate()
    {
        // Arrange
        using var context = new RenderingContext();
        var viewEngine = new TemplateViewEngine();
        var sut = CreateColumn(context, viewEngine);
        sut.Field = nameof(GridProduct.Notes);
        sut.Template = "Notes";
        sut.EmptyText = "None";

        // Act
        var cells = await RenderCellsAsync(context, sut, new GridProduct());

        // Assert
        await Assert.That(cells[0]).IsEqualTo("None");
        await Assert.That(viewEngine.Renders).IsEqualTo(0);
    }

    [Test]
    public async Task ProcessAsync_WithTemplateRendersEmptyAndEmptyValue_RendersTemplate()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateColumn(context, new TemplateViewEngine());
        sut.Field = nameof(GridProduct.Notes);
        sut.Template = "Notes";
        sut.TemplateRendersEmpty = true;
        sut.EmptyText = "None";

        // Act
        var cells = await RenderCellsAsync(context, sut, new GridProduct());

        // Assert
        await Assert.That(cells[0]).IsEqualTo("Notes:No notes:");
    }

    [Test]
    public async Task ProcessAsync_WithTemplateRendersEmptyAndNoTemplate_ShowsEmptyText()
    {
        // Arrange
        using var context = new RenderingContext();
        var viewEngine = new TemplateViewEngine();
        var sut = CreateColumn(context, viewEngine);
        sut.Field = nameof(GridProduct.Notes);
        sut.TemplateRendersEmpty = true;

        // Act
        var cells = await RenderCellsAsync(context, sut, new GridProduct());

        // Assert
        await Assert.That(cells[0]).IsEqualTo("No notes");
        await Assert.That(viewEngine.Renders).IsEqualTo(0);
    }

    [Test]
    public async Task ProcessAsync_WithComputedFieldFor_ShowsTheValueOfEachRow()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateColumn(context, new TemplateViewEngine());
        sut.FieldFor =
            (Expression<Func<GridProduct, string>>)(
                product => product.Name + " (" + product.Notes + ")"
            );

        // Act
        var cells = await RenderCellsAsync(
            context,
            sut,
            new GridProduct { Name = "Tent", Notes = "Fragile" },
            new GridProduct { Name = "Map" }
        );

        // Assert
        await Assert
            .That(cells)
            .IsEquivalentTo(["Tent (Fragile)", "Map ()"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task ProcessAsync_WithComputedFieldForThroughNull_ShowsEmptyText()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateColumn(context, new TemplateViewEngine());
        sut.FieldFor =
            (Expression<Func<GridProduct, string>>)(product => product.Notes!.ToUpperInvariant());
        sut.EmptyText = "None";

        // Act
        var cells = await RenderCellsAsync(context, sut, new GridProduct());

        // Assert
        await Assert.That(cells[0]).IsEqualTo("None");
    }

    [Test]
    public async Task ProcessAsync_WithComputedFieldForAndTemplate_RendersTemplateWithValue()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateColumn(context, new TemplateViewEngine());
        sut.FieldFor =
            (Expression<Func<GridProduct, string>>)(product => product.Name.ToUpperInvariant());
        sut.Template = "Notes";

        // Act
        var cells = await RenderCellsAsync(context, sut, new GridProduct { Name = "Tent" });

        // Assert
        await Assert.That(cells[0]).IsEqualTo("::TENT");
    }

    private static DataGridColumnTagHelper CreateColumn(
        RenderingContext context,
        ICompositeViewEngine viewEngine
    ) => new(viewEngine, context.Metadata) { ViewContext = context.ViewContext };

    private static async Task<string[]> RenderCellsAsync(
        RenderingContext context,
        DataGridColumnTagHelper column,
        params GridProduct[] items
    )
    {
        var grid = new DataGridTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Items = items,
        };
        using var html = await TagHelperRenderer.RenderAsync(
            grid,
            gridContext => TagHelperRenderer.RenderChildAsync(column, gridContext)
        );

        return html.QuerySelectorAll("tbody td").Select(cell => cell.InnerHtml).ToArray();
    }

    public sealed class GridProduct
    {
        public string Name { get; set; } = "";

        [DisplayFormat(NullDisplayText = "No notes")]
        public string? Notes { get; set; }
    }

    // Finds every template, which writes the property its metadata describes, the property's null
    // display text and the value
    private sealed class TemplateViewEngine : ICompositeViewEngine
    {
        public int Renders { get; private set; }

        public IReadOnlyList<IViewEngine> ViewEngines => [];

        public ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage) =>
            ViewEngineResult.Found(viewName, new TemplateView(this, viewName));

        public ViewEngineResult GetView(
            string? executingFilePath,
            string viewPath,
            bool isMainPage
        ) => ViewEngineResult.Found(viewPath, new TemplateView(this, viewPath));

        private sealed class TemplateView(TemplateViewEngine engine, string path) : IView
        {
            public string Path => path;

            public Task RenderAsync(ViewContext context)
            {
                engine.Renders++;
                var metadata = context.ViewData.ModelMetadata;
                return context.Writer.WriteAsync(
                    $"{metadata.PropertyName}:{metadata.NullDisplayText}:{context.ViewData.Model}"
                );
            }
        }
    }
}
