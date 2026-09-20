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
    ///     Whether resources have keys for edit links.
    /// </summary>
    bool CanEdit { get; }

    /// <summary>
    ///     The columns displayed in order.
    /// </summary>
    IReadOnlyList<DataGridColumnOptions> Columns { get; }

    /// <summary>
    ///     The create button label.
    /// </summary>
    string CreateLabel { get; }

    /// <summary>
    ///     The delete confirmation dialog.
    /// </summary>
    ResourceIndexDeleteDialogViewModel? Delete { get; }

    /// <summary>
    ///     The delete button label.
    /// </summary>
    string? DeleteLabel { get; }

    /// <summary>
    ///     The edit link label.
    /// </summary>
    string? EditLabel { get; }

    /// <summary>
    ///     The resources to display.
    /// </summary>
    IEnumerable Items { get; }

    /// <summary>
    ///     The page title.
    /// </summary>
    string Title { get; }

    /// <summary>
    ///     Returns the key used in resource links.
    /// </summary>
    string GetKey(object resource);
}
