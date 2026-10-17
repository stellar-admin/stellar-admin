using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a modal window over the page.
/// </summary>
/// <remarks>
///     The dialog renders as a native <c>&lt;dialog&gt;</c> element. Open it with a button that
///     has <c>commandfor</c> and <c>command="show-modal"</c>, and close it with one that has
///     <c>command="close"</c>. Requires <c>stellar-admin.js</c> to lock page scrolling while the
///     dialog is open.
/// </remarks>
[HtmlTargetElement("sa-dialog")]
public class DialogTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "dialog";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "dialog-content");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-dialog-content", output.GetUserSuppliedClass())
        );

        // Wrap inside web component
        output.PreElement.AppendHtml("<sel-dialog>");
        output.PostElement.AppendHtml("</sel-dialog>");

        return Task.CompletedTask;
    }
}
