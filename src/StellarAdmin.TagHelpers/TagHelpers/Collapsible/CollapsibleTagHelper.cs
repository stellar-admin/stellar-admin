using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A container whose content can be expanded or collapsed. Toggle it with the Invoker
///     Commands API — a trigger button carrying <c>commandfor</c> and a custom command.
/// </summary>
[HtmlTargetElement("sa-collapsible")]
public class CollapsibleTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "sel-collapsible";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "collapsible");

        return Task.CompletedTask;
    }
}
