using StellarAdmin.Pro.Resources.Infrastructure.Expressions;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Pro.Resources.Options;

/// <summary>
///     The configured options shared by the form pages.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public abstract class FormPageOptions<TEntity>
    where TEntity : class
{
    private readonly FormPageDefaults _defaults;

    /// <summary>
    ///     The label of the submit button to display.
    /// </summary>
    public string EffectiveSubmitLabel => _defaults.SubmitLabel;

    /// <summary>
    ///     The page title to display.
    /// </summary>
    public string EffectiveTitle => Title ?? _defaults.Title;

    /// <summary>
    ///     All form fields as a flat list, including fields inside sections, groups, and rows, in display order.
    /// </summary>
    public IReadOnlyList<FormFieldOptions> Fields => EnumerateFields(MutableItems).ToArray();

    /// <summary>
    ///     The form's top-level fields and containers, preserving nested sections, groups, and rows for rendering.
    /// </summary>
    public IReadOnlyList<FormItemOptions> Items => MutableItems;

    /// <summary>
    ///     The layout of the form sections.
    /// </summary>
    /// <remarks>
    ///     Defaults to the application setting.
    /// </remarks>
    public FormSectionLayout? SectionLayout { get; internal set; }

    /// <summary>
    ///     The page subtitle, or <c>null</c> to render no subtitle.
    /// </summary>
    public string? Subtitle { get; internal set; }

    /// <summary>
    ///     The page title.
    /// </summary>
    /// <remarks>Defaults to the title of the resource.</remarks>
    public string? Title { get; internal set; }

    internal List<FormItemOptions> MutableItems { get; } = [];

    protected FormPageOptions(FormPageDefaults defaults)
    {
        _defaults = defaults;

        foreach (var propertyName in defaults.Fields)
        {
            if (FieldExpressionHelper.BuildFieldExpression<TEntity>(propertyName) is { } field)
            {
                MutableItems.Add(new FormFieldOptions(field, propertyName));
            }
        }
    }

    private static IEnumerable<FormFieldOptions> EnumerateFields(IEnumerable<FormItemOptions> items)
    {
        foreach (var item in items)
        {
            if (item is FormFieldOptions field)
            {
                yield return field;
            }
            else if (item is FormContainerOptions container)
            {
                foreach (var child in EnumerateFields(container.Items))
                {
                    yield return child;
                }
            }
        }
    }
}
