namespace StellarAdmin.UI;

/// <summary>The row-selection declaration registered during the collect pass.</summary>
internal sealed class DataGridSelection
{
    /// <summary>The <c>name</c> of the row checkboxes, when the selection should post as form data.</summary>
    public string? Name { get; init; }

    /// <summary>The author's <c>id</c>, applied to the rendered <c>sel-table</c> element.</summary>
    public string? Id { get; init; }

    /// <summary>The author's <c>class</c> attribute, applied to the rendered <c>sel-table</c> element.</summary>
    public string? CssClass { get; init; }
}
