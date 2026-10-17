using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a group of related fields as a <c>&lt;fieldset&gt;</c> element.
/// </summary>
/// <remarks>
///     Use an <c>&lt;sa-field-legend&gt;</c> to caption the group.
/// </remarks>
[HtmlTargetElement("sa-field-set")]
public sealed class FieldSetTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "fieldset";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "field-set");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-field-set", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
