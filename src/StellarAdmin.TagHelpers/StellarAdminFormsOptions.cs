using StellarAdmin.TagHelpers;

namespace StellarAdmin;

/// <summary>
///     The application-wide defaults for forms.
/// </summary>
public sealed class StellarAdminFormsOptions
{
    /// <summary>
    ///     The default layout of form sections.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="FormSectionLayout.Split" />.
    /// </remarks>
    public FormSectionLayout SectionLayout { get; set; } = FormSectionLayout.Split;
}
