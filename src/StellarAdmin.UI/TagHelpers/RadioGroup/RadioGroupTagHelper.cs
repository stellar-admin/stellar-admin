namespace StellarAdmin.UI.TagHelpers;

/*
[HtmlTargetElement("sa-radio-group")]
public class RadioGroupTagHelper(ICssClassMerger classMerger) : StellarAdminTagHelper
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "radiogroup");
        output.Attributes.SetAttribute("data-slot", "radio-group");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("grid gap-3", output.GetUserSuppliedClass())
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
*/
