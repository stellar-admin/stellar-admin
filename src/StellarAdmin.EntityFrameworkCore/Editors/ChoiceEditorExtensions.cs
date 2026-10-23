using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Provides extension methods that load the choices of choice editors with EF Core.
/// </summary>
public static class ChoiceEditorExtensions
{
    extension(ChoiceEditor editor)
    {
        /// <summary>
        ///     Loads the choices from an entity set.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <typeparam name="TValue">The type of the value that identifies an entity.</typeparam>
        /// <param name="value">An expression that selects an entity's value.</param>
        /// <param name="title">An expression that selects an entity's title.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value" /> or <paramref name="title" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        ///     The choices load for each request from the registered database context whose model maps
        ///     <typeparamref name="TEntity" />, unless <c>UseDbContext</c> picks one.
        /// </remarks>
        public void UseItems<TEntity, TValue>(
            Expression<Func<TEntity, TValue>> value,
            Expression<Func<TEntity, string>> title
        )
            where TEntity : class => editor.UseItems<TEntity, TValue>(value, title, _ => { });

        /// <summary>
        ///     Loads the choices from an entity set, and configures what each choice shows.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <typeparam name="TValue">The type of the value that identifies an entity.</typeparam>
        /// <param name="value">An expression that selects an entity's value.</param>
        /// <param name="title">An expression that selects an entity's title.</param>
        /// <param name="configure">A delegate that configures the choices.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value" />, <paramref name="title" /> or
        ///     <paramref name="configure" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        ///     The choices load for each request from the registered database context whose model maps
        ///     <typeparamref name="TEntity" />, unless <c>UseDbContext</c> picks one.
        /// </remarks>
        public void UseItems<TEntity, TValue>(
            Expression<Func<TEntity, TValue>> value,
            Expression<Func<TEntity, string>> title,
            Action<EfCoreChoiceItemsBuilder<TEntity, TValue>> configure
        )
            where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(editor);
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(title);
            ArgumentNullException.ThrowIfNull(configure);

            var itemOptions = new EfCoreChoiceItemsOptions<TEntity, TValue>(value, title);
            configure(new EfCoreChoiceItemsBuilder<TEntity, TValue>(itemOptions));

            EfCoreChoiceItemsLoader.Configure<TEntity, TValue>(editor, itemOptions);
        }
    }
}
