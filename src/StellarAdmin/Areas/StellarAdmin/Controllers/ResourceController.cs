using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using StellarAdmin.Areas.StellarAdmin.ViewModels;
using StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;
using StellarAdmin.UI.Icons;
using StellarAdmin.UI.Toasts;

namespace StellarAdmin.Areas.StellarAdmin.Controllers;

[Area("StellarAdmin")]
internal sealed class ResourceController<TResource>(
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

    // The create page's add another button posts after=add-another, and the next create page finds the kept fields'
    // posted values in TempData
    private const string AddAnotherAction = "add-another";
    private const string AddAnotherTempDataKey = "StellarAdminAddAnother";

    // An image upload's body is the file and the multipart form around it
    private const long ImageUploadOverhead = 64 * 1024;

    // The upload's response is read by the image upload's script, whatever the host's JSON settings
    private static readonly JsonSerializerOptions ImageUploadJsonOptions = new(
        JsonSerializerDefaults.Web
    );

    private readonly ResourceLabelOptions _labelOptions = labelOptions.Value;
    private readonly ResourceOptions<TResource> _resourceOptions = options.Value;

    // An inline operation has no page, so neither its links nor its routes are available
    private bool HasCreatePage => _resourceOptions.Create is { InlineRow: null };
    private bool HasEditPage =>
        _resourceOptions.Edit is { InlineRow: null } && _resourceOptions.KeySelector is not null;

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        if (_resourceOptions.Create is not { InlineRow: null } create)
        {
            return NotFound();
        }

        // After save and add another, the new model starts with the kept fields' values and the form with focus
        var resource = create.CreateModel();
        var addingAnother = false;
        if (create.AddAnother is not null && TempData[AddAnotherTempDataKey] is string kept)
        {
            addingAnother = true;
            var values = JsonSerializer.Deserialize<Dictionary<string, string?[]>>(kept)!;
            await BindKeptFieldsAsync(
                resource,
                new FormValueProvider(
                    BindingSource.Form,
                    new FormCollection(
                        values.ToDictionary(
                            entry => entry.Key,
                            entry => new StringValues(entry.Value)
                        )
                    ),
                    CultureInfo.CurrentCulture
                ),
                ResourceFormPageViewModel.BindingPrefix
            );
        }

        return await CreateView(resource, cancellationToken, addingAnother);
    }

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

        // Save and add another confirms the save with a toast and opens the create page again, with the kept fields'
        // posted values
        if (
            _resourceOptions.Create!.AddAnother is { } addAnother
            && Request.Form.TryGetValue("after", out var after)
            && after == AddAnotherAction
        )
        {
            var prefixes = addAnother
                .KeptFields.Select(field =>
                    ModelNames.CreatePropertyModelName(
                        ResourceFormPageViewModel.BindingPrefix,
                        field
                    )
                )
                .ToArray();
            var kept = Request
                .Form.Where(entry =>
                    prefixes.Any(prefix =>
                        entry.Key.Equals(prefix, StringComparison.OrdinalIgnoreCase)
                        || entry.Key.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase)
                        || entry.Key.StartsWith(prefix + "[", StringComparison.OrdinalIgnoreCase)
                    )
                )
                .ToDictionary(entry => entry.Key, entry => entry.Value.ToArray());
            TempData[AddAnotherTempDataKey] = JsonSerializer.Serialize(kept);
            HttpContext
                .RequestServices.GetRequiredService<IToastNotifier>()
                .Success(
                    _resourceOptions.Index.CreatedMessage
                        ?? _labelOptions.Index.CreatedMessage(CreateLabelContext())
                );

            return RedirectToAction(nameof(Create));
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult CreateRow([FromQuery] ResourceIndexQuery query)
    {
        return
            _resourceOptions.Create is { InlineRow: not null } create
            && _resourceOptions.KeySelector is not null
            ? NewRowView(create.CreateModel(), query)
            : NotFound();
    }

    [HttpPost]
    [ActionName(nameof(CreateRow))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRowPost(
        [FromQuery] ResourceIndexQuery query,
        [FromQuery] bool another,
        [FromQuery] int added,
        CancellationToken cancellationToken
    )
    {
        if (
            _resourceOptions.Create is not { InlineRow: { } inline }
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

        // While adding another, a new row counts the rows added before it, and a rejected row keeps the count
        var addedBefore = _resourceOptions.Create.AddAnother is null ? 0 : Math.Max(added, 0);
        if (result is not { IsSuccess: true })
        {
            return NewRowView(resource, query, addedBefore);
        }

        // A create model other than the resource has no key selector, so its handler returns the key
        var key = result.Key ?? (resource is TResource created ? keySelector(created) : null);
        var labels = CreateLabelContext();
        var message =
            _resourceOptions.Index.CreatedMessage ?? _labelOptions.Index.CreatedMessage(labels);
        if (another && _resourceOptions.Create.AddAnother is not null)
        {
            HttpContext.RequestServices.GetRequiredService<IToastNotifier>().Success(message);

            return await AddAnotherRowsView(key, query, addedBefore + 1, cancellationToken);
        }

        if (inline.AfterSave == InlineRowAfterSave.ReloadGrid)
        {
            return await ReloadedGridView(query, key, message, cancellationToken);
        }

        HttpContext.RequestServices.GetRequiredService<IToastNotifier>().Success(message);

        return await SavedRowView(key, cancellationToken);
    }

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
            _resourceOptions.Edit.Fields,
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

    [HttpGet]
    public async Task<IActionResult> EditRow(
        [FromRoute] string id,
        [FromQuery] ResourceIndexQuery query,
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

        return resource is null
            ? NotFound()
            : await EditRowView(id, resource, query, cancellationToken);
    }

    [HttpPost]
    [ActionName(nameof(EditRow))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRowPost(
        [FromRoute] string id,
        [FromQuery] ResourceIndexQuery query,
        CancellationToken cancellationToken
    )
    {
        if (
            _resourceOptions.Edit is not { InlineRow: { } inline } edit
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
        if (
            !await TryBindConfiguredFieldsAsync(
                resource,
                edit.ModelType,
                edit.Fields,
                fields,
                prefix
            )
        )
        {
            return await EditRowView(id, resource, query, cancellationToken);
        }

        var result = await UpdateAsync(id, resource, cancellationToken);
        if (result.IsNotFound)
        {
            return NotFound();
        }

        if (!result.IsSuccess)
        {
            AddValidationErrors(result, fields, prefix);
            return await EditRowView(id, resource, query, cancellationToken);
        }

        return inline.AfterSave == InlineRowAfterSave.ReloadGrid
            ? await ReloadedGridView(query, id, null, cancellationToken)
            : await SavedRowView(id, cancellationToken);
    }

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
            || !TryConvertKey(GetValueType(options, query), value, out var current)
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
            || !TryConvertSelected(GetValueType(options, query), query.Selected, out var values)
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

        var model = await loader.LoadAsync(HttpContext.RequestServices, id, cancellationToken);
        if (model is null)
        {
            // htmx swaps error responses, so the row shows that its details are gone
            Response.StatusCode = StatusCodes.Status404NotFound;
            return PartialView("_IndexRowDetailNotFound");
        }

        return PartialView(panelOptions.PartialName, model);
    }

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

    // No parameters, so model binding leaves the body unread until its limit is the field's
    [HttpPost]
    public async Task<IActionResult> UploadImage()
    {
        var cancellationToken = HttpContext.RequestAborted;
        string? fieldName = Request.Query["field"];
        if (FindImageField(Request.Query["form"], fieldName) is not { } editor)
        {
            return NotFound();
        }

        if (!Request.HasFormContentType)
        {
            return BadRequest();
        }

        // Room for the file and the multipart form around it
        var bodyLimit = editor.MaxFileSize + ImageUploadOverhead;
        if (
            HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>() is
            { IsReadOnly: false } bodySize
        )
        {
            bodySize.MaxRequestBodySize = bodyLimit;
        }

        IFormCollection form;
        try
        {
            form = await Request.ReadFormAsync(
                new FormOptions { MultipartBodyLengthLimit = bodyLimit },
                cancellationToken
            );
        }
        catch (Exception exception)
            when (exception is InvalidDataException or BadHttpRequestException)
        {
            return ImageUploadError(
                StatusCodes.Status413PayloadTooLarge,
                $"The image is larger than the {FormatFileSize(editor.MaxFileSize)} limit."
            );
        }

        try
        {
            await HttpContext
                .RequestServices.GetRequiredService<IAntiforgery>()
                .ValidateRequestAsync(HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            return BadRequest();
        }

        if (form.Files.GetFile("file") is not { Length: > 0 } file)
        {
            return ImageUploadError(StatusCodes.Status400BadRequest, "Choose an image to upload.");
        }

        var fileName = Path.GetFileName(file.FileName);
        if (file.Length > editor.MaxFileSize)
        {
            return ImageUploadError(
                StatusCodes.Status413PayloadTooLarge,
                $"{fileName} is {FormatFileSize(file.Length)}. The limit is {FormatFileSize(editor.MaxFileSize)}."
            );
        }

        // A known format is recognised by its content; any other content goes by its declared type
        string? detectedType;
        await using (var stream = file.OpenReadStream())
        {
            detectedType = await ImageFileType.DetectAsync(stream, cancellationToken);
        }

        var type =
            detectedType
            ?? (
                ImageFileType.KnownTypes.Contains(
                    file.ContentType,
                    StringComparer.OrdinalIgnoreCase
                )
                    ? null
                    : file.ContentType
            );
        if (type is null || !editor.AcceptedTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
        {
            return ImageUploadError(
                StatusCodes.Status415UnsupportedMediaType,
                $"{fileName} can't be used. Choose a different type of image."
            );
        }

        var services = HttpContext.RequestServices;
        var value = await editor.StoreImageAsync!(file, services, cancellationToken);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"StoreImage for the image field {fieldName} returned no value."
            );
        }

        var url = await editor.ResolveUrlAsync(value, services, cancellationToken);

        return Json(new { value, url }, ImageUploadJsonOptions);
    }

    // A size as the image upload's script writes it, such as 5 MB or 640 KB
    private static string FormatFileSize(long bytes) =>
        bytes >= 1024 * 1024
            ? $"{Math.Round(bytes / (1024d * 1024), 1).ToString(CultureInfo.InvariantCulture)} MB"
            : $"{Math.Max(1, (long)Math.Round(bytes / 1024d)).ToString(CultureInfo.InvariantCulture)} KB";

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

    // Keys are formatted in the invariant culture, and converted to the field's value type for the lookup's items
    private static bool TryConvertKey(Type? valueType, string key, out object? value)
    {
        value = null;
        if (valueType is null)
        {
            return false;
        }

        var type = Nullable.GetUnderlyingType(valueType) ?? valueType;
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
    // element type of the field's value for the lookup's items
    private static bool TryConvertSelected(
        Type? valueType,
        IReadOnlyList<string> keys,
        out Array values
    )
    {
        values = Array.Empty<object>();
        if (valueType is null || GetElementType(valueType) is not { } type)
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

    // After save and add another, the saved row and a new row with the kept fields' values. A new row at the top stays
    // above the rows it added, and one at the bottom below them. The saved row is named for the script to mark and flash
    private async Task<IActionResult> AddAnotherRowsView(
        string? key,
        ResourceIndexQuery query,
        int added,
        CancellationToken cancellationToken
    )
    {
        var next = _resourceOptions.Create!.CreateModel();
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

    // Sets the kept fields of a new model from the values posted for the previous one. A value that doesn't bind is left
    // out, and the form shows no errors for them
    private async Task BindKeptFieldsAsync(object model, IValueProvider source, string prefix)
    {
        var create = _resourceOptions.Create!;
        var boundFields = create
            .Fields.Where(field =>
                field.Binder is not null && create.AddAnother!.KeptFields.Contains(field.FieldName)
            )
            .ToArray();
        var fields = create
            .AddAnother!.KeptFields.Except(boundFields.Select(field => field.FieldName))
            .ToHashSet(StringComparer.Ordinal);
        if (fields.Count > 0)
        {
            var propertyNames = fields
                .SelectMany(field => field.Split('.'))
                .ToHashSet(StringComparer.Ordinal);
            await TryUpdateModelAsync(
                model,
                create.ModelType,
                prefix,
                new ConfiguredFieldValueProvider(source, fields, prefix),
                metadata => propertyNames.Contains(metadata.PropertyName ?? "")
            );
        }

        await BindWithBindersAsync(model, boundFields, source, prefix);
        ModelState.Clear();
    }

    // The posted values stay in the model state, so an editor shows them again when the form has errors
    private async Task BindWithBindersAsync(
        object model,
        IEnumerable<FormFieldOptions> fields,
        IValueProvider source,
        string prefix
    )
    {
        foreach (var field in fields)
        {
            var key = ModelNames.CreatePropertyModelName(prefix, field.FieldName);
            var values = source.GetValue(key).Values;
            ModelState.SetModelValue(key, values, values.ToString());

            var context = new FieldBindingContext(
                field,
                model,
                values,
                HttpContext.RequestServices
            );
            await field.Binder!.BindAsync(context, HttpContext.RequestAborted);
            var errors =
                context.Errors.Count > 0 ? context.Errors : ValidateBoundField(field, model);
            foreach (var error in errors)
            {
                ModelState.AddModelError(key, error);
            }

            if (errors.Count == 0)
            {
                ModelState.MarkFieldValid(key);
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

    private async Task<ViewResult> CreateView(
        object resource,
        CancellationToken cancellationToken,
        bool focusFirstField = false
    )
    {
        // The create sheet of a lookup has no add another button: it closes and selects what it created
        var model = await CreateFormModelAsync(resource, cancellationToken);
        model.AddAnotherLabel = _resourceOptions.Create!.AddAnother is { } addAnother
            ? addAnother.Label ?? _labelOptions.Create.AddAnotherLabel(CreateLabelContext())
            : null;
        model.FocusFirstField = focusFirstField;

        return ResourceView(nameof(Create), model);
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

    // The editable image field of the create or edit form, which must store uploads
    private ImageEditor? FindImageField(string? form, string? fieldName)
    {
        var fields = form?.ToLowerInvariant() switch
        {
            "create" => _resourceOptions.Create?.Fields,
            "edit" when _resourceOptions.KeySelector is not null => _resourceOptions.Edit?.Fields,
            _ => null,
        };

        return
            fields?.FirstOrDefault(options => options.FieldName == fieldName)
                is { IsReadOnly: false, Editor: ImageEditor { HasStore: true } editor }
            ? editor
            : null;
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

    // The type of the value a lookup field's editor edits: the binder's value, or else the property's
    private Type? GetValueType(FormFieldOptions options, ResourceLookupQuery query) =>
        options.Binder is null
            ? options.PropertyPath[^1].PropertyType
            : options.GetValue(CreateLookupModel(query), HttpContext.RequestServices)?.GetType();

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
                var context = new FieldDisplayContext(field.Key, model, field.GetValue(model));
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
            if (field.Editor is { } editor)
            {
                templates[field.FieldName] = editor.TemplateName;
            }

            if (field.HandlerType is { } handlerType)
            {
                var handler = (IFieldEditorHandler)
                    ActivatorUtilities.CreateInstance(
                        HttpContext.RequestServices,
                        handlerType,
                        field.Editor!
                    );
                var context = new FieldEditorContext(
                    field.FieldName,
                    model,
                    field.GetValue(model, HttpContext.RequestServices)
                );
                data[field.FieldName] = await handler.PrepareAsync(context, cancellationToken);
            }
        }

        return (data, templates);
    }

    private JsonResult ImageUploadError(int statusCode, string error)
    {
        var result = Json(new { error }, ImageUploadJsonOptions);
        result.StatusCode = statusCode;

        return result;
    }

    private async Task<IActionResult> IndexView(
        ResourceIndexQuery query,
        ResourceListRequest request,
        CancellationToken cancellationToken,
        bool redirectOutOfRange = true,
        Action<IReadOnlyList<TResource>>? loaded = null
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

    private ViewResult ResourceView(string action, object model)
    {
        var viewName = viewEngine.FindView(ControllerContext, action, isMainPage: true).Success
            ? action
            : "Resource" + action;

        return View(viewName, model);
    }

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
            loaded: items => shown = items.Any(item => _resourceOptions.KeySelector!(item) == key)
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
            || !TryConvertSelected(GetValueType(options, query), query.Selected, out var values)
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
                    item.Title.Contains(term, StringComparison.CurrentCultureIgnoreCase)
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

        if (
            !await TryBindConfiguredFieldsAsync(
                resource,
                create.ModelType,
                create.Fields,
                fields,
                prefix
            )
        )
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

    private async Task<bool> TryBindConfiguredFieldsAsync(
        object model,
        Type modelType,
        IReadOnlyList<FormFieldOptions> formFields,
        HashSet<string> fields,
        string prefix
    )
    {
        // A field with a binder is bound by it, after MVC binds the others
        var boundFields = formFields
            .Where(field => field.Binder is not null && fields.Contains(field.FieldName))
            .ToArray();
        fields = fields
            .Except(boundFields.Select(field => field.FieldName), StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);
        var propertyNames = fields
            .SelectMany(field => field.Split('.'))
            .ToHashSet(StringComparer.Ordinal);

        // An icon field's stored value, which saves unchanged even when it isn't an icon the field offers
        var iconFields = formFields
            .Where(field =>
                field is { IsReadOnly: false, Editor: IconEditor }
                && fields.Contains(field.FieldName)
            )
            .Select(field =>
                (field, ResourcePropertyPath.GetValue(model, field.PropertyPath) as string)
            )
            .ToArray();

        var source = await CompositeValueProvider.CreateAsync(ControllerContext);
        if (
            !await TryUpdateModelAsync(
                model,
                modelType,
                prefix,
                new ConfiguredFieldValueProvider(source, fields, prefix),
                metadata => propertyNames.Contains(metadata.PropertyName ?? "")
            )
        )
        {
            ScopeValidationErrors(fields, prefix);
        }

        ValidateIconFields(model, iconFields, prefix);
        await BindWithBindersAsync(model, boundFields, source, prefix);

        return ModelState.IsValid;
    }

    // Model validation runs before the binders, so the attributes of a bound field's property, such as [MinLength],
    // validate the value it bound
    private IReadOnlyList<string> ValidateBoundField(FormFieldOptions field, object model)
    {
        var property = field.PropertyPath[^1];
        var container = ResourcePropertyPath.GetValue(model, field.PropertyPath.SkipLast(1));
        if (container is null)
        {
            return [];
        }

        var context = new ValidationContext(container)
        {
            DisplayName =
                field.Title
                ?? MetadataProvider
                    .GetMetadataForProperty(property.DeclaringType!, property.Name)
                    .GetDisplayName(),
            MemberName = property.Name,
        };
        var value = property.GetValue(container);

        return property
            .GetCustomAttributes<ValidationAttribute>(inherit: true)
            .Select(attribute => attribute.GetValidationResult(value, context)?.ErrorMessage)
            .OfType<string>()
            .ToArray();
    }

    // A new value must be an icon the field offers: one of its list, or any registered icon
    private void ValidateIconFields(
        object model,
        (FormFieldOptions Field, string? Stored)[] iconFields,
        string prefix
    )
    {
        if (iconFields.Length == 0)
        {
            return;
        }

        var icons = HttpContext.RequestServices.GetRequiredService<IOptions<IconOptions>>().Value;
        foreach (var (field, stored) in iconFields)
        {
            var key = ModelNames.CreatePropertyModelName(prefix, field.FieldName);
            if (
                ModelState.GetFieldValidationState(key) == ModelValidationState.Invalid
                || ResourcePropertyPath.GetValue(model, field.PropertyPath)
                    is not string { Length: > 0 } value
                || value == stored
            )
            {
                continue;
            }

            var offered = ((IconEditor)field.Editor!).Icons is { Count: > 0 } list
                ? list.Contains(value, StringComparer.OrdinalIgnoreCase)
                : icons.TryGetIcon(value, out _);
            if (!offered)
            {
                ModelState.AddModelError(key, "Choose an icon from the list.");
            }
        }
    }

    // Validation checks the whole model, but the form only sets its configured fields. The errors of
    // other properties are dropped, such as a required navigation whose foreign key is a field, and
    // the data source still rejects what it can't save. An error no field displays, such as the
    // model's own, moves to the summary so a submission never fails without a message.
    private void ScopeValidationErrors(HashSet<string> fields, string prefix)
    {
        var fieldKeys = fields
            .Select(field => ModelNames.CreatePropertyModelName(prefix, field))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var unshown = new List<string>();
        foreach (var (key, entry) in ModelState.ToArray())
        {
            if (entry.Errors.Count == 0 || key.Length == 0 || fieldKeys.Contains(key))
            {
                continue;
            }

            var withinField = fieldKeys.Any(fieldKey =>
                key.StartsWith(fieldKey + ".", StringComparison.OrdinalIgnoreCase)
                || key.StartsWith(fieldKey + "[", StringComparison.OrdinalIgnoreCase)
            );
            var otherProperty =
                !withinField
                && (
                    prefix.Length == 0
                    || key.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase)
                );
            if (!otherProperty)
            {
                unshown.AddRange(
                    entry.Errors.Select(error =>
                        string.IsNullOrEmpty(error.ErrorMessage)
                            ? $"The value for {key} is invalid."
                            : error.ErrorMessage
                    )
                );
            }

            entry.Errors.Clear();
            entry.ValidationState = ModelValidationState.Valid;
        }

        foreach (var message in unshown.Distinct())
        {
            ModelState.AddModelError(string.Empty, message);
        }
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
