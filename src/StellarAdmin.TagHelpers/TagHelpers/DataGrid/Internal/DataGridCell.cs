namespace StellarAdmin.TagHelpers;

/// <summary>A rendered body cell produced during a row pass.</summary>
internal readonly record struct DataGridCell(string? CssClass, string Html);
