using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures a checkbox group editor for a collection property.
/// </summary>
public sealed class CheckboxGroupEditor : ChoiceEditor, IFieldEditor<CheckboxGroupEditorHandler>
{
    /// <summary>
    ///     How the choices are displayed. Defaults to <see cref="CheckboxGroupAppearance.Default" />.
    /// </summary>
    public CheckboxGroupAppearance Appearance { get; set; }

    /// <summary>
    ///     The order in which the choices fill the columns set with <see cref="Columns(int)" />. Defaults to
    ///     <see cref="CheckboxGroupFlow.Down" />.
    /// </summary>
    public CheckboxGroupFlow Flow { get; set; }

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
