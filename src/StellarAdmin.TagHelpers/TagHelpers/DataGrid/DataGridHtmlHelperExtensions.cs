using Microsoft.AspNetCore.Mvc.Rendering;

namespace StellarAdmin.TagHelpers;

public static class DataGridHtmlHelperExtensions
{
    extension(IHtmlHelper htmlHelper)
    {
        /// <summary>
        ///     Returns the data item of the data grid row currently being rendered. Only valid
        ///     inside the content of an <c>sa-data-grid-column</c>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     No data grid row is being rendered, or the row item is not of type
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
