using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a layout that arranges its children in a row.
/// </summary>
[HtmlTargetElement("sa-group")]
public sealed class GroupTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets how the children align vertically.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the alignment. The default is
    ///     <see cref="GroupAlign.Start" />.
    /// </value>
    [HtmlAttributeName("align")]
    public GroupAlign? Align { get; set; }

    /// <summary>
    ///     Gets or sets the space between the children.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the space. The default is
    ///     <see cref="GroupGap.Default" />.
    /// </value>
    [HtmlAttributeName("gap")]
    public GroupGap? Gap { get; set; }

    /// <summary>
    ///     Gets or sets how the children are spread horizontally.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the spread. The default is
    ///     <see cref="GroupJustify.Start" />.
    /// </value>
    [HtmlAttributeName("justify")]
    public GroupJustify? Justify { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveAlign = Align ?? GroupAlign.Start;
        var effectiveGap = Gap ?? GroupGap.Default;
        var effectiveJustify = Justify ?? GroupJustify.Start;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-group",
                effectiveAlign.GetClass(),
                effectiveGap.GetClassName(),
                effectiveJustify.GetClass(),
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
