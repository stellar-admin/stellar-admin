using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A decoration attached to an input group, such as an icon, text, or button, aligned to one of
///     the input's edges. Clicking the add-on focuses the group's input.
/// </summary>
[HtmlTargetElement("sa-input-group-addon")]
public class InputGroupAddOnTagHelper : StellarAdminTagHelperBase
{
    private static readonly Dictionary<
        InputGroupAddOnVariantAlignment,
        string?[]
    > AlignmentClasses = new Dictionary<InputGroupAddOnVariantAlignment, string?[]>
    {
        [InputGroupAddOnVariantAlignment.InlineStart] = ["sa-input-group-addon-align-inline-start"],
        [InputGroupAddOnVariantAlignment.InlineEnd] = ["sa-input-group-addon-align-inline-end"],
        [InputGroupAddOnVariantAlignment.BlockStart] = ["sa-input-group-addon-align-block-start"],
        [InputGroupAddOnVariantAlignment.BlockEnd] = ["sa-input-group-addon-align-block-end"],
    };

    /// <summary>
    ///     Where the add-on is positioned relative to the input.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="InputGroupAddOnVariantAlignment.InlineStart" />.
    /// </remarks>
    [HtmlAttributeName("align")]
    public InputGroupAddOnVariantAlignment? Alignment { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveAlignment = Alignment ?? InputGroupAddOnVariantAlignment.InlineStart;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "group");
        output.Attributes.SetAttribute("data-slot", "input-group-addon");
        output.Attributes.SetAttribute("data-align", effectiveAlignment.GetDataAttributeText());

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
                    .Union(AlignmentClasses[effectiveAlignment])
                    .Append(output.GetUserSuppliedClass())
                    .ToArray()
            )
        );

        return Task.CompletedTask;
    }
}
