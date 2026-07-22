using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A visual separator between breadcrumb items, rendered as a presentational
///     <c>&lt;li&gt;</c>; defaults to a chevron icon when no content is supplied.
/// </summary>
[HtmlTargetElement("sa-breadcrumb-separator")]
public class BreadcrumbSeparatorTagHelper : StellarAdminTagHelperBase
{
    private readonly IIconManager _iconManager;

    public BreadcrumbSeparatorTagHelper(IIconManager iconManager)
    {
        _iconManager = iconManager ?? throw new ArgumentNullException(nameof(iconManager));
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "li";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "breadcrumb-separator");
        output.Attributes.SetAttribute("role", "presentation");
        output.Attributes.SetAttribute("aria-hidden", "true");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-breadcrumb-separator", output.GetUserSuppliedClass())
        );

        var childContent = await output.GetChildContentAsync();
        if (!childContent.IsEmptyOrWhiteSpace)
        {
            output.Content.AppendHtml(await output.GetChildContentAsync());
        }
        else
        {
            /* Render the chevron-right icon */
            var iconOutput = new TagHelperOutput(
                "svg",
                [new TagHelperAttribute("class", "size-4")],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );
            var iconTagHelper = new IconTagHelper(_iconManager) { Name = "chevron-right" };
            await iconTagHelper.ProcessAsync(context, iconOutput);
            output.Content.AppendHtml(iconOutput);
        }
    }
}
