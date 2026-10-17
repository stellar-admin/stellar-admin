using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a pagination link to the previous page.
/// </summary>
/// <remarks>
///     With no content, the link shows an arrow icon and the label "Previous".
/// </remarks>
[HtmlTargetElement("sa-pagination-previous")]
public sealed class PaginationPreviousTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Gets or sets the size of the link.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="ButtonSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public ButtonSize? Size { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PaginationPreviousTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that builds the link URL from the routing attributes.</param>
    /// <param name="iconOptions">The icon options that supply the default arrow icon.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public PaginationPreviousTagHelper(
        IHtmlGenerator htmlGenerator,
        IOptions<IconOptions> iconOptions
    )
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconOptions = iconOptions.Value;
    }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-pagination-previous", output.GetUserSuppliedClass())
        );
        var linkTagHelper = new PaginationLinkTagHelper(_htmlGenerator)
        {
            ViewContext = ViewContext,
            Action = Action,
            Area = Area,
            Controller = Controller,
            Fragment = Fragment,
            Host = Host,
            Page = Page,
            PageHandler = PageHandler,
            Protocol = Protocol,
            Route = Route,
            RouteValues = RouteValues,
            Active = false,
            Size = Size,
        };
        await linkTagHelper.ProcessAsync(context, output);

        var content = await output.GetChildContentAsync();

        if (!content.IsEmptyOrWhiteSpace)
        {
            output.Content.AppendHtml(content);
        }
        else
        {
            // Render the icon
            var iconOutput = new TagHelperOutput(
                "svg",
                [],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );
            var iconTagHelper = new IconTagHelper(_iconOptions)
            {
                Name = _iconOptions.GetSemanticIconName(SemanticIconRole.PaginationPrevious),
            };
            await iconTagHelper.ProcessAsync(context, iconOutput);
            output.Content.AppendHtml(iconOutput);

            // Render the text
            var textBlockTagBuilder = new TagBuilder("span");
            textBlockTagBuilder.AddCssClass("sa-pagination-link-label");
            textBlockTagBuilder.InnerHtml.AppendHtml("Previous");
            output.Content.AppendHtml(textBlockTagBuilder);
        }
    }
}
