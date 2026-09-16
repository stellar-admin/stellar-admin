namespace StellarAdmin.TagHelpers;

/// <summary>A column definition registered during the collect pass.</summary>
internal sealed class DataGridColumn
{
    public string? Title { get; init; }

    /// <summary>Rendered header-template HTML, which wins over <see cref="Title" />.</summary>
    public string? HeaderHtml { get; init; }

    /// <summary>The author's <c>class</c> attribute, applied to both the header and body cells.</summary>
    public string? CssClass { get; init; }

    /// <summary>Whether the column header renders a sort link.</summary>
    public bool Sortable { get; init; }

    /// <summary>The sort field of a sortable column, substituted for <c>{sort}</c> in its links.</summary>
    public string? SortField { get; init; }
}
