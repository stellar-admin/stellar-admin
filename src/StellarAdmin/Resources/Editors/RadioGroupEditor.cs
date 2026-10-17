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
    ///     Gets or sets how the choices are displayed.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies how the choices are displayed. The default
    ///     is <see cref="RadioGroupAppearance.Default" />.
    /// </value>
    public RadioGroupAppearance Appearance { get; set; }

    /// <summary>
    ///     Gets or sets the order in which the choices fill their columns.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the order in which the choices fill their
    ///     columns. The default is <see cref="RadioGroupFlow.Across" />.
    /// </value>
    public RadioGroupFlow Flow { get; set; }

    /// <summary>
    ///     Gets the additional CSS classes for the parts of the editor and its choices.
    /// </summary>
    public override RadioGroupEditorClassNames ClassNames { get; } = new();

    internal FormGridColumnDefinitions? ColumnDefinitions { get; private set; }

    /// <summary>
    ///     Arranges the choices in columns, starting at the medium breakpoint.
    /// </summary>
    /// <param name="count">The number of columns, from 1 to 12.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="count" /> is less than 1 or greater than 12.
    /// </exception>
    /// <remarks>
    ///     The default is 1. A narrower field uses one column. Breakpoints are measured against the
    ///     field's width.
    /// </remarks>
    public void Columns(int count)
    {
        ColumnDefinitions = FormGridColumnDefinitions.FromCount(count);
    }

    /// <summary>
    ///     Arranges the choices in columns at each breakpoint.
    /// </summary>
    /// <param name="configure">
    ///     A delegate that sets the number of columns at each breakpoint.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Breakpoints are measured against the field's width.
    /// </remarks>
    public void Columns(Action<GridColumnsBuilder> configure)
    {
        ColumnDefinitions = GridColumnsBuilder.Build(configure);
    }
}
