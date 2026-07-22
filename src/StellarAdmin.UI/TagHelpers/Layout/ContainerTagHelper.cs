using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A centered, width-constrained wrapper that horizontally centers page content and
///     applies responsive horizontal padding.
/// </summary>
[HtmlTargetElement("sa-container")]
public class ContainerTagHelper : StellarAdminTagHelperBase
{
    public ContainerTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute(
            "class",
            BuildClassString("sa-container", output.GetUserSuppliedClass())
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
