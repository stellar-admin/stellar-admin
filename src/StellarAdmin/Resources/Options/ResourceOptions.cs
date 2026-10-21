using System.Globalization;
using System.Reflection;
using Humanizer;
using Microsoft.Extensions.Logging;
using StellarAdmin.Diagnostics;

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
    /// <remarks>
    ///     An empty or white space label is a configuration problem, and the default is used.
    /// </remarks>
    public string PluralLabel
    {
        get => field ?? SingularLabel.Pluralize();
        set
        {
            ArgumentNullException.ThrowIfNull(value);
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
    /// <remarks>
    ///     An empty or white space label is a configuration problem, and the default is used.
    /// </remarks>
    public string SingularLabel
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = DefaultSingularLabel;

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
    internal bool IsUnavailable => Problems.Any(problem => problem.MakesResourceUnavailable);
    internal string? KeyPropertyName { get; set; }
    internal Func<
        IServiceProvider,
        string,
        CancellationToken,
        Task<object?>
    >? ViewLoader { get; set; }
    internal List<ConfigurationProblem> Problems { get; } = [];

    private static string DefaultSingularLabel => typeof(TResource).Name.Split('`')[0].Humanize();

    /// <summary>
    ///     Sets the property that identifies a resource.
    /// </summary>
    /// <param name="property">The property that holds the resource's key.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="property" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The key is converted to a string with the invariant culture. A property that isn't a
    ///     property of the resource with a public instance getter, or is an indexer, is a
    ///     configuration problem, and the key is left unchanged.
    /// </remarks>
    public void UseKey(PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);
        if (
            property.GetMethod is not { IsPublic: true, IsStatic: false }
            || property.GetIndexParameters().Length != 0
            || property.DeclaringType?.IsAssignableFrom(typeof(TResource)) != true
        )
        {
            Report(
                ConfigurationProblemKind.InvalidSelector,
                $"UseKey selects '{property.Name}', which isn't a direct property of {typeof(TResource).Name} with a public getter.",
                "The key is left unchanged."
            );
            return;
        }

        KeySelector = resource =>
            Convert.ToString(property.GetValue(resource), CultureInfo.InvariantCulture)!;
        KeyPropertyName = property.Name;
    }

    /// <summary>
    ///     Reports a problem in the resource's configuration that an integration worked around.
    /// </summary>
    /// <param name="eventId">The ID of the log event, which identifies the kind of problem.</param>
    /// <param name="message">What is wrong and how to fix it.</param>
    /// <param name="outcome">What the admin panel does instead, such as "The form doesn't show the field."</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="message" /> or <paramref name="outcome" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Call it from a post-configuration callback after correcting the options, so that they
    ///     can still be used. The problem is logged once when the app starts, and Development
    ///     shows it.
    /// </remarks>
    public void ReportProblem(EventId eventId, string message, string outcome)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(outcome);

        Problems.Add(
            new(ConfigurationProblemKind.Integration, message, outcome)
            {
                IntegrationEventId = eventId,
            }
        );
    }

    /// <summary>
    ///     Reports a problem in the resource's configuration that makes the resource unavailable.
    /// </summary>
    /// <param name="eventId">The ID of the log event, which identifies the kind of problem.</param>
    /// <param name="message">What is wrong and how to fix it.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="message" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The resource is hidden from the sidebar, and its pages return 404. The problem is
    ///     logged once when the app starts, and Development shows it.
    /// </remarks>
    public void ReportUnavailable(EventId eventId, string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        Problems.Add(
            ConfigurationProblem.Unavailable(ConfigurationProblemKind.Integration, message) with
            {
                IntegrationEventId = eventId,
            }
        );
    }

    internal void Report(ConfigurationProblemKind kind, string message, string outcome) =>
        Problems.Add(new(kind, message, outcome));

    // Labels set to blank text fall back to their defaults
    internal void RepairLabels()
    {
        if (string.IsNullOrWhiteSpace(SingularLabel))
        {
            Report(
                ConfigurationProblemKind.BlankLabel,
                "The resource's SingularLabel is blank. Set it to text, or leave it unset.",
                $"The resource uses the default label '{DefaultSingularLabel}'."
            );
            SingularLabel = DefaultSingularLabel;
        }

        if (string.IsNullOrWhiteSpace(SidebarItem.Group))
        {
            Report(
                ConfigurationProblemKind.BlankLabel,
                "The resource's sidebar item has a blank Group. Set it to text, or leave it unset.",
                $"The item appears in the '{ResourceSidebarItemOptions.DefaultGroup}' group."
            );
            SidebarItem.Group = ResourceSidebarItemOptions.DefaultGroup;
        }

        if (SidebarItem.Label is { } label && string.IsNullOrWhiteSpace(label))
        {
            Report(
                ConfigurationProblemKind.BlankLabel,
                "The resource's sidebar item has a blank Label. Set it to text, or leave it unset.",
                "The item shows the resource's plural label."
            );
            SidebarItem.Label = null;
        }

        if (SidebarItem.Icon is { } icon && string.IsNullOrWhiteSpace(icon))
        {
            Report(
                ConfigurationProblemKind.BlankLabel,
                "The resource's sidebar item has a blank Icon. Set it to an icon name, or leave it unset.",
                "The item has no icon."
            );
            SidebarItem.Icon = null;
        }

        if (string.IsNullOrWhiteSpace(PluralLabel))
        {
            Report(
                ConfigurationProblemKind.BlankLabel,
                "The resource's PluralLabel is blank. Set it to text, or leave it unset.",
                $"The resource uses the default plural label '{SingularLabel.Pluralize()}'."
            );
            PluralLabel = SingularLabel.Pluralize();
        }
    }
}
