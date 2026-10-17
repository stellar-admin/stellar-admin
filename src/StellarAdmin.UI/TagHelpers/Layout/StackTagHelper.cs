using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a layout that arranges its children in a column.
/// </summary>
[HtmlTargetElement("sa-stack")]
public class StackTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets how the children align horizontally.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the alignment. The default is
    ///     <see cref="StackAlign.Stretch" />.
    /// </value>
    [HtmlAttributeName("align")]
    public StackAlign? Align { get; set; }

    /// <summary>
    ///     Gets or sets the space between the children.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the space. The default is
    ///     <see cref="StackGap.Default" />.
    /// </value>
    [HtmlAttributeName("gap")]
    public StackGap? Gap { get; set; }

    /// <summary>
    ///     Gets or sets how the children are spread vertically.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the spread. The default is
    ///     <see cref="StackJustify.Start" />.
    /// </value>
    [HtmlAttributeName("justify")]
    public StackJustify? Justify { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveAlign = Align ?? StackAlign.Stretch;
        var effectiveGap = Gap ?? StackGap.Default;
        var effectiveJustify = Justify ?? StackJustify.Start;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-stack",
                effectiveAlign.GetClass(),
                effectiveGap.GetClassName(),
                effectiveJustify.GetClass(),
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
