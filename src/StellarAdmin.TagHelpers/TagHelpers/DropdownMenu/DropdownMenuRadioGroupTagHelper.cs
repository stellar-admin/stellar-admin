using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Groups <c>sa-dropdown-menu-radio-item</c> children into a single-selection set and tracks
///     which value is selected.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-radio-group")]
public class DropdownMenuRadioGroupTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The value of the currently selected item in the group.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var groupName = $"--sa-dropdown-menu-radio-{context.UniqueId}";
        SetContext(
            context,
            new DropdownMenuRadioGroupContext { GroupName = groupName, SelectedValue = Value }
        );

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "group");
        output.Attributes.SetAttribute("data-slot", "dropdown-menu-radio-group");
        output.Attributes.SetAttribute("data-radio-group", groupName);

        var userClass = output.GetUserSuppliedClass();
        if (!string.IsNullOrEmpty(userClass))
        {
            output.Attributes.SetAttribute("class", userClass);
        }

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
