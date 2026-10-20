using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a status of your own for a checklist item, such as a warning or a failure, in place
///     of the built-in status icon.
/// </summary>
/// <remarks>
///     Put an icon, or any other content, inside it. The item's row is styled as no particular
///     status.
/// </remarks>
[HtmlTargetElement("sa-checklist-item-status")]
public sealed class ChecklistItemStatusTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the name of the status that assistive technology announces before the
    ///     title, such as "Failed". Required.
    /// </summary>
    [HtmlAttributeName("label")]
    public string? Label { get; set; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     <see cref="Label" /> is not set.
    /// </exception>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(Label))
        {
            throw new InvalidOperationException(
                "The 'label' attribute is required on <sa-checklist-item-status>."
            );
        }

        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "checklist-item-status");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-checklist-item-status", output.GetUserSuppliedClass())
        );

        if (GetContext<ChecklistItemContext>(context) is { } item)
        {
            item.HasStatus = true;
        }

        output.Content.AppendHtml(await output.GetChildContentAsync());
        output.Content.AppendHtml(ChecklistItemRenderingHelper.RenderStatusText(Label));
    }
}
