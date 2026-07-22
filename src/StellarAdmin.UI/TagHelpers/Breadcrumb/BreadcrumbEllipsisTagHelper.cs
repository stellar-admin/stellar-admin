using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     An ellipsis that stands in for collapsed breadcrumb items, rendered as a
///     presentational <c>&lt;span&gt;</c> with an icon and screen-reader text.
/// </summary>
[HtmlTargetElement("sa-breadcrumb-ellipsis")]
public class BreadcrumbEllipsisTagHelper : StellarAdminTagHelperBase
{
    private readonly IIconManager _iconManager;

    public BreadcrumbEllipsisTagHelper(IIconManager iconManager)
    {
        _iconManager = iconManager ?? throw new ArgumentNullException(nameof(iconManager));
    }

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
        var iconTagHelper = new IconTagHelper(_iconManager) { Name = "ellipsis" };
        await iconTagHelper.ProcessAsync(context, iconOutput);
        output.Content.AppendHtml(iconOutput);

        /* Render the screen reader text */
        var srTagBuilder = new TagBuilder("span");
        srTagBuilder.Attributes.Add("class", "sr-only");
        srTagBuilder.InnerHtml.AppendHtml("More");
        output.Content.AppendHtml(srTagBuilder);
    }
}
