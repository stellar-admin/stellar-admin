using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a separator between groups of slots in an <c>&lt;sa-input-otp&gt;</c>.
/// </summary>
/// <remarks>
///     The separator shows an icon unless it has content of its own.
/// </remarks>
[HtmlTargetElement("sa-input-otp-separator")]
public sealed class InputOtpSeparatorTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InputOtpSeparatorTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">The icon options that supply the separator icon.</param>
    public InputOtpSeparatorTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var userClass = output.GetUserSuppliedClass();

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("data-slot", "input-otp-separator");
        output.Attributes.SetAttribute("role", "separator");
        output.Attributes.SetAttribute(
            "class",
            InputOtpRenderer.SeparatorClass(
                JoinCssClasses(
                    GetContext<InputOtpContext>(context)?.ClassNames?.Separator,
                    userClass
                )
            )
        );

        var childContent = await output.GetChildContentAsync();
        if (!childContent.IsEmptyOrWhiteSpace)
        {
            output.Content.AppendHtml(childContent);
        }
        else
        {
            await InputOtpRenderer.RenderDefaultSeparatorContentAsync(
                output.Content,
                context,
                _iconOptions
            );
        }
    }
}
