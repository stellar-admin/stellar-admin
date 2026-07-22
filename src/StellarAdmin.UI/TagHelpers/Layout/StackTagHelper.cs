using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A vertical flex layout that arranges its children in a column, with configurable
///     alignment, spacing, and justification.
/// </summary>
[HtmlTargetElement("sa-stack")]
public class StackTagHelper : StellarAdminTagHelperBase
{
    public StackTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    /// <summary>
    ///     How the children are aligned along the cross axis (horizontally).
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="StackAlign.Stretch" />.
    /// </remarks>
    [HtmlAttributeName("align")]
    public StackAlign? Align { get; set; }

    /// <summary>
    ///     The vertical spacing between children.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="StackGap.Default" />.
    /// </remarks>
    [HtmlAttributeName("gap")]
    public StackGap? Gap { get; set; }

    /// <summary>
    ///     How the children are distributed along the main axis (vertically).
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="StackJustify.Start" />.
    /// </remarks>
    [HtmlAttributeName("justify")]
    public StackJustify? Justify { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveAlign = Align ?? StackAlign.Stretch;
        var effectiveGap = Gap ?? StackGap.Default;
        var effectiveJustify = Justify ?? StackJustify.Start;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute(
            "class",
            BuildClassString(
                new ThemeToken("sa-stack"),
                effectiveAlign.GetClass(),
                effectiveGap.GetThemeToken(),
                effectiveJustify.GetClass(),
                output.GetUserSuppliedClass()
            )
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
