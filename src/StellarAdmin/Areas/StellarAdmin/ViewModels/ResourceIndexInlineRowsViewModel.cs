using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The inline rows of an index page, and the row a row request renders in edit mode.
/// </summary>
public sealed class ResourceIndexInlineRowsViewModel
{
    /// <summary>
    ///     The cancel button label.
    /// </summary>
    public required string CancelLabel { get; init; }

    /// <summary>
    ///     The field each column edits in a new row, by column position, or null when resources
    ///     are not created inline.
    /// </summary>
    public IReadOnlyList<FormFieldOptions?>? CreateFields { get; init; }

    /// <summary>
    ///     The field each column edits in an edited row, by column position, or null when
    ///     resources are not edited inline.
    /// </summary>
    public IReadOnlyList<FormFieldOptions?>? EditFields { get; init; }

    /// <summary>
    ///     The key of the row rendered in edit mode, or null.
    /// </summary>
    public string? EditingKey { get; init; }

    /// <summary>
    ///     The edit model of the row rendered in edit mode, or null.
    /// </summary>
    public object? EditModel { get; init; }

    /// <summary>
    ///     The name of the resource's key property, which keys the grid's rows.
    /// </summary>
    public required string KeyField { get; init; }

    /// <summary>
    ///     The create model of the new row, or null when no new row is rendered.
    /// </summary>
    public object? NewItem { get; init; }

    /// <summary>
    ///     The save button label.
    /// </summary>
    public required string SaveLabel { get; init; }
}
