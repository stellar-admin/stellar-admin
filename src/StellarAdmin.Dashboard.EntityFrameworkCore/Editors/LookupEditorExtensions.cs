using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures EF Core items for lookup editors.
/// </summary>
public static class LookupEditorExtensions
{
    extension(LookupEditor editor)
    {
        /// <summary>
        ///     Searches lookup items in an EF Core entity set using required value and text selectors.
        /// </summary>
        public void UseItems<TContext, TEntity, TValue>(
            Expression<Func<TEntity, TValue>> value,
            Expression<Func<TEntity, string>> text,
            Action<EfCoreLookupItemsBuilder<TEntity, TValue>>? configure = null
        )
            where TContext : DbContext
            where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(editor);
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(text);

            var itemOptions = new EfCoreLookupItemsOptions<TEntity, TValue>(value, text);
            configure?.Invoke(new EfCoreLookupItemsBuilder<TEntity, TValue>(itemOptions));

            editor.UseItems(new EfCoreLookupItems<TContext, TEntity, TValue>(itemOptions));
        }
    }
}
