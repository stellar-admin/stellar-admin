using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a bar across the top of the window that names the environment the application runs
///     in.
/// </summary>
/// <remarks>
///     The indicator is fixed to the window, so it can be placed anywhere in the page.
/// </remarks>
[HtmlTargetElement("sa-environment-banner")]
public sealed class EnvironmentBannerTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the colour of the indicator.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the colour. The default is
    ///     <see cref="EnvironmentBannerColor.Neutral" />.
    /// </value>
    /// <remarks>
    ///     <see cref="CustomColor" /> and <see cref="CustomDarkColor" /> override it.
    /// </remarks>
    [HtmlAttributeName("color")]
    public EnvironmentBannerColor? Color { get; set; }

    /// <summary>
    ///     Gets or sets a custom colour for the indicator.
    /// </summary>
    /// <value>
    ///     A CSS colour, such as <c>#7c3aed</c>, or <see langword="null" /> to use
    ///     <see cref="Color" />. The default is <see langword="null" />.
    /// </value>
    [HtmlAttributeName("custom-color")]
    public string? CustomColor { get; set; }

    /// <summary>
    ///     Gets or sets a custom colour for the indicator in dark mode.
    /// </summary>
    /// <value>
    ///     A CSS colour, or <see langword="null" /> to use <see cref="CustomColor" /> in dark mode
    ///     too. The default is <see langword="null" />.
    /// </value>
    [HtmlAttributeName("custom-dark-color")]
    public string? CustomDarkColor { get; set; }

    /// <summary>
    ///     Gets or sets the text shown after the name of the environment.
    /// </summary>
    /// <value>
    ///     The description, or <see langword="null" /> for none. The default is
    ///     <see langword="null" />.
    /// </value>
    [HtmlAttributeName("description")]
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the name of the environment.
    /// </summary>
    [HtmlAttributeName("label")]
    public required string Label { get; set; }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">
    ///     <see cref="Label" /> is empty or white space.
    /// </exception>
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(Label))
        {
            throw new ArgumentException(
                "The 'label' attribute is required on <sa-environment-banner>."
            );
        }

        var effectiveColor = Color ?? EnvironmentBannerColor.Neutral;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "environment-banner");
        output.Attributes.SetAttribute("data-color", effectiveColor.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-environment-banner", output.GetUserSuppliedClass())
        );
        EnvironmentIndicatorRendering.RenderCustomColors(output, CustomColor, CustomDarkColor);

        output.Content.SetHtmlContent(CreateContent());

        return Task.CompletedTask;
    }

    private IHtmlContent CreateContent()
    {
        var content = new HtmlContentBuilder();

        var label = new TagBuilder("strong");
        label.AddCssClass("sa-environment-banner-label");
        label.InnerHtml.Append(Label);
        content.AppendHtml(label);

        if (!string.IsNullOrWhiteSpace(Description))
        {
            var description = new TagBuilder("span");
            description.AddCssClass("sa-environment-banner-description");
            description.InnerHtml.Append(Description);
            content.AppendHtml(description);
        }

        return content;
    }
}
