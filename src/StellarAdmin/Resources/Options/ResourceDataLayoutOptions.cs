using System.Linq.Expressions;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of one data layout of a resource's index page.
/// </summary>
public abstract class ResourceDataLayoutOptions
{
    /// <summary>
    ///     Gets the name of the icon on the data layout's switcher segment.
    /// </summary>
    /// <value>
    ///     The icon name, or <see langword="null" /> to show the title instead. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? Icon { get; internal set; }

    /// <summary>
    ///     Gets a value that indicates whether the data layout ignores the scopes.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to hide the scopes and list every resource while the data layout
    ///     is shown; otherwise, <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    public bool IgnoresScopes { get; internal set; }

    /// <summary>
    ///     Gets a value that indicates whether the data layout ignores the search.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to hide the search box and leave the list unsearched while the
    ///     data layout is shown; otherwise, <see langword="false" />. The default is
    ///     <see langword="false" />.
    /// </value>
    public bool IgnoresSearch { get; internal set; }

    /// <summary>
    ///     Gets the key that identifies the data layout.
    /// </summary>
    public string Key { get; }

    /// <summary>
    ///     Gets the data layout's title, which names its switcher segment.
    /// </summary>
    /// <value>
    ///     The title, or <see langword="null" /> to use the key in sentence case, so
    ///     <c>"by-day"</c> reads "By day". The default is <see langword="null" />.
    /// </value>
    public string? Title { get; internal set; }

    // The expressions the data layout reads from each item, which its list requests carry so the data source can load
    // them
    internal virtual IReadOnlyList<LambdaExpression> Fields => [];

    // The view page of another resource that each of a handler's items links to, or null
    internal ResourceRowLink? Link { get; set; }

    // The key of the resource's record that each of a handler's items is, which links it to the record's pages; or null
    internal Func<object, string?>? RecordKey { get; set; }

    private protected ResourceDataLayoutOptions(string key) => Key = key;
}
