using StellarAdmin.TagHelpers;

namespace StellarAdmin;

/// <summary>
///     Configures the application-wide defaults for forms.
/// </summary>
public sealed class StellarAdminFormsBuilder
{
    private readonly StellarAdminFormsOptions _options;

    /// <summary>
    ///     The default layout of form sections.
    /// </summary>
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
