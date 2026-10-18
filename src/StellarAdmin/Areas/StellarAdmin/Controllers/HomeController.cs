using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;
using StellarAdmin.Home;
using StellarAdmin.Infrastructure.Authorization;
using StellarAdmin.Infrastructure.Icons;
using StellarAdmin.Resources;
using StellarAdmin.Sidebar;

namespace StellarAdmin.Areas.StellarAdmin.Controllers;

[Area("StellarAdmin")]
internal sealed class HomeController(
    SetupStatusResolver setup,
    BrandingResolver branding,
    IEnumerable<ResourceRegistration> resources,
    StellarAdminOptions options,
    IServiceProvider services
) : Controller
{
    public async Task<IActionResult> Index()
    {
        // Null outside Development and when the checklist is turned off, without detecting anything
        if (await setup.ResolveAsync(HttpContext) is { } status)
        {
            var checklist = SetupSteps.Create(
                status,
                branding.Title,
                Url.Action("Index", "Home") ?? "/",
                status.ThemeStylesheet is { } path ? StylesheetName(path) : null
            );

            return View(new HomePageViewModel(branding.Title, checklist, []));
        }

        return View(new HomePageViewModel(branding.Title, null, await GetResourceLinksAsync()));
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
