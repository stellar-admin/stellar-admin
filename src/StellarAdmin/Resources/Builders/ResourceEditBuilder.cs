using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Builders;

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

        configure(
            new ResourceFormFieldsBuilder<TModel>(action => _configure(options => action(options)))
        );

        return this;
    }

    /// <summary>
    ///     Edits resources in their index grid row instead of on the edit page.
    /// </summary>
    /// <remarks>
    ///     Every field must match an index column, by name or through the column's
    ///     <c>EditsField</c>. The edit page is not available.
    /// </remarks>
    public ResourceInlineEditBuilder UseInlineRow()
    {
        _configure(options => options.InlineRow ??= new());

        return new(configure => _configure(options => configure(options.InlineRow ??= new())));
    }

    /// <summary>
    ///     Edits resources in their index grid row instead of on the edit page.
    /// </summary>
    /// <remarks>
    ///     Every field must match an index column, by name or through the column's
    ///     <c>EditsField</c>. The edit page is not available.
    /// </remarks>
    public ResourceEditBuilder<TModel> UseInlineRow(Action<ResourceInlineEditBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(UseInlineRow());

        return this;
    }
}
