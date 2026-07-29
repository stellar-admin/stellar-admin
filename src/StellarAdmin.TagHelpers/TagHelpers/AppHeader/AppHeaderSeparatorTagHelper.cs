using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A vertical separator sized for use inside <c>&lt;sa-app-header&gt;</c>, typically between
///     the sidebar trigger and the content that follows it.
/// </summary>
[HtmlTargetElement("sa-app-header-separator")]
public class AppHeaderSeparatorTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("data-slot", "app-header-separator");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-app-header-separator", output.GetUserSuppliedClass())
        );

        var separatorTagHelper = new SeparatorTagHelper()
        {
            Orientation = SeparatorOrientation.Vertical,
        };

        await separatorTagHelper.ProcessAsync(context, output);
    }
}
