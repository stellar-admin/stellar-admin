using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Shared rendering for the media of radio and checkbox group options.
/// </summary>
public abstract class ChoiceGroupItemMediaTagHelper : StellarAdminTagHelperBase
{
    protected async Task RenderAsync(
        TagHelperContext context,
        TagHelperOutput output,
        bool multiple
    )
    {
        var item = GetContext<ChoiceGroupItemContext>(context);
        if (item == null || item.Multiple != multiple)
        {
            throw new InvalidOperationException(
                "Group item media requires a matching radio-group-item or checkbox-group-item parent."
            );
        }

        if (item.Media != null)
        {
            throw new InvalidOperationException("A group item can have only one media element.");
        }

        // The option places its media, before the text or leading a card
        item.Media = await output.GetChildContentAsync();
        output.SuppressOutput();
    }
}
