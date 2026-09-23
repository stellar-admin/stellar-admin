using Microsoft.AspNetCore.Mvc.Rendering;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin;

/// <summary>
///     The configured properties available to a form field's editor.
/// </summary>
public sealed record FormFieldProperties
{
    /// <summary>
    ///     The reference lookup items for the field, or null for an ordinary editor.
    /// </summary>
    public IReadOnlyList<SelectListItem>? LookupItems { get; init; }

    /// <summary>
    ///     The editor's configuration.
    /// </summary>
    public EditorOptions Editor { get; init; } = new();

    /// <summary>
    ///     Whether the field is read-only.
    /// </summary>
    public bool IsReadOnly { get; init; }

    /// <summary>
    ///     The field label, or null to use the property's display name.
    /// </summary>
    public string? Title { get; init; }
}
