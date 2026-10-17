using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Adds a checkbox column to a data grid, so users can select rows.
/// </summary>
/// <remarks>
///     <para>
///         Requires the grid's <c>key-field</c>. Each row's checkbox posts the row's key, and the
///         header has a checkbox that selects all rows.
///     </para>
///     <para>
///         The grid wraps its table in a <c>sel-table</c> element, which gets the <c>id</c> and
///         <c>class</c> set here. Read the selection from the element's <c>selectedValues</c>
///         property, or listen for its <c>rowselectionchange</c> event. Requires
///         <c>stellar-admin.js</c>.
///     </para>
/// </remarks>
[HtmlTargetElement(
    "sa-data-grid-selection",
    ParentTag = "sa-data-grid",
    TagStructure = TagStructure.WithoutEndTag
)]
public class DataGridSelectionTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the name of the row checkboxes, so the selected keys post with the form.
    /// </summary>
    /// <remarks>
    ///     Without a name, the selection is only available in the browser.
    /// </remarks>
    [HtmlAttributeName("name")]
    public string? Name { get; set; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     The grid has no <c>key-field</c>.
    /// </exception>
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
