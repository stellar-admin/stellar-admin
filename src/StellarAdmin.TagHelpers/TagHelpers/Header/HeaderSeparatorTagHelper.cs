using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A vertical separator sized for use inside <c>&lt;sa-header&gt;</c>, typically between the
///     sidebar trigger and the content that follows it.
/// </summary>
[HtmlTargetElement("sa-header-separator")]
public class HeaderSeparatorTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("data-slot", "header-separator");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-header-separator", output.GetUserSuppliedClass())
        );

        var separatorTagHelper = new SeparatorTagHelper()
        {
            Orientation = SeparatorOrientation.Vertical,
        };

        await separatorTagHelper.ProcessAsync(context, output);
    }
}
