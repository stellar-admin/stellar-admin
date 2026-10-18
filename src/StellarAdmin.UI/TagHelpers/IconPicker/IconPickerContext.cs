using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

// Collects the icons that the picker's child sa-icon elements render, for the picker to place.
internal sealed class IconPickerContext
{
    public List<(string? Name, TagHelperOutput Svg)> Icons { get; } = [];

    public required StellarAdminTagHelperBase Owner { get; init; }
}
