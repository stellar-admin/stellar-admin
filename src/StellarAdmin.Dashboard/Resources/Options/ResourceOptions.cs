using Humanizer;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures a resource.
/// </summary>
public sealed class ResourceOptions<TResource>
{
    /// <summary>
    ///     The index page configuration.
    /// </summary>
    public ResourceIndexOptions Index { get; } = new();

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
}
