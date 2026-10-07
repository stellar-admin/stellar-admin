using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels;
using StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels.Internal;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Editors;
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
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        return _resourceOptions.Create is { } create
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
        if (_resourceOptions.Create is null)
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
    ///     Displays the create form in the shared sheet, for a lookup field that creates its item.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CreateSheet(
        [FromQuery(Name = "for")] string? lookup,
        CancellationToken cancellationToken
    )
    {
        return _resourceOptions.Create is { } create && !string.IsNullOrEmpty(lookup)
            ? await CreateSheetView(create.CreateModel(), lookup, cancellationToken)
            : NotFound();
    }

    /// <summary>
    ///     Creates a resource from the create form in the shared sheet, and returns its key to the lookup field.
    /// </summary>
    [HttpPost]
    [ActionName(nameof(CreateSheet))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSheetPost(
        [FromQuery(Name = "for")] string? lookup,
        CancellationToken cancellationToken
    )
    {
        if (_resourceOptions.Create is null || string.IsNullOrEmpty(lookup))
        {
            return NotFound();
        }

        var (resource, result) = await SubmitCreateAsync(
            CreateSheetViewModel.BindingPrefix,
            cancellationToken
        );
        if (result is { IsNotFound: true })
        {
            return NotFound();
        }

        if (result is not { IsSuccess: true })
        {
            return await CreateSheetView(resource, lookup, cancellationToken);
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
            if (origin == "edit")
            {
                TempData[TempDataKeys.DeleteErrors] = JsonSerializer.Serialize(
                    result.Errors.Select(error => error.Message)
                );
                return RedirectToAction(nameof(Edit), new { id });
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

        if (TempData[TempDataKeys.DeleteErrors] is string deleteErrors)
        {
            foreach (var message in JsonSerializer.Deserialize<string[]>(deleteErrors) ?? [])
            {
                ModelState.AddModelError(string.Empty, message);
            }
        }

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
            AddValidationErrors(result, fields, ResourceFormPageViewModel.BindingPrefix);
            return await EditView(resource, id, cancellationToken);
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
                    selected = query.Selected,
                }
            )
            : null;

        // Only the first page reports an empty result; a later page simply ends the list
        return PartialView(
            "_LookupResults",
            new LookupResultsViewModel(
                editor,
                results.Items,
                moreUrl,
                query.Selected,
                term,
                false,
                query.Skip == 0 && results.Items.Count == 0,
                labels
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
            || FindLookupField(query) is not ({ } options, { Items: { } items } editor)
            || !TryConvertKey(options, value, out var current)
        )
        {
            return NotFound();
        }

        // The new item is shown without the form's model, so a model that has never been bound stands in for it
        var model = query.Form!.Equals("create", StringComparison.OrdinalIgnoreCase)
            ? _resourceOptions.Create!.CreateModel()
            : RuntimeHelpers.GetUninitializedObject(_resourceOptions.Edit!.ModelType);
        var item = await items.FindAsync(
            HttpContext.RequestServices,
            new FieldEditorContext(options.FieldName, model, current),
            cancellationToken
        );

        // Selected values are posted with the form, which binds them in the current culture
        return PartialView(
            "_LookupSelectionTemplate",
            new LookupSelectionTemplateViewModel(
                Convert.ToString(current, CultureInfo.CurrentCulture) ?? "",
                new LookupSelectionViewModel(
                    item,
                    editor.Layout,
                    editor.FieldOptions.ShowMedia,
                    false
                )
            )
        );
    }

    /// <summary>
    ///     Renders the search panel of a lookup field for the shared sheet.
    /// </summary>
    [HttpGet]
    public IActionResult LookupSheet([FromQuery] ResourceLookupQuery query)
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
            }
        )!;

        return PartialView(
            "_LookupSheet",
            new LookupSheetViewModel(
                editor,
                query.For,
                editor.SheetOptions.Title ?? label,
                resultsUrl,
                new LookupLabelContext(label, editor.SheetOptions.MinimumSearchLength, null)
            )
        );
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

    private ResourceLabelContext CreateLabelContext() =>
        new(_resourceOptions.SingularLabel, _resourceOptions.PluralLabel);

    private async Task<ResourceFormPageViewModel> CreateFormModelAsync(
        object resource,
        CancellationToken cancellationToken
    )
    {
        var create = _resourceOptions.Create!;
        var fields = create.Fields.ToArray();
        var labels = CreateLabelContext();
        var editors = await PrepareEditorsAsync(fields, resource, cancellationToken);

        return new ResourceFormPageViewModel
        {
            Entity = resource,
            Fields = fields,
            Items = create.Items.ToArray(),
            Columns = create.Columns,
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
        CancellationToken cancellationToken
    )
    {
        var form = await CreateFormModelAsync(resource, cancellationToken);
        var postUrl = Url.Action(nameof(CreateSheet), new { id = (string?)null, @for = lookup })!;

        return PartialView("_CreateSheet", new CreateSheetViewModel(form, lookup, postUrl));
    }

    private async Task<ViewResult> CreateView(object resource, CancellationToken cancellationToken)
    {
        return ResourceView(
            nameof(Create),
            await CreateFormModelAsync(resource, cancellationToken)
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

        return ResourceView(
            nameof(Edit),
            new ResourceFormPageViewModel
            {
                Delete = _resourceOptions.Delete is { } delete
                    ? new(
                        delete.Title ?? _labelOptions.Delete.Title(labels),
                        delete.Message ?? _labelOptions.Delete.Message(labels),
                        delete.ConfirmLabel ?? _labelOptions.Delete.ConfirmLabel(labels),
                        delete.CancelLabel ?? _labelOptions.Delete.CancelLabel(labels),
                        id
                    )
                    : null,
                Entity = resource,
                Fields = edit.Fields,
                Items = edit.Items.ToArray(),
                Columns = edit.Columns,
                EditorData = editors.Data,
                EditorTemplates = editors.Templates,
                SectionLayout = edit.SectionLayout,
                Title = edit.Title ?? _labelOptions.Edit.Title(labels),
                SubmitLabel = edit.SubmitLabel ?? _labelOptions.Edit.SubmitLabel(labels),
            }
        );
    }

    private (FormFieldOptions, LookupEditor)? FindLookupField(ResourceLookupQuery query)
    {
        var fields = query.Form?.ToLowerInvariant() switch
        {
            "create" => _resourceOptions.Create?.Fields,
            "edit" when _resourceOptions.KeySelector is not null => _resourceOptions.Edit?.Fields,
            _ => null,
        };

        return
            fields?.FirstOrDefault(options => options.FieldName == query.Field)
                is { IsReadOnly: false, Editor: LookupEditor editor } options
            ? (options, editor)
            : null;
    }

    private string GetFieldLabel(FormFieldOptions options)
    {
        var property = options.PropertyPath[^1];

        return options.Title
            ?? MetadataProvider
                .GetMetadataForProperty(property.DeclaringType!, property.Name)
                .GetDisplayName();
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
                EditLabel =
                    _resourceOptions.Index.EditLabel ?? _labelOptions.Index.EditLabel(labels),
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

    // Binds and saves the create form; the result is null when binding failed
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
