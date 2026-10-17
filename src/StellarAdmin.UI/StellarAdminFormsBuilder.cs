namespace StellarAdmin.UI;

/// <summary>
///     Configures the defaults for forms across the application.
/// </summary>
public sealed class StellarAdminFormsBuilder
{
    private readonly StellarAdminFormsOptions _options;

    /// <summary>
    ///     Gets or sets the layout of form sections that don't set their own.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the layout. The default is
    ///     <see cref="FormSectionLayout.Split" />.
    /// </value>
    public FormSectionLayout SectionLayout
    {
        get => _options.SectionLayout;
        set => _options.SectionLayout = value;
    }

    internal StellarAdminFormsBuilder(StellarAdminFormsOptions options)
    {
        _options = options;
    }
}
