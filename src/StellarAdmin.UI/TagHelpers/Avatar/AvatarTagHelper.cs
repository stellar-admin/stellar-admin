using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a user's image, or their initials when there's no image.
/// </summary>
[HtmlTargetElement("sa-avatar")]
public sealed class AvatarTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the initials shown when there's no image.
    /// </summary>
    /// <value>
    ///     The default is the initials of the user's name.
    /// </value>
    [HtmlAttributeName("initials")]
    public string? Initials { get; set; }

    /// <summary>
    ///     Gets or sets the user's name.
    /// </summary>
    /// <remarks>
    ///     The name is the image's alternative text. Without an image or <c>initials</c>, the
    ///     avatar shows the first letters of the name's first two words, or the first two letters
    ///     of a single word.
    /// </remarks>
    [HtmlAttributeName("name")]
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the size of the avatar.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="AvatarSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public AvatarSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets the URL of the image.
    /// </summary>
    /// <value>
    ///     The URL, or <see langword="null" /> to show initials. The default is
    ///     <see langword="null" />.
    /// </value>
    [HtmlAttributeName("src")]
    public string? Source { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveAvatarSize = Size ?? AvatarSize.Default;

        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "avatar");
        output.Attributes.SetAttribute("data-size", effectiveAvatarSize.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-avatar", output.GetUserSuppliedClass())
        );

        if (Source != null)
        {
            var imageTagBuilder = new TagBuilder("img");
            imageTagBuilder.Attributes.Add("data-slot", "avatar-image");
            imageTagBuilder.Attributes.Add("src", Source);
            imageTagBuilder.Attributes.Add("alt", Name);
            imageTagBuilder.Attributes.Add("class", JoinCssClasses("sa-avatar-image"));
            output.Content.AppendHtml(imageTagBuilder);
        }
        else
        {
            var initials = GetInitials();
            var fallbackTagBuilder = new TagBuilder("span");
            fallbackTagBuilder.Attributes.Add("data-slot", "avatar-fallback");
            fallbackTagBuilder.Attributes.Add("class", JoinCssClasses("sa-avatar-fallback"));
            if (initials is null)
            {
                fallbackTagBuilder.InnerHtml.AppendHtml("&nbsp;");
            }
            else
            {
                fallbackTagBuilder.InnerHtml.Append(initials);
            }

            output.Content.AppendHtml(fallbackTagBuilder);
        }

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }

    private string? GetInitials()
    {
        return (Initials, Name) switch
        {
            ({ } initials, _) => initials,
            (_, { } name) => DetermineInitialsFromName(name),
            _ => null,
        };

        string? DetermineInitialsFromName(string name)
        {
            // Words that don't start with a letter or digit, such as "&", are skipped
            var splitName = name.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                )
                .Where(word => char.IsLetterOrDigit(word[0]))
                .ToArray();

            // The first letters of the first two words, or the first two letters of a single word
            return splitName switch
            {
                [var first, var second, ..] => $"{first[0]}{second[0]}".ToUpperInvariant(),
                [var word] => word[..Math.Min(2, word.Length)].ToUpperInvariant(),
                _ => null,
            };
        }
    }
}
