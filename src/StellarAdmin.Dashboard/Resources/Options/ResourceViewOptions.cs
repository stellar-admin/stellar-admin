using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures a resource's view page.
/// </summary>
public abstract class ResourceViewOptions : IFormScope
{
    /// <summary>
    ///     All fields in display order, including fields inside sections and groups.
    /// </summary>
    public IReadOnlyList<FormDisplayFieldOptions> Fields =>
        ResourceFormOptions.EnumerateDisplayFields(Items).ToArray();

    /// <summary>
    ///     The page's fields, sections and groups in display order.
    /// </summary>
    public IList<FormItemOptions> Items { get; } = new List<FormItemOptions>();

    /// <summary>
    ///     The model the page displays.
    /// </summary>
    public abstract Type ModelType { get; }

    /// <summary>
    ///     The layout of the page's sections.
    /// </summary>
    /// <remarks>
    ///     Defaults to the application's form settings.
    /// </remarks>
    public FormSectionLayout? SectionLayout { get; set; }

    /// <summary>
    ///     The page title.
    /// </summary>
    public string? Title { get; set; }

    internal FormGridColumnDefinitions Columns { get; set; }

    FormGridColumnDefinitions IFormScope.Columns
    {
        get => Columns;
        set => Columns = value;
    }

    private protected ResourceViewOptions() { }
}
