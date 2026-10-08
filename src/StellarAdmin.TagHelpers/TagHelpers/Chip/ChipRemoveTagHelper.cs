using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The button that removes a chip, placed after its label. It renders a close icon when it
///     has no content, and requires an <c>aria-label</c> that names the chip, such as
///     "Remove Lisbon".
/// </summary>
/// <remarks>
///     The button does not remove the chip itself; handle its click to update the value.
/// </remarks>
[HtmlTargetElement("sa-chip-remove")]
public class ChipRemoveTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    public ChipRemoveTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

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
