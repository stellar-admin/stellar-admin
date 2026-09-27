using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures EF Core choices for checkbox group editors.
/// </summary>
public static class CheckboxGroupEditorOptionsExtensions
{
    extension(CheckboxGroupEditorOptions options)
    {
        /// <summary>
        ///     Loads checkbox choices from an EF Core entity set using required value and text selectors.
        /// </summary>
        public void UseItems<TContext, TEntity, TValue>(
            Expression<Func<TEntity, TValue>> value,
            Expression<Func<TEntity, string>> text,
            Action<EfCoreCheckboxGroupItemsBuilder<TEntity, TValue>>? configure = null
        )
            where TContext : DbContext
            where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(text);

            var itemOptions = new EfCoreSelectListItemsOptions<TEntity, TValue>(value, text);
            var builder = new EfCoreCheckboxGroupItemsBuilder<TEntity, TValue>(itemOptions);
            configure?.Invoke(builder);

            EfCoreChoiceItemsLoader.Configure<TContext, TEntity, TValue>(options, itemOptions);
        }
    }
}
