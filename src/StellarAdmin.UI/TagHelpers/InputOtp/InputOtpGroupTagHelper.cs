using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A group of <c>sa-input-otp-slot</c>s within a <c>sa-input-otp</c>. Groups are separated by
///     a <c>sa-input-otp-separator</c>.
/// </summary>
[HtmlTargetElement("sa-input-otp-group")]
public class InputOtpGroupTagHelper : StellarAdminTagHelperBase
{
    public InputOtpGroupTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var userClass = output.GetUserSuppliedClass();

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "input-otp-group");
        output.Attributes.SetAttribute(
            "class",
            InputOtpRenderer.GroupClass(ClassMerger, userClass)
        );
    }
}
