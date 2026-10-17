using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a button that expands and collapses a table row's details.
/// </summary>
/// <remarks>
///     Place the button in any cell of the row. It controls the
///     <c>&lt;sa-table-row-detail&gt;</c> after its row, or the one that its
///     <c>aria-controls</c> attribute names. The button shows a chevron unless you give it
///     content.
/// </remarks>
[HtmlTargetElement("sa-table-row-detail-toggle")]
public sealed class TableRowDetailToggleTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TableRowDetailToggleTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">The icon options that supply the chevron icon.</param>
    public TableRowDetailToggleTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Gets or sets a value that indicates whether the button expands and collapses every row's
    ///     details.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the button controls every row; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Place a button that controls every row in a header cell. It is hidden when only one row
    ///     can be expanded at a time.
    /// </remarks>
    [HtmlAttributeName("all")]
    public bool? All { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var all = All ?? false;

        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (!output.Attributes.ContainsName("type"))
        {
            output.Attributes.SetAttribute("type", "button");
        }

        if (!output.Attributes.ContainsName("aria-label"))
        {
            output.Attributes.SetAttribute(
                "aria-label",
                all ? "Toggle all row details" : "Toggle row details"
            );
        }

        output.Attributes.SetAttribute("aria-expanded", "false");
        output.Attributes.SetAttribute("data-slot", "table-row-detail-toggle");
        if (all)
        {
            output.Attributes.SetAttribute("data-all", "true");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-table-row-detail-toggle", output.GetUserSuppliedClass())
        );

        ButtonRenderingHelper.RenderAttributes(output, ButtonVariant.Ghost, ButtonSize.IconSmall);

        var content = await output.GetChildContentAsync();
        if (content.IsEmptyOrWhiteSpace)
        {
            var icon = new TagHelperOutput(
                "svg",
                [],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );

            new IconTagHelper(_iconOptions)
            {
                Name = _iconOptions.GetSemanticIconName(SemanticIconRole.RowDetailIndicator),
            }.Process(context, icon);

            output.Content.SetHtmlContent(icon);
        }
        else
        {
            output.Content.SetHtmlContent(content);
        }
    }
}
