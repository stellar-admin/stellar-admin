using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     Represents the inline rows of an index page.
/// </summary>
public sealed class ResourceIndexInlineRowsViewModel
{
    /// <summary>
    ///     Gets the label of the button that saves the new row and opens another, or
    ///     <see langword="null" /> when the new row has no such button.
    /// </summary>
    public string? AddAnotherLabel { get; init; }

    /// <summary>
    ///     Gets the number of resources added with the add another button since the new row was
    ///     first opened.
    /// </summary>
    public int AddedCount { get; init; }

    /// <summary>
    ///     Gets the label of the cancel button.
    /// </summary>
    public required string CancelLabel { get; init; }

    /// <summary>
    ///     Gets the inline create settings, or <see langword="null" /> when resources aren't
    ///     created inline.
    /// </summary>
    public ResourceInlineCreateOptions? Create { get; init; }

    /// <summary>
    ///     Gets the field that each column edits in a new row, by column position, or
    ///     <see langword="null" /> when resources aren't created inline.
    /// </summary>
    /// <remarks>
    ///     An item is <see langword="null" /> for a column that doesn't edit a field.
    /// </remarks>
    public IReadOnlyList<FormFieldOptions?>? CreateFields { get; init; }

    /// <summary>
    ///     Gets the inline edit settings, or <see langword="null" /> when resources aren't
    ///     edited inline.
    /// </summary>
    public ResourceInlineEditOptions? Edit { get; init; }

    /// <summary>
    ///     Gets the field that each column edits in an edited row, by column position, or
    ///     <see langword="null" /> when resources aren't edited inline.
    /// </summary>
    /// <remarks>
    ///     An item is <see langword="null" /> for a column that doesn't edit a field.
    /// </remarks>
    public IReadOnlyList<FormFieldOptions?>? EditFields { get; init; }

    /// <summary>
    ///     Gets the key of the row that is being edited, or <see langword="null" /> when no row
    ///     is being edited.
    /// </summary>
    public string? EditingKey { get; init; }

    /// <summary>
    ///     Gets the edit model of the row that is being edited, or <see langword="null" /> when
    ///     no row is being edited.
    /// </summary>
    public object? EditModel { get; init; }

    /// <summary>
    ///     Gets the name of the resource's key property.
    /// </summary>
    public required string KeyField { get; init; }

    /// <summary>
    ///     Gets the create model of the new row, or <see langword="null" /> when there is no new
    ///     row.
    /// </summary>
    public object? NewItem { get; init; }

    /// <summary>
    ///     Gets the label of the save button.
    /// </summary>
    public required string SaveLabel { get; init; }

    internal ResourceIndexInlineRowsViewModel() { }
}
