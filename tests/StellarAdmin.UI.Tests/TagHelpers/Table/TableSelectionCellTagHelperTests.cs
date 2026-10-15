using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Table;

public class TableSelectionCellTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithDefaults_RendersCellWithMarkedCheckbox()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new TableSelectionCellTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
        };

        // Act
        using var html = await TagHelperRenderer.RenderInTableRowAsync(sut);

        // Assert
        var cell = html.QuerySelector("td[data-slot=table-cell]");
        await Assert.That(cell?.ClassList).Contains("sa-table-cell");
        await Assert.That(cell?.ClassList).Contains("existing-class");
        var checkbox = cell?.QuerySelector("input[type=checkbox]");
        await Assert.That(checkbox?.HasAttribute("data-row-select")).IsTrue();
        await Assert.That(checkbox?.ClassList).Contains("sa-checkbox");
        await Assert.That(checkbox?.GetAttribute("aria-label")).IsEqualTo("Select row");
        await Assert.That(checkbox?.HasAttribute("name")).IsFalse();
        await Assert.That(checkbox?.HasAttribute("checked")).IsFalse();
        await Assert.That(checkbox?.HasAttribute("disabled")).IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WithSettings_RendersCheckboxWithThem()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new TableSelectionCellTagHelper(context.Generator, context.Icons)
        {
            Checked = true,
            Disabled = true,
            Label = "Select booking VG-24817",
            Name = "selectedBookings",
            Value = "VG-24817",
            ViewContext = context.ViewContext,
        };

        // Act
        using var html = await TagHelperRenderer.RenderInTableRowAsync(sut);

        // Assert
        var checkbox = html.QuerySelector("td input[type=checkbox]");
        await Assert.That(checkbox?.GetAttribute("name")).IsEqualTo("selectedBookings");
        await Assert.That(checkbox?.GetAttribute("value")).IsEqualTo("VG-24817");
        await Assert.That(checkbox?.GetAttribute("aria-label")).IsEqualTo("Select booking VG-24817");
        await Assert.That(checkbox?.HasAttribute("checked")).IsTrue();
        await Assert.That(checkbox?.HasAttribute("disabled")).IsTrue();
    }

    [Test]
    public async Task ProcessAsync_WithCheckedFalse_RendersUncheckedCheckbox()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new TableSelectionCellTagHelper(context.Generator, context.Icons)
        {
            Checked = false,
            Disabled = false,
            ViewContext = context.ViewContext,
        };

        // Act
        using var html = await TagHelperRenderer.RenderInTableRowAsync(sut);

        // Assert
        var checkbox = html.QuerySelector("td input[type=checkbox]");
        await Assert.That(checkbox?.HasAttribute("checked")).IsFalse();
        await Assert.That(checkbox?.HasAttribute("disabled")).IsFalse();
    }
}
