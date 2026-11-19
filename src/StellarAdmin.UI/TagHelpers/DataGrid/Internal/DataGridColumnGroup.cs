namespace StellarAdmin.UI;

/// <summary>A column group registered during the collect pass.</summary>
internal sealed class DataGridColumnGroup
{
    public string? Title { get; init; }

    public DataGridColumnGroupAlign Align { get; init; }

    /// <summary>The author's <c>class</c> attribute, applied to the group's header cell.</summary>
    public string? CssClass { get; init; }

    /// <summary>The group this group sits in, or <c>null</c> for a group in the grid.</summary>
    public DataGridColumnGroup? Parent { get; init; }
}
