using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a table header cell with a checkbox that selects or clears every row.
/// </summary>
/// <remarks>
///     Use it in a <c>&lt;sa-table&gt;</c> with <c>selection</c>, above the column of
///     <c>&lt;sa-table-selection-cell&gt;</c> cells.
/// </remarks>
[HtmlTargetElement("sa-table-selection-head", TagStructure = TagStructure.WithoutEndTag)]
public class TableSelectionHeadTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TableSelectionHeadTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders the checkbox.</param>
    /// <param name="iconOptions">The icon options that supply the checkbox's check mark.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public TableSelectionHeadTagHelper(
        IHtmlGenerator htmlGenerator,
        IOptions<IconOptions> iconOptions
    )
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Gets or sets the accessible label of the checkbox.
    /// </summary>
    /// <value>
    ///     The label. The default is <c>Select all</c>.
    /// </value>
    [HtmlAttributeName("label")]
    public string? Label { get; set; }

    /// <summary>
    ///     Gets or sets the context of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "th";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "table-head");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-table-head", output.GetUserSuppliedClass())
        );
        if (!output.Attributes.ContainsName("style"))
        {
            output.Attributes.SetAttribute("style", "width: 3rem");
        }

        output.Content.SetHtmlContent(
            await TableSelectionRendering.RenderCheckboxAsync(
                _htmlGenerator,
                _iconOptions,
                ViewContext,
                context,
                [new("type", "checkbox"), new("aria-label", Label ?? "Select all")]
            )
        );
    }
}
