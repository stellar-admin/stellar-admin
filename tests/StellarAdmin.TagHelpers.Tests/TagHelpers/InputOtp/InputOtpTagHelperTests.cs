using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.InputOtp;

public partial class InputOtpTagHelperTests
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ProcessAsync_WhenRendering_EmitsCaretClasses(bool composed)
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
            composed
                ? async parent =>
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
                : null
        );

        // Assert
        await Assert
            .That(html.QuerySelector("sel-input-otp")?.GetAttribute("data-caret-class"))
            .IsEqualTo("sa-input-otp-caret");
        await Assert
            .That(html.QuerySelector("sel-input-otp")?.GetAttribute("data-caret-line-class"))
            .IsEqualTo("sa-input-otp-caret-line");
    }
}
