using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a container that keeps a field's label and description stacked beside its control.
/// </summary>
/// <remarks>
///     Use it in a horizontal field.
/// </remarks>
[HtmlTargetElement("sa-field-content")]
public sealed class FieldContentTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "field-content");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-field-content", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
