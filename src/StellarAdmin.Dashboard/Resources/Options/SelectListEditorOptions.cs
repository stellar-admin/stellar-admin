using Microsoft.AspNetCore.Mvc.Rendering;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures a select editor with application-supplied choices.
/// </summary>
public sealed class SelectListEditorOptions : EditorOptions, IFieldEditorOptions<SelectListEditor>
{
    internal Func<IReadOnlyList<SelectListItem>>? ItemsFactory { get; private set; }

    internal Type? ItemsProviderType { get; private set; }

    /// <summary>
    ///     Supplies the choices when the form is rendered.
    /// </summary>
    public void UseItems(Func<IReadOnlyList<SelectListItem>> itemsFactory)
    {
        ArgumentNullException.ThrowIfNull(itemsFactory);

        ItemsFactory = itemsFactory;
        ItemsProviderType = null;
    }

    /// <summary>
    ///     Selects a registered provider that supplies choices for each request.
    /// </summary>
    public void UseItemsFrom<TProvider>()
        where TProvider : class, ISelectListItemsProvider
    {
        ItemsProviderType = typeof(TProvider);
        ItemsFactory = null;
    }
}
