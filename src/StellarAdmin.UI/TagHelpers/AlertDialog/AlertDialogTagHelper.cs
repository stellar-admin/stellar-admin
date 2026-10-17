using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a modal dialog that asks the user to confirm an important action.
/// </summary>
/// <remarks>
///     The dialog renders as a native <c>&lt;dialog&gt;</c> element. Open it with a button that
///     has <c>commandfor</c> and <c>command="show-modal"</c>. Pressing Esc closes it, but clicking
///     the backdrop doesn't, unless the element sets its own <c>closedby</c>.
/// </remarks>
[HtmlTargetElement("sa-alert-dialog")]
public sealed class AlertDialogTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the size of the dialog.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="AlertDialogSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public AlertDialogSize? Size { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveSize = Size ?? AlertDialogSize.Default;

        output.TagName = "dialog";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "alert-dialog-content");
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());

        // Alert dialogs are not light-dismissable: pressing Esc cancels (resolves the
        // JS helper to `confirmed: false`), but clicking the backdrop does not close.
        // `closerequest` is the native value for exactly that; honour an author override.
        if (!output.Attributes.ContainsName("closedby"))
        {
            output.Attributes.SetAttribute("closedby", "closerequest");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-alert-dialog-content",
                // Establishes the named group the header/title/media tokens react to.
                output.GetUserSuppliedClass()
            )
        );

        // Wrap inside web component (reuses Dialog's sel-dialog: scroll-lock + data-open state).
        output.PreElement.AppendHtml("<sel-dialog>");
        output.PostElement.AppendHtml("</sel-dialog>");

        return Task.CompletedTask;
    }
}
