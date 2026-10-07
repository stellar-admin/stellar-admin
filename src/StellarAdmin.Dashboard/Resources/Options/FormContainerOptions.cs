namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     A container of fields and other form containers.
/// </summary>
public abstract class FormContainerOptions : FormItemOptions, IFormScope
{
    /// <summary>
    ///     The container's immediate fields and child containers, in display order, preserving any further nesting.
    /// </summary>
    public IReadOnlyList<FormItemOptions> Items => MutableItems;

    internal FormGridColumnDefinitions Columns { get; set; }

    internal List<FormItemOptions> MutableItems { get; } = [];

    FormGridColumnDefinitions IFormScope.Columns
    {
        get => Columns;
        set => Columns = value;
    }

    IList<FormItemOptions> IFormScope.Items => MutableItems;

    private protected FormContainerOptions() { }
}
