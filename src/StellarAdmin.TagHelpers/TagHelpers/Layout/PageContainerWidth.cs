namespace StellarAdmin.TagHelpers;

/// <summary>
///     The maximum content width of a <c>&lt;sa-page-container&gt;</c>.
/// </summary>
public enum PageContainerWidth
{
    /// <summary>No maximum width; the content fills the available space. Suited to data tables and grids.</summary>
    Full,

    /// <summary>A wide layout suited to dashboards.</summary>
    Large,

    /// <summary>A medium-width layout suited to detail and settings pages.</summary>
    Medium,

    /// <summary>A narrow layout suited to single-column forms.</summary>
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
