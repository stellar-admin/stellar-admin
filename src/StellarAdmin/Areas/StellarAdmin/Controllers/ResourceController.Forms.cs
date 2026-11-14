using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using StellarAdmin.Areas.StellarAdmin.ViewModels;
using StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI.Toasts;
using StellarAdmin.Widgets;

namespace StellarAdmin.Areas.StellarAdmin.Controllers;

internal sealed partial class ResourceController<TResource>
{
    // The create page's add another button posts after=add-another, and the next create page finds the kept fields'
    // posted values in TempData
    private const string AddAnotherAction = "add-another";
    private const string AddAnotherTempDataKey = "StellarAdminAddAnother";

    // Shown when a create or update handler throws
    private const string SaveFailedMessage = "This record couldn't be saved. Try again.";

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        if (_resourceOptions.Create is not { InlineRow: null } create)
        {
            return NotFound();
        }

        // After save and add another, the new model starts with the kept fields' values and the form with focus
        var resource = await create.CreateModelAsync(
            HttpContext.RequestServices,
            cancellationToken
        );
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
    public async Task<IActionResult> CreateRow(
        [FromQuery] ResourceIndexQuery query,
        CancellationToken cancellationToken
    )
    {
        return
            _resourceOptions.Create is { InlineRow: not null } create
            && _resourceOptions.KeySelector is not null
            ? NewRowView(
                await create.CreateModelAsync(HttpContext.RequestServices, cancellationToken),
                query
            )
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
            ? await CreateSheetView(
                await create.CreateModelAsync(HttpContext.RequestServices, cancellationToken),
                lookup,
                level,
                cancellationToken
            )
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

    // When a field changes, the fields that depend on it, directly or through other dependents, as the form or the
    // grid row renders them. The form posts its parents' values; the dependents' own values are cleared.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Dependents(
        [FromRoute] string? id,
        [FromQuery] string? form,
        [FromQuery] string? prefix,
        [FromQuery] int? level,
        [FromQuery] string? layout,
        [FromForm] string? changed,
        CancellationToken cancellationToken
    )
    {
        if (
            FindForm(form) is not { } formOptions
            || prefix is null
            || string.IsNullOrEmpty(changed)
            || FieldDependencies.FindDependents(formOptions.Fields, changed)
                is not { Length: > 0 } dependents
        )
        {
            return NotFound();
        }

        var model = CreateFormModel(form!, id);
        await BindParentsAsync(model, formOptions, dependents);
        var editors = await PrepareEditorsAsync(dependents, model, cancellationToken);

        ViewData.TemplateInfo.HtmlFieldPrefix = prefix;
        ViewData[ViewDataKeys.DependentFields] = dependents;
        ViewData[ViewDataKeys.DependentFieldsInGrid] = layout == "grid";
        ViewData[ViewDataKeys.EditorData] = editors.Data;
        ViewData[ViewDataKeys.EditorTemplates] = editors.Templates;
        ViewData[ViewDataKeys.FormFields] = formOptions.Fields;
        ViewData[ViewDataKeys.LookupForm] = form!.ToLowerInvariant();
        if (level > 0)
        {
            ViewData[ViewDataKeys.SheetLevel] = level;
        }

        return PartialView("_DependentFields", model);
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

        return await EditView(resource, id, FindRecordTitle(resource), cancellationToken);
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

        // The stored record's title, before binding changes the record to the values that weren't saved
        var recordTitle = FindRecordTitle(resource);
        var fields = GetEditFieldNames(resource);
        var valid = await TryBindConfiguredFieldsAsync(
            resource,
            _resourceOptions.Edit.ModelType,
            _resourceOptions.Edit.Fields,
            fields,
            ResourceFormPageViewModel.BindingPrefix
        );
        if (!valid)
        {
            return await EditView(resource, id, recordTitle, cancellationToken);
        }

        var result = await UpdateAsync(id, resource, cancellationToken);
        if (result.IsNotFound)
        {
            return NotFound();
        }

        if (!result.IsSuccess)
        {
            AddValidationErrors(
                result,
                fields,
                _resourceOptions.Edit.Fields,
                ResourceFormPageViewModel.BindingPrefix
            );
            return await EditView(resource, id, recordTitle, cancellationToken);
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

        var fields = GetEditFieldNames(resource);
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
            AddValidationErrors(result, fields, [], prefix);
            return await EditRowView(id, resource, query, cancellationToken);
        }

        return inline.AfterSave == InlineRowAfterSave.ReloadGrid
            ? await ReloadedGridView(query, id, null, cancellationToken)
            : await SavedRowView(id, cancellationToken);
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
        ViewData[ViewDataKeys.Widgets] = await RenderWidgetsAsync(
            view.Widgets,
            CreateWidgetContext(typeof(RecordWidgetContext<>), view.ModelType, id, resource),
            WidgetPageView,
            cancellationToken
        );

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
                Title = view.Title ?? FindRecordTitle(resource) ?? _labelOptions.View.Title(labels),
            }
        );
    }

    // The record's title for its view or edit page, or null when the page shows another model, the resource has no
    // title but its key, or the title is empty or reads a navigation property the record didn't load
    private string? FindRecordTitle(object record)
    {
        if (
            FindResource(record, _resourceOptions.Edit) is not { } resource
            || _resourceOptions.PageTitle is not { } title
        )
        {
            return null;
        }

        try
        {
            return title(resource) is { } text && !string.IsNullOrWhiteSpace(text) ? text : null;
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }

    // The resource a form's model is, or holds in the property of a model that wraps it
    private static TResource? FindResource(object model, ResourceFormOptions? form) =>
        model is TResource resource ? resource
        : form is { ResourcePropertyName: { } name }
        && FormModelType(form) == model.GetType()
        && model.GetType().GetProperty(name)?.GetValue(model) is TResource wrapped
            ? wrapped
        : default;

    private static Type? FormModelType(ResourceFormOptions form) =>
        form switch
        {
            ResourceCreateOptions create => create.ModelType,
            ResourceEditOptions edit => edit.ModelType,
            _ => null,
        };

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
        ViewData[ViewDataKeys.Widgets] = await RenderWidgetsAsync(
            _resourceOptions.Create.Widgets,
            CreateWidgetContext(typeof(CreateWidgetContext<>), _resourceOptions.Create.ModelType),
            WidgetPageCreate,
            cancellationToken
        );

        return ResourceView(nameof(Create), model);
    }

    private async Task<ViewResult> EditView(
        object resource,
        string id,
        string? recordTitle,
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
        ViewData[ViewDataKeys.Widgets] = await RenderWidgetsAsync(
            edit.Widgets,
            CreateWidgetContext(typeof(RecordWidgetContext<>), edit.ModelType, id, resource),
            WidgetPageEdit,
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
                Title = edit.Title ?? recordTitle ?? _labelOptions.Edit.Title(labels),
                SubmitLabel = edit.SubmitLabel ?? _labelOptions.Edit.SubmitLabel(labels),
            }
        );
    }

    // The fields a form binds for a model, decided against it before binding: not those read-only or disabled for it
    private HashSet<string> GetEditableFieldNames(
        IEnumerable<FormFieldOptions> fields,
        object model
    ) =>
        fields
            .Where(field => FieldStates.IsEditable(field, model, MetadataProvider))
            .Select(field => field.FieldName)
            .ToHashSet(StringComparer.Ordinal);

    // The edit fields that are bound for the record as it loaded; the key of a resource edited as itself is not
    private HashSet<string> GetEditFieldNames(object resource)
    {
        var fields = GetEditableFieldNames(_resourceOptions.Edit!.Fields, resource);
        if (
            _resourceOptions.Edit.ModelType == typeof(TResource)
            && _resourceOptions.KeyPropertyName is { } key
        )
        {
            fields.Remove(key);
        }

        return fields;
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

            // A field waiting for a field its items depend on has no items to load. A configuration mistake found while
            // preparing is reported as the field renders.
            if (
                field.HandlerType is { } handlerType
                && FieldDependencies.GetWaitingText(
                    field,
                    model,
                    HttpContext.RequestServices,
                    MetadataProvider
                )
                    is null
            )
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
                try
                {
                    data[field.FieldName] = await handler.PrepareAsync(context, cancellationToken);
                }
                catch (ConfigurationException exception)
                {
                    data[field.FieldName] = exception;
                }
            }
        }

        return (data, templates);
    }

    private async Task<object?> LoadEditModelAsync(
        string id,
        CancellationToken cancellationToken
    ) =>
        _resourceOptions.EditLoader is { } loader
            ? await loader(HttpContext.RequestServices, id, cancellationToken)
            : await ((IResourceEditHandler<TResource>)dataSource).FindAsync(id, cancellationToken);

    // Binds and saves the create form; the result is null when binding failed
    private async Task<(object Resource, ResourceOperationResult? Result)> SubmitCreateAsync(
        string prefix,
        CancellationToken cancellationToken
    )
    {
        var create = _resourceOptions.Create!;
        var resource = create.CreateModel();
        var fields = GetEditableFieldNames(create.Fields, resource);

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

        var result = await RunHandlerAsync(
            () =>
                _resourceOptions.CreateHandler is { } handler
                    ? handler(HttpContext.RequestServices, resource, cancellationToken)
                    : ((IResourceCreateHandler<TResource>)dataSource).CreateAsync(
                        (TResource)resource,
                        cancellationToken
                    ),
            "create",
            SaveFailedMessage,
            cancellationToken
        );
        // An inline row shows a list or dictionary field's value, not its rows
        if (!result.IsSuccess && !result.IsNotFound)
        {
            AddValidationErrors(
                result,
                fields,
                create.InlineRow is null ? create.Fields : [],
                prefix
            );
        }

        return (resource, result);
    }

    private Task<ResourceOperationResult> UpdateAsync(
        string id,
        object resource,
        CancellationToken cancellationToken
    ) =>
        RunHandlerAsync(
            () =>
                _resourceOptions.EditHandler is { } handler
                    ? handler(HttpContext.RequestServices, id, resource, cancellationToken)
                    : ((IResourceEditHandler<TResource>)dataSource).UpdateAsync(
                        id,
                        (TResource)resource,
                        cancellationToken
                    ),
            "update",
            SaveFailedMessage,
            cancellationToken
        );
}
