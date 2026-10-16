using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures editing a resource in its index grid row.
/// </summary>
public sealed class ResourceInlineEditBuilder
{
    private readonly Action<Action<ResourceInlineEditOptions>> _configure;

    /// <summary>
    ///     What the grid shows after a row is saved. Defaults to
    ///     <see cref="InlineRowAfterSave.ReloadGrid" />.
    /// </summary>
    public InlineRowAfterSave AfterSave
    {
        set => _configure(options => options.AfterSave = value);
    }

    /// <summary>
    ///     Whether the other rows and the page's controls are dimmed and locked while a row is edited,
    ///     until it is saved or cancelled. Defaults to <c>true</c>.
    /// </summary>
    public bool DimOtherRows
    {
        set => _configure(options => options.DimOtherRows = value);
    }

    internal ResourceInlineEditBuilder(Action<Action<ResourceInlineEditOptions>> configure) =>
        _configure = configure;
}
