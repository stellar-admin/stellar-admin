using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;
using StellarAdmin.Widgets;

namespace StellarAdmin.Areas.StellarAdmin.Controllers;

internal sealed partial class ResourceController<TResource>
{
    // The pages a widget's request names, besides the index
    private const string WidgetPageCreate = "create";
    private const string WidgetPageEdit = "edit";
    private const string WidgetPageView = "view";

    // A widget's content, loaded after its page has rendered or again when it refreshes. The page is the view, create
    // or edit page, or the index without one.
    [HttpGet]
    public async Task<IActionResult> Widget(
        [FromRoute] string? id,
        [FromQuery] string? page,
        CancellationToken cancellationToken
    )
    {
        var configured = page switch
        {
            null => _resourceOptions.Index.Widgets,
            WidgetPageView when HasViewPage => _resourceOptions.View!.Widgets,
            WidgetPageCreate when HasCreatePage => _resourceOptions.Create!.Widgets,
            WidgetPageEdit when HasEditPage => _resourceOptions.Edit!.Widgets,
            _ => null,
        };
        if (configured is null)
        {
            return NotFound();
        }

        return await widgets.LoadAsync(
            this,
            configured,
            GetWidgetPage(page),
            id,
            (name, parameters) => CreateWidgetUrl(page, name, parameters),
            cancellationToken
        );
    }

    // Names the page in its widgets' protected parameters, so one page's parameters are refused by another's widget
    private string GetWidgetPage(string? page) =>
        $"{ControllerContext.ActionDescriptor.ControllerName}/{page ?? "index"}";

    private string CreateWidgetUrl(string? page, string name, string? parameters) =>
        Url.Action(
            nameof(Widget),
            new
            {
                id = name,
                page,
                p = parameters,
            }
        )!;

    // The page's widgets by placement. An index request that doesn't render them still has their placements, since
    // the search box it renders replaces them on a search.
    private async Task<PageWidgetsViewModel> RenderWidgetsAsync(
        IReadOnlyList<WidgetOptions> configured,
        WidgetContext context,
        string? page,
        CancellationToken cancellationToken,
        bool render = true
    )
    {
        return new(
            await RenderAsync(WidgetPlacement.Top),
            await RenderAsync(WidgetPlacement.Aside),
            await RenderAsync(WidgetPlacement.Bottom)
        );

        async Task<IReadOnlyList<WidgetViewModel>?> RenderAsync(WidgetPlacement placement)
        {
            var placed = configured.Where(widget => widget.Placement == placement).ToArray();

            if (placed.Length == 0)
            {
                return null;
            }

            return !render
                ? []
                : await widgets.RenderAsync(
                    placed,
                    context,
                    GetWidgetPage(page),
                    (name, parameters) => CreateWidgetUrl(page, name, parameters),
                    cancellationToken
                );
        }
    }

    // The context of a create, view or edit page's widgets, whose type is closed over the page's model
    private WidgetContext CreateWidgetContext(Type context, Type modelType, params object[] args) =>
        (WidgetContext)
            Activator.CreateInstance(
                context.MakeGenericType(modelType),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                [HttpContext, .. args],
                null
            )!;
}
