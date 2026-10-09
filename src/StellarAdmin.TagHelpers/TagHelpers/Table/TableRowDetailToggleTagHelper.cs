using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The button that expands and collapses a row's details, placed in any cell of the row.
///     It controls the <c>sa-table-row-detail</c> after its row, or the one named by an
///     <c>aria-controls</c> attribute. Renders a chevron when it has no content.
/// </summary>
[HtmlTargetElement("sa-table-row-detail-toggle")]
public class TableRowDetailToggleTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    public TableRowDetailToggleTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Whether the button expands and collapses every row's details instead of one row's,
    ///     typically placed in a header cell. It is hidden when only one row can be expanded.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    [HtmlAttributeName("all")]
    public bool? All { get; set; }

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
