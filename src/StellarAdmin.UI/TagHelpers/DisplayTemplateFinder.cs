using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace StellarAdmin.UI;

// Finds a display template by name in a folder, or at a path that starts with ~/ or /. Templates resolve by name only,
// never by type, so the framework's built-in display templates (a bool as a disabled checkbox) never apply. A missing
// template throws rather than falling back, so a mistyped name is caught.
internal static class DisplayTemplateFinder
{
    public const string Folder = "DisplayTemplates";

    public const string CompactFolder = "DisplayTemplates/Compact";

    public static IView Find(
        ICompositeViewEngine viewEngine,
        ViewContext viewContext,
        string folder,
        string templateName
    )
    {
        var viewName =
            templateName.StartsWith('/') || templateName.StartsWith("~/", StringComparison.Ordinal)
                ? templateName
                : $"{folder}/{templateName}";

        var getViewResult = viewEngine.GetView(
            viewContext.ExecutingFilePath,
            viewName,
            isMainPage: false
        );
        var result = getViewResult.Success
            ? getViewResult
            : viewEngine.FindView(viewContext, viewName, isMainPage: false);
        if (!result.Success)
        {
            var searchedLocations = getViewResult
                .SearchedLocations.Concat(result.SearchedLocations)
                .Distinct();
            throw new InvalidOperationException(
                $"The display template '{viewName}' was not found. The following locations "
                    + $"were searched: {string.Join(", ", searchedLocations)}."
            );
        }

        return result.View;
    }
}
