namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the search settings of a resource's index page.
/// </summary>
public class ResourceSearchOptions
{
    /// <summary>
    ///     Gets or sets the placeholder of the search box.
    /// </summary>
    /// <value>
    ///     The placeholder, or <see langword="null" /> to use the default from
    ///     <see cref="Builders.ResourceIndexLabelsBuilder.SearchPlaceholder" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? Placeholder { get; set; }
}
