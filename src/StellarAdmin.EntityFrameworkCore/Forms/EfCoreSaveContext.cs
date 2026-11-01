using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using StellarAdmin.Resources;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Provides the record that a create or edit form is about to save, and lets an event reject
///     the save or save the record itself.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TModel">The type of the model the form binds to.</typeparam>
/// <remarks>
///     The entity is tracked by <see cref="EfCoreFormContext{TEntity, TModel}.Db" />, with the
///     form's values. Whatever else an event adds to the context is saved with it.
/// </remarks>
public sealed class EfCoreSaveContext<TEntity, TModel> : EfCoreFormContext<TEntity, TModel>
    where TEntity : class
{
    private readonly List<ResourceValidationError> _errors = [];

    /// <summary>
    ///     Gets the entity's change tracking entry.
    /// </summary>
    /// <value>
    ///     The entry, whose original values are the stored ones when the form edits a record.
    /// </value>
    public EntityEntry<TEntity> Entry => Db.Entry(Entity);

    internal IReadOnlyList<ResourceValidationError> Errors => _errors;

    internal bool IsHandled { get; private set; }

    internal EfCoreSaveContext(
        DbContext db,
        TEntity entity,
        TModel model,
        IServiceProvider services
    )
        : base(db, entity, model, services) { }

    /// <summary>
    ///     Rejects the save with an error for the whole form.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <exception cref="ArgumentException">
    ///     <paramref name="message" /> is <see langword="null" />, empty or white space.
    /// </exception>
    /// <remarks>
    ///     Nothing is saved, including what the event saved itself.
    /// </remarks>
    public void Fail(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        _errors.Add(new(null, message));
    }

    /// <summary>
    ///     Rejects the save with an error on a field.
    /// </summary>
    /// <typeparam name="TValue">The type of the field's value.</typeparam>
    /// <param name="field">An expression that selects the field's property on the model.</param>
    /// <param name="message">The error message.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="field" /> isn't a property path of the model, or
    ///     <paramref name="message" /> is <see langword="null" />, empty or white space.
    /// </exception>
    /// <remarks>
    ///     Nothing is saved, including what the event saved itself. An error on a property that
    ///     the form has no field for is shown for the whole form.
    /// </remarks>
    public void Fail<TValue>(Expression<Func<TModel, TValue>> field, string message)
    {
        ArgumentNullException.ThrowIfNull(field);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var path =
            EfCorePropertyPaths.GetPropertyPath(field)
            ?? throw new ArgumentException(
                $"`{field}` isn't a property path of `{typeof(TModel).Name}`.",
                nameof(field)
            );
        _errors.Add(new(string.Join('.', path.Select(property => property.Name)), message));
    }

    /// <summary>
    ///     Skips saving the record, because the event saved it itself.
    /// </summary>
    /// <remarks>
    ///     The event saves through <see cref="EfCoreFormContext{TEntity, TModel}.Db" />, or a
    ///     service that uses the same context, such as ASP.NET Core Identity's
    ///     <c>UserManager</c>. Its saves are part of the form's transaction.
    /// </remarks>
    public void Handled() => IsHandled = true;
}
