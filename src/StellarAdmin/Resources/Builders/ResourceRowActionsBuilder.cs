using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures which of the resource's row actions a table shows.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
/// <remarks>
///     A table shows an action only when the resource also allows it, so these settings can only
///     turn actions off.
/// </remarks>
public sealed class ResourceRowActionsBuilder<TResource>
{
    private readonly IServiceCollection _services;
    private readonly string _tableKey;

    /// <summary>
    ///     Sets a value that indicates whether rows have a delete button.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to show the button when the resource can be deleted; otherwise,
    ///     <see langword="false" />. The default is <see langword="true" />.
    /// </value>
    public bool Delete
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.GetOrAddTable(_tableKey).RowActions.Delete = value
            );
    }

    /// <summary>
    ///     Sets a value that indicates whether rows have an edit link or edit button.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to show the link or button when the resource can be edited;
    ///     otherwise, <see langword="false" />. The default is <see langword="true" />.
    /// </value>
    public bool Edit
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.GetOrAddTable(_tableKey).RowActions.Edit = value
            );
    }

    /// <summary>
    ///     Sets a value that indicates whether rows have a view link.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to show the link when the resource has a view page; otherwise,
    ///     <see langword="false" />. The default is <see langword="true" />.
    /// </value>
    public bool View
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.GetOrAddTable(_tableKey).RowActions.View = value
            );
    }

    internal ResourceRowActionsBuilder(IServiceCollection services, string tableKey)
    {
        _services = services;
        _tableKey = tableKey;
    }
}
