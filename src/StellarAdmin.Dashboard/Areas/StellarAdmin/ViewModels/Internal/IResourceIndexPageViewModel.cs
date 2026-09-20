using System.Collections;
using System.ComponentModel;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels.Internal;

/// <summary>
///     Provides index page data to the default views.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IResourceIndexPageViewModel
{
    /// <summary>
    ///     The columns displayed in order.
    /// </summary>
    IReadOnlyList<DataGridColumnOptions> Columns { get; }

    /// <summary>
    ///     The create button label.
    /// </summary>
    string CreateLabel { get; }

    /// <summary>
    ///     The resources to display.
    /// </summary>
    IEnumerable Items { get; }

    /// <summary>
    ///     The page title.
    /// </summary>
    string Title { get; }
}
