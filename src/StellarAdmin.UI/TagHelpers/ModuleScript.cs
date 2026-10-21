using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StellarAdmin.UI;

// The module script a large, rarely used component renders inside its element, so its bundle loads
// only on pages that use it. The browser runs a module URL once, however many components render it,
// and htmx recreates the script in swapped-in partials. The URL is the fingerprinted one from the
// page's static asset collection when the endpoint has one, and the plain one otherwise.
internal static class ModuleScript
{
    private const string ContentPath = "_content/StellarAdmin.UI/";

    public static TagBuilder Create(ViewContext viewContext, string path)
    {
        var request = viewContext.HttpContext.Request;
        var label = ContentPath + path;
        var assets = viewContext
            .HttpContext.GetEndpoint()
            ?.Metadata.GetMetadata<ResourceAssetCollection>();

        var script = new TagBuilder("script");
        script.Attributes.Add("type", "module");
        script.Attributes.Add("src", $"{request.PathBase}/{assets?[label] ?? label}");

        return script;
    }
}
