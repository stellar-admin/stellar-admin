using Microsoft.AspNetCore.Mvc.Rendering;

namespace StellarAdmin.UI;

/// <summary>
///     Provides extension methods on <see cref="IHtmlHelper" /> for data grid content.
/// </summary>
public static class DataGridHtmlHelperExtensions
{
    extension(IHtmlHelper htmlHelper)
    {
        /// <summary>
        ///     Returns the item of the data grid row that is being rendered.
        /// </summary>
        /// <remarks>
        ///     Use it inside the content of an <c>sa-data-grid-column</c>.
        /// </remarks>
        /// <typeparam name="T">The type of the row's item.</typeparam>
        /// <returns>The row's item.</returns>
        /// <exception cref="InvalidOperationException">
        ///     No data grid row is being rendered, or the row's item isn't a
        ///     <typeparamref name="T" />.
        /// </exception>
        public T GridItem<T>()
            where T : class
        {
            return htmlHelper.ViewData[DataGridConsts.ItemKey] switch
            {
                null => throw new InvalidOperationException(
                    "Html.GridItem<T>() is only available inside the content of a data grid column."
                ),
                T item => item,
                var value => throw new InvalidOperationException(
                    $"The data grid item is of type '{value.GetType()}', not '{typeof(T)}'."
                ),
            };
        }
    }
}
