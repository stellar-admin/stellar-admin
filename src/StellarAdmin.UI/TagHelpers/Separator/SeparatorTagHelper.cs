using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a thin line between sections of content.
/// </summary>
[HtmlTargetElement("sa-separator")]
public class SeparatorTagHelper : StellarAdminTagHelperBase
{
    private static readonly Dictionary<SeparatorOrientation, string> OrientationClasses = new()
    {
        [SeparatorOrientation.Horizontal] = "sa-separator-horizontal",
        [SeparatorOrientation.Vertical] = "sa-separator-vertical",
    };

    /// <summary>
    ///     Gets or sets the orientation of the separator.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the orientation. The default is
    ///     <see cref="SeparatorOrientation.Horizontal" />.
    /// </value>
    [HtmlAttributeName("orientation")]
    public SeparatorOrientation? Orientation { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveOrientation = Orientation ?? SeparatorOrientation.Horizontal;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "separator");
        output.Attributes.SetAttribute(
            "aria-orientation",
            effectiveOrientation.GetDataAttributeText()
        );
        output.Attributes.SetAttribute("data-slot", "separator");
        output.Attributes.SetAttribute(
            "data-orientation",
            effectiveOrientation.GetDataAttributeText()
        );

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-separator",
                OrientationClasses[effectiveOrientation],
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
