namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures a lookup editor, which selects one item from a long list in a searchable sheet.
/// </summary>
public sealed class LookupEditor : FieldEditor, IFieldEditor<LookupEditorHandler>
{
    /// <summary>
    ///     Whether the selection can be cleared, or null to allow it when the field is optional.
    /// </summary>
    public bool? AllowClear { get; set; }

    /// <summary>
    ///     Hint text displayed while nothing is selected.
    /// </summary>
    public string? EmptyText { get; set; }

    /// <summary>
    ///     The number of characters entered before searching, or 0 to list items when the sheet opens.
    /// </summary>
    public int MinimumSearchLength
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            field = value;
        }
    }

    /// <summary>
    ///     The number of items loaded at a time.
    /// </summary>
    public int PageSize
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            field = value;
        }
    } = 20;

    /// <summary>
    ///     Hint text displayed in the search input.
    /// </summary>
    public string? SearchPlaceholder { get; set; }

    /// <summary>
    ///     The sheet title, or null to use the field label.
    /// </summary>
    public string? SheetTitle { get; set; }

    /// <summary>
    ///     The configured items, or null until items are selected.
    /// </summary>
    public LookupItems? Items { get; private set; }

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
        Func<TEntity, string> text
    )
        where TSource : class, ILookupSource<TEntity, TValue> =>
        UseItems<TSource, TEntity, TValue>(value, text, _ => { });

    /// <summary>
    ///     Selects a registered source that supplies the items for each request, and configures how they are displayed.
    /// </summary>
    public void UseItems<TSource, TEntity, TValue>(
        Func<TEntity, TValue> value,
        Func<TEntity, string> text,
        Action<LookupItemsBuilder<TEntity>> configure
    )
        where TSource : class, ILookupSource<TEntity, TValue>
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(configure);

        var items = new LookupItemsBuilder<TEntity>();
        configure(items);
        Items = new LookupItems<TSource, TEntity, TValue>(value, text, items.Description);
    }
}
