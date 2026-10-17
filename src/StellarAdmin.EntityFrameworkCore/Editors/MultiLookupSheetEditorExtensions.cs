using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Configures EF Core items for multi-select lookup editors.
/// </summary>
public static class MultiLookupSheetEditorExtensions
{
    extension(MultiLookupSheetEditor editor)
    {
        /// <summary>
        ///     Searches lookup items in an EF Core entity set using required value and title selectors.
        /// </summary>
        public void UseItems<TContext, TEntity, TValue>(
            Expression<Func<TEntity, TValue>> value,
            Expression<Func<TEntity, string>> title
        )
            where TContext : DbContext
            where TEntity : class =>
            editor.UseItems<TContext, TEntity, TValue>(value, title, _ => { });

        /// <summary>
        ///     Searches lookup items in an EF Core entity set using required value and title selectors, configured further by
        ///     <paramref name="configure" />.
        /// </summary>
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
