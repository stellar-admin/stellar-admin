using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Areas.StellarAdmin.ViewModels;
using StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;
using StellarAdmin.Diagnostics;
using StellarAdmin.Infrastructure.Authorization;
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

    // The data layout the index page shows, which the query string selects. Every request from the page carries it. Null
    // when the index has no data layouts
    private ResourceDataLayoutOptions? IndexLayout =>
        _resourceOptions.Index.FindLayout(Request.Query["layout"].ToString());

    // The cards the index page shows, or null when its data layout isn't a cards data layout
    private ResourceCardsDataLayoutOptions? IndexCards =>
        IndexLayout as ResourceCardsDataLayoutOptions;

    // The paging of the table or cards the index page shows, or null when they aren't paged
    private ResourcePagingOptions? IndexPaging => IndexTable?.Paging ?? IndexCards?.Paging;

    // The handler that lists the shown table's or cards' items, or null when they show the resource's own rows
    private ResourceTableLoader? IndexLoader => IndexTable?.Loader ?? IndexCards?.Loader;

    // The table the index page shows, or null when its data layout is a partial or cards
    private ResourceTableDataLayoutOptions? IndexTable =>
        IndexLayout as ResourceTableDataLayoutOptions;

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
            || IndexTable?.RowDetails?.Panels.FirstOrDefault(options => options.Name == panel)
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
            IndexTable!.InlineCreate!.Position == InlineCreatePosition.Top
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
        int added = 0,
        object? layoutModel = null,
        IReadOnlyList<object>? handlerRows = null,
        Func<object, string?>? linkUrl = null
    )
    {
        var labels = CreateLabelContext();
        var title = _resourceOptions.Index.Title ?? _labelOptions.Index.Title(labels);
        var createLabel =
            _resourceOptions.Index.CreateLabel ?? _labelOptions.Index.CreateLabel(labels);
        if (IndexLayout is not { } layout)
        {
            // An index without data layouts shows its page without records
            return new()
            {
                CanCreate = HasCreatePage,
                Columns = [],
                CreateLabel = createLabel,
                Items = [],
                Query = query,
                Title = title,
            };
        }

        var table = layout as ResourceTableDataLayoutOptions;

        // A table's or cards' handler loads items that are the resource's records when it has a record key, and that
        // otherwise link to another resource's view page or nowhere. Only records get the row actions
        var handler = IndexLoader is not null;
        Func<object, string?>? rowKey = layout switch
        {
            _ when !handler => null,
            { RecordKey: { } recordKey } => recordKey,
            { Link: { } link } => link.Key,
            _ => _ => null,
        };
        var actions =
            handler && layout.RecordKey is null ? null
            : table is not null ? table.RowActions
            : (layout as ResourceCardsDataLayoutOptions)?.RowActions;
        var keySelector = _resourceOptions.KeySelector;
        Func<object, string?>? viewUrl = null;
        if (handler && layout.Link is not null)
        {
            viewUrl = linkUrl;
        }
        else if (HasViewPage && actions?.View == true)
        {
            var viewKey = rowKey ?? (resource => keySelector!((TResource)resource));
            viewUrl = resource =>
                viewKey(resource) is { } id ? Url.Action(ViewActionName, new { id }) : null;
        }

        return new ResourceIndexPageViewModel<TResource>
        {
            // A table that creates or edits inline does so instead of linking to the page
            CanCreate = HasCreatePage && table?.InlineCreate is null,
            CanEdit = HasEditPage && actions?.Edit == true && table?.InlineEditing is null,
            CanView = viewUrl is not null,
            Columns = table?.Columns.ToArray() ?? [],
            CreateLabel = createLabel,
            Delete =
                actions?.Delete != true
                || _resourceOptions.Delete is null
                || _resourceOptions.KeySelector is null
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
            HandlerRows = handlerRows,
            Items = items,
            Layouts = CreateLayoutSwitcher(layout),
            Paging = pagingModel,
            PartialLayout = layout is ResourcePartialDataLayoutOptions partial
                ? new(partial.PartialName, layoutModel ?? CreateLayoutModel(items, query, request))
                : null,
            Query = query,
            RowDetails = GetRowDetails(),
            RowKey = rowKey,
            RowsOnly = rowsOnly,
            Scopes =
                _resourceOptions.Index.Scopes is { } scopes && !layout.IgnoresScopes
                    ? scopes
                        .Items.Select(scope => new ResourceIndexScopeViewModel(
                            scope.Id,
                            scope.Title,
                            scope.Id == request.Scope,
                            scope.Id == scopes.DefaultScope ? null : scope.Id
                        ))
                        .ToArray()
                    : [],
            Search =
                _resourceOptions.Index.Search is { } search && !layout.IgnoresSearch
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
            Title = title,
            ViewLabel = _resourceOptions.Index.ViewLabel ?? _labelOptions.Index.ViewLabel(labels),
            ViewUrl = viewUrl,
        };
    }

    // A partial's items with links to their pages, and to the index as it shows
    private ResourceDataLayoutModel<TResource> CreateLayoutModel(
        IReadOnlyList<TResource> items,
        ResourceIndexQuery query,
        ResourceListRequest request
    ) =>
        new(
            items,
            request,
            item =>
                HasViewPage
                    ? Url.Action(ViewActionName, new { id = _resourceOptions.KeySelector!(item) })
                    : null,
            item =>
                HasEditPage
                    ? Url.Action(nameof(Edit), new { id = _resourceOptions.KeySelector!(item) })
                    : null,
            CreateLayoutIndexUrl(query)
        );

    // The index as it shows, with the data layout, scope and search the query string holds, plus or replacing the
    // values
    private Func<object?, string> CreateLayoutIndexUrl(ResourceIndexQuery query) =>
        values =>
        {
            var routeValues = new RouteValueDictionary
            {
                ["id"] = null,
                ["layout"] = query.Layout,
                ["scope"] = query.Scope,
                ["search"] = query.Search,
            };
            foreach (var (name, value) in new RouteValueDictionary(values))
            {
                routeValues[name] = value;
            }

            return Url.Action(nameof(Index), routeValues)!;
        };

    // The switcher's segments, which link to each data layout with the page's scope and search. Sort and paging belong
    // to the table they were chosen in. The first data layout links without a key
    private ResourceIndexDataLayoutViewModel[] CreateLayoutSwitcher(
        ResourceDataLayoutOptions active
    )
    {
        var layouts = _resourceOptions.Index.Layouts;
        if (layouts.Count < 2)
        {
            return [];
        }

        return layouts
            .Select(layout => new ResourceIndexDataLayoutViewModel(
                layout.Key,
                layout.Title ?? layout.Key.Humanize(LetterCasing.Sentence),
                layout.Icon,
                layout == active
            ))
            .ToArray();
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
        // Inline rows edit the shown table's columns, which the inline row check matched to the form's fields
        if (IndexTable is not { } table)
        {
            return null;
        }

        var edit =
            table.RowActions.Edit && _resourceOptions.Edit is not null ? table.InlineEditing : null;
        var create = _resourceOptions.Create is not null ? table.InlineCreate : null;
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

            return table
                .Columns.Select(column =>
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
        var rowDetails = IndexTable?.RowDetails;
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
        // An index without data layouts lists nothing
        if (IndexLayout is null)
        {
            return await IndexResultAsync([], null, null);
        }

        // A data layout with a handler lists its own items instead of the resource's
        if (IndexLayout is ResourcePartialDataLayoutOptions { Loader: { } loader } partial)
        {
            var layoutModel = await loader(
                HttpContext.RequestServices,
                request,
                await CreateLayoutUrlsAsync(partial, query),
                cancellationToken
            );

            return await IndexResultAsync([], null, layoutModel);
        }

        var result = await ListRowsAsync(request, cancellationToken);
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
                            layout = query.Layout,
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
                result = await ListRowsAsync(request with { Paging = paging }, cancellationToken);
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
                IndexPaging!.PageSizes
            );
        }

        // A table's or cards' handler loads items of its own, which link to another resource's view page when the user
        // may open it
        if (IndexLoader is not null)
        {
            return await IndexResultAsync(
                [],
                pagingModel,
                null,
                result.Items,
                await CreateLinkUrlAsync(IndexLayout!)
            );
        }

        var items = result.Items.Cast<TResource>().ToArray();
        loaded?.Invoke(items);

        return await IndexResultAsync(items, pagingModel, null);

        async Task<IActionResult> IndexResultAsync(
            IReadOnlyList<TResource> items,
            ResourceIndexPagingViewModel? paging,
            object? layoutModel,
            IReadOnlyList<object>? handlerRows = null,
            Func<object, string?>? linkUrl = null
        )
        {
            ViewData[ViewDataKeys.IndexCards] = IndexCards;
            ViewData[ViewDataKeys.IndexWithoutDataLayouts] = IndexLayout is null;
            ViewData[ViewDataKeys.Widgets] = await RenderWidgetsAsync(
                _resourceOptions.Index.Widgets,
                new IndexWidgetContext<TResource>(HttpContext, request.Scope, request.Search),
                null,
                cancellationToken,
                renderWidgets
            );

            return ResourceView(
                nameof(Index),
                CreateIndexModel(
                    items,
                    query,
                    request,
                    paging,
                    layoutModel: layoutModel,
                    handlerRows: handlerRows,
                    linkUrl: linkUrl
                )
            );
        }
    }

    // The rows of the shown table or cards: the resources from the data source, or the items their handler loads
    private async Task<ResourceListResult<object>> ListRowsAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    ) =>
        IndexLoader is { } loader
            ? await loader(HttpContext.RequestServices, request, cancellationToken)
            : await HandlerDataLayoutLoader.ListRowsAsync(
                dataSource.ListAsync(request, cancellationToken)
            );

    // A partial handler's items link to the resource's pages when they're its records, or to another resource's view
    // page when the user may open it
    private async Task<ResourceDataLayoutUrls> CreateLayoutUrlsAsync(
        ResourcePartialDataLayoutOptions partial,
        ResourceIndexQuery query
    )
    {
        var viewUrl = await CreateLinkUrlAsync(partial) ?? (_ => null);
        Func<object, string?> editUrl = _ => null;
        if (partial.RecordKey is { } key)
        {
            if (HasViewPage)
            {
                viewUrl = item =>
                    key(item) is { } id ? Url.Action(ViewActionName, new { id }) : null;
            }

            if (HasEditPage)
            {
                editUrl = item => key(item) is { } id ? Url.Action(nameof(Edit), new { id }) : null;
            }
        }

        return new(viewUrl, editUrl, CreateLayoutIndexUrl(query));
    }

    // The view page of another resource that a handler data layout's items link to, or null when the resource has no
    // view page or the user may not open it
    private async Task<Func<object, string?>?> CreateLinkUrlAsync(ResourceDataLayoutOptions layout)
    {
        if (layout.Link is not { } link)
        {
            return null;
        }

        var services = HttpContext.RequestServices;
        var registration = services
            .GetServices<ResourceRegistration>()
            .FirstOrDefault(registration => registration.ResourceType == link.ResourceType);
        if (registration is null || !registration.ResolveCanView(services))
        {
            // Other resources aren't known while this resource's options are built, so this is found when the data
            // layout renders
            var name = link.ResourceType.Name;
            services
                .GetRequiredService<ConfigurationProblems>()
                .Report(
                    new(
                        ConfigurationProblemKind.RowLinkUnavailable,
                        registration is null
                            ? $"The items of the `{layout.Key}` data layout don't link to `{name}` because no resource is registered for it."
                            : $"The items of the `{layout.Key}` data layout don't link to `{name}` because the `{registration.ControllerName}` resource has no view page.",
                        registration is null
                            ? $"Add a resource for `{name}`, or remove `LinkTo`."
                            : $"Allow view on the `{registration.ControllerName}` resource, or remove `LinkTo`."
                    ),
                    HttpContext.GetRouteValue("controller") as string
                );

            return null;
        }

        if (
            !await AuthorizationMetadata.AuthorizeAsync(
                registration.ResolveAuthorizationMetadata(services),
                HttpContext
            )
        )
        {
            return null;
        }

        return resource =>
            link.Key(resource) is { } id
                ? Url.Action(ViewActionName, registration.ControllerName, new { id })
                : null;
    }

    // A row as the index displays it, through the index query, whatever the scope or search
    private async Task<TResource[]> LoadRowsAsync(string key, CancellationToken cancellationToken)
    {
        var result = await dataSource.ListAsync(
            new()
            {
                Fields = IndexLayout?.Fields ?? [],
                Keys = [key],
                Layout = IndexLayout?.Key,
            },
            cancellationToken
        );

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

        // A data layout that ignores the scopes or the search lists without them, though they stay in the query string.
        // A partial lists every item in the data source's order. Cards sort only by their default sort's field for now
        if (IndexLayout is not { } layout)
        {
            return true;
        }

        var table = layout as ResourceTableDataLayoutOptions;
        var cards = layout as ResourceCardsDataLayoutOptions;
        var sortDirection =
            direction == "desc"
                ? ResourceSortDirection.Descending
                : ResourceSortDirection.Ascending;
        request = new()
        {
            Fields = layout.Fields,
            Layout = layout.Key,
            Scope =
                _resourceOptions.Index.Scopes is { } scopes && !layout.IgnoresScopes
                    ? scopes
                        .Items.FirstOrDefault(scope =>
                            string.Equals(scope.Id, query.Scope, StringComparison.OrdinalIgnoreCase)
                        )
                        ?.Id
                        ?? scopes.DefaultScope
                    : null,
            Search =
                _resourceOptions.Index.Search is null
                || layout.IgnoresSearch
                || string.IsNullOrWhiteSpace(query.Search)
                    ? null
                    : query.Search.Trim(),
            Sort =
                table?.FindSort(query.SortBy, sortDirection)
                ?? table?.FindDefaultSort()
                ?? cards?.FindSort(query.SortBy, sortDirection)
                ?? cards?.FindDefaultSort(),
        };
        if (IndexPaging is not { } options)
        {
            return true;
        }

        if (query.Page is <= 0)
        {
            return false;
        }

        var page = query.Page ?? 1;

        // Cards have no page-size picker, so they ignore the query's page size
        var pageSize =
            table is not null && query.PageSize is { } size && options.PageSizes.Contains(size)
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
