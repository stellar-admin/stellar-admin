using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Provides the configuration shared by the builders that configure how a resource's records
///     look when another record refers to them.
/// </summary>
/// <typeparam name="TResource">The type of the resource.</typeparam>
/// <typeparam name="TBuilder">The type of the derived builder, returned for chaining.</typeparam>
public abstract class ResourceReferenceBuilderBase<TResource, TBuilder>
    where TBuilder : ResourceReferenceBuilderBase<TResource, TBuilder>
{
    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="ResourceReferenceBuilderBase{TResource, TBuilder}" /> class.
    /// </summary>
    /// <param name="services">The services the configuration is registered with.</param>
    protected ResourceReferenceBuilderBase(IServiceCollection services) => Services = services;

    /// <summary>
    ///     Gets the services the configuration is registered with.
    /// </summary>
    protected IServiceCollection Services { get; }

    /// <summary>
    ///     Shows an avatar with each record, from an image URL or, without one, the title's
    ///     initials.
    /// </summary>
    /// <param name="imageUrl">An expression that selects a record's image URL.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="imageUrl" /> is <see langword="null" />.
    /// </exception>
    public TBuilder UseAvatar(Expression<Func<TResource, string?>> imageUrl) =>
        UseMedia<ItemMedia.Avatar>(imageUrl);

    /// <summary>
    ///     Shows a short code with each record, such as an airport code.
    /// </summary>
    /// <param name="code">An expression that selects a record's code.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="code" /> is <see langword="null" />.
    /// </exception>
    public TBuilder UseCode(Expression<Func<TResource, string?>> code) =>
        UseMedia<ItemMedia.Code>(code);

    /// <summary>
    ///     Shows a description with each record.
    /// </summary>
    /// <param name="description">An expression that selects a record's description.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="description" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     A lookup whose items have a description shows its selection as a card.
    /// </remarks>
    public TBuilder UseDescription(Expression<Func<TResource, string?>> description)
    {
        ArgumentNullException.ThrowIfNull(description);

        Services.Configure<ResourceOptions<TResource>>(options =>
            options.Reference.Description = description
        );

        return (TBuilder)this;
    }

    /// <summary>
    ///     Shows an icon with each record.
    /// </summary>
    /// <param name="iconName">An expression that selects the name of a record's icon.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="iconName" /> is <see langword="null" />.
    /// </exception>
    public TBuilder UseIcon(Expression<Func<TResource, string?>> iconName) =>
        UseMedia<ItemMedia.Icon>(iconName);

    /// <summary>
    ///     Shows an image with each record.
    /// </summary>
    /// <param name="imageUrl">An expression that selects a record's image URL.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="imageUrl" /> is <see langword="null" />.
    /// </exception>
    public TBuilder UseImage(Expression<Func<TResource, string?>> imageUrl) =>
        UseMedia<ItemMedia.Image>(imageUrl);

    private TBuilder UseMedia<TMedia>(Expression<Func<TResource, string?>> selector)
        where TMedia : ItemMedia
    {
        ArgumentNullException.ThrowIfNull(selector);

        Services.Configure<ResourceOptions<TResource>>(options =>
        {
            options.Reference.Media = selector;
            options.Reference.MediaType = typeof(TMedia);
        });

        return (TBuilder)this;
    }
}
