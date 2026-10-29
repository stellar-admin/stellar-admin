using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of content displayed in a row's details.
/// </summary>
public sealed class ResourceRowDetailPanelOptions
{
    /// <summary>
    ///     Gets the name of the icon shown before the panel's title.
    /// </summary>
    /// <value>
    ///     The icon name, or <see langword="null" /> for no icon. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? Icon { get; internal set; }

    /// <summary>
    ///     Gets a value that indicates whether the content loads when it's shown.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the content loads when it's shown; otherwise,
    ///     <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Content that doesn't load on demand renders with the index, with the row's resource as
    ///     its model.
    /// </remarks>
    public bool LoadsOnDemand => Loader is not null;

    /// <summary>
    ///     Gets the name that identifies the panel among a row's panels.
    /// </summary>
    /// <value>
    ///     The name, or <see langword="null" /> when the panel is the only one.
    /// </value>
    public string? Name { get; internal set; }

    /// <summary>
    ///     Gets the name of the partial view that renders the content.
    /// </summary>
    public string PartialName { get; }

    /// <summary>
    ///     Gets the title of the panel's accordion item or tab.
    /// </summary>
    /// <value>
    ///     The title, or <see langword="null" /> when the panel is the only one.
    /// </value>
    public string? Title { get; }

    internal ResourceRowDetailLoader? Loader { get; }

    internal ConfigurationSource? Source { get; set; }

    internal ResourceRowDetailPanelOptions(
        string partialName,
        ResourceRowDetailLoader? loader,
        string? title = null
    )
    {
        PartialName = partialName;
        Loader = loader;
        Title = title;
    }
}
