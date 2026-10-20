using StellarAdmin.UI;

namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Shows a value as a badge.
/// </summary>
/// <remarks>
///     A collection shows a badge for each item. To choose the badge by value, write a display
///     template that switches on it. To show a property of each item, such as the name of a
///     related entity, call <see cref="UseText{TItem}(Func{TItem, string})" />.
/// </remarks>
public sealed class BadgeDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Badge";

    /// <summary>
    ///     Gets or sets the variant of the badge.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the variant. The default is
    ///     <see cref="BadgeVariant.Secondary" />.
    /// </value>
    public BadgeVariant Variant { get; set; } = BadgeVariant.Secondary;

    internal Func<object, string?>? Text { get; private set; }

    internal Type? TextItemType { get; private set; }

    /// <summary>
    ///     Sets the text of each badge.
    /// </summary>
    /// <typeparam name="TItem">
    ///     The type of the value, or of each item when the value is a collection.
    /// </typeparam>
    /// <param name="text">A delegate that returns the text of an item's badge.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="text" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The delegate isn't called for an empty item. The application fails to start when
    ///     <typeparamref name="TItem" /> doesn't match the field's value or items.
    /// </remarks>
    public void UseText<TItem>(Func<TItem, string?> text)
    {
        ArgumentNullException.ThrowIfNull(text);

        Text = item => text((TItem)item);
        TextItemType = typeof(TItem);
    }
}
