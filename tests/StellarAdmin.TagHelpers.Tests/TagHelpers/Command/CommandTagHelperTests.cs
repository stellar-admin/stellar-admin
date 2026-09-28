using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Command;

public class CommandTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenFilterIsNotSet_RendersClientFilter()
    {
        // Arrange
        var sut = new CommandTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var command = html.QuerySelector("sel-command");
        await Assert.That(command?.GetAttribute("data-filter")).IsEqualTo("client");
        await Assert.That(command?.HasAttribute("data-loop")).IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WhenFilterIsNone_RendersNoneFilter()
    {
        // Arrange
        var sut = new CommandTagHelper { Filter = CommandFilter.None, Loop = true };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var command = html.QuerySelector("sel-command");
        await Assert.That(command?.GetAttribute("data-filter")).IsEqualTo("none");
        await Assert.That(command?.GetAttribute("data-loop")).IsEqualTo("true");
    }

    [Test]
    public async Task ProcessAsync_WhenInputAndListAreChildren_LinksInputToListAndLabel()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new CommandTagHelper { Label = "Search trips" };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            async parent =>
                await TagHelperRenderer.RenderChildAsync(
                    new CommandInputTagHelper(context.Icons),
                    parent
                ) + await TagHelperRenderer.RenderChildAsync(new CommandListTagHelper(), parent)
        );

        // Assert
        var commandId = html.QuerySelector("sel-command")?.Id;
        var input = html.QuerySelector("input[data-slot=command-input]");
        var list = html.QuerySelector("[data-slot=command-list]");
        var label = html.QuerySelector("label");
        await Assert.That(commandId).StartsWith("sa-command-");
        await Assert.That(input?.Id).IsEqualTo($"{commandId}-input");
        await Assert.That(list?.Id).IsEqualTo($"{commandId}-list");
        await Assert.That(input?.GetAttribute("aria-controls")).IsEqualTo(list?.Id);
        await Assert.That(label?.Id).IsEqualTo($"{commandId}-label");
        await Assert.That(label?.TextContent).IsEqualTo("Search trips");
        await Assert.That(input?.GetAttribute("aria-labelledby")).IsEqualTo(label?.Id);
    }
}
