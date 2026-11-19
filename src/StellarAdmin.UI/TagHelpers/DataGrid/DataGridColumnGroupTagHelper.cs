using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Groups data grid columns under a head that spans them.
/// </summary>
/// <remarks>
///     <para>
///         Place <c>sa-data-grid-column</c> elements in the group, and groups in a group to nest
///         them. The header gets a row for each level of groups. A column outside a group spans
///         the group rows.
///     </para>
///     <para>
///         The group's head isn't sortable; its columns keep their sort links. The <c>class</c>
///         applies to the group's head.
///     </para>
/// </remarks>
[HtmlTargetElement("sa-data-grid-column-group")]
public sealed class DataGridColumnGroupTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets how the group aligns its head over its columns.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the alignment. The default is
    ///     <see cref="DataGridColumnGroupAlign.Center" />.
    /// </value>
    [HtmlAttributeName("align")]
    public DataGridColumnGroupAlign? Align { get; set; }

    /// <summary>
    ///     Gets or sets the group's head text.
    /// </summary>
    [HtmlAttributeName("title")]
    public string? Title { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var cssClass = output.GetUserSuppliedClass();
        output.SuppressOutput();

        var gridContext = GetContext<DataGridContext>(context);
        if (gridContext is null)
        {
            return;
        }

        if (!gridContext.Collecting)
        {
            // In a row pass, the group's columns add their cells.
            await output.GetChildContentAsync(useCachedResult: false);
            return;
        }

        var parent = gridContext.CurrentGroup;
        gridContext.CurrentGroup = new DataGridColumnGroup
        {
            Title = string.IsNullOrEmpty(Title) ? null : Title,
            Align = Align ?? DataGridColumnGroupAlign.Center,
            CssClass = string.IsNullOrEmpty(cssClass) ? null : cssClass,
            Parent = parent,
        };
        try
        {
            await output.GetChildContentAsync(useCachedResult: false);
        }
        finally
        {
            gridContext.CurrentGroup = parent;
        }
    }
}
