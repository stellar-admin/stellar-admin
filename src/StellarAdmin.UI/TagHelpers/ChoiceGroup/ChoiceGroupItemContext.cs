using Microsoft.AspNetCore.Html;

namespace StellarAdmin.UI;

internal sealed class ChoiceGroupItemContext
{
    public IHtmlContent? Media { get; set; }

    public required bool Multiple { get; init; }
}
