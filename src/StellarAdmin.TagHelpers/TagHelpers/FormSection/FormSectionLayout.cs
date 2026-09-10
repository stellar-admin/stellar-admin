namespace StellarAdmin.TagHelpers;

/// <summary>
///     The layout of a form section.
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
