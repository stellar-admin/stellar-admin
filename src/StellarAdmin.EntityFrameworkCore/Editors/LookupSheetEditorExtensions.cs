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
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="value">
        ///     An expression that selects an entity's value, of the type the field's property holds.
        /// </param>
        /// <param name="title">An expression that selects an entity's title.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value" /> or <paramref name="title" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        ///     The items load for each request from the registered database context whose model maps
        ///     <typeparamref name="TEntity" />, unless <c>UseDbContext</c> picks one.
        /// </remarks>
        public void UseItems<TEntity>(
            Expression<Func<TEntity, object?>> value,
            Expression<Func<TEntity, string>> title
        )
            where TEntity : class => editor.UseItems<TEntity>(value, title, _ => { });

        /// <summary>
        ///     Looks up the items in an entity set, and configures how they're searched and shown.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="value">
        ///     An expression that selects an entity's value, of the type the field's property holds.
        /// </param>
        /// <param name="title">An expression that selects an entity's title.</param>
        /// <param name="configure">A delegate that configures the items.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value" />, <paramref name="title" /> or
        ///     <paramref name="configure" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        ///     The items load for each request from the registered database context whose model maps
        ///     <typeparamref name="TEntity" />, unless <c>UseDbContext</c> picks one.
        /// </remarks>
        public void UseItems<TEntity>(
            Expression<Func<TEntity, object?>> value,
            Expression<Func<TEntity, string>> title,
            Action<EfCoreLookupItemsBuilder<TEntity>> configure
        )
            where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(editor);
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(title);
            ArgumentNullException.ThrowIfNull(configure);

            var itemOptions = EfCoreLookupItemsOptions<TEntity>.Create(value, title);
            configure(new EfCoreLookupItemsBuilder<TEntity>(itemOptions));

            editor.UseItems(itemOptions.CreateItems());
        }
    }
}
