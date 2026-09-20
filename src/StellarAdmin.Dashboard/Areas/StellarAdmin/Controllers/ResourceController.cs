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
    /// <summary>
    ///     Displays the create form.
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        return CreateView(Activator.CreateInstance<TResource>()!);
    }

    /// <summary>
    ///     Creates a resource from the submitted form.
    /// </summary>
    [HttpPost]
    [ActionName(nameof(Create))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost(CancellationToken cancellationToken)
    {
        var resource = Activator.CreateInstance<TResource>()!;
        var fields = options
            .Value.Create.Fields.Select(field => field.FieldName)
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
        var resourceOptions = options.Value;

        return ResourceView(
            nameof(Index),
            new ResourceIndexPageViewModel<TResource>
            {
                Columns = resourceOptions.Index.Columns.ToArray(),
                Items = items,
                Title = resourceOptions.Index.Title ?? resourceOptions.PluralLabel,
            }
        );
    }

    private ViewResult CreateView(object resource)
    {
        var configuration = options.Value;
        var fields = configuration.Create.Fields.ToArray();

        return ResourceView(
            nameof(Create),
            new ResourceFormPageViewModel
            {
                Entity = resource,
                Fields = fields,
                Items = fields,
                Title = configuration.Create.Title ?? "Create " + configuration.SingularLabel,
                SubmitLabel =
                    configuration.Create.SubmitLabel ?? "Create " + configuration.SingularLabel,
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
