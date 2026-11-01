namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Runs code when a create form of a resource that EF Core saves is shown and saved.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TModel">The type of the model the form binds to.</typeparam>
/// <remarks>
///     Set the delegates, or derive from the class, override its methods, and register the class
///     with <c>UseEvents</c>.
/// </remarks>
public class EfCoreCreateEvents<TEntity, TModel>
    where TEntity : class
{
    /// <summary>
    ///     Gets or sets the delegate that runs after a new record is saved.
    /// </summary>
    /// <value>
    ///     The delegate. The default does nothing.
    /// </value>
    /// <remarks>
    ///     The record is already saved, so an exception is logged, and the form continues as
    ///     saved.
    /// </remarks>
    public Func<
        EfCoreSavedContext<TEntity, TModel>,
        CancellationToken,
        Task
    > OnCreated { get; set; } = (_, _) => Task.CompletedTask;

    /// <summary>
    ///     Gets or sets the delegate that runs before a new record is saved.
    /// </summary>
    /// <value>
    ///     The delegate. The default does nothing.
    /// </value>
    /// <remarks>
    ///     The delegate runs after the form's values are on the entity, which the database
    ///     context tracks, and inside the transaction of the save. It can add other records to
    ///     the context, reject the save with <c>Fail</c>, or save the record itself and call
    ///     <c>Handled</c>.
    /// </remarks>
    public Func<
        EfCoreSaveContext<TEntity, TModel>,
        CancellationToken,
        Task
    > OnCreating { get; set; } = (_, _) => Task.CompletedTask;

    /// <summary>
    ///     Gets or sets the delegate that runs before a create form shows a new record.
    /// </summary>
    /// <value>
    ///     The delegate. The default does nothing.
    /// </value>
    /// <remarks>
    ///     The delegate runs after the model is created, wherever a create form is shown: on the
    ///     create page, in a new inline row, in a lookup's create sheet, and in the next form
    ///     after saving and adding another, before the kept fields' values. Use it for defaults
    ///     the user sees and can change. A value the form doesn't show belongs in
    ///     <see cref="OnCreating" />.
    /// </remarks>
    public Func<
        EfCoreInitializedContext<TEntity, TModel>,
        CancellationToken,
        Task
    > OnInitialized { get; set; } = (_, _) => Task.CompletedTask;

    /// <summary>
    ///     Runs after a new record is saved.
    /// </summary>
    /// <param name="context">The saved record.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task that completes when the event has run.</returns>
    public virtual Task CreatedAsync(
        EfCoreSavedContext<TEntity, TModel> context,
        CancellationToken cancellationToken
    ) => OnCreated(context, cancellationToken);

    /// <summary>
    ///     Runs before a new record is saved.
    /// </summary>
    /// <param name="context">The record to save.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task that completes when the event has run.</returns>
    public virtual Task CreatingAsync(
        EfCoreSaveContext<TEntity, TModel> context,
        CancellationToken cancellationToken
    ) => OnCreating(context, cancellationToken);

    /// <summary>
    ///     Runs before a create form shows a new record.
    /// </summary>
    /// <param name="context">The new record.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task that completes when the event has run.</returns>
    public virtual Task InitializedAsync(
        EfCoreInitializedContext<TEntity, TModel> context,
        CancellationToken cancellationToken
    ) => OnInitialized(context, cancellationToken);
}
