namespace StellarAdmin.TagHelpers;

/// <summary>The row details declaration registered during the collect pass.</summary>
internal sealed class DataGridRowDetail
{
    /// <summary>Where the grid places the toggle buttons.</summary>
    public required DataGridRowDetailToggle Toggle { get; init; }

    /// <summary>The settings rendered on the <c>sel-table-row-details</c> element.</summary>
    public required TableRowDetailSettings Settings { get; init; }

    /// <summary>The row item property compared against <see cref="ExpandedKeys" />.</summary>
    public string? KeyField { get; init; }

    /// <summary>The keys of the rows rendered expanded, as strings.</summary>
    public required IReadOnlySet<string> ExpandedKeys { get; init; }
}
