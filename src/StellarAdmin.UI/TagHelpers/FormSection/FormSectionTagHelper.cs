using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a group of form content under a heading with an optional description.
/// </summary>
/// <param name="options">The forms options that supply the default section layout.</param>
[HtmlTargetElement("sa-form-section")]
public sealed class FormSectionTagHelper(IOptions<StellarAdminFormsOptions> options)
    : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the text shown below the section heading.
    /// </summary>
    [HtmlAttributeName("description")]
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets how the section places its heading and fields.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the layout. The default is the
    ///     application's form section layout.
    /// </value>
    /// <remarks>
    ///     The application's layout is set with <see cref="StellarAdminFormsOptions.SectionLayout" />.
    /// </remarks>
    [HtmlAttributeName("layout")]
    public FormSectionLayout? Layout { get; set; }

    /// <summary>
    ///     Gets or sets the section heading.
    /// </summary>
    /// <remarks>
    ///     Required.
    /// </remarks>
    [HtmlAttributeName("title")]
    public string Title { get; set; } = string.Empty;

    /// <inheritdoc />
    /// <exception cref="ArgumentException">
    ///     <see cref="Title" /> is empty or whitespace.
    /// </exception>
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
        heading.AddCssClass("sa-form-section-title");
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
