using System.ComponentModel;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using StellarAdmin.Areas.StellarAdmin.ViewModels;
using StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin.Controllers;

internal sealed partial class ResourceController<TResource>
{
    [HttpGet]
    public async Task<IActionResult> Lookup(
        [FromQuery] ResourceLookupQuery query,
        CancellationToken cancellationToken
    )
    {
        if (
            !ModelState.IsValid
            || query.Skip < 0
            || FindLookupField(query) is not ({ } options, { } editor, { } items)
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
                items,
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
                    items,
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

        // The items load with the values of the fields they depend on, and only once those all have one
        var model = await CreateLookupModelAsync(options, query);
        if (
            FieldDependencies.GetWaitingText(
                options,
                model,
                HttpContext.RequestServices,
                MetadataProvider
            )
            is not null
        )
        {
            return NotFound();
        }

        var results = await items.SearchAsync(
            HttpContext.RequestServices,
            new LookupQuery(term, query.Skip, editor.SheetOptions.PageSize) { Model = model },
            cancellationToken
        );
        var moreUrl = results.HasMore
            ? WithParentValues(
                Url.Action(
                    nameof(Lookup),
                    new
                    {
                        id = RouteData.Values["id"],
                        form = query.Form,
                        field = query.Field,
                        term,
                        skip = query.Skip + results.Items.Count,
                    }
                )!
            )
            : null;

        // The picker sends the selected values with every request, so Load more marks those selected by then. Only the
        // first page reports an empty result, which offers New; a later page simply ends the list.
        var isEmpty = query.Skip == 0 && results.Items.Count == 0;

        return PartialView(
            "_LookupResults",
            new LookupResultsViewModel(
                editor,
                items,
                results.Items,
                moreUrl,
                query.Selected,
                term,
                false,
                isEmpty,
                false,
                labels,
                isEmpty ? await GetLookupCreateUrlAsync(options, editor, items, query) : null
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
            || FindLookupField(query) is not ({ } options, LookupSheetEditor editor, { } items)
            || !TryConvertKey(GetValueType(options, query), value, out var current)
        )
        {
            return NotFound();
        }

        var item = (
            await items.FindAsync(
                HttpContext.RequestServices,
                new FieldEditorContext(
                    options.FieldName,
                    CreateFormModel(query.Form!, RouteData.Values["id"] as string),
                    current
                ),
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
                    editor.GetLayout(items),
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
            || FindLookupField(query) is not ({ } options, { } editor, { } items)
        )
        {
            return NotFound();
        }

        // Searches send the values of the fields the items depend on, as the sheet's request did
        var label = GetFieldLabel(options);
        var resultsUrl = WithParentValues(
            Url.Action(
                nameof(Lookup),
                new
                {
                    id = RouteData.Values["id"],
                    form = query.Form,
                    field = query.Field,
                    @for = editor is MultiLookupSheetEditor ? query.For : null,
                    level = query.Level > 0 ? query.Level : (int?)null,
                }
            )!
        );

        return PartialView(
            "_LookupSheet",
            new LookupSheetViewModel(
                editor,
                items,
                query.For,
                editor.SheetOptions.Title ?? label,
                resultsUrl,
                new LookupLabelContext(label, editor.SheetOptions.MinimumSearchLength, null),
                await GetLookupCreateUrlAsync(options, editor, items, query)
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
            || FindLookupField(query) is not ({ } options, MultiLookupSheetEditor editor, { } items)
            || !TryConvertSelected(GetValueType(options, query), query.Selected, out var values)
        )
        {
            return NotFound();
        }

        var selected = await items.FindAsync(
            HttpContext.RequestServices,
            new FieldEditorContext(
                options.FieldName,
                CreateFormModel(query.Form!, RouteData.Values["id"] as string),
                values
            ),
            cancellationToken
        );

        return PartialView(
            "_MultiLookupItems",
            new MultiLookupItemsViewModel(editor, selected, GetFieldLabel(options))
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

    // A lookup's form model, with the values of the fields its items depend on, which the sheet's requests send
    private async Task<object> CreateLookupModelAsync(
        FormFieldOptions options,
        ResourceLookupQuery query
    )
    {
        var model = CreateFormModel(query.Form!, RouteData.Values["id"] as string);
        await BindParentsAsync(model, FindForm(query.Form)!, [options]);

        return model;
    }

    // The lookup field and its items; a field whose resource can't be referred to has none, and was reported
    private (FormFieldOptions, LookupSheetEditorBase, LookupItems)? FindLookupField(
        ResourceLookupQuery query
    )
    {
        return
            FindForm(query.Form)?.Fields.FirstOrDefault(options => options.FieldName == query.Field)
                is { IsNeverEditable: false, Editor: LookupSheetEditorBase editor } options
            && editor.ResolveItems(HttpContext.RequestServices) is { } items
            ? (options, editor, items)
            : null;
    }

    private string GetFieldLabel(FormFieldOptions options) =>
        FieldLabels.Get(options, HttpContext.RequestServices, MetadataProvider);

    // A multi-select lookup's create form opens one level above the form that contains the field
    private async Task<string?> GetLookupCreateUrlAsync(
        FormFieldOptions options,
        LookupSheetEditorBase editor,
        LookupItems items,
        ResourceLookupQuery query
    )
    {
        if (
            editor is not MultiLookupSheetEditor { CreateEnabled: true } multi
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
            multi.CreateResourceType,
            multi.CreateSource
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

    // The type of the value a lookup field's editor edits: the binder's value, or else the property's
    private Type? GetValueType(FormFieldOptions options, ResourceLookupQuery query) =>
        options.Binder is null
            ? options.PropertyPath[^1].PropertyType
            : options
                .GetValue(
                    CreateFormModel(query.Form!, RouteData.Values["id"] as string),
                    HttpContext.RequestServices
                )
                ?.GetType();

    // The Selected view of a multi-select lookup: the selected items whose title or description contains the term,
    // in the order they were selected, without paging
    private async Task<IActionResult> SelectedLookupResults(
        ResourceLookupQuery query,
        FormFieldOptions options,
        LookupSheetEditorBase editor,
        LookupItems items,
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

        var selected = await items.FindAsync(
            HttpContext.RequestServices,
            new FieldEditorContext(
                options.FieldName,
                CreateFormModel(query.Form!, RouteData.Values["id"] as string),
                values
            ),
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
                items,
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

    // A lookup URL with the values of the fields the lookup's items depend on, from the current request
    private string WithParentValues(string url) =>
        QueryHelpers.AddQueryString(
            url,
            Request
                .Query.Where(entry =>
                    entry.Key.StartsWith(DependencyPrefix + ".", StringComparison.OrdinalIgnoreCase)
                )
                .SelectMany(entry =>
                    entry.Value.Select(value => KeyValuePair.Create(entry.Key, value))
                )
        );
}
