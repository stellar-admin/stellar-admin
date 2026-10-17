namespace StellarAdmin.UI;

/// <summary>
///     Specifies the maximum content width of a <c>&lt;sa-page-container&gt;</c>.
/// </summary>
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

internal static class PageContainerWidthExtensions
{
    extension(PageContainerWidth width)
    {
        public string GetDataAttributeText() =>
            width switch
            {
                PageContainerWidth.Full => "full",
                PageContainerWidth.Large => "large",
                PageContainerWidth.Medium => "medium",
                PageContainerWidth.Small => "small",
                _ => "",
            };

        public string GetWidthCssClass() =>
            width switch
            {
                PageContainerWidth.Full => "sa-page-container-width-full",
                PageContainerWidth.Large => "sa-page-container-width-large",
                PageContainerWidth.Medium => "sa-page-container-width-medium",
                PageContainerWidth.Small => "sa-page-container-width-small",
                _ => "",
            };
    }
}
