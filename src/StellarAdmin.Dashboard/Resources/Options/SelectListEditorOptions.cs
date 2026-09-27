using Microsoft.AspNetCore.Mvc.Rendering;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures a select editor with application-supplied choices.
/// </summary>
public sealed class SelectListEditorOptions
    : ChoiceItemsEditorOptions,
        IFieldEditorOptions<SelectListEditor>
{
    /// <summary>
    ///     Selects a registered provider that supplies choices for each request.
    /// </summary>
    public new void UseItems<TProvider>()
        where TProvider : class, ISelectListItemsProvider => base.UseItems<TProvider>();

    /// <summary>
    ///     Supplies a fixed set of choices.
    /// </summary>
    public new void UseItems(IEnumerable<SelectListItem> items) => base.UseItems(items);

    /// <summary>
    ///     Supplies choices from a request-aware asynchronous loader.
    /// </summary>
    public new void UseItems(
        Func<IServiceProvider, CancellationToken, Task<IReadOnlyList<SelectListItem>>> itemsLoader
    ) => base.UseItems(itemsLoader);
}
