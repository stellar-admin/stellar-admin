using StellarAdmin.UI;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Stores additional CSS classes for the parts of a radio group editor and its choices.
/// </summary>
/// <remarks>
///     <see cref="FieldClassNames.Root" /> styles the group, <see cref="FieldClassNames.Label" />
///     its legend and <see cref="EditorClassNames.Control" /> the container of the choices. Use
///     <see cref="Option" /> for the parts of each choice.
/// </remarks>
public sealed class RadioGroupEditorClassNames : ChoiceEditorClassNames
{
    /// <summary>
    ///     Gets the additional CSS classes for the parts of each choice.
    /// </summary>
    /// <remarks>
    ///     <see cref="FieldClassNames.Root" /> styles the choice or card,
    ///     <see cref="FieldClassNames.Label" /> its title and
    ///     <see cref="InputClassNames.Control" /> its radio button.
    /// </remarks>
    public InputClassNames Option { get; } = new();
}
