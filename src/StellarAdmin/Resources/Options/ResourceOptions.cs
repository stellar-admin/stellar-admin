using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Humanizer;
using Microsoft.Extensions.Logging;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Editors;

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
    ///     Gets how the resource's records look when another record refers to them, such as in a
    ///     lookup.
    /// </summary>
    public ResourceReferenceOptions<TResource> Reference { get; } = new();

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
    ///     Gets the expression that selects a record's title, which names the record wherever
    ///     another record refers to it.
    /// </summary>
    /// <value>
    ///     The expression <c>UseTitle</c> sets. Without it, the <see langword="string" /> property
    ///     the resource's <c>[DisplayColumn]</c> names, then its <c>Name</c> or <c>Title</c>
    ///     property, then its key; <see langword="null" /> when the resource has none of these.
    /// </value>
    public Expression<Func<TResource, string?>>? Title =>
        TitleExpression ?? ResourceTitle.FindConvention<TResource>(KeyPropertyName);

    /// <summary>
    ///     Gets the configuration of the view page.
    /// </summary>
    /// <value>
    ///     The configuration, or <see langword="null" /> when the resource has no view page.
    /// </value>
    public ResourceViewOptions? View { get; internal set; }

    // How fields on other resources' forms refer to this resource, created once its options are built
    internal ResourceReference AsReference => field ??= ResourceReference.Create(this);
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
        IReadOnlyList<LambdaExpression>,
        ResourceReferenceItems
    >? ReferenceItemsFactory { get; set; }
    internal Expression<Func<TResource, string?>>? TitleExpression { get; set; }
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
                $"The key isn't changed because `UseKey` selects `{property.Name}`, which isn't a direct property of `{typeof(TResource).Name}` with a public getter.",
                $"Pass `UseKey` a property of `{typeof(TResource).Name}` with a public getter."
            );
            return;
        }

        KeySelector = resource =>
            Convert.ToString(property.GetValue(resource), CultureInfo.InvariantCulture)!;
        KeyPropertyName = property.Name;
    }

    /// <summary>
    ///     Sets how an integration creates the items of fields that refer to the resource.
    /// </summary>
    /// <param name="createItems">
    ///     A delegate that creates a field's items from its filters. Each filter is an expression
    ///     that takes a record and the field's form, and returns whether the record is an item.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="createItems" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     By default, the items load through the resource's data source, which doesn't apply
    ///     filters.
    /// </remarks>
    public void UseReferenceItems(
        Func<IReadOnlyList<LambdaExpression>, ResourceReferenceItems> createItems
    )
    {
        ArgumentNullException.ThrowIfNull(createItems);

        ReferenceItemsFactory = createItems;
    }

    /// <summary>
    ///     Reports an adjustment that an integration made next to what the app configured.
    /// </summary>
    /// <param name="eventId">The ID of the log event, which identifies the kind of problem.</param>
    /// <param name="message">What the developer notices, why, and what the admin panel does instead.</param>
    /// <param name="fix">How to fix the configuration.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="message" /> or <paramref name="fix" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Call it from a post-configuration callback after correcting the options, so that they
    ///     can still be used. Backticks mark code in <paramref name="message" /> and
    ///     <paramref name="fix" />. The problem is logged once when the app starts, and
    ///     Development shows it.
    /// </remarks>
    public void ReportInfo(EventId eventId, string message, string fix) =>
        ReportIntegrationProblem(eventId, message, fix, null, ConfigurationProblemLevel.Info);

    /// <summary>
    ///     Reports an adjustment that an integration made next to an index column.
    /// </summary>
    /// <param name="eventId">The ID of the log event, which identifies the kind of problem.</param>
    /// <param name="message">What the developer notices, why, and what the admin panel does instead.</param>
    /// <param name="fix">How to fix the configuration.</param>
    /// <param name="column">The column the problem is about.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="message" />, <paramref name="fix" /> or <paramref name="column" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Call it from a post-configuration callback after correcting the options, so that they
    ///     can still be used. The problem names the line of the app's code that added the column.
    /// </remarks>
    public void ReportInfo(
        EventId eventId,
        string message,
        string fix,
        DataGridColumnOptions column
    )
    {
        ArgumentNullException.ThrowIfNull(column);

        ReportIntegrationProblem(
            eventId,
            message,
            fix,
            column.Source,
            ConfigurationProblemLevel.Info
        );
    }

    /// <summary>
    ///     Reports an adjustment that an integration made next to a form field or another form
    ///     item.
    /// </summary>
    /// <param name="eventId">The ID of the log event, which identifies the kind of problem.</param>
    /// <param name="message">What the developer notices, why, and what the admin panel does instead.</param>
    /// <param name="fix">How to fix the configuration.</param>
    /// <param name="item">The field or other form item the problem is about.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="message" />, <paramref name="fix" /> or <paramref name="item" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Call it from a post-configuration callback after correcting the options, so that they
    ///     can still be used. The problem names the line of the app's code that added the item.
    /// </remarks>
    public void ReportInfo(EventId eventId, string message, string fix, FormItemOptions item)
    {
        ArgumentNullException.ThrowIfNull(item);

        ReportIntegrationProblem(
            eventId,
            message,
            fix,
            item.Source,
            ConfigurationProblemLevel.Info
        );
    }

    /// <summary>
    ///     Reports a problem in the resource's configuration that an integration worked around.
    /// </summary>
    /// <param name="eventId">The ID of the log event, which identifies the kind of problem.</param>
    /// <param name="message">What the developer notices, why, and what the admin panel does instead.</param>
    /// <param name="fix">How to fix the configuration.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="message" /> or <paramref name="fix" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Call it from a post-configuration callback after correcting the options, so that they
    ///     can still be used. Backticks mark code in <paramref name="message" /> and
    ///     <paramref name="fix" />. The problem is logged once when the app starts, and
    ///     Development shows it.
    /// </remarks>
    public void ReportProblem(EventId eventId, string message, string fix) =>
        ReportIntegrationProblem(eventId, message, fix, null, ConfigurationProblemLevel.Warning);

    /// <summary>
    ///     Reports a problem with an index column that an integration worked around.
    /// </summary>
    /// <param name="eventId">The ID of the log event, which identifies the kind of problem.</param>
    /// <param name="message">What the developer notices, why, and what the admin panel does instead.</param>
    /// <param name="fix">How to fix the configuration.</param>
    /// <param name="column">The column the problem is about.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="message" />, <paramref name="fix" /> or <paramref name="column" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Call it from a post-configuration callback after correcting the options, so that they
    ///     can still be used. The problem names the line of the app's code that added the column.
    /// </remarks>
    public void ReportProblem(
        EventId eventId,
        string message,
        string fix,
        DataGridColumnOptions column
    )
    {
        ArgumentNullException.ThrowIfNull(column);

        ReportIntegrationProblem(
            eventId,
            message,
            fix,
            column.Source,
            ConfigurationProblemLevel.Warning
        );
    }

    /// <summary>
    ///     Reports a problem with a form field or another form item that an integration worked
    ///     around.
    /// </summary>
    /// <param name="eventId">The ID of the log event, which identifies the kind of problem.</param>
    /// <param name="message">What the developer notices, why, and what the admin panel does instead.</param>
    /// <param name="fix">How to fix the configuration.</param>
    /// <param name="item">The field or other form item the problem is about.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="message" />, <paramref name="fix" /> or <paramref name="item" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Call it from a post-configuration callback after correcting the options, so that they
    ///     can still be used. The problem names the line of the app's code that added the item.
    /// </remarks>
    public void ReportProblem(EventId eventId, string message, string fix, FormItemOptions item)
    {
        ArgumentNullException.ThrowIfNull(item);

        ReportIntegrationProblem(
            eventId,
            message,
            fix,
            item.Source,
            ConfigurationProblemLevel.Warning
        );
    }

    /// <summary>
    ///     Reports a problem in the resource's configuration that makes the resource unavailable.
    /// </summary>
    /// <param name="eventId">The ID of the log event, which identifies the kind of problem.</param>
    /// <param name="message">What the developer notices and why.</param>
    /// <param name="fix">How to fix the configuration.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="message" /> or <paramref name="fix" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The resource is hidden from the sidebar, and its pages return 404. Backticks mark code
    ///     in <paramref name="message" /> and <paramref name="fix" />. The problem is logged once
    ///     when the app starts, and Development shows it.
    /// </remarks>
    public void ReportUnavailable(EventId eventId, string message, string fix)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(fix);

        Problems.Add(
            ConfigurationProblem.Unavailable(
                ConfigurationProblemKind.Integration,
                message,
                fix
            ) with
            {
                IntegrationEventId = eventId,
                Source = null,
            }
        );
    }

    // A check that runs after the app's code has returned passes where the app configured what it checked
    internal void Report(
        ConfigurationProblemKind kind,
        string message,
        string fix,
        ConfigurationSource? source = null
    )
    {
        var problem = new ConfigurationProblem(kind, message, fix);
        Problems.Add(source is null ? problem : problem with { Source = source });
    }

    // Labels set to blank text fall back to their defaults
    internal void RepairLabels()
    {
        if (string.IsNullOrWhiteSpace(SingularLabel))
        {
            Report(
                ConfigurationProblemKind.BlankLabel,
                $"The resource uses the default label `{DefaultSingularLabel}` because its `SingularLabel` is blank.",
                "Set `SingularLabel` to text, or leave it unset."
            );
            SingularLabel = DefaultSingularLabel;
        }

        if (string.IsNullOrWhiteSpace(SidebarItem.Group))
        {
            Report(
                ConfigurationProblemKind.BlankLabel,
                $"The resource is in the `{ResourceSidebarItemOptions.DefaultGroup}` sidebar group because its sidebar item's `Group` is blank.",
                "Set the sidebar item's `Group` to text, or leave it unset."
            );
            SidebarItem.Group = ResourceSidebarItemOptions.DefaultGroup;
        }

        if (SidebarItem.Label is { } label && string.IsNullOrWhiteSpace(label))
        {
            Report(
                ConfigurationProblemKind.BlankLabel,
                "The resource's sidebar item shows its plural label because the item's `Label` is blank.",
                "Set the sidebar item's `Label` to text, or leave it unset."
            );
            SidebarItem.Label = null;
        }

        if (SidebarItem.Icon is { } icon && string.IsNullOrWhiteSpace(icon))
        {
            Report(
                ConfigurationProblemKind.BlankLabel,
                "The resource's sidebar item has no icon because its `Icon` is blank.",
                "Set the sidebar item's `Icon` to the name of an icon, or leave it unset."
            );
            SidebarItem.Icon = null;
        }

        if (string.IsNullOrWhiteSpace(PluralLabel))
        {
            Report(
                ConfigurationProblemKind.BlankLabel,
                $"The resource uses the default plural label `{SingularLabel.Pluralize()}` because its `PluralLabel` is blank.",
                "Set `PluralLabel` to text, or leave it unset."
            );
            PluralLabel = SingularLabel.Pluralize();
        }
    }

    // The integration's code isn't where the app configured the resource, so the problem names the item's line, or
    // the resource's AddResource call
    private void ReportIntegrationProblem(
        EventId eventId,
        string message,
        string fix,
        ConfigurationSource? source,
        ConfigurationProblemLevel level
    )
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(fix);

        Problems.Add(
            new(ConfigurationProblemKind.Integration, message, fix)
            {
                IntegrationEventId = eventId,
                Level = level,
                Source = source,
            }
        );
    }
}
