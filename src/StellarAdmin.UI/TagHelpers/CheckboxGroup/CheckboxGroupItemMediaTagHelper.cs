using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the media of a checkbox group option, such as an icon, avatar or image.
/// </summary>
/// <remarks>
///     Must be placed inside an <c>sa-checkbox-group-item</c> element. The media shows before the
///     option's text, or at the start of the card in the choice card variant. An option can have
///     only one media element.
/// </remarks>
[HtmlTargetElement("sa-checkbox-group-item-media", ParentTag = "sa-checkbox-group-item")]
public sealed class CheckboxGroupItemMediaTagHelper : ChoiceGroupItemMediaTagHelper
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        return RenderAsync(context, output, true);
    }
}
