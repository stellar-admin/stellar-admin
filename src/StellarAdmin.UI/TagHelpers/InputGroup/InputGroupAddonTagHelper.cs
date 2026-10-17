using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     A decoration attached to an input group, such as an icon, text, or button, aligned to one of
///     the input's edges. Clicking the add-on focuses the group's input.
/// </summary>
[HtmlTargetElement("sa-input-group-addon")]
public class InputGroupAddonTagHelper : StellarAdminTagHelperBase
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
    ///     Where the add-on is positioned relative to the input.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="InputGroupAddonAlign.Start" />.
    /// </remarks>
    public InputGroupAddonAlign? Align { get; set; }

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
