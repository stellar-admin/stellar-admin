namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a form section places its heading and fields.
/// </summary>
public enum FormSectionLayout
{
    /// <summary>
    ///     Displays the heading above the fields.
    /// </summary>
    Stacked,

    /// <summary>
    ///     Displays the heading beside the fields when space permits.
    /// </summary>
    Split,

    /// <summary>
    ///     Displays the heading and fields inside a bordered card.
    /// </summary>
    Card,
}

internal static class FormSectionLayoutExtensions
{
    extension(FormSectionLayout layout)
    {
        public string GetDataAttributeText() =>
            layout switch
            {
                FormSectionLayout.Stacked => "stacked",
                FormSectionLayout.Split => "split",
                FormSectionLayout.Card => "card",
                _ => throw new ArgumentOutOfRangeException(nameof(layout), layout, null),
            };
    }
}
