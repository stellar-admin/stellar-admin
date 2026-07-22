using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A run of text or an icon displayed inside an input group, typically within an add-on.
/// </summary>
[HtmlTargetElement("sa-input-group-text")]
public class InputGroupTextTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-input-group-text", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
