using StellarAdmin.Diagnostics;
using StellarAdmin.UI;
using StellarAdmin.Widgets;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a resource form.
/// </summary>
public abstract class ResourceFormOptions : IFormScope
{
    /// <summary>
    ///     Gets every read-only field of the form, in display order.
    /// </summary>
    /// <value>
    ///     The read-only fields, including those inside sections, groups and details sections.
    /// </value>
    public IReadOnlyList<FormDisplayFieldOptions> DisplayFields =>
        EnumerateDisplayFields(Items).ToArray();

    /// <summary>
    ///     Gets every editable field of the form, in display order.
    /// </summary>
    /// <value>
    ///     The fields, including those inside sections and groups.
    /// </value>
    public IReadOnlyList<FormFieldOptions> Fields => EnumerateFields(Items).ToArray();

    /// <summary>
    ///     Gets the form's top-level fields, sections and groups, in display order.
    /// </summary>
    public IReadOnlyList<FormItemOptions> Items => MutableItems;

    /// <summary>
    ///     Gets the layout of the form's sections.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the layout, or <see langword="null" /> to
    ///     use <see cref="StellarAdminFormsOptions.SectionLayout" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public FormSectionLayout? SectionLayout { get; internal set; }

    /// <summary>
    ///     Gets the label of the submit button.
    /// </summary>
    /// <value>
    ///     The label, or <see langword="null" /> to use the default label of the create or edit
    ///     page. The default is <see langword="null" />.
    /// </value>
    public string? SubmitLabel { get; internal set; }

    /// <summary>
    ///     Gets the page title.
    /// </summary>
    /// <value>
    ///     The title, or <see langword="null" /> to use the default title of the create or edit
    ///     page. The default is <see langword="null" />.
    /// </value>
    public string? Title { get; internal set; }

    internal FormGridColumnDefinitions Columns { get; set; }

    internal List<FormItemOptions> MutableItems { get; } = [];

    internal ConfigurationSource? Source { get; set; }

    // The widgets as configured, then, once checked, those that can be shown
    internal List<WidgetOptions> Widgets { get; } = [];

    IList<FormItemOptions> IFormScope.Items => MutableItems;

    ConfigurationSource? IFormScope.Source => Source;

    FormGridColumnDefinitions IFormScope.Columns
    {
        get => Columns;
        set => Columns = value;
    }

    /// <summary>
    ///     Removes a field from the form, wherever its sections and groups hold it.
    /// </summary>
    /// <param name="field">The field to remove.</param>
    /// <returns>
    ///     <see langword="true" /> if the form had the field; otherwise, <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     An integration calls it from a post-configuration callback to leave out a field it
    ///     can't support, and reports the problem with
    ///     <see cref="ResourceOptions{TResource}.ReportProblem" />.
    /// </remarks>
    public bool RemoveField(FormFieldOptions field)
    {
        ArgumentNullException.ThrowIfNull(field);

        return FormItems.Remove(this, field);
    }

    internal static IEnumerable<FormDisplayFieldOptions> EnumerateDisplayFields(
        IEnumerable<FormItemOptions> items
    )
    {
        foreach (var item in items)
        {
            switch (item)
            {
                case FormDisplayFieldOptions field:
                    yield return field;
                    break;
                case FormDetailsOptions details:
                    foreach (var field in details.Fields)
                    {
                        yield return field;
                    }

                    break;
                case FormContainerOptions container:
                    foreach (var field in EnumerateDisplayFields(container.Items))
                    {
                        yield return field;
                    }

                    break;
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
