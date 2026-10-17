using StellarAdmin.Resources.Builders;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a multi-select lookup editor, which selects several items from a long list in a
///     searchable sheet.
/// </summary>
/// <remarks>
///     The field binds a collection of values, such as an array or a list.
/// </remarks>
public sealed class MultiLookupSheetEditor
    : FieldEditor,
        IFieldEditor<MultiLookupSheetEditorHandler>,
        ILookupSheetEditor
{
    /// <summary>
    ///     Gets the additional CSS classes for the parts of the editor.
    /// </summary>
    public override MultiLookupSheetEditorClassNames ClassNames { get; } = new();

    /// <summary>
    ///     Gets the items the editor looks up.
    /// </summary>
    /// <value>
    ///     The items, or <see langword="null" /> until <c>UseItems</c> is called.
    /// </value>
    public LookupItems? Items { get; private set; }

    /// <inheritdoc />
    public override string TemplateName => "Editors/MultiLookupSheet";

    internal bool CreateEnabled { get; private set; }

    internal Type? CreateResourceType { get; private set; }

    internal MultiLookupFieldOptions FieldOptions { get; } = new();

    internal LookupSheetOptions SheetOptions { get; } = new();

    string? ILookupSheetEditor.MediaClassName => ClassNames.Media;

    LookupSheetOptions ILookupSheetEditor.SheetOptions => SheetOptions;

    /// <summary>
    ///     Configures how the field is displayed in the form.
    /// </summary>
    /// <param name="configure">A delegate that configures the field.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public void Field(Action<MultiLookupFieldOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(FieldOptions);
    }

    /// <summary>
    ///     Shows a button in the sheet that creates a new item and selects it.
    /// </summary>
    /// <remarks>
    ///     The create form of the resource registered for the items' type creates the item. That
    ///     resource must have a create page. Only users who can create the resource see the button.
    ///     When the items have no type, use <see cref="EnableCreate{TResource}" /> instead.
    /// </remarks>
    public void EnableCreate() => CreateEnabled = true;

    /// <summary>
    ///     Shows a button in the sheet that creates a new item and selects it.
    /// </summary>
    /// <typeparam name="TResource">
    ///     The type of the resource whose create form creates the item.
    /// </typeparam>
    /// <remarks>
    ///     The resource must have a create page. Only users who can create the resource see the
    ///     button.
    /// </remarks>
    public void EnableCreate<TResource>()
    {
        CreateEnabled = true;
        CreateResourceType = typeof(TResource);
    }

    /// <summary>
    ///     Configures the sheet in which items are searched.
    /// </summary>
    /// <param name="configure">A delegate that configures the sheet.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public void Sheet(Action<LookupSheetOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(SheetOptions);
    }

    /// <summary>
    ///     Sets the items the editor looks up.
    /// </summary>
    /// <param name="items">The items, such as those an integration package supplies.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="items" /> is <see langword="null" />.
    /// </exception>
    public void UseItems(LookupItems items)
    {
        ArgumentNullException.ThrowIfNull(items);

        Items = items;
    }

    /// <summary>
    ///     Sets a registered source that supplies the items for each request.
    /// </summary>
    /// <typeparam name="TSource">The type of the source that supplies the items.</typeparam>
    /// <typeparam name="TEntity">The type of the items.</typeparam>
    /// <typeparam name="TValue">The type of the value that identifies an item.</typeparam>
    /// <param name="value">A delegate that returns an item's value.</param>
    /// <param name="title">A delegate that returns an item's title.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="value" /> or <paramref name="title" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Register <typeparamref name="TSource" /> as a service.
    /// </remarks>
    public void UseItems<TSource, TEntity, TValue>(
        Func<TEntity, TValue> value,
        Func<TEntity, string> title
    )
        where TSource : class, ILookupSource<TEntity, TValue> =>
        UseItems<TSource, TEntity, TValue>(value, title, _ => { });

    /// <summary>
    ///     Sets a registered source that supplies the items for each request, and configures what
    ///     each item shows.
    /// </summary>
    /// <typeparam name="TSource">The type of the source that supplies the items.</typeparam>
    /// <typeparam name="TEntity">The type of the items.</typeparam>
    /// <typeparam name="TValue">The type of the value that identifies an item.</typeparam>
    /// <param name="value">A delegate that returns an item's value.</param>
    /// <param name="title">A delegate that returns an item's title.</param>
    /// <param name="configure">A delegate that configures what each item shows.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="value" />, <paramref name="title" /> or <paramref name="configure" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Register <typeparamref name="TSource" /> as a service.
    /// </remarks>
    public void UseItems<TSource, TEntity, TValue>(
        Func<TEntity, TValue> value,
        Func<TEntity, string> title,
        Action<LookupItemsBuilder<TEntity>> configure
    )
        where TSource : class, ILookupSource<TEntity, TValue> =>
        Items = LookupItems.Create<TSource, TEntity, TValue>(value, title, configure);
}
