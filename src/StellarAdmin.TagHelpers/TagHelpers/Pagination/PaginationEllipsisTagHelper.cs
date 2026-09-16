using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A non-interactive item that indicates omitted pages within the pagination.
/// </summary>
[HtmlTargetElement("sa-pagination-ellipsis")]
public class PaginationEllipsisTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    public PaginationEllipsisTagHelper(IOptions<IconOptions> iconOptions)
        : this(iconOptions.Value) { }

    internal PaginationEllipsisTagHelper(IconOptions iconOptions)
    {
        _iconOptions = iconOptions;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("aria-hidden", "true");
        output.Attributes.SetAttribute("data-slot", "pagination-ellipsis");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-pagination-ellipsis", output.GetUserSuppliedClass())
        );

        var content = await output.GetChildContentAsync();

        if (!content.IsEmptyOrWhiteSpace)
        {
            output.Content.AppendHtml(content);
        }
        else
        {
            // Render the icon
            var iconOutput = new TagHelperOutput(
                "svg",
                [new TagHelperAttribute("class", "size-4")],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );
            var iconTagHelper = new IconTagHelper(_iconOptions)
            {
                Name = _iconOptions.GetSemanticIconName(SemanticIconRole.PaginationEllipsis),
            };
            await iconTagHelper.ProcessAsync(context, iconOutput);
            output.Content.AppendHtml(iconOutput);

            // Render the text
            var textBlockTagBuilder = new TagBuilder("span");
            textBlockTagBuilder.AddCssClass("sr-only");
            textBlockTagBuilder.InnerHtml.AppendHtml("More pages");
            output.Content.AppendHtml(textBlockTagBuilder);
        }
    }
}
