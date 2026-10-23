using StellarAdmin.Resources.Builders;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a select editor.
/// </summary>
public class SelectEditor : ChoiceEditor
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/Select";

    internal override bool KeepsEmptyChoiceWhileUnset => true;

    internal SelectEditor() { }
}

/// <summary>
///     Configures a select editor on a form.
/// </summary>
/// <typeparam name="TModel">The type of the form's model.</typeparam>
public sealed class SelectEditor<TModel> : SelectEditor
{
    internal SelectEditor() { }

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
