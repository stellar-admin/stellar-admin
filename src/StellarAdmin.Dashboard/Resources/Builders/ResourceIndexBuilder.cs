using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a resource's index page.
/// </summary>
public sealed class ResourceIndexBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The create button label.
    /// </summary>
    public string? CreateLabel
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.CreateLabel = value
            );
    }

    /// <summary>
    ///     The delete button label.
    /// </summary>
    public string? DeleteLabel
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.DeleteLabel = value
            );
    }

    /// <summary>
    ///     The edit link label.
    /// </summary>
    public string? EditLabel
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.EditLabel = value
            );
    }

    /// <summary>
    ///     The page title.
    /// </summary>
    public string? Title
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options => options.Index.Title = value);
    }

    internal ResourceIndexBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    ///     Configures the index columns.
    /// </summary>
    public ResourceIndexBuilder<TResource> Columns(
        Action<ResourceColumnsBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Enables index paging.
    /// </summary>
    public ResourcePagingBuilder<TResource> EnablePaging()
    {
        _services.Configure<ResourceOptions<TResource>>(options => options.Index.Paging = new());

        return new(_services);
    }

    /// <summary>
    ///     Enables and configures index paging.
    /// </summary>
    public ResourceIndexBuilder<TResource> EnablePaging(
        Action<ResourcePagingBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnablePaging());

        return this;
    }
}
