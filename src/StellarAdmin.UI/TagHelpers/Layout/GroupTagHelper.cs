using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A horizontal flex layout that arranges its children in a row, with configurable
///     alignment, spacing, and justification.
/// </summary>
[HtmlTargetElement("sa-group")]
public class GroupTagHelper : StellarAdminTagHelperBase
{
    public GroupTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

    /// <summary>
    ///     How the children are aligned along the cross axis (vertically).
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="GroupAlign.Start" />.
    /// </remarks>
    [HtmlAttributeName("align")]
    public GroupAlign? Align { get; set; }

    /// <summary>
    ///     The horizontal spacing between children.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="GroupGap.Default" />.
    /// </remarks>
    [HtmlAttributeName("gap")]
    public GroupGap? Gap { get; set; }

    /// <summary>
    ///     How the children are distributed along the main axis (horizontally).
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="GroupJustify.Start" />.
    /// </remarks>
    [HtmlAttributeName("justify")]
    public GroupJustify? Justify { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveAlign = Align ?? GroupAlign.Start;
        var effectiveGap = Gap ?? GroupGap.Default;
        var effectiveJustify = Justify ?? GroupJustify.Start;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute(
            "class",
            BuildClassString(
                "flex flex-row",
                effectiveAlign.GetClass(),
                effectiveGap.GetThemeToken(),
                effectiveJustify.GetClass(),
                output.GetUserSuppliedClass()
            )
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
