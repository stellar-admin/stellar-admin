using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Icons;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A callout that displays a short, important message to the user, optionally with an
///     icon, title, and description.
/// </summary>
[HtmlTargetElement("sa-alert")]
public class AlertTagHelper : StellarAdminTagHelperBase
{
    private readonly IIconManager _iconManager;

    public AlertTagHelper(ICssClassMerger classMerger, IIconManager iconManager)
        : base(classMerger)
    {
        _iconManager = iconManager ?? throw new ArgumentNullException(nameof(iconManager));
    }

    private const string DescriptionAttributeName = "description";
    private const string IconAttributeName = "icon";
    private const string TitleAttributeName = "title";

    private static readonly Dictionary<AlertVariant, ThemeToken> AlertVariantClasses = new()
    {
        [AlertVariant.Default] = new ThemeToken("sa-alert-variant-default"),
        [AlertVariant.Destructive] = new ThemeToken("sa-alert-variant-destructive"),
    };

    /// <summary>
    ///     The descriptive body text of the alert. When set, the alert renders its description
    ///     automatically and child content is not allowed.
    /// </summary>
    [HtmlAttributeName(DescriptionAttributeName)]
    public string? Description { get; set; }

    /// <summary>
    ///     The name of the icon to display in the alert. When set, the alert renders the icon
    ///     automatically and child content is not allowed.
    /// </summary>
    [HtmlAttributeName(IconAttributeName)]
    public string? Icon { get; set; }

    /// <summary>
    ///     The title text of the alert. When set, the alert renders its title automatically and
    ///     child content is not allowed.
    /// </summary>
    [HtmlAttributeName(TitleAttributeName)]
    public string? Title { get; set; }

    /// <summary>
    ///     The visual style of the alert.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="AlertVariant.Default" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public AlertVariant? Variant { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? AlertVariant.Default;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "alert");
        output.Attributes.SetAttribute("role", "alert");
        output.Attributes.SetAttribute(
            "class",
            BuildClassString(
                new ThemeToken("sa-alert"),
                "w-full relative group/alert",
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
            var iconTagHelper = new IconTagHelper(ClassMerger, _iconManager) { Name = Icon };
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
            var titleTagHelper = new AlertTitleTagHelper(ClassMerger);
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

            var descriptionTagHelper = new AlertDescriptionTagHelper(ClassMerger);
            await descriptionTagHelper.ProcessAsync(context, descriptionTagHelperOutput);

            output.Content.AppendHtml(descriptionTagHelperOutput);
        }
    }
}
