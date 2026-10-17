using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Provides the rendering shared by the media of radio and checkbox group options.
/// </summary>
public abstract class ChoiceGroupItemMediaTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Renders the content as the media of the option that contains it.
    /// </summary>
    /// <param name="context">The context of the element being rendered.</param>
    /// <param name="output">The output of the element being rendered.</param>
    /// <param name="multiple">
    ///     <see langword="true" /> if the media belongs to a checkbox group option; otherwise,
    ///     <see langword="false" />.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">
    ///     The element isn't inside a matching radio or checkbox group option, or the option already
    ///     has media.
    /// </exception>
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
