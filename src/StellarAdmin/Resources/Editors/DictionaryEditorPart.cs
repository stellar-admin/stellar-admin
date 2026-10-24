using Microsoft.AspNetCore.Mvc.Rendering;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a dictionary editor's keys or values.
/// </summary>
/// <remarks>
///     A key or value is a select when it is an enum or has items, and otherwise a text or number
///     input.
/// </remarks>
public abstract class DictionaryEditorPart
{
    /// <summary>
    ///     Gets or sets how a text key or value's case changes as it is entered.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the case. The default is
    ///     <see cref="DictionaryEditorCasing.None" />.
    /// </value>
    public DictionaryEditorCasing Casing { get; set; }

    /// <summary>
    ///     Gets or sets the label of the column.
    /// </summary>
    /// <value>
    ///     The label, or <see langword="null" /> to use the dictionary editor's label option. The
    ///     default is <see langword="null" />.
    /// </value>
    public string? Label { get; set; }

    /// <summary>
    ///     Gets or sets the largest number of characters a text key or value can have.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    ///     Gets or sets a regular expression that each text key or value must match.
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    ///     Gets or sets the message shown when a key or value doesn't match <see cref="Pattern" />.
    /// </summary>
    /// <value>
    ///     The message, or <see langword="null" /> to use the browser's message, and the dictionary
    ///     editor's label option on the server. The default is <see langword="null" />.
    /// </value>
    public string? PatternMessage { get; set; }

    /// <summary>
    ///     Gets or sets the placeholder of an empty key or value, or the text of a select's empty
    ///     choice.
    /// </summary>
    public string? Placeholder { get; set; }

    internal ChoiceSource Source { get; } = new();

    private protected DictionaryEditorPart() { }

    /// <summary>
    ///     Sets a registered provider that supplies the items for each request.
    /// </summary>
    /// <typeparam name="TProvider">The type of the provider.</typeparam>
    /// <remarks>
    ///     Register <typeparamref name="TProvider" /> as a service.
    /// </remarks>
    public void UseItems<TProvider>()
        where TProvider : class, IChoiceItemsProvider => Source.UseItems<TProvider>();

    /// <summary>
    ///     Sets the items an integration package supplies for each request.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="items" /> is <see langword="null" />.
    /// </exception>
    public void UseItems(ChoiceItems items) => Source.UseItems(items);

    /// <summary>
    ///     Sets a fixed list of items.
    /// </summary>
    /// <param name="items">The items, in display order.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="items" /> is <see langword="null" />.
    /// </exception>
    public void UseItems(IEnumerable<ChoiceItem> items) => Source.UseItems(items);

    /// <summary>
    ///     Sets a fixed list of items from select list items.
    /// </summary>
    /// <param name="items">The select list items, in display order.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="items" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     An item without a value posts its text. <see cref="SelectListItem.Selected" /> is
    ///     ignored.
    /// </remarks>
    public void UseItems(IEnumerable<SelectListItem> items) => Source.UseItems(items);

    /// <summary>
    ///     Sets a delegate that loads the items for each request.
    /// </summary>
    /// <param name="itemsLoader">A delegate that loads the items with the request's services.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="itemsLoader" /> is <see langword="null" />.
    /// </exception>
    public void UseItems(
        Func<IServiceProvider, CancellationToken, Task<IReadOnlyList<ChoiceItem>>> itemsLoader
    ) => Source.UseItems(itemsLoader);

    /// <summary>
    ///     Takes the items from a resource: its records, named by their titles.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <remarks>
    ///     See <see cref="ChoiceEditor.UseResource{TResource}()" />.
    /// </remarks>
    public void UseResource<TResource>() => Source.UseResource(ReferencedResource.For<TResource>());
}
