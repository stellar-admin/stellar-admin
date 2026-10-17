using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Provides the base class for tag helpers that render a view.
/// </summary>
/// <remarks>
///     The view is found like a partial view, so an application can replace the markup with its
///     own view of the same name. Child content other than <c>&lt;sa-slot-content&gt;</c> isn't
///     rendered.
/// </remarks>
public abstract class StellarAdminTemplatedTagHelperBase : StellarAdminTagHelperBase
{
    internal const string SlotHostViewDataKey = "__StellarAdminSlotHost";

    private readonly ICompositeViewEngine _viewEngine;

    /// <summary>
    ///     Gets or sets the name or path of a view that replaces the default view.
    /// </summary>
    /// <value>
    ///     The name of the view, or a path relative to the application root. The default is the
    ///     tag helper's own view.
    /// </value>
    [HtmlAttributeName("view")]
    public string? View { get; set; }

    /// <summary>
    ///     Gets or sets the context of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <summary>
    ///     Gets the name of the default view.
    /// </summary>
    protected abstract string ViewName { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StellarAdminTemplatedTagHelperBase" />
    ///     class.
    /// </summary>
    /// <param name="viewEngine">The view engine that finds the view.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="viewEngine" /> is <see langword="null" />.
    /// </exception>
    protected StellarAdminTemplatedTagHelperBase(ICompositeViewEngine viewEngine)
    {
        _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     The view can't be found.
    /// </exception>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Executing the child content runs any <sa-slot-content> children, which assign
        // themselves to this tag helper; any other child output is discarded.
        await output.GetChildContentAsync();

        var viewName = View ?? ViewName;
        var view = ResolveView(viewName);

        // Model is assigned explicitly so a null model reaches the view as null instead
        // of the view data falling back to the calling view's model.
        var model = GetViewModel();
        var viewData = new ViewDataDictionary<object?>(ViewContext.ViewData, model)
        {
            Model = model,
        };

        // The view's <sa-slot-outlet> elements read the assigned slot content back from
        // this entry. Always set, so a nested templated render cannot see an outer host.
        viewData[SlotHostViewDataKey] = this;

        using var writer = new StringWriter();
        await view.RenderAsync(new ViewContext(ViewContext, view, viewData, writer));

        output.TagName = null;
        output.Content.SetHtmlContent(writer.ToString());
    }

    /// <summary>
    ///     Returns the model to pass to the view.
    /// </summary>
    /// <returns>The model of the view.</returns>
    protected abstract object? GetViewModel();

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
