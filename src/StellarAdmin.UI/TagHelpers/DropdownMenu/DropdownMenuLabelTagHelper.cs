using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A non-interactive label used to caption a section of menu items.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-label")]
public class DropdownMenuLabelTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Whether the label is inset, aligning its text with items that have a leading icon.
    /// </summary>
    [HtmlAttributeName("inset")]
    public bool? Inset { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "dropdown-menu-label");
        if (Inset == true)
        {
            output.Attributes.SetAttribute("data-inset", "true");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-dropdown-menu-label", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
