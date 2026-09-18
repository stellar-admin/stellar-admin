using Microsoft.AspNetCore.Html;

namespace StellarAdmin.TagHelpers;

internal sealed class ChoiceGroupContext
{
    public int ItemCount { get; set; }

    public required bool Multiple { get; init; }

    public required Func<
        string,
        IHtmlContent,
        string?,
        bool,
        string?,
        Task<IHtmlContent>
    > RenderItem { get; init; }
}
