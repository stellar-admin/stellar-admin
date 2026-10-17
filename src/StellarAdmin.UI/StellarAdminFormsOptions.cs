namespace StellarAdmin.UI;

/// <summary>
///     Provides the defaults for forms across the application.
/// </summary>
/// <seealso cref="StellarAdminUIBuilder.ConfigureForms" />
public sealed class StellarAdminFormsOptions
{
    /// <summary>
    ///     Gets or sets the layout of form sections that don't set their own.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the layout. The default is
    ///     <see cref="FormSectionLayout.Split" />.
    /// </value>
    public FormSectionLayout SectionLayout { get; set; } = FormSectionLayout.Split;
}
