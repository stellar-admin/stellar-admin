using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;
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

    [Test]
    public async Task ProcessAsync_WhenGroupsRenderFromOneSourceTag_GivesEachHeadingItsOwnId()
    {
        // Arrange
        var sut = new CommandTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            async parent =>
                await RenderGroupAsync("Flights", parent) + await RenderGroupAsync("Hotels", parent)
        );

        // Assert
        var headings = html.QuerySelectorAll("[data-slot=command-group-heading]");
        var groups = html.QuerySelectorAll("[role=group]");
        await Assert.That(headings.Length).IsEqualTo(2);
        await Assert.That(headings[0].Id).IsNotEqualTo(headings[1].Id);
        await Assert.That(groups[0].GetAttribute("aria-labelledby")).IsEqualTo(headings[0].Id);
        await Assert.That(groups[1].GetAttribute("aria-labelledby")).IsEqualTo(headings[1].Id);
    }

    // Razor gives every execution of a tag in a loop the same unique id.
    private static async Task<string> RenderGroupAsync(string heading, TagHelperContext parent)
    {
        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(parent.Items),
            "group"
        );
        var output = new TagHelperOutput(
            "sa-command-group",
            new TagHelperAttributeList(),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        await new CommandGroupTagHelper { Heading = heading }.ProcessAsync(context, output);
        using var writer = new StringWriter();
        output.WriteTo(writer, HtmlEncoder.Default);

        return writer.ToString();
    }
}
