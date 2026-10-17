namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a toggle group editor.
/// </summary>
/// <remarks>
///     A collection property selects several values. Any other property selects one.
/// </remarks>
public sealed class ToggleGroupEditor : ChoiceEditor
{
    /// <summary>
    ///     Gets or sets how the choices are displayed.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies how the choices are displayed. The default
    ///     is <see cref="ToggleGroupAppearance.Chips" />.
    /// </value>
    public ToggleGroupAppearance Appearance { get; set; }

    /// <summary>
    ///     Gets or sets where chips show their check mark while on.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies where the check mark is shown. The default
    ///     is <see cref="ToggleGroupCheckPlacement.ReplaceMedia" />.
    /// </value>
    /// <remarks>
    ///     Only <see cref="ToggleGroupAppearance.Chips" /> shows a check mark.
    /// </remarks>
    public ToggleGroupCheckPlacement CheckPlacement { get; set; }

    /// <inheritdoc />
    public override string TemplateName => "Editors/ToggleGroup";
}
