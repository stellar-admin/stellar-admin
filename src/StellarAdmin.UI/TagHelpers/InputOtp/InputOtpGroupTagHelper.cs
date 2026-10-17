using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a group of slots in an <c>&lt;sa-input-otp&gt;</c>.
/// </summary>
/// <remarks>
///     Separate groups with an <c>&lt;sa-input-otp-separator&gt;</c>.
/// </remarks>
[HtmlTargetElement("sa-input-otp-group")]
public sealed class InputOtpGroupTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var userClass = output.GetUserSuppliedClass();

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "input-otp-group");
        output.Attributes.SetAttribute(
            "class",
            InputOtpRenderer.GroupClass(
                JoinCssClasses(GetContext<InputOtpContext>(context)?.ClassNames?.Group, userClass)
            )
        );
    }
}
