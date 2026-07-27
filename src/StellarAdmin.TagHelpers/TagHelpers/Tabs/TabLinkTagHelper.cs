using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A single tab within a <c>&lt;sa-tab-list&gt;</c>, rendered as a link to its target view.
/// </summary>
[HtmlTargetElement("sa-tab-link")]
public class TabLinkTagHelper : StellarAdminAnchorTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Whether the tab is disabled and cannot be selected.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    [HtmlAttributeName("disabled")]
    public bool? IsDisabled { get; set; }

    /// <summary>
    ///     Whether this tab is the currently selected one. When not set, it is inferred from whether
    ///     the tab's route matches the current request.
    /// </summary>
    [HtmlAttributeName("is-active")]
    public bool? IsActive { get; set; }

    public TabLinkTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveIsDisabled = IsDisabled ?? false;

        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        var anchorTagHelper = new AnchorTagHelper(_htmlGenerator)
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
        };
        await anchorTagHelper.ProcessAsync(context, output);

        var isActive = IsActive ?? IsActiveRoute();
        output.Attributes.SetAttribute("role", "tab");
        output.Attributes.SetAttribute("data-slot", "tabs-trigger");
        output.Attributes.SetAttribute("aria-selected", isActive ? "true" : "false");
        if (isActive)
        {
            output.Attributes.SetAttribute("data-active", "");
        }

        output.Attributes.SetAttribute("aria-disabled", effectiveIsDisabled ? "true" : "false");
        if (effectiveIsDisabled)
        {
            if (output.Attributes.TryGetAttribute("href", out var hrefAttribute))
            {
                output.Attributes.Remove(hrefAttribute);
            }
            output.Attributes.SetAttribute("data-disabled", "");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-tabs-trigger", output.GetUserSuppliedClass())
        );

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}
