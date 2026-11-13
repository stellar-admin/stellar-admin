namespace StellarAdmin.UI;

/// <summary>
///     Specifies the maximum content width of a <c>&lt;sa-page-container&gt;</c>.
/// </summary>
[GenerateDataAttributeText]
public enum PageContainerWidth
{
    /// <summary>
    ///     No maximum width, so the content fills the available space.
    /// </summary>
    /// <remarks>
    ///     Suits data tables and grids.
    /// </remarks>
    Full,

    /// <summary>
    ///     A wide layout that suits dashboards.
    /// </summary>
    Large,

    /// <summary>
    ///     A medium-width layout that suits detail and settings pages.
    /// </summary>
    Medium,

    /// <summary>
    ///     A narrow layout that suits single-column forms.
    /// </summary>
    Small,
}
