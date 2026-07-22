using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Groups related menu items together, exposed to assistive technology as a
///     <c>role="group"</c>.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-group")]
public class DropdownMenuGroupTagHelper : StellarAdminTagHelperBase
{
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
