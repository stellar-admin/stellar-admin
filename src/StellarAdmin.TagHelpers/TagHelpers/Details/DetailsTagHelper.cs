using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Lays out read-only display fields in a description list.
/// </summary>
[HtmlTargetElement("sa-details")]
public class DetailsTagHelper : StellarAdminTagHelperBase
{
    private const int MaxColumns = 4;

    /// <summary>
    ///     The number of columns when there is room for them, from 1 to 4.
    /// </summary>
    /// <remarks>
    ///     Defaults to 1. Narrower lists use fewer columns.
    /// </remarks>
    [HtmlAttributeName("columns")]
    public int? Columns { get; set; }

    /// <summary>
    ///     How each field arranges its label and value, unless the field sets its own.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="FieldOrientation.Vertical" />.
    /// </remarks>
    [HtmlAttributeName("orientation")]
    public FieldOrientation? Orientation { get; set; }

    /// <summary>
    ///     The visual style of the list.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="DetailsVariant.Default" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public DetailsVariant? Variant { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveColumns = Columns ?? 1;
        if (effectiveColumns is < 1 or > MaxColumns)
        {
            throw new InvalidOperationException(
                $"The <sa-details> columns attribute must be between 1 and {MaxColumns}."
            );
        }

        var effectiveVariant = Variant ?? DetailsVariant.Default;
        SetContext(
            context,
            new DetailsContext { Columns = effectiveColumns, Orientation = Orientation }
        );

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "details");
        output.Attributes.SetAttribute("data-columns", effectiveColumns);
        output.Attributes.SetAttribute("data-variant", effectiveVariant.GetDataAttributeText());

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-details", output.GetUserSuppliedClass())
        );

        // The wrapper is the container the column breakpoints query; the list is the grid itself
        output.PreContent.SetHtmlContent(
            """<dl class="sa-details-list" data-slot="details-list">"""
        );
        output.PostContent.SetHtmlContent("</dl>");

        return Task.CompletedTask;
    }
}
