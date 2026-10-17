using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a divider between items within a button group.
/// </summary>
[HtmlTargetElement("sa-button-group-separator")]
public class ButtonGroupSeparatorTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The orientation of the separator.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ButtonGroupSeparatorOrientation.Vertical" />.
    /// </remarks>
    [HtmlAttributeName("orientation")]
    public ButtonGroupSeparatorOrientation? Orientation { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveOrientation = Orientation switch
        {
            ButtonGroupSeparatorOrientation.Horizontal => SeparatorOrientation.Horizontal,
            _ => SeparatorOrientation.Vertical,
        };

        output.Attributes.SetAttribute("data-slot", "button-group-separator");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-button-group-separator", output.GetUserSuppliedClass())
        );

        var separatorTagHelper = new SeparatorTagHelper() { Orientation = effectiveOrientation };

        await separatorTagHelper.ProcessAsync(context, output);
    }
}
