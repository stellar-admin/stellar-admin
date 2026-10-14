using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a radio group editor.
/// </summary>
public sealed class RadioGroupEditor : ChoiceEditor, IFieldEditor<RadioGroupEditorHandler>
{
    /// <summary>
    ///     How the choices are displayed. Defaults to <see cref="RadioGroupAppearance.Default" />.
    /// </summary>
    public RadioGroupAppearance Appearance { get; set; }

    /// <summary>
    ///     The order in which the choices fill the columns set with <see cref="Columns(int)" />. Defaults to
    ///     <see cref="RadioGroupFlow.Across" />.
    /// </summary>
    public RadioGroupFlow Flow { get; set; }

    /// <summary>
    ///     Additional CSS classes for the radio editor and its choices.
    /// </summary>
    public override RadioGroupEditorClassNames ClassNames { get; } = new();

    internal FormGridColumnDefinitions? ColumnDefinitions { get; private set; }

    /// <summary>
    ///     Arranges the choices in columns, starting at the medium breakpoint.
    /// </summary>
    /// <remarks>
    ///     Breakpoints are measured against the field's width. Below the medium breakpoint, the choices use one
    ///     column.
    /// </remarks>
    public void Columns(int count)
    {
        ColumnDefinitions = FormGridColumnDefinitions.FromCount(count);
    }

    /// <summary>
    ///     Arranges the choices in columns at each breakpoint.
    /// </summary>
    /// <remarks>
    ///     Breakpoints are measured against the field's width.
    /// </remarks>
    public void Columns(Action<GridColumnsBuilder> configure)
    {
        ColumnDefinitions = GridColumnsBuilder.Build(configure);
    }
}
