using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Configures EF Core choices for choice editors.
/// </summary>
public static class ChoiceEditorExtensions
{
    extension(ChoiceEditor editor)
    {
        /// <summary>
        ///     Loads choices from an EF Core entity set using required value and title selectors.
        /// </summary>
        public void UseItems<TContext, TEntity, TValue>(
            Expression<Func<TEntity, TValue>> value,
            Expression<Func<TEntity, string>> title
        )
            where TContext : DbContext
            where TEntity : class =>
            editor.UseItems<TContext, TEntity, TValue>(value, title, _ => { });

        /// <summary>
        ///     Loads choices from an EF Core entity set using required value and title selectors, configured further by
        ///     <paramref name="configure" />.
        /// </summary>
        public void UseItems<TContext, TEntity, TValue>(
            Expression<Func<TEntity, TValue>> value,
            Expression<Func<TEntity, string>> title,
            Action<EfCoreChoiceItemsBuilder<TEntity, TValue>> configure
        )
            where TContext : DbContext
            where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(editor);
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(title);
            ArgumentNullException.ThrowIfNull(configure);

            var itemOptions = new EfCoreChoiceItemsOptions<TEntity, TValue>(value, title);
            configure(new EfCoreChoiceItemsBuilder<TEntity, TValue>(itemOptions));

            EfCoreChoiceItemsLoader.Configure<TContext, TEntity, TValue>(editor, itemOptions);
        }
    }
}
