using StellarAdmin.UI.Icons;
using StellarAdmin.UI.Tests.Support;
using TUnit.Assertions.Enums;

namespace StellarAdmin.UI.Tests.TagHelpers.Command;

public class CommandDialogTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenShowCloseButtonIsTrue_RendersCloseIconBeforeLabel()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new CommandDialogTagHelper(context.Icons) { ShowCloseButton = true };
        context.Icons.Value.TryGetIcon(
            context.Icons.Value.GetSemanticIconName(SemanticIconRole.Close)!,
            out var closeIcon
        );

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var icon = html.QuerySelector(".sa-dialog-close > svg:first-child");
        await Assert.That(icon?.GetAttribute("class")).IsEqualTo("size-4");
        await Assert
            .That(icon?.Attributes.Select(attribute => attribute.Name).ToArray())
            .IsEquivalentTo(["class", .. closeIcon!.Attributes.Keys], CollectionOrdering.Matching);
        await Assert
            .That(icon?.Children.Select(shape => shape.GetAttribute("d")).ToArray())
            .IsEquivalentTo(
                closeIcon.Shapes.Select(shape => shape.Attributes["d"]).ToArray(),
                CollectionOrdering.Matching
            );
        await Assert
            .That(html.QuerySelector(".sa-dialog-close > .sr-only")?.TextContent)
            .IsEqualTo("Close");
    }

    [Test]
    public async Task ProcessAsync_WhenShowCloseButtonIsNotSet_OmitsCloseButton()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new CommandDialogTagHelper(context.Icons);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelector(".sa-dialog-close")).IsNull();
    }
}
