using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A flexible row for presenting content, combining media, a title, description, and actions.
/// </summary>
[HtmlTargetElement("sa-item")]
public class ItemTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The size of the item, controlling its padding and spacing.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ItemSize.Default" />.
    /// </remarks>
    [HtmlAttributeName("size")]
    public ItemSize? Size { get; set; }

    /// <summary>
    ///     The visual style of the item.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ItemVariant.Default" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public ItemVariant? Variant { get; set; }

    public ItemTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        await ItemRenderingHelper.RenderAsync(output, ClassMerger, Size, Variant);
    }
}
