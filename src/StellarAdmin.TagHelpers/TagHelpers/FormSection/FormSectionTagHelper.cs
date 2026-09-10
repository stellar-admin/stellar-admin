using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Groups form content under a title with an optional description.
/// </summary>
[HtmlTargetElement("sa-form-section")]
public class FormSectionTagHelper(IOptions<StellarAdminFormsOptions> options)
    : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Supporting text displayed below the section title.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     The section layout.
    /// </summary>
    /// <remarks>
    ///     Defaults to the application's form section layout.
    /// </remarks>
    public FormSectionLayout? Layout { get; set; }

    /// <summary>
    ///     The section title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Title);

        var effectiveLayout = Layout ?? options.Value.SectionLayout;
        var headingId = $"sa-form-section-{Guid.NewGuid():N}";

        output.TagName = "section";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "form-section");
        output.Attributes.SetAttribute("data-layout", effectiveLayout.GetDataAttributeText());
        if (
            !output.Attributes.ContainsName("aria-label")
            && !output.Attributes.ContainsName("aria-labelledby")
        )
        {
            output.Attributes.SetAttribute("aria-labelledby", headingId);
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-form-section", output.GetUserSuppliedClass())
        );

        var heading = new TagBuilder("h2");
        heading.Attributes["id"] = headingId;
        heading.Attributes["data-slot"] = "form-section-title";
        heading.AddCssClass("sa-form-section-title sa-font-heading font-heading");
        heading.InnerHtml.Append(Title);

        var header = new TagBuilder("div");
        header.Attributes["data-slot"] = "form-section-header";
        header.AddCssClass("sa-form-section-header");
        header.InnerHtml.AppendHtml(heading);
        if (!string.IsNullOrWhiteSpace(Description))
        {
            var description = new TagBuilder("p");
            description.Attributes["data-slot"] = "form-section-description";
            description.AddCssClass("sa-form-section-description");
            description.InnerHtml.Append(Description);
            header.InnerHtml.AppendHtml(description);
        }

        var content = new TagBuilder("div");
        content.Attributes["data-slot"] = "form-section-content";
        content.AddCssClass("sa-form-section-content");
        content.InnerHtml.AppendHtml(await output.GetChildContentAsync());

        var inner = new TagBuilder("div");
        inner.AddCssClass("sa-form-section-inner");
        inner.InnerHtml.AppendHtml(header);
        inner.InnerHtml.AppendHtml(content);
        output.Content.SetHtmlContent(inner);
    }
}
