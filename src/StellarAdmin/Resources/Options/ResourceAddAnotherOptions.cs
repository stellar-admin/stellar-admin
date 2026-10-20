namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of saving a new resource and starting the next one in the same
///     step.
/// </summary>
public sealed class ResourceAddAnotherOptions
{
    /// <summary>
    ///     Gets the names of the fields whose saved values carry into the next new resource.
    /// </summary>
    public IReadOnlyList<string> KeptFields => KeptFieldNames;

    /// <summary>
    ///     Gets the label of the button that saves and starts the next resource.
    /// </summary>
    /// <value>
    ///     The label, or <see langword="null" /> to use the default from
    ///     <see cref="ResourceCreateLabelOptions.AddAnotherLabel" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? Label { get; internal set; }

    internal List<string> KeptFieldNames { get; } = [];
}
