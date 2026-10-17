using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Groups related items in a dropdown menu.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-group")]
public sealed class DropdownMenuGroupTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "group");
        output.Attributes.SetAttribute("data-slot", "dropdown-menu-group");

        var userClass = output.GetUserSuppliedClass();
        if (!string.IsNullOrEmpty(userClass))
        {
            output.Attributes.SetAttribute("class", userClass);
        }

        return Task.CompletedTask;
    }
}
