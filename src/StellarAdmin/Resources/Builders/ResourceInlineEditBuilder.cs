using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures editing a resource in its index grid row.
/// </summary>
public sealed class ResourceInlineEditBuilder
{
    private readonly Action<Action<ResourceInlineEditOptions>> _configure;

    /// <summary>
    ///     Sets what the grid shows after a row is saved.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies what the grid shows. The default is
    ///     <see cref="InlineRowAfterSave.ReloadGrid" />.
    /// </value>
    public InlineRowAfterSave AfterSave
    {
        set => _configure(options => options.AfterSave = value);
    }

    /// <summary>
    ///     Sets a value that indicates whether the rest of the page is dimmed while a row is
    ///     edited.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the other rows and the page's controls are dimmed and locked
    ///     until the row is saved or cancelled; otherwise, <see langword="false" />. The default is
    ///     <see langword="true" />.
    /// </value>
    public bool DimOtherRows
    {
        set => _configure(options => options.DimOtherRows = value);
    }

    internal ResourceInlineEditBuilder(Action<Action<ResourceInlineEditOptions>> configure) =>
        _configure = configure;
}
