using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Command;

public class CommandItemTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenStateIsSet_RendersStateAttributes()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new CommandItemTagHelper(context.Icons)
        {
            Value = "kyoto",
            Keywords = "japan",
            Disabled = true,
            Checked = true,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var item = html.QuerySelector("[data-slot=command-item]");
        await Assert.That(item?.GetAttribute("role")).IsEqualTo("option");
        await Assert.That(item?.GetAttribute("data-value")).IsEqualTo("kyoto");
        await Assert.That(item?.GetAttribute("data-keywords")).IsEqualTo("japan");
        await Assert.That(item?.GetAttribute("data-disabled")).IsEqualTo("true");
        await Assert.That(item?.GetAttribute("aria-disabled")).IsEqualTo("true");
        await Assert.That(item?.GetAttribute("data-checked")).IsEqualTo("true");
    }

    [Test]
    public async Task ProcessAsync_WhenStateIsNotSet_OmitsStateAttributes()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new CommandItemTagHelper(context.Icons);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var item = html.QuerySelector("[data-slot=command-item]");
        await Assert.That(item?.GetAttribute("aria-selected")).IsEqualTo("false");
        await Assert.That(item?.HasAttribute("data-value")).IsFalse();
        await Assert.That(item?.HasAttribute("data-keywords")).IsFalse();
        await Assert.That(item?.HasAttribute("data-disabled")).IsFalse();
        await Assert.That(item?.HasAttribute("data-checked")).IsFalse();
        await Assert.That(item?.QuerySelector(".sa-command-item-indicator")).IsNotNull();
    }
}
