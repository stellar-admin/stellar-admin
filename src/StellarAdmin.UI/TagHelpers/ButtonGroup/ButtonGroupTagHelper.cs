using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Groups related buttons together as a single visual unit.
/// </summary>
[HtmlTargetElement("sa-button-group")]
public class ButtonGroupTagHelper : StellarAdminTagHelperBase
{
    private static readonly Dictionary<ButtonGroupOrientation, ClassElement[]> OrientationClasses =
        new Dictionary<ButtonGroupOrientation, ClassElement[]>
        {
            [ButtonGroupOrientation.Horizontal] =
            [
                new ThemeToken("sa-button-group-orientation-horizontal"),
            ],
            [ButtonGroupOrientation.Vertical] =
            [
                new ThemeToken("sa-button-group-orientation-vertical"),
            ],
        };

    /// <summary>
    ///     The direction in which the group lays out its items.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ButtonGroupOrientation.Horizontal" />.
    /// </remarks>
    [HtmlAttributeName("orientation")]
    public ButtonGroupOrientation? Orientation { get; set; }

    public ButtonGroupTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

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
            BuildClassString(
                new ClassElement[] { new ThemeToken("sa-button-group") }
                    .Union(OrientationClasses[effectiveOrientation])
                    .Union([output.GetUserSuppliedClass()])
                    .ToArray()
            )
        );
        return base.ProcessAsync(context, output);
    }
}
