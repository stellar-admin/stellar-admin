using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StellarAdmin.Sidebar;

// Finds sidebar links and user menu items whose page or action doesn't exist. Development shows them as errors;
// elsewhere they are hidden, and each is logged once. The URL alone can't tell: with a conventional route, a missing
// page still gets one (/?page=%2FMissing).
internal sealed class LinkChecker(
    IActionDescriptorCollectionProvider actions,
    IHostEnvironment environment,
    ILogger<LinkChecker> logger
)
{
    private readonly ConcurrentDictionary<string, byte> _logged = new();

    public bool ShowsProblems => environment.IsDevelopment();

    public string? FindProblem(IUrlHelper url, SidebarLinkItemBase item) =>
        item switch
        {
            SidebarActionLinkItem actionItem => ActionProblem(
                url,
                actionItem.Label,
                actionItem.Action,
                actionItem.Controller,
                actionItem.Area
            ),
            SidebarPageLinkItem pageItem => PageProblem(
                url,
                pageItem.Label,
                pageItem.Page,
                pageItem.Area
            ),
            _ => null,
        };

    public string? FindProblem(IUrlHelper url, UserMenuItem item) =>
        item switch
        {
            ActionUserMenuItem actionItem => ActionProblem(
                url,
                actionItem.Text,
                actionItem.ActionName,
                actionItem.ControllerName,
                actionItem.Area
            ),
            PageUserMenuItem pageItem => PageProblem(
                url,
                pageItem.Text,
                pageItem.PageName,
                pageItem.Area
            ),
            _ => null,
        };

    private string? ActionProblem(
        IUrlHelper url,
        string text,
        string action,
        string controller,
        string? area
    ) =>
        Exists(("controller", controller), ("action", action), ("area", area))
        && url.Action(action, controller, new { area = area ?? "" }) is not null
            ? null
            : Report(text, $"No action {controller}.{action}{InArea(area)}");

    private string? PageProblem(IUrlHelper url, string text, string page, string? area) =>
        Exists(("page", page), ("area", area))
        && url.Page(page, new { area = area ?? "" }) is not null
            ? null
            : Report(text, $"No page {page}{InArea(area)}");

    private bool Exists(params (string Key, string? Value)[] routeValues) =>
        actions.ActionDescriptors.Items.Any(descriptor =>
            routeValues.All(routeValue =>
                string.Equals(
                    (
                        descriptor.RouteValues.TryGetValue(routeValue.Key, out var value)
                            ? value
                            : null
                    ) ?? "",
                    routeValue.Value ?? "",
                    StringComparison.OrdinalIgnoreCase
                )
            )
        );

    private string Report(string text, string problem)
    {
        if (_logged.TryAdd($"{text}\n{problem}", 0))
        {
            logger.LogWarning("The admin panel link {Text} has no URL: {Problem}", text, problem);
        }

        return problem;
    }

    private static string InArea(string? area) =>
        string.IsNullOrEmpty(area) ? "" : $" in area {area}";
}
