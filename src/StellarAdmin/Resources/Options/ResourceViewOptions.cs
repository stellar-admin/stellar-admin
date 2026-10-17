using StellarAdmin.UI;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a resource's view page.
/// </summary>
public abstract class ResourceViewOptions : IFormScope
{
    /// <summary>
    ///     Gets every field of the page, in display order.
    /// </summary>
    /// <value>
    ///     The fields, including those inside sections, groups and details sections.
    /// </value>
    public IReadOnlyList<FormDisplayFieldOptions> Fields =>
        ResourceFormOptions.EnumerateDisplayFields(Items).ToArray();

    /// <summary>
    ///     Gets the page's top-level fields, sections and groups, in display order.
    /// </summary>
    public IList<FormItemOptions> Items { get; } = new List<FormItemOptions>();

    /// <summary>
    ///     Gets the type of the model the page displays.
    /// </summary>
    public abstract Type ModelType { get; }

    /// <summary>
    ///     Gets or sets the layout of the page's sections.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the layout, or <see langword="null" /> to
    ///     use <see cref="StellarAdminFormsOptions.SectionLayout" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public FormSectionLayout? SectionLayout { get; set; }

    /// <summary>
    ///     Gets or sets the page title.
    /// </summary>
    /// <value>
    ///     The title, or <see langword="null" /> to use
    ///     <see cref="ResourceViewLabelOptions.Title" />. The default is <see langword="null" />.
    /// </value>
    public string? Title { get; set; }

    internal FormGridColumnDefinitions Columns { get; set; }

    FormGridColumnDefinitions IFormScope.Columns
    {
        get => Columns;
        set => Columns = value;
    }

    private protected ResourceViewOptions() { }
}
