using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using FrameworkAnchorTagHelper = Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Base class for StellarAdmin tag helpers that render an anchor (<c>&lt;a&gt;</c>) with ASP.NET
///     Core routing support (<c>asp-action</c>, <c>asp-controller</c>, <c>asp-page</c>, etc.).
/// </summary>
public class StellarAdminAnchorTagHelperBase : StellarAdminTagHelperBase
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
    ///     The name of the action method.
    /// </summary>
    /// <remarks>
    ///     Must be <c>null</c> if <see cref="Route" /> or <see cref="Page" /> is non-<c>null</c>.
    /// </remarks>
    [HtmlAttributeName(ActionAttributeName)]
    public string? Action { get; set; }

    /// <summary>
    ///     The name of the area.
    /// </summary>
    /// <remarks>
    ///     Must be <c>null</c> if <see cref="Route" /> is non-<c>null</c>.
    /// </remarks>
    [HtmlAttributeName(AreaAttributeName)]
    public string? Area { get; set; }

    /// <summary>
    ///     The name of the controller.
    /// </summary>
    /// <remarks>
    ///     Must be <c>null</c> if <see cref="Route" /> or <see cref="Page" /> is non-<c>null</c>.
    /// </remarks>
    [HtmlAttributeName(ControllerAttributeName)]
    public string? Controller { get; set; }

    /// <summary>
    ///     The URL fragment name.
    /// </summary>
    [HtmlAttributeName(FragmentAttributeName)]
    public string? Fragment { get; set; }

    /// <summary>
    ///     The host name.
    /// </summary>
    [HtmlAttributeName(HostAttributeName)]
    public string? Host { get; set; }

    /// <summary>
    ///     The name of the page.
    /// </summary>
    /// <remarks>
    ///     Must be <c>null</c> if <see cref="Route" /> or <see cref="Action" />, <see cref="Controller" />
    ///     is non-<c>null</c>.
    /// </remarks>
    [HtmlAttributeName(PageAttributeName)]
    public string? Page { get; set; }

    /// <summary>
    ///     The name of the page handler.
    /// </summary>
    /// <remarks>
    ///     Must be <c>null</c> if <see cref="Route" /> or <see cref="Action" />, or <see cref="Controller" />
    ///     is non-<c>null</c>.
    /// </remarks>
    [HtmlAttributeName(PageHandlerAttributeName)]
    public string? PageHandler { get; set; }

    /// <summary>
    ///     The protocol for the URL, such as &quot;http&quot; or &quot;https&quot;.
    /// </summary>
    [HtmlAttributeName(ProtocolAttributeName)]
    public string? Protocol { get; set; }

    /// <summary>
    ///     Name of the route.
    /// </summary>
    /// <remarks>
    ///     Must be <c>null</c> if one of <see cref="Action" />, <see cref="Controller" />, <see cref="Area" />
    ///     or <see cref="Page" /> is non-<c>null</c>.
    /// </remarks>
    [HtmlAttributeName(RouteAttributeName)]
    public string? Route { get; set; }

    /// <summary>
    ///     Additional parameters for the route.
    /// </summary>
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
    ///     Gets or sets the <see cref="Microsoft.AspNetCore.Mvc.Rendering.ViewContext" /> for the current request.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <summary>
    ///     Whether a routing target (<c>asp-page</c>, <c>asp-action</c>, <c>asp-controller</c>,
    ///     <c>asp-route</c>, <c>asp-area</c>, <c>asp-page-handler</c> or <c>asp-route-*</c>) is set.
    ///     <c>asp-fragment</c>, <c>asp-host</c> and <c>asp-protocol</c> are modifiers and only apply
    ///     alongside a target.
    /// </summary>
    protected bool HasRouteTarget =>
        Page != null
        || Action != null
        || Controller != null
        || Route != null
        || Area != null
        || PageHandler != null
        || RouteValues.Count > 0;

    /// <summary>
    ///     Lets the framework anchor tag helper resolve the <c>href</c> from the routing attributes.
    ///     Does nothing when no routing target is set, so an anchor without one keeps (or lacks)
    ///     the author's <c>href</c> instead of linking to the current action.
    /// </summary>
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

    protected bool IsActiveRoute()
    {
        var isActionLink = Controller != null || Action != null;
        var isPageLink = Page != null || PageHandler != null;

        if (isPageLink)
        {
            return ViewContext.RouteData.Values["area"]?.ToString() == Area
                && ViewContext.RouteData.Values["page"]?.ToString() == Page
                && ViewContext.RouteData.Values["handler"]?.ToString() == PageHandler
                && MatchesRouteValues();
        }

        if (isActionLink)
        {
            return ViewContext.RouteData.Values["area"]?.ToString() == Area
                && ViewContext.RouteData.Values["controller"]?.ToString() == Controller
                && ViewContext.RouteData.Values["action"]?.ToString() == Action
                && MatchesRouteValues();
        }

        return false;
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
