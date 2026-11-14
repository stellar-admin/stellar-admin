using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Areas.StellarAdmin.ViewModels;
using StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;
using StellarAdmin.UI.Toasts;
using StellarAdmin.Widgets;

namespace StellarAdmin.Areas.StellarAdmin.Controllers;

internal sealed partial class ResourceController<TResource>
{
    // An inline row's fields are named rows[key].Field, with the key "new" for a new row
    private const string InlineRowsPrefix = "rows";
    private const string NewRowKey = "new";

    // The ids of the index page's grid and search box, which tell a request for the grid alone from a search
    private const string IndexGridId = "index-page-data-grid";
    private const string IndexSearchInputId = "index-page-search-input";

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

        // A request for the grid alone, such as another page or sort, leaves the widgets as they are; a search renders
        // them again, since their delegates receive it
        var renderWidgets =
            !Request.Headers["HX-Target"].ToString().EndsWith($"#{IndexGridId}")
            || Request.Headers["HX-Source"].ToString().EndsWith($"#{IndexSearchInputId}");

        return await IndexView(query, request, cancellationToken, renderWidgets: renderWidgets);
    }

    [HttpGet]
    public async Task<IActionResult> RowDetail(
        [FromRoute] string id,
        [FromQuery] string? panel,
        CancellationToken cancellationToken
    )
    {
        if (
            string.IsNullOrEmpty(id)
            || _resourceOptions.KeySelector is null
            || _resourceOptions.Index.RowDetails?.Panels.FirstOrDefault(options =>
                options.Name == panel
            )
                is not { Loader: { } loader } panelOptions
        )
        {
            return NotFound();
        }

        var model = await loader(HttpContext.RequestServices, id, cancellationToken);
        if (model is null)
        {
            // htmx swaps error responses, so the row shows that its details are gone
            Response.StatusCode = StatusCodes.Status404NotFound;
            return PartialView("_IndexRowDetailNotFound");
        }

        return PartialView(panelOptions.PartialName, model);
    }

    // After save and add another, the saved row and a new row with the kept fields' values. A new row at the top stays
    // above the rows it added, and one at the bottom below them. The saved row is named for the script to mark and flash
    private async Task<IActionResult> AddAnotherRowsView(
        string? key,
        ResourceIndexQuery query,
        int added,
        CancellationToken cancellationToken
    )
    {
        var next = await _resourceOptions.Create!.CreateModelAsync(
            HttpContext.RequestServices,
            cancellationToken
        );
        await BindKeptFieldsAsync(
            next,
            await CompositeValueProvider.CreateAsync(ControllerContext),
            ModelNames.CreateIndexModelName(InlineRowsPrefix, NewRowKey)
        );

        var savedRows = key is null ? [] : await LoadRowsAsync(key, cancellationToken);
        var newRow = CreateIndexModel(
            [],
            query,
            new(),
            null,
            rowsOnly: true,
            newItem: next,
            added: added
        );
        var saved = CreateIndexModel(savedRows, new(), new(), null, rowsOnly: true);
        if (savedRows.Length > 0)
        {
            Response.Headers["SA-Saved-Row"] = key;
        }

        return PartialView(
            "_IndexAddAnotherRows",
            _resourceOptions.Create.InlineRow!.Position == InlineCreatePosition.Top
                ? new IndexAddAnotherRowsViewModel(newRow, saved)
                : new IndexAddAnotherRowsViewModel(saved, newRow)
        );
    }

    private ResourceIndexPageViewModel<TResource> CreateIndexModel(
        IReadOnlyList<TResource> items,
        ResourceIndexQuery query,
        ResourceListRequest request,
        ResourceIndexPagingViewModel? pagingModel,
        bool rowsOnly = false,
        string? editingKey = null,
        object? editModel = null,
        object? newItem = null,
        int added = 0
    )
    {
        var labels = CreateLabelContext();

        return new ResourceIndexPageViewModel<TResource>
        {
            CanCreate = HasCreatePage,
            CanEdit = HasEditPage,
            CanView = _resourceOptions.View is not null && _resourceOptions.KeySelector is not null,
            Columns = _resourceOptions.Index.Columns.ToArray(),
            CreateLabel =
                _resourceOptions.Index.CreateLabel ?? _labelOptions.Index.CreateLabel(labels),
            Delete =
                _resourceOptions.Delete is null || _resourceOptions.KeySelector is null
                    ? null
                    : new(
                        _resourceOptions.Delete.Title ?? _labelOptions.Delete.Title(labels),
                        _resourceOptions.Delete.Message ?? _labelOptions.Delete.Message(labels),
                        _resourceOptions.Delete.ConfirmLabel
                            ?? _labelOptions.Delete.ConfirmLabel(labels),
                        _resourceOptions.Delete.CancelLabel
                            ?? _labelOptions.Delete.CancelLabel(labels)
                    ),
            DeleteLabel =
                _resourceOptions.Index.DeleteLabel ?? _labelOptions.Index.DeleteLabel(labels),
            EditLabel = _resourceOptions.Index.EditLabel ?? _labelOptions.Index.EditLabel(labels),
            KeySelector = _resourceOptions.KeySelector,
            InlineRows = CreateInlineRows(labels, editingKey, editModel, newItem, added),
            Items = items,
            Paging = pagingModel,
            Query = query,
            RowDetails = GetRowDetails(),
            RowsOnly = rowsOnly,
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
                    search.Placeholder ?? _labelOptions.Index.SearchPlaceholder(labels)
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
            Title = _resourceOptions.Index.Title ?? _labelOptions.Index.Title(labels),
            ViewLabel = _resourceOptions.Index.ViewLabel ?? _labelOptions.Index.ViewLabel(labels),
        };
    }

    // The inline rows' fields by column position: each field is edited by the first column that shows it, or that names
    // it with EditsField
    private ResourceIndexInlineRowsViewModel? CreateInlineRows(
        ResourceLabelContext labels,
        string? editingKey,
        object? editModel,
        object? newItem,
        int added
    )
    {
        var edit = _resourceOptions.Edit?.InlineRow;
        var create = _resourceOptions.Create?.InlineRow;
        if (
            (edit is null && create is null) || _resourceOptions.KeyPropertyName is not { } keyField
        )
        {
            return null;
        }

        return new()
        {
            AddAnotherLabel =
                create is null || _resourceOptions.Create!.AddAnother is not { } addAnother
                    ? null
                    : addAnother.Label ?? _labelOptions.Create.AddAnotherLabel(labels),
            AddedCount = added,
            CancelLabel =
                _resourceOptions.Index.CancelLabel ?? _labelOptions.Index.CancelLabel(labels),
            Create = create,
            CreateFields = create is null ? null : GetColumnFields(_resourceOptions.Create!),
            Edit = edit,
            EditFields = edit is null ? null : GetColumnFields(_resourceOptions.Edit!),
            EditingKey = editingKey,
            EditModel = editModel,
            KeyField = keyField,
            NewItem = newItem,
            SaveLabel = _resourceOptions.Index.SaveLabel ?? _labelOptions.Index.SaveLabel(labels),
        };

        FormFieldOptions?[] GetColumnFields(ResourceFormOptions form)
        {
            var remaining = form.Fields.ToList();

            return _resourceOptions
                .Index.Columns.Select(column =>
                {
                    var name = column.EditsFieldName ?? column.FieldName;
                    var field = remaining.FirstOrDefault(field =>
                        form.GetResourceFieldName(field) == name
                    );
                    if (field is not null)
                    {
                        remaining.Remove(field);
                    }

                    return field;
                })
                .ToArray();
        }
    }

    private async Task<IActionResult> EditRowView(
        string id,
        object resource,
        ResourceIndexQuery query,
        CancellationToken cancellationToken
    )
    {
        var rows = await LoadRowsAsync(id, cancellationToken);

        return rows.Length == 0
            ? NotFound()
            : RowsView(
                CreateIndexModel(
                    rows,
                    query,
                    new(),
                    null,
                    rowsOnly: true,
                    editingKey: id,
                    editModel: resource
                )
            );
    }

    // Rows load details by their key, so details that load on demand need one
    private ResourceRowDetailsOptions? GetRowDetails()
    {
        var rowDetails = _resourceOptions.Index.RowDetails;
        if (
            rowDetails?.Panels.Any(panel => panel.LoadsOnDemand) == true
            && _resourceOptions.KeySelector is null
        )
        {
            throw new InvalidOperationException(
                "Row details that load when a row expands require a resource key. Call UseKey on the resource."
            );
        }

        return rowDetails;
    }

    private async Task<IActionResult> IndexView(
        ResourceIndexQuery query,
        ResourceListRequest request,
        CancellationToken cancellationToken,
        bool redirectOutOfRange = true,
        Action<IReadOnlyList<TResource>>? loaded = null,
        bool renderWidgets = true
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

        loaded?.Invoke(result.Items);
        ViewData[ViewDataKeys.Widgets] = await RenderWidgetsAsync(
            _resourceOptions.Index.Widgets,
            new IndexWidgetContext<TResource>(HttpContext, request.Scope, request.Search),
            null,
            cancellationToken,
            renderWidgets
        );

        return ResourceView(
            nameof(Index),
            CreateIndexModel(result.Items, query, request, pagingModel)
        );
    }

    // A row as the index displays it, through the index query, whatever the scope or search
    private async Task<TResource[]> LoadRowsAsync(string key, CancellationToken cancellationToken)
    {
        var result = await dataSource.ListAsync(new() { Keys = [key] }, cancellationToken);

        return result
            .Items.Where(item => _resourceOptions.KeySelector!(item) == key)
            .Take(1)
            .ToArray();
    }

    // The row keeps the page's query, so a save can reload the page as it shows
    private PartialViewResult NewRowView(
        object resource,
        ResourceIndexQuery query,
        int added = 0
    ) =>
        RowsView(
            CreateIndexModel(
                [],
                query,
                new(),
                null,
                rowsOnly: true,
                newItem: resource,
                added: added
            )
        );

    // After an inline save, the page's grid as its query shows it, replacing the whole grid. A row the page shows is
    // named for the script to flash and focus; one it doesn't is confirmed by a toast, as a new row always is
    private async Task<IActionResult> ReloadedGridView(
        ResourceIndexQuery query,
        string? key,
        string? createdMessage,
        CancellationToken cancellationToken
    )
    {
        if (!TryCreateListRequest(query, out var request))
        {
            return BadRequest();
        }

        var shown = false;
        var view = await IndexView(
            query,
            request,
            cancellationToken,
            redirectOutOfRange: false,
            loaded: items => shown = items.Any(item => _resourceOptions.KeySelector!(item) == key),
            renderWidgets: false
        );

        var labels = CreateLabelContext();
        var notShown =
            key is null || shown
                ? null
                : _resourceOptions.Index.NotShownMessage
                    ?? _labelOptions.Index.NotShownMessage(labels);
        var toasts = HttpContext.RequestServices.GetRequiredService<IToastNotifier>();
        if (createdMessage is not null)
        {
            toasts.Add(
                new Toast
                {
                    Title = createdMessage,
                    Description = notShown,
                    Type = ToastType.Success,
                }
            );
        }
        else if (notShown is not null)
        {
            toasts.Success(
                _resourceOptions.Index.SavedMessage ?? _labelOptions.Index.SavedMessage(labels),
                notShown
            );
        }

        Response.Headers["HX-Retarget"] = "#index-page-data-grid";
        Response.Headers["HX-Reswap"] = "outerHTML";
        Response.Headers["HX-Reselect"] = "#index-page-data-grid";
        if (shown)
        {
            Response.Headers["SA-Saved-Row"] = key;
        }

        return view;
    }

    private PartialViewResult RowsView(object model) => PartialView("_IndexDataGrid", model);

    // A saved row that cannot be shown, such as one created without a key, reloads the page instead
    private async Task<IActionResult> SavedRowView(string? key, CancellationToken cancellationToken)
    {
        var rows = key is null ? [] : await LoadRowsAsync(key, cancellationToken);
        if (rows.Length == 0)
        {
            Response.Headers["HX-Refresh"] = "true";
            return Ok();
        }

        return RowsView(CreateIndexModel(rows, new(), new(), null, rowsOnly: true));
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
            && string.Equals(column.SortField, query.SortBy, StringComparison.OrdinalIgnoreCase)
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
                    column.SortField!,
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
