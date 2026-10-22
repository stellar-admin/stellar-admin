using System.Linq.Expressions;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores how a resource's records look when another record refers to them, such as in a
///     lookup.
/// </summary>
/// <typeparam name="TResource">The type of the resource.</typeparam>
public sealed class ResourceReferenceOptions<TResource>
{
    internal ResourceReferenceOptions() { }

    /// <summary>
    ///     Gets the expression that selects a record's description.
    /// </summary>
    /// <value>
    ///     The expression, or <see langword="null" /> when the records have no description.
    /// </value>
    public Expression<Func<TResource, string?>>? Description { get; internal set; }

    /// <summary>
    ///     Gets the expression that selects the value of a record's media, such as an image URL.
    /// </summary>
    /// <value>
    ///     The expression, or <see langword="null" /> when the records have no media.
    /// </value>
    public Expression<Func<TResource, string?>>? Media { get; internal set; }

    /// <summary>
    ///     Gets the type of media the records display.
    /// </summary>
    /// <value>
    ///     The media type, such as <see cref="Editors.ItemMedia.Avatar" />, or
    ///     <see langword="null" /> when the records have no media.
    /// </value>
    public Type? MediaType { get; internal set; }
}
