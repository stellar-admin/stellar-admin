using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures a select editor with application-supplied choices.
/// </summary>
public sealed class SelectListEditorOptions : EditorOptions, IFieldEditorOptions<SelectListEditor>
{
    internal Func<
        IServiceProvider,
        CancellationToken,
        Task<IReadOnlyList<SelectListItem>>
    >? ItemsLoader { get; private set; }

    /// <summary>
    ///     Selects a registered provider that supplies choices for each request.
    /// </summary>
    public void UseItems<TProvider>()
        where TProvider : class, ISelectListItemsProvider
    {
        ItemsLoader = (services, cancellationToken) =>
            services.GetRequiredService<TProvider>().GetItemsAsync(cancellationToken);
    }

    /// <summary>
    ///     Supplies a fixed set of choices.
    /// </summary>
    public void UseItems(IEnumerable<SelectListItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var snapshot = items.ToArray();
        ItemsLoader = (_, _) => Task.FromResult<IReadOnlyList<SelectListItem>>(snapshot);
    }

    /// <summary>
    ///     Supplies choices from a request-aware asynchronous loader.
    /// </summary>
    public void UseItems(
        Func<IServiceProvider, CancellationToken, Task<IReadOnlyList<SelectListItem>>> itemsLoader
    )
    {
        ArgumentNullException.ThrowIfNull(itemsLoader);

        ItemsLoader = itemsLoader;
    }
}
