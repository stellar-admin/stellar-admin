using System.Globalization;
using System.Reflection;
using Humanizer;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures a resource.
/// </summary>
public sealed class ResourceOptions<TResource>
{
    /// <summary>
    ///     The create page configuration, or null when create is disabled.
    /// </summary>
    public ResourceCreateOptions? Create { get; internal set; }

    /// <summary>
    ///     The delete configuration, or null when delete is disabled.
    /// </summary>
    public ResourceDeleteOptions? Delete { get; internal set; }

    /// <summary>
    ///     The edit page configuration, or null when edit is disabled.
    /// </summary>
    public ResourceEditOptions? Edit { get; internal set; }

    /// <summary>
    ///     The index page configuration.
    /// </summary>
    public ResourceIndexOptions Index { get; internal set; } = new();

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
    ///     The resource's sidebar item configuration.
    /// </summary>
    public ResourceSidebarItemOptions SidebarItem { get; } = new();

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
    internal Func<
        IServiceProvider,
        object,
        CancellationToken,
        Task<ResourceOperationResult>
    >? CreateHandler { get; set; }
    internal Type? DataSourceType { get; set; }
    internal Func<
        IServiceProvider,
        string,
        object,
        CancellationToken,
        Task<ResourceOperationResult>
    >? EditHandler { get; set; }
    internal Func<
        IServiceProvider,
        string,
        CancellationToken,
        Task<object?>
    >? EditLoader { get; set; }
    internal string? KeyPropertyName { get; set; }

    /// <summary>
    ///     Specifies the property that identifies a resource.
    /// </summary>
    public void UseKey(PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);
        if (
            property.GetMethod is not { IsPublic: true, IsStatic: false }
            || property.GetIndexParameters().Length != 0
            || property.DeclaringType?.IsAssignableFrom(typeof(TResource)) != true
        )
        {
            throw new ArgumentException(
                "Select a direct resource property with a public getter.",
                nameof(property)
            );
        }

        KeySelector = resource =>
            Convert.ToString(property.GetValue(resource), CultureInfo.InvariantCulture)!;
        KeyPropertyName = property.Name;
    }
}
