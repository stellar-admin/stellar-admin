using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     An option in a checkbox group.
/// </summary>
[HtmlTargetElement("sa-checkbox-group-item")]
public class CheckboxGroupItemTagHelper : ChoiceGroupItemTagHelper
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        return RenderAsync(context, output, true);
    }
}
