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
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
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
        var iconOutput = _iconOptions.RenderIcon(
            SemanticIconRole.BreadcrumbEllipsis,
            ("class", "size-4")
        );
        output.Content.AppendHtml(iconOutput);

        /* Render the screen reader text */
        var srTagBuilder = new TagBuilder("span");
        srTagBuilder.Attributes.Add("class", "sr-only");
        srTagBuilder.InnerHtml.AppendHtml("More");
        output.Content.AppendHtml(srTagBuilder);

        return Task.CompletedTask;
    }
}
