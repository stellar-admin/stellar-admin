using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Chip;

public class ChipGroupTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithDefaults_RendersPlainGroup()
    {
        // Arrange
        var sut = new ChipGroupTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var group = html.QuerySelector("div[data-slot=chip-group]");
        await Assert.That(group?.GetAttribute("data-appearance")).IsEqualTo("plain");
        await Assert.That(group?.ClassList).Contains("sa-chip-group");
        await Assert.That(group?.ClassList).Contains("existing-class");
        await Assert.That(group?.HasAttribute("aria-invalid")).IsFalse();
        await Assert.That(group?.HasAttribute("data-disabled")).IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WithInputAppearanceAndInvalid_RendersInvalidInputGroup()
    {
        // Arrange
        var sut = new ChipGroupTagHelper { Appearance = ChipGroupAppearance.Input, Invalid = true };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var group = html.QuerySelector("[data-slot=chip-group]");
        await Assert.That(group?.GetAttribute("data-appearance")).IsEqualTo("input");
        await Assert.That(group?.GetAttribute("aria-invalid")).IsEqualTo("true");
    }

    [Test]
    public async Task ProcessAsync_WhenDisabled_DisablesItsChips()
    {
        // Arrange
        var sut = new ChipGroupTagHelper { Disabled = true };
        var chip = new ChipTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            groupContext => TagHelperRenderer.RenderChildAsync(chip, groupContext)
        );

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=chip-group]")?.GetAttribute("data-disabled"))
            .IsEqualTo("true");
        await Assert
            .That(html.QuerySelector("[data-slot=chip]")?.GetAttribute("data-disabled"))
            .IsEqualTo("true");
    }
}
