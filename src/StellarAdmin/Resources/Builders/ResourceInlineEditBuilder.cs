using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures editing a resource in its index grid row.
/// </summary>
public sealed class ResourceInlineEditBuilder
{
    private readonly Action<Action<ResourceInlineEditOptions>> _configure;

    /// <summary>
    ///     Whether the other rows are dimmed while a row is edited. Defaults to <c>true</c>.
    /// </summary>
    public bool DimOtherRows
    {
        set => _configure(options => options.DimOtherRows = value);
    }

    internal ResourceInlineEditBuilder(Action<Action<ResourceInlineEditOptions>> configure) =>
        _configure = configure;
}
