using Microsoft.AspNetCore.Html;

namespace StellarAdmin.TagHelpers;

internal sealed class ChoiceGroupItemContext
{
    public IHtmlContent? Media { get; set; }

    public required bool Multiple { get; init; }
}
