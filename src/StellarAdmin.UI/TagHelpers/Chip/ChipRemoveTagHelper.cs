using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a button that removes a chip.
/// </summary>
/// <remarks>
///     Requires an <c>aria-label</c> that names the chip, such as "Remove Lisbon". Shows a close
///     icon when the element has no content. The button doesn't remove the chip. Handle its click
///     to update the value.
/// </remarks>
[HtmlTargetElement("sa-chip-remove")]
public sealed class ChipRemoveTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChipRemoveTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">The icon options that supply the close icon.</param>
    public ChipRemoveTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     The element has no <c>aria-label</c> attribute.
    /// </exception>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (!output.Attributes.ContainsName("aria-label"))
        {
            throw new InvalidOperationException(
                "<sa-chip-remove> requires an aria-label that names the chip, such as \"Remove Lisbon\"."
            );
        }

        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (!output.Attributes.ContainsName("type"))
        {
            output.Attributes.SetAttribute("type", "button");
        }

        if (GetContext<ChipContext>(context)?.Disabled ?? false)
        {
            output.Attributes.SetAttribute("disabled", "disabled");
        }

        output.Attributes.SetAttribute("data-slot", "chip-remove");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-chip-remove", output.GetUserSuppliedClass())
        );

        ButtonRenderingHelper.RenderAttributes(
            output,
            ButtonVariant.Ghost,
            ButtonSize.IconExtraSmall
        );

        var content = await output.GetChildContentAsync();
        if (content.IsEmptyOrWhiteSpace)
        {
            var icon = new TagHelperOutput(
                "svg",
                [new TagHelperAttribute("class", "size-4")],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );

            new IconTagHelper(_iconOptions)
            {
                Name = _iconOptions.GetSemanticIconName(SemanticIconRole.Close),
            }.Process(context, icon);

            output.Content.SetHtmlContent(icon);
        }
        else
        {
            output.Content.SetHtmlContent(content);
        }
    }
}
