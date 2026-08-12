using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Base class for tag helpers that render through a view. The view resolves like a
///     partial view, so an application can override the markup by supplying its own view
///     with the same name.
/// </summary>
public abstract class StellarAdminTemplatedTagHelperBase : StellarAdminTagHelperBase
{
    private readonly ICompositeViewEngine _viewEngine;

    /// <summary>
    ///     The name or application-relative path of a view that replaces the tag helper's
    ///     default view.
    /// </summary>
    [HtmlAttributeName("view")]
    public string? View { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <summary>The name of the view the tag helper renders.</summary>
    protected abstract string ViewName { get; }

    protected StellarAdminTemplatedTagHelperBase(ICompositeViewEngine viewEngine)
    {
        _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var viewName = View ?? ViewName;
        var view = ResolveView(viewName);

        // Model is assigned explicitly so a null model reaches the view as null instead
        // of the view data falling back to the calling view's model.
        var model = GetViewModel();
        var viewData = new ViewDataDictionary<object?>(ViewContext.ViewData, model)
        {
            Model = model,
        };

        using var writer = new StringWriter();
        await view.RenderAsync(new ViewContext(ViewContext, view, viewData, writer));

        output.TagName = null;
        output.Content.SetHtmlContent(writer.ToString());
    }

    /// <summary>Returns the model of the view. Defaults to the model of the calling view.</summary>
    protected virtual object? GetViewModel()
    {
        return ViewContext.ViewData.Model;
    }

    private IView ResolveView(string viewName)
    {
        var getViewResult = _viewEngine.GetView(
            ViewContext.ExecutingFilePath,
            viewName,
            isMainPage: false
        );
        var result = getViewResult.Success
            ? getViewResult
            : _viewEngine.FindView(ViewContext, viewName, isMainPage: false);
        if (!result.Success)
        {
            var searchedLocations = getViewResult
                .SearchedLocations.Concat(result.SearchedLocations)
                .Distinct();
            throw new InvalidOperationException(
                $"The view '{viewName}' was not found. The following locations were "
                    + $"searched: {string.Join(", ", searchedLocations)}."
            );
        }

        return result.View;
    }
}
