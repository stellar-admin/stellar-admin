using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a panel in a row's details.
/// </summary>
public sealed class ResourceRowDetailPanelBuilder
{
    private readonly ResourceRowDetailPanelOptions _options;

    /// <summary>
    ///     Sets the icon shown before the panel's title.
    /// </summary>
    /// <value>
    ///     The name of the icon, or <see langword="null" /> for none. The default is
    ///     <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     The icon shows on the panel's accordion title or tab. An empty or white space name
    ///     shows no icon.
    /// </remarks>
    public string? Icon
    {
        set => _options.Icon = value;
    }

    /// <summary>
    ///     Sets the name that identifies the panel among a row's panels.
    /// </summary>
    /// <value>
    ///     The name. The default is the title in lower case, with dashes in place of spaces and
    ///     punctuation.
    /// </value>
    /// <remarks>
    ///     Each panel in a row's details needs a unique name. An empty or white space name, or one
    ///     another panel has, is a configuration problem, and the panel is left out.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public string Name
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _options.Name = value;
        }
    }

    internal ResourceRowDetailPanelBuilder(ResourceRowDetailPanelOptions options) =>
        _options = options;
}
