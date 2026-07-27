using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Groups related buttons together as a single visual unit.
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
    ///     The direction in which the group lays out its items.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ButtonGroupOrientation.Horizontal" />.
    /// </remarks>
    [HtmlAttributeName("orientation")]
    public ButtonGroupOrientation? Orientation { get; set; }

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
