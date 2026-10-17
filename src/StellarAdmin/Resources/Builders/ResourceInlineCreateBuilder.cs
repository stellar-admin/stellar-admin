using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures adding a resource as a new row in its index grid.
/// </summary>
public sealed class ResourceInlineCreateBuilder
{
    private readonly Action<Action<ResourceInlineCreateOptions>> _configure;

    /// <summary>
    ///     Sets where the Add button is shown.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies where the button is shown. The default is
    ///     <see cref="InlineAddButton.Both" />.
    /// </value>
    public InlineAddButton AddButton
    {
        set => _configure(options => options.AddButton = value);
    }

    /// <summary>
    ///     Sets what the grid shows after the new row is saved.
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
    ///     Sets a value that indicates whether the rest of the page is dimmed while the new row is
    ///     edited.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the other rows and the page's controls are dimmed and locked
    ///     until the new row is saved or cancelled; otherwise, <see langword="false" />. The
    ///     default is <see langword="true" />.
    /// </value>
    public bool DimOtherRows
    {
        set => _configure(options => options.DimOtherRows = value);
    }

    /// <summary>
    ///     Sets where the new row is added.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies where the row is added. The default is
    ///     <see cref="InlineCreatePosition.Top" />.
    /// </value>
    public InlineCreatePosition Position
    {
        set => _configure(options => options.Position = value);
    }

    internal ResourceInlineCreateBuilder(Action<Action<ResourceInlineCreateOptions>> configure) =>
        _configure = configure;
}
