using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     A header cell with the checkbox that selects or clears every row, for an
///     <c>sa-table</c> with <c>selection</c>. It heads the column of
///     <c>sa-table-selection-cell</c> cells.
/// </summary>
[HtmlTargetElement("sa-table-selection-head", TagStructure = TagStructure.WithoutEndTag)]
public class TableSelectionHeadTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;

    public TableSelectionHeadTagHelper(
        IHtmlGenerator htmlGenerator,
        IOptions<IconOptions> iconOptions
    )
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     The checkbox's accessible label.
    /// </summary>
    /// <remarks>
    ///     Defaults to "Select all".
    /// </remarks>
    [HtmlAttributeName("label")]
    public string? Label { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

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
