namespace StellarAdmin.UI;

/// <summary>
///     The attributes of the <c>sel-table</c> element that <c>sa-table</c> and the data grid
///     wrap their table in, switching on the behaviors they use.
/// </summary>
internal static class TableElementRendering
{
    public static IEnumerable<KeyValuePair<string, string>> GetAttributes(
        bool selection,
        TableRowDetailSettings? rowDetails
    )
    {
        yield return new("data-slot", "table-interactivity");
        if (selection)
        {
            yield return new("data-selection", "");
        }

        if (rowDetails is not null)
        {
            yield return new("class", "sa-table-row-details");
            foreach (var attribute in TableRowDetailRendering.GetWrapperAttributes(rowDetails))
            {
                yield return attribute;
            }
        }
    }
}
