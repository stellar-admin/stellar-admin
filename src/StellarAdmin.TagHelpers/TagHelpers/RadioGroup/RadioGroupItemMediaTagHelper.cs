using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The media of a radio group option, such as an icon, avatar or image. It sits before the option's text, or
///     leads the card in the choice card variant.
/// </summary>
[HtmlTargetElement("sa-radio-group-item-media", ParentTag = "sa-radio-group-item")]
public class RadioGroupItemMediaTagHelper : ChoiceGroupItemMediaTagHelper
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        return RenderAsync(context, output, false);
    }
}
