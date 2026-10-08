using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Chip;

public class ChipTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithDefaults_RendersEnabledChip()
    {
        // Arrange
        var sut = new ChipTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, _ => Task.FromResult("Lisbon"));

        // Assert
        var chip = html.QuerySelector("span[data-slot=chip]");
        await Assert.That(chip?.TextContent).IsEqualTo("Lisbon");
        await Assert.That(chip?.ClassList).Contains("sa-chip");
        await Assert.That(chip?.ClassList).Contains("existing-class");
        await Assert.That(chip?.HasAttribute("data-disabled")).IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WhenDisabled_DisablesItsRemoveButton()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ChipTagHelper { Disabled = true };
        var remove = new ChipRemoveTagHelper(context.Icons);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            chipContext =>
                TagHelperRenderer.RenderChildAsync(
                    remove,
                    chipContext,
                    outputAttributes: [new TagHelperAttribute("aria-label", "Remove Lisbon")]
                )
        );

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=chip]")?.GetAttribute("data-disabled"))
            .IsEqualTo("true");
        await Assert
            .That(html.QuerySelector("[data-slot=chip-remove]")?.HasAttribute("disabled"))
            .IsTrue();
    }

    [Test]
    public async Task ProcessAsync_WhenEnabledInDisabledGroup_StaysEnabled()
    {
        // Arrange
        var group = new ChipGroupTagHelper { Disabled = true };
        var sut = new ChipTagHelper { Disabled = false };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            group,
            groupContext => TagHelperRenderer.RenderChildAsync(sut, groupContext)
        );

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=chip]")?.HasAttribute("data-disabled"))
            .IsFalse();
    }
}
