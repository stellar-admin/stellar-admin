using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the default text of every resource's index page.
/// </summary>
/// <remarks>
///     Text set on a resource's own index builder overrides these defaults.
/// </remarks>
public sealed class ResourceIndexLabelsBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets the delegate that returns the default label of the cancel button on an inline row.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns "Cancel".
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ResourceLabelContext, string> CancelLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Index.CancelLabel = value);
        }
    }

    /// <summary>
    ///     Sets the delegate that returns the default label of the create button on the index page.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns "Create".
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ResourceLabelContext, string> CreateLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Index.CreateLabel = value);
        }
    }

    /// <summary>
    ///     Sets the delegate that returns the default message of the toast shown when a resource is
    ///     added in a new row.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns the resource's
    ///     singular label followed by "added".
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ResourceLabelContext, string> CreatedMessage
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Index.CreatedMessage = value
            );
        }
    }

    /// <summary>
    ///     Sets the delegate that returns the default label of the delete button on the index page.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns "Delete" followed
    ///     by the resource's singular label.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ResourceLabelContext, string> DeleteLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Index.DeleteLabel = value);
        }
    }

    /// <summary>
    ///     Sets the delegate that returns the default label of the edit link on the index page.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns "Edit" followed by
    ///     the resource's singular label.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ResourceLabelContext, string> EditLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Index.EditLabel = value);
        }
    }

    /// <summary>
    ///     Sets the delegate that returns the default description of the toast shown when a saved
    ///     inline row isn't on the reloaded page.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns "It isn't on this
    ///     page with the current search, scope and sort.".
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ResourceLabelContext, string> NotShownMessage
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Index.NotShownMessage = value
            );
        }
    }

    /// <summary>
    ///     Sets the delegate that returns the default label of the save button on an inline row.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns "Save".
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ResourceLabelContext, string> SaveLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Index.SaveLabel = value);
        }
    }

    /// <summary>
    ///     Sets the delegate that returns the default message of the toast shown when an edited
    ///     inline row isn't on the reloaded page.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns the resource's
    ///     singular label followed by "saved".
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ResourceLabelContext, string> SavedMessage
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Index.SavedMessage = value
            );
        }
    }

    /// <summary>
    ///     Sets the delegate that returns the default placeholder of the search box on the index
    ///     page.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns "Search" followed
    ///     by the resource's plural label and "...".
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ResourceLabelContext, string> SearchPlaceholder
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Index.SearchPlaceholder = value
            );
        }
    }

    /// <summary>
    ///     Sets the delegate that returns the default title of the index page.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns the resource's
    ///     plural label.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ResourceLabelContext, string> Title
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Index.Title = value);
        }
    }

    /// <summary>
    ///     Sets the delegate that returns the default label of the view link on the index page.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns "View" followed by
    ///     the resource's singular label.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ResourceLabelContext, string> ViewLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Index.ViewLabel = value);
        }
    }

    internal ResourceIndexLabelsBuilder(IServiceCollection services) => _services = services;
}
