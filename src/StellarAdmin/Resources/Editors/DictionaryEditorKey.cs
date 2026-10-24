using StellarAdmin.Resources.Builders;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a dictionary editor's keys.
/// </summary>
/// <remarks>
///     Each key can be used once.
/// </remarks>
public class DictionaryEditorKey : DictionaryEditorPart
{
    internal DictionaryEditorKey() { }
}

/// <summary>
///     Configures a dictionary editor's keys on a form.
/// </summary>
/// <typeparam name="TModel">The type of the form's model.</typeparam>
public sealed class DictionaryEditorKey<TModel> : DictionaryEditorKey
{
    internal DictionaryEditorKey() { }

    /// <summary>
    ///     Sets a delegate that loads the items for each request from the form's values.
    /// </summary>
    /// <param name="itemsLoader">
    ///     A delegate that loads the items from the form's model with the request's services.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="itemsLoader" /> is <see langword="null" />.
    /// </exception>
    public void UseItems(
        Func<
            TModel,
            IServiceProvider,
            CancellationToken,
            Task<IReadOnlyList<ChoiceItem>>
        > itemsLoader
    ) => Source.UseFormItems(itemsLoader, nameof(DictionaryEditor));

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
        Source.UseResource(ReferencedResource.For(configure));
}
