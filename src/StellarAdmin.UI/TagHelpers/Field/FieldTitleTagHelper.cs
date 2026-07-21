using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A title for a field or field set that is styled like a label but is not associated with a control.
/// </summary>
[HtmlTargetElement("sa-field-title")]
public class FieldTitleTagHelper : StellarAdminTagHelperBase
{
    public FieldTitleTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "field-label");
        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-field-title"),
                "flex w-fit items-center leading-snug",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
