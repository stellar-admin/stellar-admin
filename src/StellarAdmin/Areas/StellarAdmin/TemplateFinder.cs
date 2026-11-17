using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Areas.StellarAdmin;

// Finds a display or editor template's view: relative to the executing view first, then in the view locations. The
// result lists every location searched when the view isn't found.
internal static class TemplateFinder
{
    public const string CompactDisplayFolder = "DisplayTemplates/Compact";

    public static ViewEngineResult Find(ViewContext viewContext, string viewName)
    {
        var viewEngine =
            viewContext.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();
        var getViewResult = viewEngine.GetView(viewContext.ExecutingFilePath, viewName, false);
        if (getViewResult.Success)
        {
            return getViewResult;
        }

        var findViewResult = viewEngine.FindView(viewContext, viewName, false);
        return findViewResult.Success
            ? findViewResult
            : ViewEngineResult.NotFound(
                viewName,
                getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations).Distinct()
            );
    }
}
