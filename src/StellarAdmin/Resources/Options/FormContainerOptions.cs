using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a form item that holds fields and other form items.
/// </summary>
public abstract class FormContainerOptions : FormItemOptions, IFormScope
{
    /// <summary>
    ///     Gets the container's fields and child containers.
    /// </summary>
    /// <value>
    ///     The direct child items, in display order.
    /// </value>
    public IReadOnlyList<FormItemOptions> Items => MutableItems;

    internal FormGridColumnDefinitions Columns { get; set; }

    internal List<FormItemOptions> MutableItems { get; } = [];

    FormGridColumnDefinitions IFormScope.Columns
    {
        get => Columns;
        set => Columns = value;
    }

    IList<FormItemOptions> IFormScope.Items => MutableItems;

    ConfigurationSource? IFormScope.Source => Source;

    private protected FormContainerOptions() { }
}
