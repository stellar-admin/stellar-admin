using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an option in a checkbox group.
/// </summary>
/// <remarks>
///     Must be placed inside an <c>sa-checkbox-group</c> element.
/// </remarks>
[HtmlTargetElement("sa-checkbox-group-item")]
public class CheckboxGroupItemTagHelper : ChoiceGroupItemTagHelper
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        return RenderAsync(context, output, true);
    }
}
