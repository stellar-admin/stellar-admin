using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Renders a divider between items within a button group.
/// </summary>
[HtmlTargetElement("sa-button-group-separator")]
public class ButtonGroupSeparatorTagHelper : StellarAdminTagHelperBase
{
    public ButtonGroupSeparatorTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    /// <summary>
    ///     The orientation of the separator.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="SeparatorOrientation.Vertical" />.
    /// </remarks>
    [HtmlAttributeName("orientation")]
    public SeparatorOrientation? Orientation { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveOrientation = Orientation ?? SeparatorOrientation.Vertical;

        output.Attributes.SetAttribute("data-slot", "button-group-separator");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(
                new ThemeToken("sa-button-group-separator"),
                "relative self-stretch data-[orientation=horizontal]:mx-px data-[orientation=horizontal]:w-auto data-[orientation=vertical]:my-px data-[orientation=vertical]:h-auto",
                output.GetUserSuppliedClass()
            )
        );

        var separatorTagHelper = new SeparatorTagHelper(ClassMerger)
        {
            Orientation = effectiveOrientation,
        };

        await separatorTagHelper.ProcessAsync(context, output);
    }
}
