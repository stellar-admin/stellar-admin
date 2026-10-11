using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Displays a user's image, falling back to initials or a name-derived monogram when no
///     image is available.
/// </summary>
[HtmlTargetElement("sa-avatar")]
public class AvatarTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Explicit initials to display when no image is available. Takes precedence over
    ///     initials derived from <see cref="Name" />.
    /// </summary>
    [HtmlAttributeName("initials")]
    public string? Initials { get; set; }

    /// <summary>
    ///     The user's name, used as the image's alt text and to derive fallback initials.
    /// </summary>
    [HtmlAttributeName("name")]
    public string? Name { get; set; }

    /// <summary>
    ///     The size of the avatar.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="AvatarSize.Default" />.
    /// </remarks>
    [HtmlAttributeName("size")]
    public AvatarSize? Size { get; set; }

    /// <summary>
    ///     The URL of the avatar image. When omitted, a fallback with initials is rendered.
    /// </summary>
    [HtmlAttributeName("src")]
    public string? Source { get; set; }

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
