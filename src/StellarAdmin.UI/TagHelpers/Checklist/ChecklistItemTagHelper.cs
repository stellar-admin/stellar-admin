using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a row of a checklist: its status, a title and description, and anything else the row holds.
/// </summary>
[HtmlTargetElement("sa-checklist-item")]
public sealed class ChecklistItemTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChecklistItemTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">The icon options that supply the status icons.</param>
    public ChecklistItemTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Gets or sets the description shown under the title.
    /// </summary>
    /// <remarks>
    ///     Use a <c>&lt;sa-checklist-item-description&gt;</c> child instead for a description
    ///     with markup. The item can't have both.
    /// </remarks>
    [HtmlAttributeName("description")]
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the status of the item, which sets its icon.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the status. The default is
    ///     <see cref="ChecklistStatus.Todo" />.
    /// </value>
    /// <remarks>
    ///     Use a <c>&lt;sa-checklist-item-status&gt;</c> child instead for a status of your own.
    ///     The item can't have both.
    /// </remarks>
    [HtmlAttributeName("status")]
    public ChecklistStatus? Status { get; set; }

    /// <summary>
    ///     Gets or sets the title of the item.
    /// </summary>
    /// <remarks>
    ///     Use a <c>&lt;sa-checklist-item-title&gt;</c> child instead for a title with markup or
    ///     a muted title. The item can't have both.
    /// </remarks>
    [HtmlAttributeName("title")]
    public string? Title { get; set; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     The item sets <see cref="Status" />, <see cref="Title" /> or <see cref="Description" />
    ///     and also has the matching child part.
    /// </exception>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "li";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "checklist-item");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-checklist-item", output.GetUserSuppliedClass())
        );

        var item = new ChecklistItemContext();
        SetContext(context, item);

        var childContent = await output.GetChildContentAsync();

        output.Content.SetHtmlContent(
            ChecklistItemRenderingHelper.RenderContent(
                context,
                output,
                item,
                childContent,
                _iconOptions,
                "sa-checklist-item",
                Status,
                Title,
                Description
            )
        );
    }
}
