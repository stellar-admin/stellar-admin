using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a callout that shows a short, important message.
/// </summary>
[HtmlTargetElement("sa-alert")]
public sealed class AlertTagHelper : StellarAdminTagHelperBase
{
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AlertTagHelper" /> class.
    /// </summary>
    /// <param name="iconOptions">
    ///     The icon options that supply the icon set by <see cref="Icon" />.
    /// </param>
    public AlertTagHelper(IOptions<IconOptions> iconOptions)
    {
        _iconOptions = iconOptions.Value;
    }

    private const string DescriptionAttributeName = "description";
    private const string IconAttributeName = "icon";
    private const string TitleAttributeName = "title";

    private static readonly Dictionary<AlertVariant, string> AlertVariantClasses = new()
    {
        [AlertVariant.Default] = "sa-alert-variant-default",
        [AlertVariant.Destructive] = "sa-alert-variant-destructive",
    };

    /// <summary>
    ///     Gets or sets the description of the alert.
    /// </summary>
    /// <remarks>
    ///     When <c>title</c>, <c>description</c> or <c>icon</c> is set, the alert renders them
    ///     and can't have child content.
    /// </remarks>
    [HtmlAttributeName(DescriptionAttributeName)]
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the name of the icon shown in the alert.
    /// </summary>
    /// <remarks>
    ///     When <c>title</c>, <c>description</c> or <c>icon</c> is set, the alert renders them
    ///     and can't have child content.
    /// </remarks>
    [HtmlAttributeName(IconAttributeName)]
    public string? Icon { get; set; }

    /// <summary>
    ///     Gets or sets the title of the alert.
    /// </summary>
    /// <remarks>
    ///     When <c>title</c>, <c>description</c> or <c>icon</c> is set, the alert renders them
    ///     and can't have child content.
    /// </remarks>
    [HtmlAttributeName(TitleAttributeName)]
    public string? Title { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the alert.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style. The default is
    ///     <see cref="AlertVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public AlertVariant? Variant { get; set; }

    /// <inheritdoc />
    /// <exception cref="Exception">
    ///     The alert has child content and <see cref="Title" />, <see cref="Description" /> or
    ///     <see cref="Icon" /> is set.
    /// </exception>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? AlertVariant.Default;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "alert");
        output.Attributes.SetAttribute("role", "alert");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-alert",
                AlertVariantClasses[effectiveVariant],
                output.GetUserSuppliedClass()
            )
        );

        var childContent = await output.GetChildContentAsync();

        if (
            !string.IsNullOrEmpty(Title)
            || !string.IsNullOrEmpty(Description)
            || !string.IsNullOrEmpty(Icon)
        )
        {
            if (!childContent.IsEmptyOrWhiteSpace)
            {
                throw new Exception(
                    $"Cannot add child content to <sa-alert> when specifying '{TitleAttributeName}', '{DescriptionAttributeName}', or '{IconAttributeName}' attribute."
                );
            }

            await RenderImplicitChildContent(context, output);
        }
        else
        {
            output.Content.SetHtmlContent(childContent);
        }
    }

    private async Task RenderImplicitChildContent(TagHelperContext context, TagHelperOutput output)
    {
        if (!string.IsNullOrEmpty(Icon))
        {
            var iconTagHelperOutput = new TagHelperOutput(
                string.Empty,
                [],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );
            var iconTagHelper = new IconTagHelper(_iconOptions) { Name = Icon };
            await iconTagHelper.ProcessAsync(context, iconTagHelperOutput);

            output.Content.AppendHtml(iconTagHelperOutput);
        }

        if (!string.IsNullOrEmpty(Title))
        {
            var titleContent = new DefaultTagHelperContent();
            titleContent.Append(Title);

            var titleTagHelperOutput = new TagHelperOutput(
                string.Empty,
                [],
                (_, _) => Task.FromResult<TagHelperContent>(titleContent)
            );
            var titleTagHelper = new AlertTitleTagHelper();
            await titleTagHelper.ProcessAsync(context, titleTagHelperOutput);

            output.Content.AppendHtml(titleTagHelperOutput);
        }

        if (!string.IsNullOrEmpty(Description))
        {
            var descriptionContent = new DefaultTagHelperContent();
            descriptionContent.Append(Description);

            var descriptionTagHelperOutput = new TagHelperOutput(
                string.Empty,
                [],
                (_, _) => Task.FromResult<TagHelperContent>(descriptionContent)
            );

            var descriptionTagHelper = new AlertDescriptionTagHelper();
            await descriptionTagHelper.ProcessAsync(context, descriptionTagHelperOutput);

            output.Content.AppendHtml(descriptionTagHelperOutput);
        }
    }
}
