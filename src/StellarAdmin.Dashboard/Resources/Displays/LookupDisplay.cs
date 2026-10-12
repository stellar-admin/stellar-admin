using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Resources.Displays;

/// <summary>
///     Displays the title of the item a value refers to, such as a category's name for its key.
/// </summary>
/// <remarks>
///     A collection of values shows each item's title. A value no item matches shows the value.
/// </remarks>
public sealed class LookupDisplay : FieldDisplay, IFieldDisplay<LookupDisplayHandler>
{
    /// <summary>
    ///     The items the value is looked up in.
    /// </summary>
    public LookupItems? Items { get; private set; }

    /// <inheritdoc />
    public override string TemplateName => "Displays/Lookup";

    /// <summary>
    ///     Supplies the items from an integration's implementation.
    /// </summary>
    public void UseItems(LookupItems items)
    {
        ArgumentNullException.ThrowIfNull(items);

        Items = items;
    }

    /// <summary>
    ///     Selects a registered source that supplies the items for each request.
    /// </summary>
    public void UseItems<TSource, TEntity, TValue>(
        Func<TEntity, TValue> value,
        Func<TEntity, string> title
    )
        where TSource : class, ILookupSource<TEntity, TValue> =>
        UseItems<TSource, TEntity, TValue>(value, title, _ => { });

    /// <summary>
    ///     Selects a registered source that supplies the items for each request, and configures what each item contains.
    /// </summary>
    public void UseItems<TSource, TEntity, TValue>(
        Func<TEntity, TValue> value,
        Func<TEntity, string> title,
        Action<LookupItemsBuilder<TEntity>> configure
    )
        where TSource : class, ILookupSource<TEntity, TValue> =>
        Items = LookupItems.Create<TSource, TEntity, TValue>(value, title, configure);
}
