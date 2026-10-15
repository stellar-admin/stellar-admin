using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures adding a resource as a new row in its index grid.
/// </summary>
public sealed class ResourceInlineCreateBuilder
{
    private readonly Action<Action<ResourceInlineCreateOptions>> _configure;

    /// <summary>
    ///     Where the Add button is shown. Defaults to <see cref="InlineAddButton.Both" />.
    /// </summary>
    public InlineAddButton AddButton
    {
        set => _configure(options => options.AddButton = value);
    }

    /// <summary>
    ///     Whether the other rows are dimmed while the new row is edited. Defaults to <c>true</c>.
    /// </summary>
    public bool DimOtherRows
    {
        set => _configure(options => options.DimOtherRows = value);
    }

    /// <summary>
    ///     Where the new row is added. Defaults to <see cref="InlineCreatePosition.Top" />.
    /// </summary>
    public InlineCreatePosition Position
    {
        set => _configure(options => options.Position = value);
    }

    internal ResourceInlineCreateBuilder(Action<Action<ResourceInlineCreateOptions>> configure) =>
        _configure = configure;
}
