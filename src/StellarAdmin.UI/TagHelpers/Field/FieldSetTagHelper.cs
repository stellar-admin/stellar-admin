using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Groups related fields under a common legend, rendered as a <c>&lt;fieldset&gt;</c> element.
/// </summary>
[HtmlTargetElement("sa-field-set")]
public class FieldSetTagHelper : StellarAdminTagHelperBase
{
    public FieldSetTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "fieldset";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "field-set");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(
                new ThemeToken("sa-field-set"),
                "flex flex-col",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
