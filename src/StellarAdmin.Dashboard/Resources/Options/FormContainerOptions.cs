namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     A container of fields and other form containers.
/// </summary>
public abstract class FormContainerOptions : FormItemOptions
{
    /// <summary>
    ///     The container's immediate fields and child containers, in display order, preserving any further nesting.
    /// </summary>
    public IReadOnlyList<FormItemOptions> Items => MutableItems;

    internal List<FormItemOptions> MutableItems { get; } = [];

    private protected FormContainerOptions() { }
}
