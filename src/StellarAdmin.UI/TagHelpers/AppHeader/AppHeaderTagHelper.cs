using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the bar at the top of the application, as a <c>&lt;header&gt;</c> element.
/// </summary>
/// <remarks>
///     Place it at the top of <c>&lt;sa-sidebar-inset&gt;</c>.
/// </remarks>
[HtmlTargetElement("sa-app-header")]
public sealed class AppHeaderTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "header";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "app-header");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-app-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
