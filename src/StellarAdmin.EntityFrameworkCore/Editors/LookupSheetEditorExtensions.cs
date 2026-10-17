using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Provides extension methods that load the items of lookup editors with EF Core.
/// </summary>
public static class LookupSheetEditorExtensions
{
    extension(LookupSheetEditor editor)
    {
        /// <summary>
        ///     Looks up the items in an entity set.
        /// </summary>
        /// <typeparam name="TContext">
        ///     The type of the database context that holds the entities.
        /// </typeparam>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <typeparam name="TValue">The type of the value that identifies an entity.</typeparam>
        /// <param name="value">An expression that selects an entity's value.</param>
        /// <param name="title">An expression that selects an entity's title.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value" /> or <paramref name="title" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        ///     The items load for each request from the <typeparamref name="TContext" /> registered
        ///     as a service.
        /// </remarks>
        public void UseItems<TContext, TEntity, TValue>(
            Expression<Func<TEntity, TValue>> value,
            Expression<Func<TEntity, string>> title
        )
            where TContext : DbContext
            where TEntity : class =>
            editor.UseItems<TContext, TEntity, TValue>(value, title, _ => { });

        /// <summary>
        ///     Looks up the items in an entity set, and configures how they're searched and shown.
        /// </summary>
        /// <typeparam name="TContext">
        ///     The type of the database context that holds the entities.
        /// </typeparam>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <typeparam name="TValue">The type of the value that identifies an entity.</typeparam>
        /// <param name="value">An expression that selects an entity's value.</param>
        /// <param name="title">An expression that selects an entity's title.</param>
        /// <param name="configure">A delegate that configures the items.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value" />, <paramref name="title" /> or
        ///     <paramref name="configure" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        ///     The items load for each request from the <typeparamref name="TContext" /> registered
        ///     as a service.
        /// </remarks>
        public void UseItems<TContext, TEntity, TValue>(
            Expression<Func<TEntity, TValue>> value,
            Expression<Func<TEntity, string>> title,
            Action<EfCoreLookupItemsBuilder<TEntity, TValue>> configure
        )
            where TContext : DbContext
            where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(editor);
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(title);
            ArgumentNullException.ThrowIfNull(configure);

            var itemOptions = new EfCoreLookupItemsOptions<TEntity, TValue>(value, title);
            configure(new EfCoreLookupItemsBuilder<TEntity, TValue>(itemOptions));

            editor.UseItems(new EfCoreLookupItems<TContext, TEntity, TValue>(itemOptions));
        }
    }
}
