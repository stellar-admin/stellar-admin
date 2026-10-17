using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a vertical separator sized for an app header.
/// </summary>
[HtmlTargetElement("sa-app-header-separator")]
public sealed class AppHeaderSeparatorTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
