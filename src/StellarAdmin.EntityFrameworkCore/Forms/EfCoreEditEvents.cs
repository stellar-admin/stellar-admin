namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Runs code when an edit form of a resource that EF Core saves loads and saves a record.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TModel">The type of the model the form binds to.</typeparam>
/// <remarks>
///     Set the delegates, or derive from the class, override its methods, and register the class
///     with <c>UseEvents</c>.
/// </remarks>
public class EfCoreEditEvents<TEntity, TModel>
    where TEntity : class
{
    /// <summary>
    ///     Gets or sets the delegate that runs after the form's record is loaded.
    /// </summary>
    /// <value>
    ///     The delegate. The default does nothing.
    /// </value>
    /// <remarks>
    ///     The delegate runs each time the record is loaded: before the form is shown, and before
    ///     a submitted form's values are bound to it. The entity isn't tracked by the context's
    ///     <see cref="EfCoreFormContext{TEntity, TModel}.Db" />.
    /// </remarks>
    public Func<
        EfCoreFormContext<TEntity, TModel>,
        CancellationToken,
        Task
    > OnLoaded { get; set; } = (_, _) => Task.CompletedTask;

    /// <summary>
    ///     Gets or sets the delegate that runs after the record is saved.
    /// </summary>
    /// <value>
    ///     The delegate. The default does nothing.
    /// </value>
    /// <remarks>
    ///     The record is already saved, so an exception is logged, and the form continues as
    ///     saved.
    /// </remarks>
    public Func<
        EfCoreFormContext<TEntity, TModel>,
        CancellationToken,
        Task
    > OnUpdated { get; set; } = (_, _) => Task.CompletedTask;

    /// <summary>
    ///     Gets or sets the delegate that runs before the record is saved.
    /// </summary>
    /// <value>
    ///     The delegate. The default does nothing.
    /// </value>
    /// <remarks>
    ///     The delegate runs after the form's values are copied onto the stored entity, which the
    ///     database context tracks, and inside the transaction of the save. It can add other
    ///     records to the context, reject the save with <c>Fail</c>, or save the record itself
    ///     and call <c>Handled</c>.
    /// </remarks>
    public Func<
        EfCoreSaveContext<TEntity, TModel>,
        CancellationToken,
        Task
    > OnUpdating { get; set; } = (_, _) => Task.CompletedTask;

    /// <summary>
    ///     Runs after the form's record is loaded.
    /// </summary>
    /// <param name="context">The loaded record.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task that completes when the event has run.</returns>
    public virtual Task LoadedAsync(
        EfCoreFormContext<TEntity, TModel> context,
        CancellationToken cancellationToken
    ) => OnLoaded(context, cancellationToken);

    /// <summary>
    ///     Runs after the record is saved.
    /// </summary>
    /// <param name="context">The saved record.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task that completes when the event has run.</returns>
    public virtual Task UpdatedAsync(
        EfCoreFormContext<TEntity, TModel> context,
        CancellationToken cancellationToken
    ) => OnUpdated(context, cancellationToken);

    /// <summary>
    ///     Runs before the record is saved.
    /// </summary>
    /// <param name="context">The record to save.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task that completes when the event has run.</returns>
    public virtual Task UpdatingAsync(
        EfCoreSaveContext<TEntity, TModel> context,
        CancellationToken cancellationToken
    ) => OnUpdating(context, cancellationToken);
}
