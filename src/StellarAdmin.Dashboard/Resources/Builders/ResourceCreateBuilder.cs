using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a resource's create page.
/// </summary>
public sealed class ResourceCreateBuilder<TModel>
{
    private readonly Action<Action<ResourceCreateOptions<TModel>>> _configure;

    /// <summary>
    ///     The layout of form sections.
    /// </summary>
    public FormSectionLayout? SectionLayout
    {
        set => _configure(options => options.SectionLayout = value);
    }

    /// <summary>
    ///     The submit button label.
    /// </summary>
    public string? SubmitLabel
    {
        set => _configure(options => options.SubmitLabel = value);
    }

    /// <summary>
    ///     The page title.
    /// </summary>
    public string? Title
    {
        set => _configure(options => options.Title = value);
    }

    internal ResourceCreateBuilder(Action<Action<ResourceCreateOptions<TModel>>> configure) =>
        _configure = configure;

    /// <summary>
    ///     Configures the form fields.
    /// </summary>
    public ResourceCreateBuilder<TModel> Fields(Action<ResourceFieldsBuilder<TModel>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(action => _configure(options => action(options.Items))));

        return this;
    }

    /// <summary>
    ///     Specifies how to instantiate a resource for the create form.
    /// </summary>
    public ResourceCreateBuilder<TModel> UseFactory(Func<TModel> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _configure(options => options.Factory = factory);

        return this;
    }
}
