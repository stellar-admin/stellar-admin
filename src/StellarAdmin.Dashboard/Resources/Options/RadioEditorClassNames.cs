using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Additional CSS classes for a radio editor and its choices.
/// </summary>
/// <remarks>
///     Root styles the fieldset, Label its legend, and Control the choices container.
///     Use Option.Content for choice content and Error for group validation.
/// </remarks>
public class RadioEditorClassNames : EditorClassNames
{
    /// <summary>
    ///     Additional CSS classes applied to each choice.
    /// </summary>
    /// <remarks>
    ///     Root styles the outer choice or card, Label its title, and Control its radio control.
    /// </remarks>
    public InputClassNames Option { get; } = new();
}
