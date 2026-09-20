using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Options;
using StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin.Controllers;

/// <summary>
///     Handles resource pages using the configured data source.
/// </summary>
[Area("StellarAdmin")]
public class ResourceController<TResource>(
    IResourceDataSource<TResource> dataSource,
    IOptions<ResourceOptions<TResource>> options,
    ICompositeViewEngine viewEngine
) : Controller
{
    private readonly ResourceOptions<TResource> _resourceOptions = options.Value;

    /// <summary>
    ///     Displays the create form.
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        return CreateView(_resourceOptions.Create.CreateInstance()!);
    }

    /// <summary>
    ///     Creates a resource from the submitted form.
    /// </summary>
    [HttpPost]
    [ActionName(nameof(Create))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost(CancellationToken cancellationToken)
    {
        var resource = _resourceOptions.Create.CreateInstance()!;
        var fields = _resourceOptions
            .Create.Fields.Select(field => field.FieldName)
            .ToHashSet(StringComparer.Ordinal);

        var valid = await TryUpdateModelAsync(
            resource,
            typeof(TResource),
            ResourceFormPageViewModel.BindingPrefix,
            await CompositeValueProvider.CreateAsync(ControllerContext),
            metadata => fields.Contains(metadata.PropertyName ?? "")
        );
        if (!valid)
        {
            return CreateView(resource);
        }

        await dataSource.CreateAsync(resource, cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    ///     Displays the resource index page.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await dataSource.ListAsync(cancellationToken);

        return ResourceView(
            nameof(Index),
            new ResourceIndexPageViewModel<TResource>
            {
                Columns = _resourceOptions.Index.Columns.ToArray(),
                Items = items,
                Title = _resourceOptions.Index.Title ?? _resourceOptions.PluralLabel,
            }
        );
    }

    private ViewResult CreateView(object resource)
    {
        var fields = _resourceOptions.Create.Fields.ToArray();

        return ResourceView(
            nameof(Create),
            new ResourceFormPageViewModel
            {
                Entity = resource,
                Fields = fields,
                Items = fields,
                Title = _resourceOptions.Create.Title ?? "Create " + _resourceOptions.SingularLabel,
                SubmitLabel =
                    _resourceOptions.Create.SubmitLabel
                    ?? "Create " + _resourceOptions.SingularLabel,
            }
        );
    }

    private ViewResult ResourceView(string action, object model)
    {
        var viewName = viewEngine.FindView(ControllerContext, action, isMainPage: true).Success
            ? action
            : "Resource" + action;

        return View(viewName, model);
    }
}
