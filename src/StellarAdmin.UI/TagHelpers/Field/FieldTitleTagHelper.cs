using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a title that looks like a field label but isn't tied to a control.
/// </summary>
[HtmlTargetElement("sa-field-title")]
public sealed class FieldTitleTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "field-label");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-field-title", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
