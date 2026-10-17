using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders related buttons as a single unit.
/// </summary>
[HtmlTargetElement("sa-button-group")]
public class ButtonGroupTagHelper : StellarAdminTagHelperBase
{
    private static readonly Dictionary<ButtonGroupOrientation, string?[]> OrientationClasses =
        new Dictionary<ButtonGroupOrientation, string?[]>
        {
            [ButtonGroupOrientation.Horizontal] = ["sa-button-group-orientation-horizontal"],
            [ButtonGroupOrientation.Vertical] = ["sa-button-group-orientation-vertical"],
        };

    /// <summary>
    ///     Gets or sets the direction in which the group arranges its items.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the direction. The default is
    ///     <see cref="ButtonGroupOrientation.Horizontal" />.
    /// </value>
    [HtmlAttributeName("orientation")]
    public ButtonGroupOrientation? Orientation { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveOrientation = Orientation ?? ButtonGroupOrientation.Horizontal;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "group");
        output.Attributes.SetAttribute("data-slot", "button-group");
        output.Attributes.SetAttribute(
            "data-orientation",
            effectiveOrientation.GetDataAttributeText()
        );
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                new string?[] { "sa-button-group" }
                    .Union(OrientationClasses[effectiveOrientation])
                    .Union([output.GetUserSuppliedClass()])
                    .ToArray()
            )
        );
        return base.ProcessAsync(context, output);
    }
}
