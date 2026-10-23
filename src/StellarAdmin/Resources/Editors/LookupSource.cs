namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Supplies the items of a lookup editor that don't come from a resource.
/// </summary>
/// <remarks>
///     Derive from <see cref="LookupSource{TItem, TValue}" />, or from
///     <see cref="LookupSource{TItem, TValue, TArgument}" /> when the search needs a value from the
///     form.
/// </remarks>
public abstract class LookupSource
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LookupSource" /> class.
    /// </summary>
    protected LookupSource() { }

    internal abstract Task<IReadOnlyList<ChoiceItem>> FindItemsAsync(
        FieldEditorContext context,
        CancellationToken cancellationToken
    );

    internal abstract Task<LookupResults> SearchItemsAsync(
        object? argument,
        LookupQuery query,
        CancellationToken cancellationToken
    );
}

/// <summary>
///     Supplies the items of a lookup editor, and says what each item shows.
/// </summary>
/// <typeparam name="TItem">The type of the items.</typeparam>
/// <typeparam name="TValue">The type of the value that identifies an item.</typeparam>
/// <remarks>
///     The source is resolved from the request's services, or created with them when it isn't
///     registered. Override one of <see cref="GetAvatar" />, <see cref="GetCode" />,
///     <see cref="GetIcon" /> or <see cref="GetImage" /> to show media with each item.
/// </remarks>
public abstract class LookupSource<TItem, TValue> : LookupSource, ILookupSourceItem<TItem, TValue>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LookupSource{TItem, TValue}" /> class.
    /// </summary>
    protected LookupSource() { }

    /// <summary>
    ///     Returns the items with the given values.
    /// </summary>
    /// <param name="values">The values of the selected items.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The items. Leave out values that have no item.</returns>
    public abstract Task<IReadOnlyCollection<TItem>> FindAsync(
        IReadOnlyCollection<TValue> values,
        CancellationToken cancellationToken
    );

    /// <summary>
    ///     Returns the image URL of an item's avatar.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The image URL, or <see langword="null" /> to show the title's initials.</returns>
    public virtual string? GetAvatar(TItem item) => null;

    /// <summary>
    ///     Returns a short code shown with an item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The code, or <see langword="null" /> for none.</returns>
    public virtual string? GetCode(TItem item) => null;

    /// <summary>
    ///     Returns the description shown under an item's title.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The description, or <see langword="null" /> for none.</returns>
    public virtual string? GetDescription(TItem item) => null;

    /// <summary>
    ///     Returns the name of the icon shown with an item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The icon name, or <see langword="null" /> for none.</returns>
    public virtual string? GetIcon(TItem item) => null;

    /// <summary>
    ///     Returns the URL of the image shown with an item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The image URL, or <see langword="null" /> for none.</returns>
    public virtual string? GetImage(TItem item) => null;

    /// <summary>
    ///     Returns an item's title.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The title.</returns>
    public abstract string GetTitle(TItem item);

    /// <summary>
    ///     Returns the value that identifies an item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The value, which the field binds.</returns>
    public abstract TValue GetValue(TItem item);

    /// <summary>
    ///     Returns a page of items that match a query.
    /// </summary>
    /// <param name="query">The search term and the page of items to return.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The matching items, and whether more items follow them.</returns>
    public abstract Task<LookupPage<TItem>> SearchAsync(
        LookupQuery query,
        CancellationToken cancellationToken
    );

    internal sealed override Task<IReadOnlyList<ChoiceItem>> FindItemsAsync(
        FieldEditorContext context,
        CancellationToken cancellationToken
    ) => LookupSourceItems.FindAsync(this, FindAsync, context, cancellationToken);

    internal sealed override async Task<LookupResults> SearchItemsAsync(
        object? argument,
        LookupQuery query,
        CancellationToken cancellationToken
    ) => LookupSourceItems.ToResults(this, await SearchAsync(query, cancellationToken));
}

/// <summary>
///     Supplies the items of a lookup editor, searched with a value from the form, and says what
///     each item shows.
/// </summary>
/// <typeparam name="TItem">The type of the items.</typeparam>
/// <typeparam name="TValue">The type of the value that identifies an item.</typeparam>
/// <typeparam name="TArgument">The type of the value the search takes from the form.</typeparam>
/// <remarks>
///     The source is resolved from the request's services, or created with them when it isn't
///     registered. Override one of <see cref="GetAvatar" />, <see cref="GetCode" />,
///     <see cref="GetIcon" /> or <see cref="GetImage" /> to show media with each item.
/// </remarks>
public abstract class LookupSource<TItem, TValue, TArgument>
    : LookupSource,
        ILookupSourceItem<TItem, TValue>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LookupSource{TItem, TValue, TArgument}" />
    ///     class.
    /// </summary>
    protected LookupSource() { }

    /// <summary>
    ///     Returns the items with the given values.
    /// </summary>
    /// <param name="values">The values of the selected items.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The items. Leave out values that have no item.</returns>
    /// <remarks>
    ///     The selected items are found whatever the form's values are.
    /// </remarks>
    public abstract Task<IReadOnlyCollection<TItem>> FindAsync(
        IReadOnlyCollection<TValue> values,
        CancellationToken cancellationToken
    );

    /// <summary>
    ///     Returns the image URL of an item's avatar.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The image URL, or <see langword="null" /> to show the title's initials.</returns>
    public virtual string? GetAvatar(TItem item) => null;

    /// <summary>
    ///     Returns a short code shown with an item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The code, or <see langword="null" /> for none.</returns>
    public virtual string? GetCode(TItem item) => null;

    /// <summary>
    ///     Returns the description shown under an item's title.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The description, or <see langword="null" /> for none.</returns>
    public virtual string? GetDescription(TItem item) => null;

    /// <summary>
    ///     Returns the name of the icon shown with an item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The icon name, or <see langword="null" /> for none.</returns>
    public virtual string? GetIcon(TItem item) => null;

    /// <summary>
    ///     Returns the URL of the image shown with an item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The image URL, or <see langword="null" /> for none.</returns>
    public virtual string? GetImage(TItem item) => null;

    /// <summary>
    ///     Returns an item's title.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The title.</returns>
    public abstract string GetTitle(TItem item);

    /// <summary>
    ///     Returns the value that identifies an item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The value, which the field binds.</returns>
    public abstract TValue GetValue(TItem item);

    /// <summary>
    ///     Returns a page of items that match a query and a value from the form.
    /// </summary>
    /// <param name="argument">The value the field takes from the form.</param>
    /// <param name="query">The search term and the page of items to return.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The matching items, and whether more items follow them.</returns>
    public abstract Task<LookupPage<TItem>> SearchAsync(
        TArgument argument,
        LookupQuery query,
        CancellationToken cancellationToken
    );

    internal sealed override Task<IReadOnlyList<ChoiceItem>> FindItemsAsync(
        FieldEditorContext context,
        CancellationToken cancellationToken
    ) => LookupSourceItems.FindAsync(this, FindAsync, context, cancellationToken);

    internal sealed override async Task<LookupResults> SearchItemsAsync(
        object? argument,
        LookupQuery query,
        CancellationToken cancellationToken
    )
    {
        // The sheet doesn't search while a field the argument depends on is empty, so a missing value means the field
        // doesn't declare it
        if (argument is not TArgument typed)
        {
            throw new InvalidOperationException(
                $"The lookup's source {GetType().Name} searches with a value from the form, but the form has none. Declare the fields the value reads with DependsOn."
            );
        }

        return LookupSourceItems.ToResults(
            this,
            await SearchAsync(typed, query, cancellationToken)
        );
    }
}
