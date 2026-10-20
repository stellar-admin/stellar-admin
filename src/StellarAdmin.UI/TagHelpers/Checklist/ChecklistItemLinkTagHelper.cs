using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a row of a checklist as a link, so the whole row can be clicked.
/// </summary>
[HtmlTargetElement("sa-checklist-item-link")]
public sealed class ChecklistItemLinkTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChecklistItemLinkTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that builds the link URL from the routing attributes.</param>
    /// <param name="iconOptions">The icon options that supply the status icons.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public ChecklistItemLinkTagHelper(
        IHtmlGenerator htmlGenerator,
        IOptions<IconOptions> iconOptions
    )
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
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
        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        await ApplyRouteAttributesAsync(_htmlGenerator, context, output);

        // The list row wraps the link, so the row's divider stays outside its hover background.
        output.PreElement.AppendHtml("<li class=\"sa-checklist-row\">");
        output.PostElement.AppendHtml("</li>");

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
                "sa-checklist-item-link",
                Status,
                Title,
                Description
            )
        );
    }
}
