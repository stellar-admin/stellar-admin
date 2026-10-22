using StellarAdmin.Diagnostics;
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
    ///     The items, or <see langword="null" /> until <c>UseItems</c> is called, and when the
    ///     editor takes its items from a resource.
    /// </value>
    public LookupItems? Items { get; private set; }

    /// <summary>
    ///     Gets the resource the editor takes its items from.
    /// </summary>
    /// <value>
    ///     The resource, or <see langword="null" /> until <c>UseResource</c> is called.
    /// </value>
    public ReferencedResource? Resource { get; private set; }

    /// <inheritdoc />
    public override string TemplateName => "Editors/MultiLookupSheet";

    internal bool CreateEnabled { get; private set; }

    internal Type? CreateResourceType { get; private set; }

    internal ConfigurationSource? CreateSource { get; private set; }

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
    public void EnableCreate()
    {
        CreateEnabled = true;
        CreateSource = ConfigurationSource.Capture();
    }

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
        CreateSource = ConfigurationSource.Capture();
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
        Resource = null;
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
        UseItems(LookupItems.Create<TSource, TEntity, TValue>(value, title, configure));

    /// <summary>
    ///     Sets a registered source that supplies the items for each request, searching them with a
    ///     value from the form.
    /// </summary>
    /// <typeparam name="TSource">The type of the source that supplies the items.</typeparam>
    /// <typeparam name="TEntity">The type of the items.</typeparam>
    /// <typeparam name="TValue">The type of the value that identifies an item.</typeparam>
    /// <typeparam name="TArgument">The type of the value the source searches with.</typeparam>
    /// <typeparam name="TModel">The type of the form's model.</typeparam>
    /// <param name="value">A delegate that returns an item's value.</param>
    /// <param name="title">A delegate that returns an item's title.</param>
    /// <param name="argument">
    ///     A delegate that returns the value the source searches with, from the form's model.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="value" />, <paramref name="title" /> or <paramref name="argument" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Register <typeparamref name="TSource" /> as a service. Declare each field the argument
    ///     reads with <c>DependsOn</c>: the form holds only those fields' values, and the key on the
    ///     edit form, and the sheet doesn't search while any of them has no value.
    /// </remarks>
    public void UseItems<TSource, TEntity, TValue, TArgument, TModel>(
        Func<TEntity, TValue> value,
        Func<TEntity, string> title,
        Func<TModel, TArgument> argument
    )
        where TSource : class, ILookupSource<TEntity, TValue, TArgument> =>
        UseItems<TSource, TEntity, TValue, TArgument, TModel>(value, title, argument, _ => { });

    /// <summary>
    ///     Sets a registered source that supplies the items for each request, searching them with a
    ///     value from the form, and configures what each item shows.
    /// </summary>
    /// <typeparam name="TSource">The type of the source that supplies the items.</typeparam>
    /// <typeparam name="TEntity">The type of the items.</typeparam>
    /// <typeparam name="TValue">The type of the value that identifies an item.</typeparam>
    /// <typeparam name="TArgument">The type of the value the source searches with.</typeparam>
    /// <typeparam name="TModel">The type of the form's model.</typeparam>
    /// <param name="value">A delegate that returns an item's value.</param>
    /// <param name="title">A delegate that returns an item's title.</param>
    /// <param name="argument">
    ///     A delegate that returns the value the source searches with, from the form's model.
    /// </param>
    /// <param name="configure">A delegate that configures what each item shows.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="value" />, <paramref name="title" />, <paramref name="argument" /> or
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Register <typeparamref name="TSource" /> as a service. Declare each field the argument
    ///     reads with <c>DependsOn</c>: the form holds only those fields' values, and the key on the
    ///     edit form, and the sheet doesn't search while any of them has no value.
    /// </remarks>
    public void UseItems<TSource, TEntity, TValue, TArgument, TModel>(
        Func<TEntity, TValue> value,
        Func<TEntity, string> title,
        Func<TModel, TArgument> argument,
        Action<LookupItemsBuilder<TEntity>> configure
    )
        where TSource : class, ILookupSource<TEntity, TValue, TArgument> =>
        UseItems(
            LookupItems.Create<TSource, TEntity, TValue, TArgument, TModel>(
                value,
                title,
                argument,
                configure,
                GetType()
            )
        );

    /// <summary>
    ///     Takes the items from a resource: its records, named by their titles.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <remarks>
    ///     Each item's value is the record's key. The resource's <c>Reference</c> settings add a
    ///     description and media, and an EF Core resource's settings search, order and filter the
    ///     items. When no resource is registered for <typeparamref name="TResource" />, the editor
    ///     has no items.
    /// </remarks>
    public void UseResource<TResource>() => UseResource<TResource>(_ => { });

    /// <summary>
    ///     Takes the items from a resource, and configures them for this field.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="configure">A delegate that configures the items.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     See <see cref="UseResource{TResource}()" />.
    /// </remarks>
    public void UseResource<TResource>(Action<ResourceItemsBuilder<TResource>> configure) =>
        UseResource(ReferencedResource.For(configure));

    internal void UseResource(ReferencedResource resource)
    {
        Resource = resource;
        Items = null;
    }
}
