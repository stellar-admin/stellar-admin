using StellarAdmin.Resources.Builders;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a toggle group editor.
/// </summary>
/// <remarks>
///     A collection property selects several values. Any other property selects one.
/// </remarks>
public class ToggleGroupEditor : ChoiceEditor
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

    internal ToggleGroupEditor() { }
}

/// <summary>
///     Configures a toggle group editor on a form.
/// </summary>
/// <typeparam name="TModel">The type of the form's model.</typeparam>
public sealed class ToggleGroupEditor<TModel> : ToggleGroupEditor
{
    internal ToggleGroupEditor() { }

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
