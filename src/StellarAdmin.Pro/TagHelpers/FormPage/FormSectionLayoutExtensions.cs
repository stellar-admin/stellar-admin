using StellarAdmin.TagHelpers;

namespace StellarAdmin.Pro.TagHelpers;

internal static class FormSectionLayoutExtensions
{
    public static string GetDataAttributeText(this FormSectionLayout layout) =>
        layout switch
        {
            FormSectionLayout.Stacked => "stacked",
            FormSectionLayout.Split => "split",
            FormSectionLayout.Card => "card",
            _ => throw new ArgumentOutOfRangeException(nameof(layout), layout, null),
        };
}
