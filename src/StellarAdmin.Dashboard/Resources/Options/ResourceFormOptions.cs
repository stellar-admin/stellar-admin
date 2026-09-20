using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures a resource form.
/// </summary>
public class ResourceFormOptions
{
    /// <summary>
    ///     All fields in display order, including fields inside layout containers.
    /// </summary>
    public IReadOnlyList<FormFieldOptions> Fields => EnumerateFields(Items).ToArray();

    /// <summary>
    ///     The form's fields and layout containers in display order.
    /// </summary>
    public IList<FormItemOptions> Items { get; } = new List<FormItemOptions>();

    /// <summary>
    ///     The layout of form sections.
    /// </summary>
    /// <remarks>
    ///     Defaults to the application's form settings.
    /// </remarks>
    public FormSectionLayout? SectionLayout { get; set; }

    /// <summary>
    ///     The submit button label.
    /// </summary>
    public string? SubmitLabel { get; set; }

    /// <summary>
    ///     The page title.
    /// </summary>
    public string? Title { get; set; }

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
