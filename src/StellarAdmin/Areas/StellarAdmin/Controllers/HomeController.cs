using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;
using StellarAdmin.Home;
using StellarAdmin.Infrastructure.Authorization;
using StellarAdmin.Infrastructure.Icons;
using StellarAdmin.Resources;
using StellarAdmin.Sidebar;
using StellarAdmin.Widgets;

namespace StellarAdmin.Areas.StellarAdmin.Controllers;

[Area("StellarAdmin")]
internal sealed class HomeController(
    SetupStatusResolver setup,
    BrandingResolver branding,
    IEnumerable<ResourceRegistration> resources,
    StellarAdminOptions options,
    WidgetRenderer widgets,
    IServiceProvider services
) : Controller
{
    // Names the home page in its widgets' protected parameters
    private const string WidgetPage = "home";

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var rendered = await widgets.RenderAsync(
            options.HomePage.GetCheckedWidgets(),
            new(HttpContext),
            WidgetPage,
            CreateWidgetUrl,
            cancellationToken
        );

        // Null outside Development and when the checklist is turned off, without detecting anything
        if (await setup.ResolveAsync(HttpContext) is { } status)
        {
            var checklist = SetupSteps.Create(
                status,
                branding.Title,
                Url.Action("Index", "Home") ?? "/",
                status.ThemeStylesheet is { } path ? StylesheetName(path) : null
            );

            return View(new HomePageViewModel(branding.Title, checklist, [], rendered));
        }

        // Widgets take the place of the links to the resources, which the sidebar has too
        return View(
            new HomePageViewModel(
                branding.Title,
                null,
                rendered.Count > 0 ? [] : await GetResourceLinksAsync(),
                rendered
            )
        );
    }

    // Every registered icon for the icon editor's picker. The URL carries the icons' version, so the
    // browser keeps the response for good; another version still answers, but isn't cached
    [HttpGet]
    public IActionResult Icons([FromQuery] string? v, [FromServices] IconCatalog catalog)
    {
        var etag = new EntityTagHeaderValue($"\"{catalog.Version}\"");
        Response.Headers.Vary = HeaderNames.AcceptEncoding;
        Response.Headers.CacheControl =
            v == catalog.Version ? "private, max-age=31536000, immutable" : "private, no-cache";
        Response.GetTypedHeaders().ETag = etag;
        if (Request.GetTypedHeaders().IfNoneMatch.Any(tag => tag.Compare(etag, false)))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        var encodings = Request.GetTypedHeaders().AcceptEncoding;
        byte[] body;
        if (Accepts(encodings, "br"))
        {
            Response.Headers.ContentEncoding = "br";
            body = catalog.Brotli;
        }
        else if (Accepts(encodings, "gzip"))
        {
            Response.Headers.ContentEncoding = "gzip";
            body = catalog.Gzip;
        }
        else
        {
            body = catalog.Json;
        }

        return File(body, "application/json");
    }

    // A widget's content, loaded after the page has rendered or again when it refreshes
    [HttpGet]
    public Task<IActionResult> Widget(
        [FromRoute] string? id,
        CancellationToken cancellationToken
    ) =>
        widgets.LoadAsync(
            this,
            options.HomePage.GetCheckedWidgets(),
            WidgetPage,
            id,
            CreateWidgetUrl,
            cancellationToken
        );

    private static bool Accepts(IList<StringWithQualityHeaderValue> encodings, string encoding) =>
        encodings.Any(accepted =>
            string.Equals(accepted.Value.Value, encoding, StringComparison.OrdinalIgnoreCase)
            && accepted.Quality is not 0
        );

    private static string StylesheetName(string path)
    {
        var withoutQuery = path.Split('?', '#')[0];

        return withoutQuery[(withoutQuery.LastIndexOf('/') + 1)..];
    }

    private string CreateWidgetUrl(string name, string? parameters) =>
        Url.Action(
            nameof(Widget),
            "Home",
            new
            {
                area = "StellarAdmin",
                id = name,
                p = parameters,
            }
        )!;

    // The resources the sidebar shows the current user, in the sidebar's order
    private async Task<IReadOnlyList<HomeResourceLinkViewModel>> GetResourceLinksAsync()
    {
        var items = new List<SidebarItem>();
        foreach (var resource in resources)
        {
            var item = resource.ResolveSidebarItem(services);
            if (
                item.Visible
                && await AuthorizationMetadata.AuthorizeAsync(
                    resource.ResolveAuthorizationMetadata(services),
                    HttpContext
                )
            )
            {
                items.Add(
                    new SidebarGroupItem(
                        item.Group,
                        [ResourceSidebarItemsProvider.CreateLink(resource, item)]
                    )
                );
            }
        }

        return SidebarItemsMerger
            .Merge(items, options.Sidebar)
            .SelectMany(item => ((SidebarGroupItem)item).Items)
            .Select(link => new HomeResourceLinkViewModel(
                link.Label,
                link.Icon,
                ((SidebarActionLinkItem)link).Controller
            ))
            .ToArray();
    }
}
