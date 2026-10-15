using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Adds row selection to the data grid: a leading checkbox column with a select-all
///     checkbox in the header. Requires the grid's <c>key-field</c>, whose value each row's
///     checkbox carries. Renders nothing itself — the grid wraps its table in the
///     <c>sel-table</c> web component, to which an <c>id</c> and <c>class</c> set
///     here are transferred. Read the current selection from the element's
///     <c>selectedValues</c> property, or listen for its bubbling <c>selection-change</c>
///     event; with a <c>name</c> the selection also posts as ordinary checkbox form data.
/// </summary>
[HtmlTargetElement(
    "sa-data-grid-selection",
    ParentTag = "sa-data-grid",
    TagStructure = TagStructure.WithoutEndTag
)]
public class DataGridSelectionTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The <c>name</c> of the row checkboxes, making the selected keys post as ordinary
    ///     form data. When omitted, the checkboxes have no name and the selection is only
    ///     available client-side.
    /// </summary>
    [HtmlAttributeName("name")]
    public string? Name { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.SuppressOutput();

        var gridContext = GetContext<DataGridContext>(context);
        if (gridContext is not { Collecting: true })
        {
            // The data grid re-executes its child content once per row; the selection
            // declaration only registers during the collect pass.
            return Task.CompletedTask;
        }

        if (gridContext.KeyField is null)
        {
            throw new InvalidOperationException(
                "<sa-data-grid-selection> requires the key-field attribute on the grid."
            );
        }

        gridContext.Selection = new DataGridSelection
        {
            Name = Name,
            Id = output.Attributes.TryGetAttribute("id", out var idAttribute)
                ? idAttribute.Value?.ToString()
                : null,
            CssClass = output.GetUserSuppliedClass() is { Length: > 0 } cssClass ? cssClass : null,
        };
        return Task.CompletedTask;
    }
}
