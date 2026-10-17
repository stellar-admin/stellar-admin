using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a label for a section of dropdown menu items.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-label")]
public class DropdownMenuLabelTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets a value that indicates whether the label is inset.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the label's text aligns with items that have a leading icon;
    ///     otherwise, <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("inset")]
    public bool? Inset { get; set; }

    /// <inheritdoc />
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
