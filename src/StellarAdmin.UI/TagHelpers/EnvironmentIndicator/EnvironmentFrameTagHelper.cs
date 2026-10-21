using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a border around the window, with a tab on the top edge that names the environment
///     the application runs in.
/// </summary>
/// <remarks>
///     The indicator is fixed to the window, so it can be placed anywhere in the page.
/// </remarks>
[HtmlTargetElement("sa-environment-frame")]
public sealed class EnvironmentFrameTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the colour of the indicator.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the colour. The default is
    ///     <see cref="EnvironmentFrameColor.Neutral" />.
    /// </value>
    /// <remarks>
    ///     <see cref="CustomColor" /> and <see cref="CustomDarkColor" /> override it.
    /// </remarks>
    [HtmlAttributeName("color")]
    public EnvironmentFrameColor? Color { get; set; }

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
    ///     Gets or sets the name of the environment.
    /// </summary>
    [HtmlAttributeName("label")]
    public required string Label { get; set; }

    /// <summary>
    ///     Gets or sets the size of the tab that names the environment.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="EnvironmentFrameTabSize.Medium" />.
    /// </value>
    [HtmlAttributeName("tab-size")]
    public EnvironmentFrameTabSize? TabSize { get; set; }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">
    ///     <see cref="Label" /> is empty or white space.
    /// </exception>
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(Label))
        {
            throw new ArgumentException(
                "The 'label' attribute is required on <sa-environment-frame>."
            );
        }

        var effectiveColor = Color ?? EnvironmentFrameColor.Neutral;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "environment-frame");
        output.Attributes.SetAttribute("data-color", effectiveColor.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-environment-frame", output.GetUserSuppliedClass())
        );
        EnvironmentIndicatorRendering.RenderCustomColors(output, CustomColor, CustomDarkColor);

        output.Content.SetHtmlContent(
            EnvironmentIndicatorRendering.CreateTab(
                Label,
                (TabSize ?? EnvironmentFrameTabSize.Medium).GetDataAttributeText()
            )
        );

        return Task.CompletedTask;
    }
}
