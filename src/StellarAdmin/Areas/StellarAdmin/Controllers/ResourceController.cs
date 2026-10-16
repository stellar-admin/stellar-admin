using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Areas.StellarAdmin.ViewModels;
using StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Areas.StellarAdmin.Controllers;

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
    // The view page's action, whose method cannot be named View alongside the controller's View helpers
    private const string ViewActionName = "View";

    // An inline row's fields are named rows[key].Field, with the key "new" for a new row
    private const string InlineRowsPrefix = "rows";
    private const string NewRowKey = "new";

    private readonly ResourceLabelOptions _labelOptions = labelOptions.Value;
    private readonly ResourceOptions<TResource> _resourceOptions = options.Value;

    // An inline operation has no page, so neither its links nor its routes are available
    private bool HasCreatePage => _resourceOptions.Create is { InlineRow: null };
    private bool HasEditPage =>
        _resourceOptions.Edit is { InlineRow: null } && _resourceOptions.KeySelector is not null;

    /// <summary>
    ///     Displays the create form.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        return _resourceOptions.Create is { InlineRow: null } create
            ? await CreateView(create.CreateModel(), cancellationToken)
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
        if (!HasCreatePage)
        {
            return NotFound();
        }

        var (resource, result) = await SubmitCreateAsync(
            ResourceFormPageViewModel.BindingPrefix,
            cancellationToken
        );
        if (result is { IsNotFound: true })
        {
            return NotFound();
        }

        if (result is not { IsSuccess: true })
        {
            return await CreateView(resource, cancellationToken);
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    ///     Renders a new row in edit mode, for creating a resource in the index grid.
    /// </summary>
    [HttpGet]
    public IActionResult CreateRow()
    {
        return
            _resourceOptions.Create is { InlineRow: not null } create
            && _resourceOptions.KeySelector is not null
            ? NewRowView(create.CreateModel())
            : NotFound();
    }

    /// <summary>
    ///     Creates a resource from a new row, and renders it as it displays in the index grid.
    /// </summary>
    [HttpPost]
    [ActionName(nameof(CreateRow))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRowPost(CancellationToken cancellationToken)
    {
        if (
            _resourceOptions.Create is not { InlineRow: not null }
            || _resourceOptions.KeySelector is not { } keySelector
        )
        {
            return NotFound();
        }

        var (resource, result) = await SubmitCreateAsync(
            ModelNames.CreateIndexModelName(InlineRowsPrefix, NewRowKey),
            cancellationToken
        );
        if (result is { IsNotFound: true })
        {
            return NotFound();
        }

        if (result is not { IsSuccess: true })
        {
            return NewRowView(resource);
        }

        // A create model other than the resource has no key selector, so its handler returns the key
        var key = result.Key ?? (resource is TResource created ? keySelector(created) : null);
        var labels = CreateLabelContext();
        HttpContext
            .RequestServices.GetRequiredService<IToastNotifier>()
            .Success(
                _resourceOptions.Index.CreatedMessage ?? _labelOptions.Index.CreatedMessage(labels)
            );

        return await SavedRowView(key, cancellationToken);
    }

    /// <summary>
    ///     Displays the create form in a sheet, for a lookup field that creates its item.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CreateSheet(
        [FromQuery(Name = "for")] string? lookup,
        [FromQuery] int level,
        CancellationToken cancellationToken
    )
    {
        return _resourceOptions.Create is { } create && !string.IsNullOrEmpty(lookup) && level >= 1
            ? await CreateSheetView(create.CreateModel(), lookup, level, cancellationToken)
            : NotFound();
    }

    /// <summary>
    ///     Creates a resource from the create form in a sheet, and returns its key to the lookup field.
    /// </summary>
    [HttpPost]
    [ActionName(nameof(CreateSheet))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSheetPost(
        [FromQuery(Name = "for")] string? lookup,
        [FromQuery] int level,
        CancellationToken cancellationToken
    )
    {
        if (_resourceOptions.Create is null || string.IsNullOrEmpty(lookup) || level < 1)
        {
            return NotFound();
        }

        var (resource, result) = await SubmitCreateAsync(
            CreateSheetViewModel.GetBindingPrefix(level),
            cancellationToken
        );
        if (result is { IsNotFound: true })
        {
            return NotFound();
        }

        if (result is not { IsSuccess: true })
        {
            return await CreateSheetView(resource, lookup, level, cancellationToken);
        }

        // A create model other than the resource has no key selector, so its handler returns the key
        var key =
            result.Key
            ?? (
                resource is TResource created && _resourceOptions.KeySelector is { } selector
                    ? selector(created)
                    : null
            );

        return PartialView("_LookupCreated", new LookupCreatedViewModel(lookup, key));
    }

    /// <summary>
    ///     Deletes a resource.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        [FromQuery] ResourceIndexQuery query,
        [FromForm] string? origin,
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
            if (origin is "edit" or "view")
            {
                TempData[TempDataKeys.DeleteErrors] = JsonSerializer.Serialize(
                    result.Errors.Select(error => error.Message)
                );
                return RedirectToAction(
                    origin == "edit" ? nameof(Edit) : ViewActionName,
                    new { id }
                );
            }

            AddValidationErrors(result, [], ResourceFormPageViewModel.BindingPrefix);
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
            _resourceOptions.Edit is not { InlineRow: null }
            || _resourceOptions.KeySelector is null
            || string.IsNullOrEmpty(id)
        )
        {
            return NotFound();
        }

        var resource = await LoadEditModelAsync(id, cancellationToken);

        if (resource is null)
        {
            return NotFound();
        }

        AddDeleteErrors();

        return await EditView(resource, id, cancellationToken);
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
            _resourceOptions.Edit is not { InlineRow: null }
            || _resourceOptions.KeySelector is null
            || string.IsNullOrEmpty(id)
        )
        {
            return NotFound();
        }

        var resource = await LoadEditModelAsync(id, cancellationToken);
        if (resource is null)
        {
            return NotFound();
        }

        var fields = GetEditFieldNames();
        var valid = await TryBindConfiguredFieldsAsync(
            resource,
            _resourceOptions.Edit.ModelType,
            fields,
            ResourceFormPageViewModel.BindingPrefix
        );
        if (!valid)
        {
            return await EditView(resource, id, cancellationToken);
        }

        var result = await UpdateAsync(id, resource, cancellationToken);
        if (result.IsNotFound)
        {
            return NotFound();
        }

        if (!result.IsSuccess)
        {
            AddValidationErrors(result, fields, ResourceFormPageViewModel.BindingPrefix);
            return await EditView(resource, id, cancellationToken);
        }

        return RedirectToAction(nameof(Index), new { id = (string?)null });
    }

    /// <summary>
    ///     Renders a resource's row in edit mode, for editing it in the index grid.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EditRow(
        [FromRoute] string id,
        CancellationToken cancellationToken
    )
    {
        if (
            _resourceOptions.Edit is not { InlineRow: not null }
            || _resourceOptions.KeySelector is null
            || string.IsNullOrEmpty(id)
        )
        {
            return NotFound();
        }

        var resource = await LoadEditModelAsync(id, cancellationToken);

        return resource is null ? NotFound() : await EditRowView(id, resource, cancellationToken);
    }

    /// <summary>
    ///     Updates a resource from its row, and renders it as it displays in the index grid.
    /// </summary>
    [HttpPost]
    [ActionName(nameof(EditRow))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRowPost(
        [FromRoute] string id,
        CancellationToken cancellationToken
    )
    {
        if (
            _resourceOptions.Edit is not { InlineRow: not null } edit
            || _resourceOptions.KeySelector is null
            || string.IsNullOrEmpty(id)
        )
        {
            return NotFound();
        }

        var resource = await LoadEditModelAsync(id, cancellationToken);
        if (resource is null)
        {
            return NotFound();
        }

        var fields = GetEditFieldNames();
        var prefix = ModelNames.CreateIndexModelName(InlineRowsPrefix, id);
        if (!await TryBindConfiguredFieldsAsync(resource, edit.ModelType, fields, prefix))
        {
            return await EditRowView(id, resource, cancellationToken);
        }

        var result = await UpdateAsync(id, resource, cancellationToken);
        if (result.IsNotFound)
        {
            return NotFound();
        }

        if (!result.IsSuccess)
        {
            AddValidationErrors(result, fields, prefix);
            return await EditRowView(id, resource, cancellationToken);
        }

        return await SavedRowView(id, cancellationToken);
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

    /// <summary>
    ///     Searches the items of a lookup field.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Lookup(
        [FromQuery] ResourceLookupQuery query,
        CancellationToken cancellationToken
    )
    {
        if (
            !ModelState.IsValid
            || query.Skip < 0
            || FindLookupField(query) is not ({ } options, { Items: { } items } editor)
        )
        {
            return NotFound();
        }

        var term = string.IsNullOrWhiteSpace(query.Term) ? null : query.Term.Trim();
        var labels = new LookupLabelContext(
            GetFieldLabel(options),
            editor.SheetOptions.MinimumSearchLength,
            term
        );
        if (query.SelectedOnly)
        {
            return await SelectedLookupResults(
                query,
                options,
                editor,
                term,
                labels,
                cancellationToken
            );
        }

        if ((term?.Length ?? 0) < editor.SheetOptions.MinimumSearchLength)
        {
            return PartialView(
                "_LookupResults",
                new LookupResultsViewModel(
                    editor,
                    [],
                    null,
                    query.Selected,
                    term,
                    true,
                    false,
                    false,
                    labels
                )
            );
        }

        var results = await items.SearchAsync(
            HttpContext.RequestServices,
            new LookupQuery(term, query.Skip, editor.SheetOptions.PageSize),
            cancellationToken
        );
        var moreUrl = results.HasMore
            ? Url.Action(
                nameof(Lookup),
                new
                {
                    id = (string?)null,
                    form = query.Form,
                    field = query.Field,
                    term,
                    skip = query.Skip + results.Items.Count,
                }
            )
            : null;

        // The picker sends the selected values with every request, so Load more marks those selected by then. Only the
        // first page reports an empty result, which offers New; a later page simply ends the list.
        var isEmpty = query.Skip == 0 && results.Items.Count == 0;

        return PartialView(
            "_LookupResults",
            new LookupResultsViewModel(
                editor,
                results.Items,
                moreUrl,
                query.Selected,
                term,
                false,
                isEmpty,
                false,
                labels,
                isEmpty ? await GetLookupCreateUrlAsync(options, editor, query) : null
            )
        );
    }

    /// <summary>
    ///     Renders a lookup field's display of the item with the specified value, for an item created from the field.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> LookupSelection(
        [FromQuery] ResourceLookupQuery query,
        [FromQuery] string? value,
        CancellationToken cancellationToken
    )
    {
        if (
            !ModelState.IsValid
            || string.IsNullOrEmpty(value)
            || FindLookupField(query)
                is not
                ({ } options, LookupSheetEditor { Items: { } items } editor)
            || !TryConvertKey(options, value, out var current)
        )
        {
            return NotFound();
        }

        var item = (
            await items.FindAsync(
                HttpContext.RequestServices,
                new FieldEditorContext(options.FieldName, CreateLookupModel(query), current),
                cancellationToken
            )
        ).FirstOrDefault();

        // Selected values are posted with the form, which binds them in the current culture
        return PartialView(
            "_LookupSelectionTemplate",
            new LookupSelectionTemplateViewModel(
                Convert.ToString(current, CultureInfo.CurrentCulture) ?? "",
                new LookupSelectionViewModel(
                    item,
                    editor.Layout,
                    editor.FieldOptions.ShowMedia,
                    false,
                    editor.ClassNames.Media
                )
            )
        );
    }

    /// <summary>
    ///     Renders the search panel of a lookup field for the shared sheet.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> LookupSheet([FromQuery] ResourceLookupQuery query)
    {
        if (
            !ModelState.IsValid
            || string.IsNullOrEmpty(query.For)
            || FindLookupField(query) is not ({ } options, { Items: not null } editor)
        )
        {
            return NotFound();
        }

        var label = GetFieldLabel(options);
        var resultsUrl = Url.Action(
            nameof(Lookup),
            new
            {
                id = (string?)null,
                form = query.Form,
                field = query.Field,
                @for = editor is MultiLookupSheetEditor ? query.For : null,
                level = query.Level > 0 ? query.Level : (int?)null,
            }
        )!;

        return PartialView(
            "_LookupSheet",
            new LookupSheetViewModel(
                editor,
                query.For,
                editor.SheetOptions.Title ?? label,
                resultsUrl,
                new LookupLabelContext(label, editor.SheetOptions.MinimumSearchLength, null),
                await GetLookupCreateUrlAsync(options, editor, query)
            )
        );
    }

    /// <summary>
    ///     Renders a multi-select lookup field's display of the items with the specified values, as they are selected in
    ///     its sheet.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> MultiLookupSelection(
        [FromQuery] ResourceLookupQuery query,
        CancellationToken cancellationToken
    )
    {
        if (
            !ModelState.IsValid
            || FindLookupField(query)
                is not
                ({ } options, MultiLookupSheetEditor { Items: { } items } editor)
            || !TryConvertSelected(options, query.Selected, out var values)
        )
        {
            return NotFound();
        }

        var selected = await items.FindAsync(
            HttpContext.RequestServices,
            new FieldEditorContext(options.FieldName, CreateLookupModel(query), values),
            cancellationToken
        );

        return PartialView(
            "_MultiLookupItems",
            new MultiLookupItemsViewModel(editor, selected, GetFieldLabel(options))
        );
    }

    /// <summary>
    ///     Renders a row detail panel that loads when it is first shown.
    /// </summary>
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

    /// <summary>
    ///     Displays the view page.
    /// </summary>
    [HttpGet]
    [ActionName(ViewActionName)]
    public async Task<IActionResult> Show(
        [FromRoute] string id,
        CancellationToken cancellationToken
    )
    {
        if (
            _resourceOptions.View is not { } view
            || _resourceOptions.KeySelector is null
            || string.IsNullOrEmpty(id)
        )
        {
            return NotFound();
        }

        var resource = _resourceOptions.ViewLoader is { } loader
            ? await loader(HttpContext.RequestServices, id, cancellationToken)
            : await ((IResourceViewHandler<TResource>)dataSource).FindAsync(id, cancellationToken);
        if (resource is null)
        {
            return NotFound();
        }

        AddDeleteErrors();

        var labels = CreateLabelContext();
        var fields = view.Fields;

        return ResourceView(
            ViewActionName,
            new ResourceViewPageViewModel
            {
                Delete = CreateDeleteDialog(id, "view", labels),
                DisplayData = await PrepareDisplaysAsync(fields, resource, cancellationToken),
                EditLabel = HasEditPage
                    ? _resourceOptions.Index.EditLabel ?? _labelOptions.Index.EditLabel(labels)
                    : null,
                Entity = resource,
                EntityId = id,
                Fields = fields,
                Items = view.Items.ToArray(),
                Columns = view.Columns,
                SectionLayout = view.SectionLayout,
                Title = view.Title ?? _labelOptions.View.Title(labels),
            }
        );
    }

    // The collection's elements, such as int for List<int>; a nullable element converts to its underlying type
    private static Type? GetElementType(Type collectionType)
    {
        var element = collectionType.IsArray
            ? collectionType.GetElementType()
            : collectionType
                .GetInterfaces()
                .Append(collectionType)
                .FirstOrDefault(type =>
                    type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                )
                ?.GetGenericArguments()[0];

        return element is null ? null : Nullable.GetUnderlyingType(element) ?? element;
    }

    // Keys are formatted in the invariant culture, and converted to the field's type for the lookup's items
    private static bool TryConvertKey(FormFieldOptions options, string key, out object? value)
    {
        var property = options.PropertyPath[^1];
        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        try
        {
            value = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(key);
            return true;
        }
        catch (Exception exception)
            when (exception is ArgumentException or FormatException or NotSupportedException)
        {
            value = null;
            return false;
        }
    }

    // Selected values are posted as the form posts them, in the current culture, and converted to an array of the
    // field's element type for the lookup's items
    private static bool TryConvertSelected(
        FormFieldOptions options,
        IReadOnlyList<string> keys,
        out Array values
    )
    {
        values = Array.Empty<object>();
        if (GetElementType(options.PropertyPath[^1].PropertyType) is not { } type)
        {
            return false;
        }

        var converter = TypeDescriptor.GetConverter(type);
        var converted = Array.CreateInstance(type, keys.Count);
        try
        {
            for (var index = 0; index < keys.Count; index++)
            {
                converted.SetValue(
                    converter.ConvertFromString(null, CultureInfo.CurrentCulture, keys[index]),
                    index
                );
            }
        }
        catch (Exception exception)
            when (exception is ArgumentException or FormatException or NotSupportedException)
        {
            return false;
        }

        values = converted;
        return true;
    }

    private void AddValidationErrors(
        ResourceOperationResult result,
        HashSet<string> fields,
        string prefix
    )
    {
        foreach (var error in result.Errors)
        {
            var key =
                error.FieldName is not null && fields.Contains(error.FieldName)
                    ? ModelNames.CreatePropertyModelName(prefix, error.FieldName)
                    : string.Empty;
            ModelState.AddModelError(key, error.Message);
        }
    }

    // A rejected delete from the edit or view page returns there with its errors
    private void AddDeleteErrors()
    {
        if (TempData[TempDataKeys.DeleteErrors] is string deleteErrors)
        {
            foreach (var message in JsonSerializer.Deserialize<string[]>(deleteErrors) ?? [])
            {
                ModelState.AddModelError(string.Empty, message);
            }
        }
    }

    private ResourceFormDeleteDialogViewModel? CreateDeleteDialog(
        string id,
        string origin,
        ResourceLabelContext labels
    ) =>
        _resourceOptions.Delete is { } delete
            ? new(
                delete.Title ?? _labelOptions.Delete.Title(labels),
                delete.Message ?? _labelOptions.Delete.Message(labels),
                delete.ConfirmLabel ?? _labelOptions.Delete.ConfirmLabel(labels),
                delete.CancelLabel ?? _labelOptions.Delete.CancelLabel(labels),
                id,
                origin
            )
            : null;

    private ResourceIndexPageViewModel<TResource> CreateIndexModel(
        IReadOnlyList<TResource> items,
        ResourceIndexQuery query,
        ResourceListRequest request,
        ResourceIndexPagingViewModel? pagingModel,
        bool rowsOnly = false,
        string? editingKey = null,
        object? editModel = null,
        object? newItem = null
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
            InlineRows = CreateInlineRows(labels, editingKey, editModel, newItem),
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
        object? newItem
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
            CancelLabel =
                _resourceOptions.Index.CancelLabel ?? _labelOptions.Index.CancelLabel(labels),
            Create = create,
            CreateFields = create is null ? null : GetColumnFields(_resourceOptions.Create!.Fields),
            Edit = edit,
            EditFields = edit is null ? null : GetColumnFields(_resourceOptions.Edit!.Fields),
            EditingKey = editingKey,
            EditModel = editModel,
            KeyField = keyField,
            NewItem = newItem,
            SaveLabel = _resourceOptions.Index.SaveLabel ?? _labelOptions.Index.SaveLabel(labels),
        };

        FormFieldOptions?[] GetColumnFields(IEnumerable<FormFieldOptions> fields)
        {
            var remaining = fields.Where(field => !field.IsReadOnly).ToList();

            return _resourceOptions
                .Index.Columns.Select(column =>
                {
                    var name = column.EditsFieldName ?? column.FieldName;
                    var field = remaining.FirstOrDefault(field => field.FieldName == name);
                    if (field is not null)
                    {
                        remaining.Remove(field);
                    }

                    return field;
                })
                .ToArray();
        }
    }

    private ResourceLabelContext CreateLabelContext() =>
        new(_resourceOptions.SingularLabel, _resourceOptions.PluralLabel);

    // Lookup items are found without the form's model, so a model that has never been bound stands in for it
    private object CreateLookupModel(ResourceLookupQuery query) =>
        query.Form!.Equals("create", StringComparison.OrdinalIgnoreCase)
            ? _resourceOptions.Create!.CreateModel()
            : RuntimeHelpers.GetUninitializedObject(_resourceOptions.Edit!.ModelType);

    private async Task<ResourceFormPageViewModel> CreateFormModelAsync(
        object resource,
        CancellationToken cancellationToken
    )
    {
        var create = _resourceOptions.Create!;
        var fields = create.Fields.ToArray();
        var labels = CreateLabelContext();
        var editors = await PrepareEditorsAsync(fields, resource, cancellationToken);
        var displayData = await PrepareDisplaysAsync(
            create.DisplayFields,
            resource,
            cancellationToken
        );

        return new ResourceFormPageViewModel
        {
            Entity = resource,
            Fields = fields,
            Items = create.Items.ToArray(),
            Columns = create.Columns,
            DisplayData = displayData,
            EditorData = editors.Data,
            EditorTemplates = editors.Templates,
            SectionLayout = create.SectionLayout,
            Title = create.Title ?? _labelOptions.Create.Title(labels),
            SubmitLabel = create.SubmitLabel ?? _labelOptions.Create.SubmitLabel(labels),
        };
    }

    private async Task<PartialViewResult> CreateSheetView(
        object resource,
        string lookup,
        int level,
        CancellationToken cancellationToken
    )
    {
        var form = await CreateFormModelAsync(resource, cancellationToken);
        var postUrl = Url.Action(
            nameof(CreateSheet),
            new
            {
                id = (string?)null,
                @for = lookup,
                level,
            }
        )!;

        return PartialView("_CreateSheet", new CreateSheetViewModel(form, lookup, level, postUrl));
    }

    private async Task<ViewResult> CreateView(object resource, CancellationToken cancellationToken)
    {
        return ResourceView(
            nameof(Create),
            await CreateFormModelAsync(resource, cancellationToken)
        );
    }

    private async Task<IActionResult> EditRowView(
        string id,
        object resource,
        CancellationToken cancellationToken
    )
    {
        var rows = await LoadRowsAsync(id, cancellationToken);

        return rows.Length == 0
            ? NotFound()
            : RowsView(
                CreateIndexModel(
                    rows,
                    new(),
                    new(),
                    null,
                    rowsOnly: true,
                    editingKey: id,
                    editModel: resource
                )
            );
    }

    private async Task<ViewResult> EditView(
        object resource,
        string id,
        CancellationToken cancellationToken
    )
    {
        var edit = _resourceOptions.Edit!;
        var labels = CreateLabelContext();
        var editors = await PrepareEditorsAsync(edit.Fields, resource, cancellationToken);
        var displayData = await PrepareDisplaysAsync(
            edit.DisplayFields,
            resource,
            cancellationToken
        );

        return ResourceView(
            nameof(Edit),
            new ResourceFormPageViewModel
            {
                Delete = CreateDeleteDialog(id, "edit", labels),
                Entity = resource,
                Fields = edit.Fields,
                Items = edit.Items.ToArray(),
                Columns = edit.Columns,
                DisplayData = displayData,
                EditorData = editors.Data,
                EditorTemplates = editors.Templates,
                SectionLayout = edit.SectionLayout,
                Title = edit.Title ?? _labelOptions.Edit.Title(labels),
                SubmitLabel = edit.SubmitLabel ?? _labelOptions.Edit.SubmitLabel(labels),
            }
        );
    }

    private (FormFieldOptions, ILookupSheetEditor)? FindLookupField(ResourceLookupQuery query)
    {
        var fields = query.Form?.ToLowerInvariant() switch
        {
            "create" => _resourceOptions.Create?.Fields,
            "edit" when _resourceOptions.KeySelector is not null => _resourceOptions.Edit?.Fields,
            _ => null,
        };

        return
            fields?.FirstOrDefault(options => options.FieldName == query.Field)
                is { IsReadOnly: false, Editor: ILookupSheetEditor editor } options
            ? (options, editor)
            : null;
    }

    // The edit fields that are bound from a form; the key of a resource edited as itself is not
    private HashSet<string> GetEditFieldNames() =>
        _resourceOptions
            .Edit!.Fields.Select(field => field.FieldName)
            .Where(name =>
                _resourceOptions.Edit.ModelType != typeof(TResource)
                || name != _resourceOptions.KeyPropertyName
            )
            .ToHashSet(StringComparer.Ordinal);

    private string GetFieldLabel(FormFieldOptions options)
    {
        var property = options.PropertyPath[^1];

        return options.Title
            ?? MetadataProvider
                .GetMetadataForProperty(property.DeclaringType!, property.Name)
                .GetDisplayName();
    }

    // A multi-select lookup's create form opens one level above the form that contains the field
    private async Task<string?> GetLookupCreateUrlAsync(
        FormFieldOptions options,
        ILookupSheetEditor editor,
        ResourceLookupQuery query
    )
    {
        if (
            editor is not MultiLookupSheetEditor { CreateEnabled: true, Items: { } items } multi
            || string.IsNullOrEmpty(query.For)
        )
        {
            return null;
        }

        var controller = await LookupCreateResource.FindControllerAsync(
            HttpContext.RequestServices,
            nameof(MultiLookupSheetEditor),
            options.FieldName,
            items,
            multi.CreateResourceType
        );

        return controller is null
            ? null
            : Url.Action(
                nameof(CreateSheet),
                controller,
                new
                {
                    id = (string?)null,
                    @for = query.For,
                    level = query.Level + 1,
                }
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

    private async Task<IReadOnlyDictionary<FormDisplayFieldOptions, object?>> PrepareDisplaysAsync(
        IEnumerable<FormDisplayFieldOptions> fields,
        object model,
        CancellationToken cancellationToken
    )
    {
        var data = new Dictionary<FormDisplayFieldOptions, object?>();
        foreach (var field in fields)
        {
            if (field.HandlerType is { } handlerType)
            {
                var handler = (IFieldDisplayHandler)
                    ActivatorUtilities.CreateInstance(
                        HttpContext.RequestServices,
                        handlerType,
                        field.Display!
                    );
                var context = new FieldDisplayContext(
                    field.FieldName,
                    model,
                    ResourcePropertyPath.GetValue(model, field.PropertyPath)
                );
                data[field] = await handler.PrepareAsync(context, cancellationToken);
            }
        }

        return data;
    }

    private async Task<(
        IReadOnlyDictionary<string, object?> Data,
        IReadOnlyDictionary<string, string> Templates
    )> PrepareEditorsAsync(
        IReadOnlyList<FormFieldOptions> fields,
        object model,
        CancellationToken cancellationToken
    )
    {
        var data = new Dictionary<string, object?>();
        var templates = new Dictionary<string, string>();
        foreach (var field in fields)
        {
            if (field.HandlerType is { } handlerType)
            {
                var handler = (IFieldEditorHandler)
                    ActivatorUtilities.CreateInstance(
                        HttpContext.RequestServices,
                        handlerType,
                        field.Editor
                    );
                var context = new FieldEditorContext(
                    field.FieldName,
                    model,
                    ResourcePropertyPath.GetValue(model, field.PropertyPath)
                );
                data[field.FieldName] = await handler.PrepareAsync(context, cancellationToken);
                templates[field.FieldName] = handler.TemplateName;
            }
        }

        return (data, templates);
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

        return ResourceView(
            nameof(Index),
            CreateIndexModel(result.Items, query, request, pagingModel)
        );
    }

    private async Task<object?> LoadEditModelAsync(
        string id,
        CancellationToken cancellationToken
    ) =>
        _resourceOptions.EditLoader is { } loader
            ? await loader(HttpContext.RequestServices, id, cancellationToken)
            : await ((IResourceEditHandler<TResource>)dataSource).FindAsync(id, cancellationToken);

    // A row as the index displays it, through the index query, whatever the scope or search
    private async Task<TResource[]> LoadRowsAsync(string key, CancellationToken cancellationToken)
    {
        var result = await dataSource.ListAsync(new() { Keys = [key] }, cancellationToken);

        return result
            .Items.Where(item => _resourceOptions.KeySelector!(item) == key)
            .Take(1)
            .ToArray();
    }

    private PartialViewResult NewRowView(object resource) =>
        RowsView(CreateIndexModel([], new(), new(), null, rowsOnly: true, newItem: resource));

    private ViewResult ResourceView(string action, object model)
    {
        var viewName = viewEngine.FindView(ControllerContext, action, isMainPage: true).Success
            ? action
            : "Resource" + action;

        return View(viewName, model);
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

    // Binds and saves the create form; the result is null when binding failed
    // The Selected view of a multi-select lookup: the selected items whose title or description contains the term,
    // in the order they were selected, without paging
    private async Task<IActionResult> SelectedLookupResults(
        ResourceLookupQuery query,
        FormFieldOptions options,
        ILookupSheetEditor editor,
        string? term,
        LookupLabelContext labels,
        CancellationToken cancellationToken
    )
    {
        if (
            editor is not MultiLookupSheetEditor
            || !TryConvertSelected(options, query.Selected, out var values)
        )
        {
            return NotFound();
        }

        var selected = await editor.Items!.FindAsync(
            HttpContext.RequestServices,
            new FieldEditorContext(options.FieldName, CreateLookupModel(query), values),
            cancellationToken
        );
        var matches = term is null
            ? selected
            : selected
                .Where(item =>
                    item.Text.Contains(term, StringComparison.CurrentCultureIgnoreCase)
                    || item.Description?.Contains(term, StringComparison.CurrentCultureIgnoreCase)
                        == true
                )
                .ToArray();

        return PartialView(
            "_LookupResults",
            new LookupResultsViewModel(
                editor,
                matches,
                null,
                query.Selected,
                term,
                false,
                matches.Count == 0,
                true,
                labels
            )
        );
    }

    private async Task<(object Resource, ResourceOperationResult? Result)> SubmitCreateAsync(
        string prefix,
        CancellationToken cancellationToken
    )
    {
        var create = _resourceOptions.Create!;
        var resource = create.CreateModel();
        var fields = create
            .Fields.Select(field => field.FieldName)
            .ToHashSet(StringComparer.Ordinal);

        if (!await TryBindConfiguredFieldsAsync(resource, create.ModelType, fields, prefix))
        {
            return (resource, null);
        }

        var result = _resourceOptions.CreateHandler is { } handler
            ? await handler(HttpContext.RequestServices, resource, cancellationToken)
            : await ((IResourceCreateHandler<TResource>)dataSource).CreateAsync(
                (TResource)resource,
                cancellationToken
            );
        if (!result.IsSuccess && !result.IsNotFound)
        {
            AddValidationErrors(result, fields, prefix);
        }

        return (resource, result);
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

    private async Task<bool> TryBindConfiguredFieldsAsync(
        object model,
        Type modelType,
        HashSet<string> fields,
        string prefix
    )
    {
        var propertyNames = fields
            .SelectMany(field => field.Split('.'))
            .ToHashSet(StringComparer.Ordinal);

        return await TryUpdateModelAsync(
            model,
            modelType,
            prefix,
            new ConfiguredFieldValueProvider(
                await CompositeValueProvider.CreateAsync(ControllerContext),
                fields,
                prefix
            ),
            metadata => propertyNames.Contains(metadata.PropertyName ?? "")
        );
    }

    private async Task<ResourceOperationResult> UpdateAsync(
        string id,
        object resource,
        CancellationToken cancellationToken
    ) =>
        _resourceOptions.EditHandler is { } handler
            ? await handler(HttpContext.RequestServices, id, resource, cancellationToken)
            : await ((IResourceEditHandler<TResource>)dataSource).UpdateAsync(
                id,
                (TResource)resource,
                cancellationToken
            );

    private sealed class ConfiguredFieldValueProvider(
        IValueProvider source,
        HashSet<string> fields,
        string bindingPrefix
    ) : IValueProvider
    {
        private readonly HashSet<string> _keys = fields
            .Select(field => ModelNames.CreatePropertyModelName(bindingPrefix, field))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _prefixes = CreatePrefixes(fields, bindingPrefix);

        public bool ContainsPrefix(string prefix) =>
            _prefixes.Contains(prefix) && source.ContainsPrefix(prefix);

        public ValueProviderResult GetValue(string key) =>
            _keys.Contains(key) ? source.GetValue(key) : ValueProviderResult.None;

        private static HashSet<string> CreatePrefixes(HashSet<string> fields, string bindingPrefix)
        {
            var prefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { bindingPrefix };
            foreach (var field in fields)
            {
                var prefix = bindingPrefix;
                foreach (var segment in field.Split('.'))
                {
                    prefix = ModelNames.CreatePropertyModelName(prefix, segment);
                    prefixes.Add(prefix);
                }
            }

            return prefixes;
        }
    }
}
