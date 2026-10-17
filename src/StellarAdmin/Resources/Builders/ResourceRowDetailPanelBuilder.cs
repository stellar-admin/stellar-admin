using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a panel in a row's details.
/// </summary>
public sealed class ResourceRowDetailPanelBuilder
{
    private readonly ResourceRowDetailPanelOptions _options;

    /// <summary>
    ///     Sets the name that identifies the panel among a row's tabs.
    /// </summary>
    /// <value>
    ///     The name. The default is the title in lower case, with dashes in place of spaces and
    ///     punctuation.
    /// </value>
    /// <remarks>
    ///     Each panel in a row's details needs a unique name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     The value is empty or whitespace.
    /// </exception>
    public string Name
    {
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            _options.Name = value;
        }
    }

    internal ResourceRowDetailPanelBuilder(ResourceRowDetailPanelOptions options) =>
        _options = options;
}
