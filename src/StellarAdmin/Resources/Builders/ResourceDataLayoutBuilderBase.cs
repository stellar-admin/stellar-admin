using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Provides the settings shared by the builders that configure a data layout of a resource's
///     index page.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
/// <typeparam name="TBuilder">
///     The type of the derived builder that methods return for chaining.
/// </typeparam>
public abstract class ResourceDataLayoutBuilderBase<TResource, TBuilder>
    where TBuilder : ResourceDataLayoutBuilderBase<TResource, TBuilder>
{
    // Which of HasRecordKey and LinkTo was called, since a handler's item links to one place
    private string? _linkMethod;

    /// <summary>
    ///     Sets the name of the icon on the data layout's switcher segment.
    /// </summary>
    /// <value>
    ///     The icon name, or <see langword="null" /> to show the title instead. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? Icon
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.GetLayout(LayoutKey).Icon = value
            );
    }

    /// <summary>
    ///     Sets the data layout's title, which names its switcher segment.
    /// </summary>
    /// <value>
    ///     The title, or <see langword="null" /> to use the key in sentence case, so
    ///     <c>"by-day"</c> reads "By day". The default is <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     The title is the segment's tooltip and accessible name, and its text when the data
    ///     layout has no icon.
    /// </remarks>
    public string? Title
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.GetLayout(LayoutKey).Title = value
            );
    }

    /// <summary>
    ///     Gets the service collection the builder adds the resource's configuration to.
    /// </summary>
    protected IServiceCollection Services { get; }

    // The key of the data layout the builder configures
    private protected string LayoutKey { get; }

    private protected ResourceDataLayoutBuilderBase(IServiceCollection services, string key)
    {
        Services = services;
        LayoutKey = key;
    }

    // Links each of a handler's items to another resource's view page, with the key of the record it links to
    private protected void ConfigureLink<TOther, TItem>(Func<TItem, object?> key)
    {
        ArgumentNullException.ThrowIfNull(key);
        UseLink("LinkTo");
        Services.Configure<ResourceOptions<TResource>>(options =>
            options.Index.GetLayout(LayoutKey).Link = new(
                typeof(TOther),
                item => FormatKey(key((TItem)item))
            )
        );
    }

    // Makes each of a handler's items a record of the resource, with the key of the record it is
    private protected void ConfigureRecordKey<TItem>(Func<TItem, object?> key)
    {
        ArgumentNullException.ThrowIfNull(key);
        UseLink("HasRecordKey");
        Services.Configure<ResourceOptions<TResource>>(options =>
            options.Index.GetLayout(LayoutKey).RecordKey = item => FormatKey(key((TItem)item))
        );
    }

    // Configures the table the builder configures, once the resource's options are built
    private protected void ConfigureTable(Action<ResourceTableDataLayoutOptions> configure) =>
        Services.Configure<ResourceOptions<TResource>>(options =>
            configure(options.Index.GetOrAddTable(LayoutKey))
        );

    /// <summary>
    ///     Hides the scopes and lists every resource while the data layout is shown.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The selected scope stays in the URL, so switching to another data layout applies it
    ///     again.
    /// </remarks>
    public TBuilder IgnoreScopes()
    {
        Services.Configure<ResourceOptions<TResource>>(options =>
            options.Index.GetLayout(LayoutKey).IgnoresScopes = true
        );

        return (TBuilder)this;
    }

    /// <summary>
    ///     Hides the search box and leaves the list unsearched while the data layout is shown.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The search term stays in the URL, so switching to another data layout applies it again.
    /// </remarks>
    public TBuilder IgnoreSearch()
    {
        Services.Configure<ResourceOptions<TResource>>(options =>
            options.Index.GetLayout(LayoutKey).IgnoresSearch = true
        );

        return (TBuilder)this;
    }

    private static string? FormatKey(object? key) =>
        key is null ? null : Convert.ToString(key, CultureInfo.InvariantCulture);

    private void UseLink(string method)
    {
        if (_linkMethod is not null && _linkMethod != method)
        {
            throw new InvalidOperationException(
                $"The '{LayoutKey}' data layout can't call both HasRecordKey and LinkTo, since an item links to one place."
            );
        }

        _linkMethod = method;
    }
}
