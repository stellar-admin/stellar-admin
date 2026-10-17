using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a table cell with a checkbox that selects its row.
/// </summary>
/// <remarks>
///     Use it in a <c>&lt;sa-table&gt;</c> with <c>selection</c>. The checked checkboxes post
///     their values under <see cref="Name" />.
/// </remarks>
[HtmlTargetElement("sa-table-selection-cell", TagStructure = TagStructure.WithoutEndTag)]
public sealed class TableSelectionCellTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TableSelectionCellTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders the checkbox.</param>
    /// <param name="iconOptions">The icon options that supply the checkbox's check mark.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public TableSelectionCellTagHelper(
        IHtmlGenerator htmlGenerator,
        IOptions<IconOptions> iconOptions
    )
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Gets or sets a value that indicates whether the row is selected.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the row is selected; otherwise, <see langword="false" />.
    ///     The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("checked")]
    public bool? Checked { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the row's checkbox is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the row's selection can't be changed; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Selecting or clearing every row leaves a disabled row as it is.
    /// </remarks>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Gets or sets the accessible label of the checkbox.
    /// </summary>
    /// <value>
    ///     A label that names the row the checkbox selects. The default is <c>Select row</c>.
    /// </value>
    [HtmlAttributeName("label")]
    public string? Label { get; set; }

    /// <summary>
    ///     Gets or sets the form field name that the selected rows' values post under.
    /// </summary>
    [HtmlAttributeName("name")]
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the value that identifies the row, such as its key.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <summary>
    ///     Gets or sets the context of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <inheritdoc />
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
