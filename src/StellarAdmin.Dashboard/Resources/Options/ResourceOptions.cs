using Humanizer;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures a resource.
/// </summary>
public sealed class ResourceOptions<TResource>
{
    /// <summary>
    ///     The create page configuration.
    /// </summary>
    public ResourceCreateOptions<TResource> Create { get; } = new();

    /// <summary>
    ///     The delete configuration.
    /// </summary>
    public ResourceDeleteOptions Delete { get; } = new();

    /// <summary>
    ///     The edit page configuration.
    /// </summary>
    public ResourceFormOptions Edit { get; } = new();

    /// <summary>
    ///     The index page configuration.
    /// </summary>
    public ResourceIndexOptions Index { get; } = new();

    /// <summary>
    ///     Returns a resource's key as a URL value.
    /// </summary>
    public Func<TResource, string>? KeySelector { get; set; }

    /// <summary>
    ///     The plural resource label.
    /// </summary>
    /// <remarks>
    ///     Defaults to the English plural of SingularLabel.
    /// </remarks>
    public string PluralLabel
    {
        get => field ?? SingularLabel.Pluralize();
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value;
        }
    }

    /// <summary>
    ///     The singular resource label.
    /// </summary>
    /// <remarks>
    ///     Defaults to the resource type's name with words separated by spaces.
    /// </remarks>
    public string SingularLabel
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value;
        }
    } = typeof(TResource).Name.Split('`')[0].Humanize();
    internal string? KeyPropertyName { get; set; }
}
