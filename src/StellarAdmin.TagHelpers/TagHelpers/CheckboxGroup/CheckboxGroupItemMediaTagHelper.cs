using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The media of a checkbox group option, such as an icon, avatar or image. It sits before the option's text, or
///     leads the card in the choice card variant.
/// </summary>
[HtmlTargetElement("sa-checkbox-group-item-media", ParentTag = "sa-checkbox-group-item")]
public class CheckboxGroupItemMediaTagHelper : ChoiceGroupItemMediaTagHelper
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        return RenderAsync(context, output, true);
    }
}
