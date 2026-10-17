using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using FrameworkAnchorTagHelper = Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper;

namespace StellarAdmin.UI;

/// <summary>
///     Provides the base class for StellarAdmin tag helpers that render a link and accept the
///     ASP.NET Core routing attributes, such as <c>asp-action</c> and <c>asp-page</c>.
/// </summary>
public abstract class StellarAdminAnchorTagHelperBase : StellarAdminTagHelperBase
{
    private const string ActionAttributeName = "asp-action";
    private const string ControllerAttributeName = "asp-controller";
    private const string AreaAttributeName = "asp-area";
    private const string PageAttributeName = "asp-page";
    private const string PageHandlerAttributeName = "asp-page-handler";
    private const string FragmentAttributeName = "asp-fragment";
    private const string HostAttributeName = "asp-host";
    private const string ProtocolAttributeName = "asp-protocol";
    private const string RouteAttributeName = "asp-route";
    private const string RouteValuesDictionaryName = "asp-all-route-data";
    private const string RouteValuesPrefix = "asp-route-";

    /// <summary>
    ///     Gets or sets the name of the action method that the link points to.
    /// </summary>
    /// <remarks>
    ///     Can't be combined with <c>asp-route</c> or <c>asp-page</c>.
    /// </remarks>
    [HtmlAttributeName(ActionAttributeName)]
    public string? Action { get; set; }

    /// <summary>
    ///     Gets or sets the name of the area that the link points to.
    /// </summary>
    /// <remarks>
    ///     Can't be combined with <c>asp-route</c>. An empty value points outside any area.
    /// </remarks>
    [HtmlAttributeName(AreaAttributeName)]
    public string? Area { get; set; }

    /// <summary>
    ///     Gets or sets the name of the controller that the link points to.
    /// </summary>
    /// <remarks>
    ///     Can't be combined with <c>asp-route</c> or <c>asp-page</c>.
    /// </remarks>
    [HtmlAttributeName(ControllerAttributeName)]
    public string? Controller { get; set; }

    /// <summary>
    ///     Gets or sets the fragment added to the end of the URL, after <c>#</c>.
    /// </summary>
    /// <remarks>
    ///     Applies only when another routing attribute is set.
    /// </remarks>
    [HtmlAttributeName(FragmentAttributeName)]
    public string? Fragment { get; set; }

    /// <summary>
    ///     Gets or sets the host name of the URL.
    /// </summary>
    /// <remarks>
    ///     Applies only when another routing attribute is set.
    /// </remarks>
    [HtmlAttributeName(HostAttributeName)]
    public string? Host { get; set; }

    /// <summary>
    ///     Gets or sets the name of the Razor page that the link points to.
    /// </summary>
    /// <remarks>
    ///     Can't be combined with <c>asp-route</c>, <c>asp-action</c> or <c>asp-controller</c>.
    /// </remarks>
    [HtmlAttributeName(PageAttributeName)]
    public string? Page { get; set; }

    /// <summary>
    ///     Gets or sets the name of the page handler that the link points to.
    /// </summary>
    /// <remarks>
    ///     Can't be combined with <c>asp-route</c>, <c>asp-action</c> or <c>asp-controller</c>.
    /// </remarks>
    [HtmlAttributeName(PageHandlerAttributeName)]
    public string? PageHandler { get; set; }

    /// <summary>
    ///     Gets or sets the protocol of the URL, such as <c>https</c>.
    /// </summary>
    /// <remarks>
    ///     Applies only when another routing attribute is set.
    /// </remarks>
    [HtmlAttributeName(ProtocolAttributeName)]
    public string? Protocol { get; set; }

    /// <summary>
    ///     Gets or sets the name of the route that the link points to.
    /// </summary>
    /// <remarks>
    ///     Can't be combined with <c>asp-action</c>, <c>asp-controller</c>, <c>asp-area</c> or
    ///     <c>asp-page</c>.
    /// </remarks>
    [HtmlAttributeName(RouteAttributeName)]
    public string? Route { get; set; }

    /// <summary>
    ///     Gets or sets the route values of the link.
    /// </summary>
    /// <value>
    ///     The route values, keyed by name. Each <c>asp-route-*</c> attribute adds one value.
    /// </value>
    [HtmlAttributeName(RouteValuesDictionaryName, DictionaryAttributePrefix = RouteValuesPrefix)]
    public IDictionary<string, string?> RouteValues
    {
        get
        {
            field ??= new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            return field;
        }
        set;
    }

    /// <summary>
    ///     Gets or sets the context of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <summary>
    ///     Gets a value that indicates whether a routing attribute sets the link target.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if <c>asp-page</c>, <c>asp-action</c>, <c>asp-controller</c>,
    ///     <c>asp-route</c>, <c>asp-area</c>, <c>asp-page-handler</c> or an <c>asp-route-*</c>
    ///     attribute is set; otherwise, <see langword="false" />.
    /// </value>
    protected bool HasRouteTarget =>
        Page != null
        || Action != null
        || Controller != null
        || Route != null
        || Area != null
        || PageHandler != null
        || RouteValues.Count > 0;

    /// <summary>
    ///     Sets the <c>href</c> of the output from the routing attributes.
    /// </summary>
    /// <remarks>
    ///     Does nothing when <see cref="HasRouteTarget" /> is <see langword="false" />, so the
    ///     output keeps its own <c>href</c>.
    /// </remarks>
    /// <param name="htmlGenerator">The generator that builds the URL.</param>
    /// <param name="context">The context of the executing tag helper.</param>
    /// <param name="output">The output to set the <c>href</c> on.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected async Task ApplyRouteAttributesAsync(
        IHtmlGenerator htmlGenerator,
        TagHelperContext context,
        TagHelperOutput output
    )
    {
        if (!HasRouteTarget)
        {
            return;
        }

        var anchorTagHelper = new FrameworkAnchorTagHelper(htmlGenerator)
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
    }

    /// <summary>
    ///     Returns a value that indicates whether the link points to the current page or action.
    /// </summary>
    /// <remarks>
    ///     The area, the page and handler or the controller and action, and every
    ///     <c>asp-route-*</c> value must match the current request.
    /// </remarks>
    /// <returns>
    ///     <see langword="true" /> if the link points to the current request; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    protected bool IsActiveRoute()
    {
        var isActionLink = Controller != null || Action != null;
        var isPageLink = Page != null || PageHandler != null;

        if (isPageLink)
        {
            return MatchesArea()
                && ViewContext.RouteData.Values["page"]?.ToString() == Page
                && ViewContext.RouteData.Values["handler"]?.ToString() == PageHandler
                && MatchesRouteValues();
        }

        if (isActionLink)
        {
            return MatchesArea()
                && ViewContext.RouteData.Values["controller"]?.ToString() == Controller
                && ViewContext.RouteData.Values["action"]?.ToString() == Action
                && MatchesRouteValues();
        }

        return false;
    }

    /// <summary>
    ///     Compares the link's <c>asp-area</c> with the current request. An empty <c>asp-area</c>
    ///     links outside any area, so it matches a request that has no area.
    /// </summary>
    private bool MatchesArea()
    {
        var currentArea = ViewContext.RouteData.Values["area"]?.ToString();

        return Area == "" ? string.IsNullOrEmpty(currentArea) : currentArea == Area;
    }

    /// <summary>
    ///     Compares the link's <c>asp-route-*</c> values with the current request, so links to the
    ///     same page or action that differ only by a route value (a category, an id) don't all
    ///     count as active. A value the framework put in the query string is matched there.
    /// </summary>
    private bool MatchesRouteValues()
    {
        foreach (var (key, value) in RouteValues)
        {
            var currentValue =
                ViewContext.RouteData.Values.TryGetValue(key, out var routeValue)
                    ? routeValue?.ToString()
                : ViewContext.HttpContext.Request.Query.TryGetValue(key, out var queryValue)
                    ? queryValue.ToString()
                : null;

            if (!string.Equals(currentValue, value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}
