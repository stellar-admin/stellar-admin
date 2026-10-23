using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a checkbox group editor for a collection property.
/// </summary>
public class CheckboxGroupEditor : ChoiceEditor
{
    /// <summary>
    ///     Gets or sets how the choices are displayed.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies how the choices are displayed. The default
    ///     is <see cref="CheckboxGroupAppearance.Default" />.
    /// </value>
    public CheckboxGroupAppearance Appearance { get; set; }

    /// <summary>
    ///     Gets or sets the order in which the choices fill their columns.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the order in which the choices fill their
    ///     columns. The default is <see cref="CheckboxGroupFlow.Across" />.
    /// </value>
    public CheckboxGroupFlow Flow { get; set; }

    /// <inheritdoc />
    public override string TemplateName => "Editors/CheckboxGroup";

    internal FormGridColumnDefinitions? ColumnDefinitions { get; set; }

    internal CheckboxGroupEditor() { }

    /// <summary>
    ///     Arranges the choices in columns, starting at the medium breakpoint.
    /// </summary>
    /// <param name="count">The number of columns, from 1 to 12.</param>
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

/// <summary>
///     Configures a checkbox group editor for a collection property on a form.
/// </summary>
/// <typeparam name="TModel">The type of the form's model.</typeparam>
public sealed class CheckboxGroupEditor<TModel> : CheckboxGroupEditor
{
    internal CheckboxGroupEditor() { }

    /// <summary>
    ///     Sets a delegate that loads the choices for each request from the form's values.
    /// </summary>
    /// <param name="itemsLoader">
    ///     A delegate that loads the choices from the form's model with the request's services.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="itemsLoader" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     When the field depends on other fields, the model holds only their values, and the key
    ///     on the edit form; the delegate isn't called while any of them has no value.
    /// </remarks>
    public void UseItems(
        Func<
            TModel,
            IServiceProvider,
            CancellationToken,
            Task<IReadOnlyList<ChoiceItem>>
        > itemsLoader
    ) => UseFormItems(itemsLoader);

    /// <summary>
    ///     Takes the items from a resource, and configures them for this field.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="configure">A delegate that configures the items.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     See <see cref="ChoiceEditor.UseResource{TResource}()" />.
    /// </remarks>
    public void UseResource<TResource>(Action<ResourceItemsBuilder<TModel, TResource>> configure) =>
        UseResource(ReferencedResource.For(configure));
}
