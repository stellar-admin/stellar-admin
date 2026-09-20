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
    IOptions<ResourceLabelOptions> labelOptions,
    ICompositeViewEngine viewEngine
) : Controller
{
    private readonly ResourceLabelOptions _labelOptions = labelOptions.Value;
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

        var result = await dataSource.CreateAsync(resource, cancellationToken);
        if (result.IsNotFound)
        {
            return NotFound();
        }

        if (!result.IsSuccess)
        {
            AddValidationErrors(result, fields);
            return CreateView(resource);
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    ///     Deletes a resource.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        CancellationToken cancellationToken
    )
    {
        if (_resourceOptions.KeySelector is null || string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var result = await dataSource.DeleteAsync(id, cancellationToken);
        if (result.IsNotFound)
        {
            return NotFound();
        }

        if (!result.IsSuccess)
        {
            AddValidationErrors(result, []);
            return await Index(cancellationToken);
        }

        return RedirectToAction(nameof(Index), new { id = (string?)null });
    }

    /// <summary>
    ///     Displays the edit form.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(
        [FromRoute] string id,
        CancellationToken cancellationToken
    )
    {
        if (_resourceOptions.KeySelector is null || string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var resource = await dataSource.FindAsync(id, cancellationToken);

        return resource is null ? NotFound() : EditView(resource);
    }

    /// <summary>
    ///     Updates a resource from the submitted form.
    /// </summary>
    [HttpPost]
    [ActionName(nameof(Edit))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(
        [FromRoute] string id,
        CancellationToken cancellationToken
    )
    {
        if (_resourceOptions.KeySelector is null || string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var resource = await dataSource.FindAsync(id, cancellationToken);
        if (resource is null)
        {
            return NotFound();
        }

        var fields = _resourceOptions
            .Edit.Fields.Select(field => field.FieldName)
            .Where(name => name != _resourceOptions.KeyPropertyName)
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
            return EditView(resource);
        }

        var result = await dataSource.UpdateAsync(id, resource, cancellationToken);
        if (result.IsNotFound)
        {
            return NotFound();
        }

        if (!result.IsSuccess)
        {
            AddValidationErrors(result, fields);
            return EditView(resource);
        }

        return RedirectToAction(nameof(Index), new { id = (string?)null });
    }

    /// <summary>
    ///     Displays the resource index page.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await dataSource.ListAsync(cancellationToken);
        var labels = CreateLabelContext();

        return ResourceView(
            nameof(Index),
            new ResourceIndexPageViewModel<TResource>
            {
                Columns = _resourceOptions.Index.Columns.ToArray(),
                CreateLabel =
                    _resourceOptions.Index.CreateLabel ?? _labelOptions.IndexCreateLabel(labels),
                Delete = _resourceOptions.KeySelector is null
                    ? null
                    : new(
                        _resourceOptions.Delete.Title ?? _labelOptions.DeleteTitle(labels),
                        _resourceOptions.Delete.Message ?? _labelOptions.DeleteMessage(labels),
                        _resourceOptions.Delete.ConfirmLabel
                            ?? _labelOptions.DeleteConfirmLabel(labels),
                        _resourceOptions.Delete.CancelLabel
                            ?? _labelOptions.DeleteCancelLabel(labels)
                    ),
                DeleteLabel =
                    _resourceOptions.Index.DeleteLabel ?? _labelOptions.IndexDeleteLabel(labels),
                EditLabel =
                    _resourceOptions.Index.EditLabel ?? _labelOptions.IndexEditLabel(labels),
                KeySelector = _resourceOptions.KeySelector,
                Items = items,
                Title = _resourceOptions.Index.Title ?? _labelOptions.IndexTitle(labels),
            }
        );
    }

    private void AddValidationErrors(ResourceOperationResult result, HashSet<string> fields)
    {
        foreach (var error in result.Errors)
        {
            var key =
                error.FieldName is not null && fields.Contains(error.FieldName)
                    ? ModelNames.CreatePropertyModelName(
                        ResourceFormPageViewModel.BindingPrefix,
                        error.FieldName
                    )
                    : string.Empty;
            ModelState.AddModelError(key, error.Message);
        }
    }

    private ResourceLabelContext CreateLabelContext() =>
        new(_resourceOptions.SingularLabel, _resourceOptions.PluralLabel);

    private ViewResult CreateView(object resource)
    {
        var fields = _resourceOptions.Create.Fields.ToArray();
        var labels = CreateLabelContext();

        return ResourceView(
            nameof(Create),
            new ResourceFormPageViewModel
            {
                Entity = resource,
                Fields = fields,
                Items = _resourceOptions.Create.Items.ToArray(),
                SectionLayout = _resourceOptions.Create.SectionLayout,
                Title = _resourceOptions.Create.Title ?? _labelOptions.CreateTitle(labels),
                SubmitLabel =
                    _resourceOptions.Create.SubmitLabel ?? _labelOptions.CreateSubmitLabel(labels),
            }
        );
    }

    private ViewResult EditView(object resource)
    {
        var labels = CreateLabelContext();

        return ResourceView(
            nameof(Edit),
            new ResourceFormPageViewModel
            {
                Entity = resource,
                Fields = _resourceOptions.Edit.Fields,
                Items = _resourceOptions.Edit.Items.ToArray(),
                SectionLayout = _resourceOptions.Edit.SectionLayout,
                Title = _resourceOptions.Edit.Title ?? _labelOptions.EditTitle(labels),
                SubmitLabel =
                    _resourceOptions.Edit.SubmitLabel ?? _labelOptions.EditSubmitLabel(labels),
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
