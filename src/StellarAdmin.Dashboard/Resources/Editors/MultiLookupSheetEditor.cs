namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures a multi-select lookup editor, which selects several items from a long list in a searchable sheet.
///     The field binds a collection of values, such as an array or a list.
/// </summary>
public sealed class MultiLookupSheetEditor
    : FieldEditor,
        IFieldEditor<MultiLookupSheetEditorHandler>,
        ILookupSheetEditor
{
    /// <summary>
    ///     Additional CSS classes for the parts of the editor.
    /// </summary>
    public override MultiLookupSheetEditorClassNames ClassNames { get; } = new();

    /// <summary>
    ///     The configured items, or null until items are selected.
    /// </summary>
    public LookupItems? Items { get; private set; }

    internal bool CreateEnabled { get; private set; }

    internal Type? CreateResourceType { get; private set; }

    internal MultiLookupFieldOptions FieldOptions { get; } = new();

    internal LookupSheetOptions SheetOptions { get; } = new();

    string? ILookupSheetEditor.MediaClassName => ClassNames.Media;

    LookupSheetOptions ILookupSheetEditor.SheetOptions => SheetOptions;

    /// <summary>
    ///     Configures how the field is displayed in the form.
    /// </summary>
    public void Editor(Action<MultiLookupFieldOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(FieldOptions);
    }

    /// <summary>
    ///     Shows a button in the sheet for creating a new item, which is then selected. The item is created with the
    ///     create form of the resource registered for the items' type.
    /// </summary>
    public void EnableCreate() => CreateEnabled = true;

    /// <summary>
    ///     Shows a button in the sheet for creating a new item, which is then selected. The item is created with the
    ///     create form of the specified resource.
    /// </summary>
    public void EnableCreate<TResource>()
    {
        CreateEnabled = true;
        CreateResourceType = typeof(TResource);
    }

    /// <summary>
    ///     Configures the sheet in which items are searched.
    /// </summary>
    public void Sheet(Action<LookupSheetOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(SheetOptions);
    }

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
        where TSource : class, ILookupSource<TEntity, TValue>
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(configure);

        var items = new LookupItemsBuilder<TEntity>();
        configure(items);
        Items = new LookupItems<TSource, TEntity, TValue>(
            value,
            title,
            items.Description,
            items.Media,
            items.MediaType
        );
    }
}
