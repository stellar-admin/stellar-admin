using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a separator between items in a button group.
/// </summary>
[HtmlTargetElement("sa-button-group-separator")]
public class ButtonGroupSeparatorTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the orientation of the separator.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the orientation. The default is
    ///     <see cref="ButtonGroupSeparatorOrientation.Vertical" />.
    /// </value>
    [HtmlAttributeName("orientation")]
    public ButtonGroupSeparatorOrientation? Orientation { get; set; }

    /// <inheritdoc />
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
