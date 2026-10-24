using StellarAdmin.Resources.Builders;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a dictionary editor's values.
/// </summary>
public class DictionaryEditorValue : DictionaryEditorPart
{
    /// <summary>
    ///     Gets or sets how each value is aligned within its column.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the alignment, or <see langword="null" />
    ///     to align numbers to the end and other values to the start. The default is
    ///     <see langword="null" />.
    /// </value>
    public DictionaryEditorAlign? Align { get; set; }

    /// <summary>
    ///     Gets or sets the largest number a number value can be.
    /// </summary>
    public double? Max { get; set; }

    /// <summary>
    ///     Gets or sets the smallest number a number value can be.
    /// </summary>
    public double? Min { get; set; }

    /// <summary>
    ///     Gets or sets the text shown before each value, such as a currency symbol.
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    ///     Gets or sets the step of a number value.
    /// </summary>
    /// <value>
    ///     The step, or <see langword="null" /> for <c>1</c> for a whole-number type and any step
    ///     for other numbers. The default is <see langword="null" />.
    /// </value>
    public double? Step { get; set; }

    /// <summary>
    ///     Gets or sets the text shown after each value, such as a unit.
    /// </summary>
    public string? Suffix { get; set; }

    internal DictionaryEditorValue() { }
}

/// <summary>
///     Configures a dictionary editor's values on a form.
/// </summary>
/// <typeparam name="TModel">The type of the form's model.</typeparam>
public sealed class DictionaryEditorValue<TModel> : DictionaryEditorValue
{
    internal DictionaryEditorValue() { }

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
