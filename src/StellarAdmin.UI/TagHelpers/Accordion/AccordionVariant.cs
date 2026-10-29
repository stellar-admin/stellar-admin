namespace StellarAdmin.UI;

/// <summary>
///     Specifies the visual style of an accordion.
/// </summary>
public enum AccordionVariant
{
    /// <summary>
    ///     Items separated by lines.
    /// </summary>
    Default,

    /// <summary>
    ///     Items in a bordered box, each title on a muted band.
    /// </summary>
    Contained,
}

internal static class AccordionVariantExtensions
{
    extension(AccordionVariant variant)
    {
        public string GetDataAttributeText() =>
            variant switch
            {
                AccordionVariant.Default => "default",
                AccordionVariant.Contained => "contained",
                _ => "",
            };
    }
}
