using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Checklist;

public class ChecklistTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenOptionsAreOmitted_RendersDefaultSizeAndInsetDivider()
    {
        // Arrange
        var sut = new ChecklistTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var root = html.QuerySelector("ul");
        await Assert.That(root?.GetAttribute("role")).IsEqualTo("list");
        await Assert.That(root?.GetAttribute("data-slot")).IsEqualTo("checklist");
        await Assert.That(root?.GetAttribute("data-size")).IsEqualTo("default");
        await Assert.That(root?.GetAttribute("data-divider")).IsEqualTo("inset");
        await Assert.That(root?.ClassName).IsEqualTo("sa-checklist existing-class");
    }

    [Test]
    [Arguments(ChecklistDivider.FullWidth, "full-width")]
    [Arguments(ChecklistDivider.None, "none")]
    public async Task ProcessAsync_WhenDividerIsSet_RendersIt(
        ChecklistDivider divider,
        string expected
    )
    {
        // Arrange
        var sut = new ChecklistTagHelper { Divider = divider, Size = ChecklistSize.Small };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("ul")?.GetAttribute("data-divider"))
            .IsEqualTo(expected);
        await Assert.That(html.QuerySelector("ul")?.GetAttribute("data-size")).IsEqualTo("sm");
    }
}
