using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a panel in a row's details.
/// </summary>
public sealed class ResourceRowDetailPanelBuilder
{
    private readonly ResourceRowDetailPanelOptions _options;

    /// <summary>
    ///     The name that identifies the panel among a row's tabs. Names must be unique.
    /// </summary>
    /// <remarks>
    ///     Defaults to the title in lower case, with dashes in place of spaces and punctuation.
    /// </remarks>
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
