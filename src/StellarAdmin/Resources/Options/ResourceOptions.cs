using System.Globalization;
using System.Reflection;
using Humanizer;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a resource.
/// </summary>
/// <typeparam name="TResource">The type of the resource.</typeparam>
public sealed class ResourceOptions<TResource>
{
    /// <summary>
    ///     Gets the configuration of the create page.
    /// </summary>
    /// <value>
    ///     The configuration, or <see langword="null" /> when the resource has no create page.
    /// </value>
    public ResourceCreateOptions? Create { get; internal set; }

    /// <summary>
    ///     Gets the delete configuration.
    /// </summary>
    /// <value>
    ///     The configuration, or <see langword="null" /> when resources can't be deleted.
    /// </value>
    public ResourceDeleteOptions? Delete { get; internal set; }

    /// <summary>
    ///     Gets the configuration of the edit page.
    /// </summary>
    /// <value>
    ///     The configuration, or <see langword="null" /> when the resource has no edit page.
    /// </value>
    public ResourceEditOptions? Edit { get; internal set; }

    /// <summary>
    ///     Gets the configuration of the index page.
    /// </summary>
    public ResourceIndexOptions Index { get; internal set; } = new();

    /// <summary>
    ///     Gets the delegate that returns a resource's key as a URL value.
    /// </summary>
    /// <value>
    ///     The delegate, or <see langword="null" /> when no key is configured. The default is
    ///     <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     <see cref="UseKey" /> sets this delegate.
    /// </remarks>
    public Func<TResource, string>? KeySelector { get; internal set; }

    /// <summary>
    ///     Gets or sets the label for more than one resource.
    /// </summary>
    /// <value>
    ///     The plural label. The default is the English plural of <see cref="SingularLabel" />.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     The value is empty or whitespace.
    /// </exception>
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
    ///     Gets the configuration of the resource's sidebar item.
    /// </summary>
    public ResourceSidebarItemOptions SidebarItem { get; } = new();

    /// <summary>
    ///     Gets or sets the label for one resource.
    /// </summary>
    /// <value>
    ///     The singular label. The default is the name of the resource type split into words,
    ///     with only the first word capitalized.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     The value is empty or whitespace.
    /// </exception>
    public string SingularLabel
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value;
        }
    } = typeof(TResource).Name.Split('`')[0].Humanize();

    /// <summary>
    ///     Gets the configuration of the view page.
    /// </summary>
    /// <value>
    ///     The configuration, or <see langword="null" /> when the resource has no view page.
    /// </value>
    public ResourceViewOptions? View { get; internal set; }

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
    internal Func<
        IServiceProvider,
        string,
        CancellationToken,
        Task<object?>
    >? ViewLoader { get; set; }

    /// <summary>
    ///     Sets the property that identifies a resource.
    /// </summary>
    /// <param name="property">The property that holds the resource's key.</param>
    /// <remarks>
    ///     The key is converted to a string with the invariant culture.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="property" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="property" /> isn't a property of the resource with a public instance
    ///     getter, or is an indexer.
    /// </exception>
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
