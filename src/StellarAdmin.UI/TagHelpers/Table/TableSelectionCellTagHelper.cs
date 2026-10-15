using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     A cell with the checkbox that selects its row, for an <c>sa-table</c> with
///     <c>selection</c>. The checked checkboxes post their values under <c>name</c>.
/// </summary>
[HtmlTargetElement("sa-table-selection-cell", TagStructure = TagStructure.WithoutEndTag)]
public class TableSelectionCellTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;

    public TableSelectionCellTagHelper(
        IHtmlGenerator htmlGenerator,
        IOptions<IconOptions> iconOptions
    )
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Whether the row is selected.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    [HtmlAttributeName("checked")]
    public bool? Checked { get; set; }

    /// <summary>
    ///     Whether the row cannot be selected. Select-all leaves it as it is.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     The checkbox's accessible label, naming the row it selects.
    /// </summary>
    /// <remarks>
    ///     Defaults to "Select row".
    /// </remarks>
    [HtmlAttributeName("label")]
    public string? Label { get; set; }

    /// <summary>
    ///     The form field name the selected rows' values post under.
    /// </summary>
    [HtmlAttributeName("name")]
    public string? Name { get; set; }

    /// <summary>
    ///     The value that identifies the row, such as its key.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "td";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "table-cell");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-table-cell", output.GetUserSuppliedClass())
        );

        var attributes = new List<TagHelperAttribute>
        {
            new("type", "checkbox"),
            new("data-row-select"),
            new("aria-label", Label ?? "Select row"),
        };
        if (Name is not null)
        {
            attributes.Add(new("name", Name));
        }

        if (Value is not null)
        {
            attributes.Add(new("value", Value));
        }

        if (Checked == true)
        {
            attributes.Add(new("checked", "checked"));
        }

        if (Disabled == true)
        {
            attributes.Add(new("disabled", "disabled"));
        }

        output.Content.SetHtmlContent(
            await TableSelectionRendering.RenderCheckboxAsync(
                _htmlGenerator,
                _iconOptions,
                ViewContext,
                context,
                attributes
            )
        );
    }
}
