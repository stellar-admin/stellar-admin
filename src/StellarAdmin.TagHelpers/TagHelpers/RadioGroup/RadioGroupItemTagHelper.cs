using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     An option in a radio group.
/// </summary>
[HtmlTargetElement("sa-radio-group-item")]
public class RadioGroupItemTagHelper : ChoiceGroupItemTagHelper
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        return RenderAsync(context, output, false);
    }
}
