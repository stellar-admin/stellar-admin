namespace StellarAdmin.TagHelpers;

/*
[HtmlTargetElement("sa-checkbox-group")]
public class CheckboxGroupTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "checkboxgroup");
        output.Attributes.SetAttribute("data-slot", "checkbox-group");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("grid gap-3", output.GetUserSuppliedClass())
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
*/
