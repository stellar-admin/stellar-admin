using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Options;
using StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.TagHelpers;

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

    private bool CanCreate => _resourceOptions.Create is not null;

    /// <summary>
    ///     Displays the create form.
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        return _resourceOptions.Create is { } create
            ? CreateView(create.CreateModel())
            : NotFound();
    }

    /// <summary>
    ///     Creates a resource from the submitted form.
    /// </summary>
    [HttpPost]
    [ActionName(nameof(Create))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost(CancellationToken cancellationToken)
    {
        if (_resourceOptions.Create is null)
        {
            return NotFound();
        }

        var resource = _resourceOptions.Create.CreateModel();
        var fields = _resourceOptions
            .Create.Fields.Select(field => field.FieldName)
            .ToHashSet(StringComparer.Ordinal);

        var valid = await TryUpdateModelAsync(
            resource,
            _resourceOptions.Create.ModelType,
            ResourceFormPageViewModel.BindingPrefix,
            await CompositeValueProvider.CreateAsync(ControllerContext),
            metadata => fields.Contains(metadata.PropertyName ?? "")
        );
        if (!valid)
        {
            return CreateView(resource);
        }

        var result = _resourceOptions.CreateHandler is { } handler
            ? await handler(HttpContext.RequestServices, resource, cancellationToken)
            : await ((IResourceCreateHandler<TResource>)dataSource).CreateAsync(
                (TResource)resource,
                cancellationToken
            );
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
        [FromQuery] ResourceIndexQuery query,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        if (
            _resourceOptions.Delete is null
            || _resourceOptions.KeySelector is null
            || string.IsNullOrEmpty(id)
            || dataSource is not IResourceDeleteHandler<TResource> handler
        )
        {
            return NotFound();
        }

        if (!TryCreateListRequest(query, out var request))
        {
            return BadRequest();
        }

        var result = await handler.DeleteAsync(id, cancellationToken);
        if (result.IsNotFound)
        {
            return NotFound();
        }

        if (!result.IsSuccess)
        {
            AddValidationErrors(result, []);
            return await IndexView(query, request, cancellationToken, redirectOutOfRange: false);
        }

        return RedirectToAction(
            nameof(Index),
            new
            {
                id = (string?)null,
                page = query.Page,
                pageSize = query.PageSize,
                search = query.Search,
                scope = query.Scope,
                sortBy = query.SortBy,
                sortDirection = query.SortDirection,
            }
        );
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
        if (
            _resourceOptions.Edit is null
            || _resourceOptions.KeySelector is null
            || string.IsNullOrEmpty(id)
        )
        {
            return NotFound();
        }

        var resource = _resourceOptions.EditLoader is { } loader
            ? await loader(HttpContext.RequestServices, id, cancellationToken)
            : await ((IResourceEditHandler<TResource>)dataSource).FindAsync(id, cancellationToken);

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
        if (
            _resourceOptions.Edit is null
            || _resourceOptions.KeySelector is null
            || string.IsNullOrEmpty(id)
        )
        {
            return NotFound();
        }

        var resource = _resourceOptions.EditLoader is { } loader
            ? await loader(HttpContext.RequestServices, id, cancellationToken)
            : await ((IResourceEditHandler<TResource>)dataSource).FindAsync(id, cancellationToken);
        if (resource is null)
        {
            return NotFound();
        }

        var fields = _resourceOptions
            .Edit.Fields.Select(field => field.FieldName)
            .Where(name =>
                _resourceOptions.Edit.ModelType != typeof(TResource)
                || name != _resourceOptions.KeyPropertyName
            )
            .ToHashSet(StringComparer.Ordinal);
        var valid = await TryUpdateModelAsync(
            resource,
            _resourceOptions.Edit.ModelType,
            ResourceFormPageViewModel.BindingPrefix,
            await CompositeValueProvider.CreateAsync(ControllerContext),
            metadata => fields.Contains(metadata.PropertyName ?? "")
        );
        if (!valid)
        {
            return EditView(resource);
        }

        var result = _resourceOptions.EditHandler is { } handler
            ? await handler(HttpContext.RequestServices, id, resource, cancellationToken)
            : await ((IResourceEditHandler<TResource>)dataSource).UpdateAsync(
                id,
                (TResource)resource,
                cancellationToken
            );
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
    public async Task<IActionResult> Index(
        [FromQuery] ResourceIndexQuery query,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        if (!TryCreateListRequest(query, out var request))
        {
            return BadRequest();
        }

        return await IndexView(query, request, cancellationToken);
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
        var create = _resourceOptions.Create!;
        var fields = create.Fields.ToArray();
        var labels = CreateLabelContext();

        return ResourceView(
            nameof(Create),
            new ResourceFormPageViewModel
            {
                Entity = resource,
                Fields = fields,
                Items = create.Items.ToArray(),
                SectionLayout = create.SectionLayout,
                Title = create.Title ?? _labelOptions.CreateTitle(labels),
                SubmitLabel = create.SubmitLabel ?? _labelOptions.CreateSubmitLabel(labels),
            }
        );
    }

    private ViewResult EditView(object resource)
    {
        var edit = _resourceOptions.Edit!;
        var labels = CreateLabelContext();

        return ResourceView(
            nameof(Edit),
            new ResourceFormPageViewModel
            {
                Entity = resource,
                Fields = edit.Fields,
                Items = edit.Items.ToArray(),
                SectionLayout = edit.SectionLayout,
                Title = edit.Title ?? _labelOptions.EditTitle(labels),
                SubmitLabel = edit.SubmitLabel ?? _labelOptions.EditSubmitLabel(labels),
            }
        );
    }

    private async Task<IActionResult> IndexView(
        ResourceIndexQuery query,
        ResourceListRequest request,
        CancellationToken cancellationToken,
        bool redirectOutOfRange = true
    )
    {
        var result = await dataSource.ListAsync(request, cancellationToken);
        ResourceIndexPagingViewModel? pagingModel = null;
        if (request.Paging is { } paging)
        {
            var totalPages = Math.Max(
                1,
                result.TotalCount / paging.PageSize
                    + (result.TotalCount % paging.PageSize == 0 ? 0 : 1)
            );
            if (paging.Page > totalPages)
            {
                if (redirectOutOfRange)
                {
                    return RedirectToAction(
                        nameof(Index),
                        new
                        {
                            id = (string?)null,
                            page = totalPages,
                            pageSize = query.PageSize,
                            search = query.Search,
                            scope = query.Scope,
                            sortBy = query.SortBy,
                            sortDirection = query.SortDirection,
                        }
                    );
                }

                // A rejected delete must retain its validation errors while adjusting the page.
                paging = paging with
                {
                    Page = (int)totalPages,
                };
                result = await dataSource.ListAsync(
                    request with
                    {
                        Paging = paging,
                    },
                    cancellationToken
                );
                totalPages = Math.Max(
                    1,
                    result.TotalCount / paging.PageSize
                        + (result.TotalCount % paging.PageSize == 0 ? 0 : 1)
                );
            }

            pagingModel = new(
                paging.Page,
                paging.PageSize,
                result.TotalCount,
                totalPages,
                _resourceOptions.Index.Paging!.PageSizes
            );
        }

        var labels = CreateLabelContext();

        return ResourceView(
            nameof(Index),
            new ResourceIndexPageViewModel<TResource>
            {
                CanCreate = CanCreate,
                CanEdit =
                    _resourceOptions.Edit is not null && _resourceOptions.KeySelector is not null,
                Columns = _resourceOptions.Index.Columns.ToArray(),
                CreateLabel =
                    _resourceOptions.Index.CreateLabel ?? _labelOptions.IndexCreateLabel(labels),
                Delete =
                    _resourceOptions.Delete is null || _resourceOptions.KeySelector is null
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
                Items = result.Items,
                Paging = pagingModel,
                Query = query,
                Scopes = _resourceOptions.Index.Scopes is { } scopes
                    ? scopes
                        .Items.Select(scope => new ResourceIndexScopeViewModel(
                            scope.Id,
                            scope.Title,
                            scope.Id == request.Scope,
                            scope.Id == scopes.DefaultScope ? null : scope.Id
                        ))
                        .ToArray()
                    : [],
                Search = _resourceOptions.Index.Search is { } search
                    ? new(
                        request.Search,
                        search.Placeholder ?? _labelOptions.IndexSearchPlaceholder(labels)
                    )
                    : null,
                Sort = request.Sort is { } sort
                    ? new(
                        sort.Field,
                        sort.Direction == ResourceSortDirection.Descending
                            ? DataGridSortDirection.Descending
                            : DataGridSortDirection.Ascending
                    )
                    : null,
                Title = _resourceOptions.Index.Title ?? _labelOptions.IndexTitle(labels),
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

    private bool TryCreateListRequest(ResourceIndexQuery query, out ResourceListRequest request)
    {
        request = new();
        var direction = query.SortDirection?.ToLowerInvariant();
        if (direction is not (null or "" or "asc" or "desc"))
        {
            return false;
        }

        var column = _resourceOptions.Index.Columns.FirstOrDefault(column =>
            column.Sortable
            && string.Equals(column.FieldName, query.SortBy, StringComparison.OrdinalIgnoreCase)
        );
        request = new()
        {
            Scope = _resourceOptions.Index.Scopes is { } scopes
                ? scopes
                    .Items.FirstOrDefault(scope =>
                        string.Equals(scope.Id, query.Scope, StringComparison.OrdinalIgnoreCase)
                    )
                    ?.Id
                    ?? scopes.DefaultScope
                : null,
            Search =
                _resourceOptions.Index.Search is null || string.IsNullOrWhiteSpace(query.Search)
                    ? null
                    : query.Search.Trim(),
            Sort = column is null
                ? _resourceOptions.Index.DefaultSort
                : new(
                    column.FieldName!,
                    direction == "desc"
                        ? ResourceSortDirection.Descending
                        : ResourceSortDirection.Ascending
                ),
        };
        if (_resourceOptions.Index.Paging is not { } options)
        {
            return true;
        }

        if (query.Page is <= 0)
        {
            return false;
        }

        var page = query.Page ?? 1;
        var pageSize =
            query.PageSize is { } size && options.PageSizes.Contains(size)
                ? size
                : options.PageSize;
        if ((long)(page - 1) * pageSize > int.MaxValue)
        {
            return false;
        }

        request = request with { Paging = new(page, pageSize) };
        return true;
    }
}
