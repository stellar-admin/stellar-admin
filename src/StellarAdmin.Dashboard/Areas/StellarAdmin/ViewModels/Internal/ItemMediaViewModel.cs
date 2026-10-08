using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels.Internal;

internal sealed record ItemMediaViewModel(
    ItemMedia Media,
    string Text,
    ItemMediaPlacement Placement,
    string? Class
)
{
    public static ItemMediaViewModel? Create(
        ItemMedia? media,
        string text,
        ItemMediaPlacement placement,
        string? css
    ) => media is null ? null : new ItemMediaViewModel(media, text, placement, css);
}

internal enum ItemMediaPlacement
{
    Inline,
    Card,
    Lookup,
    LookupLarge,
    Chip,
}
