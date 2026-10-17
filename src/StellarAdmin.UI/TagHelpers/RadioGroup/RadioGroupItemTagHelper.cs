using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an option in a radio group.
/// </summary>
/// <remarks>
///     Must be placed inside an <c>sa-radio-group</c> element.
/// </remarks>
[HtmlTargetElement("sa-radio-group-item")]
public class RadioGroupItemTagHelper : ChoiceGroupItemTagHelper
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        return RenderAsync(context, output, false);
    }
}
