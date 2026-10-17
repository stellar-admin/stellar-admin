using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Groups <c>sa-dropdown-menu-radio-item</c> elements so that only one can be selected.
/// </summary>
[HtmlTargetElement("sa-dropdown-menu-radio-group")]
public class DropdownMenuRadioGroupTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the value of the selected item.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var groupName = $"--sa-dropdown-menu-radio-{GetUniqueId(context)}";
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

        return Task.CompletedTask;
    }
}
