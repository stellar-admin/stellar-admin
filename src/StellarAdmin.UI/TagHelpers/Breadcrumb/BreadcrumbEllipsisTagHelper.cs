using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an ellipsis that stands in for breadcrumb items that aren't shown.
/// </summary>
[HtmlTargetElement("sa-breadcrumb-ellipsis")]
public sealed class BreadcrumbEllipsisTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BreadcrumbEllipsisTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">The icon options that supply the ellipsis icon.</param>
    public BreadcrumbEllipsisTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "breadcrumb-ellipsis");
        output.Attributes.SetAttribute("role", "presentation");
        output.Attributes.SetAttribute("aria-hidden", "true");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-breadcrumb-ellipsis", output.GetUserSuppliedClass())
        );

        /* Render the ellipsis icon */
        var iconOutput = new TagHelperOutput(
            "svg",
            [new TagHelperAttribute("class", "size-4")],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var iconTagHelper = new IconTagHelper(_iconOptions)
        {
            Name = _iconOptions.GetSemanticIconName(SemanticIconRole.BreadcrumbEllipsis),
        };
        await iconTagHelper.ProcessAsync(context, iconOutput);
        output.Content.AppendHtml(iconOutput);

        /* Render the screen reader text */
        var srTagBuilder = new TagBuilder("span");
        srTagBuilder.Attributes.Add("class", "sr-only");
        srTagBuilder.InnerHtml.AppendHtml("More");
        output.Content.AppendHtml(srTagBuilder);
    }
}
