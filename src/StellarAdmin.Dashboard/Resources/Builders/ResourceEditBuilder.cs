using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a resource's edit page.
/// </summary>
public sealed class ResourceEditBuilder<TModel>
{
    private readonly Action<Action<ResourceEditOptions<TModel>>> _configure;

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

    internal ResourceEditBuilder(Action<Action<ResourceEditOptions<TModel>>> configure) =>
        _configure = configure;

    /// <summary>
    ///     Configures the form fields.
    /// </summary>
    public ResourceEditBuilder<TModel> Fields(Action<ResourceFieldsBuilder<TModel>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(action => _configure(options => action(options.Items))));

        return this;
    }
}
