namespace StellarAdmin.Resources.Editors;

// The media a selected value creates: a code, icon or image without a value has no media, and an avatar without an
// image shows initials
internal static class ItemMediaFactory
{
    public static ItemMedia? Create(Type mediaType, string? value) =>
        mediaType == typeof(ItemMedia.Avatar) ? new ItemMedia.Avatar(value)
        : string.IsNullOrEmpty(value) ? null
        : mediaType == typeof(ItemMedia.Code) ? new ItemMedia.Code(value)
        : mediaType == typeof(ItemMedia.Icon) ? new ItemMedia.Icon(value)
        : mediaType == typeof(ItemMedia.Image) ? new ItemMedia.Image(value)
        : null;
}
