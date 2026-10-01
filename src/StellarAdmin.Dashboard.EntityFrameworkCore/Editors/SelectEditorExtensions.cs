using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures EF Core choices for select editors.
/// </summary>
public static class SelectEditorExtensions
{
    extension(SelectEditor editor)
    {
        /// <summary>
        ///     Loads select choices from an EF Core entity set using required value and text selectors.
        /// </summary>
        public void UseItems<TContext, TEntity, TValue>(
            Expression<Func<TEntity, TValue>> value,
            Expression<Func<TEntity, string>> text,
            Action<EfCoreSelectItemsBuilder<TEntity, TValue>>? configure = null
        )
            where TContext : DbContext
            where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(editor);
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(text);

            var itemOptions = new EfCoreChoiceItemsOptions<TEntity, TValue>(value, text);
            var builder = new EfCoreSelectItemsBuilder<TEntity, TValue>(itemOptions);
            configure?.Invoke(builder);
            EfCoreChoiceItemsLoader.Configure<TContext, TEntity, TValue>(editor, itemOptions);
        }
    }
}
