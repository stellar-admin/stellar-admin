using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StellarAdmin.UI;

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

    /// <summary>The sort field of a sortable column, substituted for <c>{sortField}</c> in its links.</summary>
    public string? SortField { get; init; }

    /// <summary>Whether the column has an edit template, which editing rows render.</summary>
    public bool HasEditTemplate { get; init; }

    /// <summary>
    ///     The getter of a computed <c>field-for</c> expression, compiled once for the grid's rows,
    ///     or <c>null</c> when the column reads a property chain or no field.
    /// </summary>
    public Func<object, object?>? ComputedValue { get; init; }

    /// <summary>The metadata of a computed expression's return type, which its template sees.</summary>
    public ModelMetadata? ComputedMetadata { get; init; }
}
