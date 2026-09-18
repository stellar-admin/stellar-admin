using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.InputOtp;

public partial class InputOtpTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenChildrenAreSupplied_PassesClassesWithoutGeneratingExtraParts()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new InputOtpTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Groups = "2,2",
            ClassNames = new InputOtpClassNames
            {
                Control = "otp-control",
                Group = "otp-group",
                Slot = "otp-slot",
                Separator = "otp-separator",
            },
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            async parent =>
            {
                var group = await TagHelperRenderer.RenderChildAsync(
                    new InputOtpGroupTagHelper(),
                    parent
                );
                var slot = await TagHelperRenderer.RenderChildAsync(
                    new InputOtpSlotTagHelper(),
                    parent
                );
                var separator = await TagHelperRenderer.RenderChildAsync(
                    new InputOtpSeparatorTagHelper(context.Icons),
                    parent
                );

                return group + slot + separator;
            }
        );

        // Assert
        await Assert.That(html.QuerySelectorAll(".otp-control").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("sel-input-otp.sa-input-otp.otp-control").Length)
            .IsEqualTo(1);
        await Assert.That(html.QuerySelectorAll(".otp-group").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("div.sa-input-otp-group.otp-group").Length)
            .IsEqualTo(1);
        await Assert.That(html.QuerySelectorAll(".otp-slot").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("div.sa-input-otp-slot.otp-slot").Length)
            .IsEqualTo(1);
        await Assert.That(html.QuerySelectorAll(".otp-separator").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("div.sa-input-otp-separator.otp-separator").Length)
            .IsEqualTo(1);
        await Assert.That(html.QuerySelectorAll(".sa-input-otp-group").Length).IsEqualTo(1);
        await Assert.That(html.QuerySelectorAll(".sa-input-otp-slot").Length).IsEqualTo(1);
        await Assert.That(html.QuerySelectorAll(".sa-input-otp-separator").Length).IsEqualTo(1);
    }
}
