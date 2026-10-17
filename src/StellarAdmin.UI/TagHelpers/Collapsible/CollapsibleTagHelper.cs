using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a container whose content can be expanded or collapsed.
/// </summary>
/// <remarks>
///     Toggle it with a button whose <c>commandfor</c> is the collapsible's <c>id</c> and whose
///     <c>command</c> is <c>--toggle</c>, <c>--show</c> or <c>--hide</c>. Requires
///     <c>stellar-admin.js</c>.
/// </remarks>
[HtmlTargetElement("sa-collapsible")]
public sealed class CollapsibleTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "sel-collapsible";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "collapsible");

        return Task.CompletedTask;
    }
}
