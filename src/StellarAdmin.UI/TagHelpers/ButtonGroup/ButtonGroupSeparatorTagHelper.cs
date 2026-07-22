using Microsoft.AspNetCore.Razor.TagHelpers;

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
            BuildClassString("sa-button-group-separator", output.GetUserSuppliedClass())
        );

        var separatorTagHelper = new SeparatorTagHelper(ClassMerger)
        {
            Orientation = effectiveOrientation,
        };

        await separatorTagHelper.ProcessAsync(context, output);
    }
}
