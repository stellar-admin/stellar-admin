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
                "[&>[data-slot]~[data-slot]]:rounded-l-none [&>[data-slot]~[data-slot]]:border-l-0 [&>[data-slot]]:rounded-r-none",
                /* StellarAdmin.UI additional classes */
                // Handles sa-select where the select is nested in a container
                "[&>[data-slot]>[data-slot]]:rounded-r-none [&>[data-slot]~[data-slot]>[data-slot]]:rounded-l-none  [&>[data-slot]~[data-slot]>[data-slot]]:border-l-0",
            ],
            [ButtonGroupOrientation.Vertical] =
            [
                new ThemeToken("sa-button-group-orientation-vertical"),
                "flex-col [&>[data-slot]~[data-slot]]:rounded-t-none [&>[data-slot]~[data-slot]]:border-t-0 [&>[data-slot]]:rounded-b-none",
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

    public ButtonGroupTagHelper(ThemeManager themeManager, ICssClassMerger classMerger)
        : base(themeManager, classMerger) { }

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
                new ClassElement[]
                {
                    new ThemeToken("sa-button-group"),
                    "flex w-fit items-stretch [&>*]:focus-visible:z-10 [&>*]:focus-visible:relative [&>[data-slot=select-trigger]:not([class*='w-'])]:w-fit [&>input]:flex-1",
                }
                    .Union(OrientationClasses[effectiveOrientation])
                    .Union([output.GetUserSuppliedClass()])
                    .ToArray()
            )
        );
        return base.ProcessAsync(context, output);
    }
}
