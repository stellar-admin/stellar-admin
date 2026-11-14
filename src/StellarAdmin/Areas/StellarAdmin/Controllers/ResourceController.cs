using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Options;
using StellarAdmin.Widgets;

namespace StellarAdmin.Areas.StellarAdmin.Controllers;

[Area("StellarAdmin")]
internal sealed partial class ResourceController<TResource>(
    IResourceDataSource<TResource> dataSource,
    IOptions<ResourceOptions<TResource>> options,
    IOptions<ResourceLabelOptions> labelOptions,
    ICompositeViewEngine viewEngine,
    WidgetRenderer widgets,
    ILogger<ResourceController<TResource>> logger
) : Controller
{
    // The view page's action, whose method cannot be named View alongside the controller's View helpers
    private const string ViewActionName = "View";

    // The values of the fields a field depends on are sent as Model.Field, apart from the lookup's own query values
    private const string DependencyPrefix = "Model";

    private readonly ResourceLabelOptions _labelOptions = labelOptions.Value;
    private readonly ResourceOptions<TResource> _resourceOptions = options.Value;

    // An inline operation has no page, so neither its links nor its routes are available
    private bool HasCreatePage => _resourceOptions.Create is { InlineRow: null };
    private bool HasEditPage =>
        _resourceOptions.Edit is { InlineRow: null } && _resourceOptions.KeySelector is not null;
    private bool HasViewPage =>
        _resourceOptions.View is not null && _resourceOptions.KeySelector is not null;

    // A resource that a configuration problem made unavailable has no pages. Development says why, with a 404 status.
    [NonAction]
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!_resourceOptions.IsUnavailable)
        {
            return;
        }

        context.Result = HttpContext
            .RequestServices.GetRequiredService<IHostEnvironment>()
            .IsDevelopment()
            ? new ViewResult
            {
                ViewName = "/Areas/StellarAdmin/Views/Shared/ResourceUnavailable.cshtml",
                ViewData = new(ViewData)
                {
                    Model = HttpContext
                        .RequestServices.GetRequiredService<ConfigurationProblems>()
                        .GetProblems()
                        .Where(problem =>
                            string.Equals(
                                problem.ResourceSlug,
                                RouteData.Values["controller"] as string,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        .ToArray(),
                },
                StatusCode = StatusCodes.Status404NotFound,
            }
            : NotFound();
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

    // A handler's error for a field goes under it, and for an item of a list or dictionary field whose rows the form
    // shows, such as Highlights[2] or LocalNames[1].Key, under that row
    private void AddValidationErrors(
        ResourceOperationResult result,
        HashSet<string> fields,
        IEnumerable<FormFieldOptions> rowFields,
        string prefix
    )
    {
        var rowFieldNames = rowFields
            .Where(field =>
                CollectionEditorValidator.ShowsRowErrors(field) && fields.Contains(field.FieldName)
            )
            .Select(field => field.FieldName)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var error in result.Errors)
        {
            var key =
                error.FieldName is { } fieldName && (fields.Contains(fieldName) || IsRow(fieldName))
                    ? ModelNames.CreatePropertyModelName(prefix, fieldName)
                    : string.Empty;
            ModelState.AddModelError(key, error.Message);
        }

        bool IsRow(string fieldName) =>
            fieldName.IndexOf('[') is > 0 and var index
            && rowFieldNames.Contains(fieldName[..index]);
    }

    private ResourceLabelContext CreateLabelContext() =>
        new(_resourceOptions.SingularLabel, _resourceOptions.PluralLabel);

    // A form's model for loading a field's items without the whole form: a new create model, or an edit model with
    // only its key, from the route. A model that has never been bound stands in for an edit model it can't create.
    private object CreateFormModel(string form, string? id)
    {
        if (form.Equals("create", StringComparison.OrdinalIgnoreCase))
        {
            return _resourceOptions.Create!.CreateModel();
        }

        var modelType = _resourceOptions.Edit!.ModelType;
        var model = CreateUnboundModel(modelType);

        // A model that wraps the resource holds a resource with the key
        var resource = model;
        if (
            _resourceOptions.Edit.ResourcePropertyName is { } resourceName
            && modelType.GetProperty(resourceName) is { CanWrite: true } wrapped
            && wrapped.PropertyType == typeof(TResource)
        )
        {
            resource = wrapped.GetValue(model) ?? CreateUnboundModel(typeof(TResource));
            wrapped.SetValue(model, resource);
        }

        if (
            id is not null
            && resource is TResource
            && _resourceOptions.KeyPropertyName is { } keyName
            && typeof(TResource).GetProperty(keyName) is { CanWrite: true } key
            && TryConvertKey(key.PropertyType, id, out var value)
        )
        {
            key.SetValue(resource, value);
        }

        return model;

        static object CreateUnboundModel(Type type) =>
            type.GetConstructor(Type.EmptyTypes) is not null
                ? Activator.CreateInstance(type)!
                : RuntimeHelpers.GetUninitializedObject(type);
    }

    // The create or edit form a request names; the edit form needs the key that loads it
    private ResourceFormOptions? FindForm(string? form) =>
        form?.ToLowerInvariant() switch
        {
            "create" => _resourceOptions.Create,
            "edit" when _resourceOptions.KeySelector is not null => _resourceOptions.Edit,
            _ => null,
        };

    private ViewResult ResourceView(string action, object model)
    {
        var viewName = viewEngine.FindView(ControllerContext, action, isMainPage: true).Success
            ? action
            : "Resource" + action;

        return View(viewName, model);
    }

    // A handler that throws fails its operation with a general error, so the page shows the error instead of the
    // request failing. The exception goes to the log; an aborted request still cancels
    private async Task<ResourceOperationResult> RunHandlerAsync(
        Func<Task<ResourceOperationResult>> handler,
        string operation,
        string message,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await handler();
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException
                || !cancellationToken.IsCancellationRequested
            )
        {
            logger.LogError(
                exception,
                "The {Operation} of a {Resource} failed.",
                operation,
                typeof(TResource).Name
            );

            return ResourceOperationResult.ValidationFailed(null, message);
        }
    }
}
