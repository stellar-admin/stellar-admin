using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels.Internal;

internal sealed record LookupMediaViewModel(LookupMedia Media, string Title, bool IsLarge)
{
    // A code needs text to display; an avatar without an image falls back to the title's initials
    public static LookupMediaViewModel? Create(LookupMedia? media, string title, bool isLarge) =>
        media is not null
        && (media.Type == LookupMediaType.Avatar || !string.IsNullOrEmpty(media.Value))
            ? new LookupMediaViewModel(media, title, isLarge)
            : null;
}
