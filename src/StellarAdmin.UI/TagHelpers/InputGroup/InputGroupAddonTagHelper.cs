using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an add-on, such as an icon, text or button, at one edge of an input group.
/// </summary>
/// <remarks>
///     Clicking the add-on, outside its buttons, focuses the group's input.
/// </remarks>
[HtmlTargetElement("sa-input-group-addon")]
public sealed class InputGroupAddonTagHelper : StellarAdminTagHelperBase
{
    private static readonly Dictionary<InputGroupAddonAlign, string?[]> AlignClasses =
        new Dictionary<InputGroupAddonAlign, string?[]>
        {
            [InputGroupAddonAlign.Start] = ["sa-input-group-addon-align-start"],
            [InputGroupAddonAlign.End] = ["sa-input-group-addon-align-end"],
            [InputGroupAddonAlign.Top] = ["sa-input-group-addon-align-top"],
            [InputGroupAddonAlign.Bottom] = ["sa-input-group-addon-align-bottom"],
        };

    /// <summary>
    ///     Gets or sets where the add-on is placed relative to the input.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the placement. The default is
    ///     <see cref="InputGroupAddonAlign.Start" />.
    /// </value>
    [HtmlAttributeName("align")]
    public InputGroupAddonAlign? Align { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveAlign = Align ?? InputGroupAddonAlign.Start;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "group");
        output.Attributes.SetAttribute("data-slot", "input-group-addon");
        output.Attributes.SetAttribute("data-align", effectiveAlign.GetDataAttributeText());

        output.Attributes.SetAttribute(
            "onclick",
            """
            (function(e) {
              if ((e.target).closest('button')) {
                return;
              }
              e.currentTarget.parentElement?.querySelector('input')?.focus();
            })(event);
            """
        );

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                new string?[] { "sa-input-group-addon" }
                    .Union(AlignClasses[effectiveAlign])
                    .Append(output.GetUserSuppliedClass())
                    .ToArray()
            )
        );

        return Task.CompletedTask;
    }
}
