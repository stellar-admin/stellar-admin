using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Areas.StellarAdmin;

/// <summary>
///     Represents the settings of a form field that its editor template can use.
/// </summary>
public sealed record FormFieldProperties
{
    /// <summary>
    ///     Gets the help text shown with the field, or <see langword="null" /> to use the
    ///     property's description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    ///     Gets the data prepared for the editor on the current request, or
    ///     <see langword="null" /> when there is none.
    /// </summary>
    public object? EditorData { get; init; }

    /// <summary>
    ///     Gets the editor's settings, or <see langword="null" /> when the field has no editor.
    /// </summary>
    public FieldEditor? Editor { get; init; }

    /// <summary>
    ///     Gets the property path of the field, or <see langword="null" /> when the editor isn't
    ///     rendered for a configured field.
    /// </summary>
    public string? FieldName { get; init; }

    /// <summary>
    ///     Gets a value that indicates whether the field is read-only.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the field is read-only; otherwise, <see langword="false" />.
    /// </value>
    public bool IsReadOnly { get; init; }

    /// <summary>
    ///     Gets the field's label, or <see langword="null" /> to use the property's display name.
    /// </summary>
    public string? Title { get; init; }

    // A field with a binder edits without its property's setter
    internal bool IsBound { get; init; }
}
