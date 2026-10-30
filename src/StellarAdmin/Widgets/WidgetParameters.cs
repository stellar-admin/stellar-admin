using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace StellarAdmin.Widgets;

// Carries a widget's parameters in its URL, and binds them back. The payload is the parameters as JSON, protected
// for the page and the widget, so it can't be changed or moved to another widget. Binding uses MVC model binding over
// two value providers: the request's query string, limited to the [WidgetInput] properties, then the payload.
internal sealed class WidgetParameters(
    IDataProtectionProvider dataProtection,
    IModelMetadataProvider metadataProvider,
    IModelBinderFactory binderFactory,
    ParameterBinder parameterBinder
)
{
    public const string QueryName = "p";

    public async Task<(bool Bound, object? Value)> BindAsync(
        ActionContext actionContext,
        string page,
        string widget,
        Type type,
        string? payload
    )
    {
        if (string.IsNullOrEmpty(payload))
        {
            return (false, null);
        }

        string json;
        try
        {
            json = CreateProtector(page, widget).Unprotect(payload);
        }
        catch (CryptographicException)
        {
            return (false, null);
        }

        var values = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        using (var document = JsonDocument.Parse(json))
        {
            Flatten(document.RootElement, "", values);
        }

        var inputs = GetInputNames(type);
        var query = actionContext.HttpContext.Request.Query;
        var valueProvider = new CompositeValueProvider
        {
            new QueryStringValueProvider(
                BindingSource.Query,
                new QueryCollection(
                    query
                        .Where(pair => inputs.Contains(TopLevelName(pair.Key)))
                        .ToDictionary(pair => pair.Key, pair => pair.Value)
                ),
                CultureInfo.InvariantCulture
            ),
            new QueryStringValueProvider(
                BindingSource.Query,
                new QueryCollection(values),
                CultureInfo.InvariantCulture
            ),
        };

        var metadata = metadataProvider.GetMetadataForType(type);
        var binder = binderFactory.CreateBinder(
            new ModelBinderFactoryContext { Metadata = metadata, BindingInfo = new() }
        );
        var result = await parameterBinder.BindModelAsync(
            actionContext,
            binder,
            valueProvider,
            new ParameterDescriptor { Name = "parameters", ParameterType = type },
            metadata,
            null,
            null
        );

        return (result.IsModelSet && actionContext.ModelState.IsValid, result.Model);
    }

    public string Protect(string page, string widget, Type type, object? parameters) =>
        CreateProtector(page, widget).Protect(JsonSerializer.Serialize(parameters, type));

    private static void Flatten(
        JsonElement element,
        string name,
        Dictionary<string, StringValues> values
    )
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    Flatten(
                        property.Value,
                        name.Length == 0 ? property.Name : $"{name}.{property.Name}",
                        values
                    );
                }

                break;
            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    Flatten(item, $"{name}[{index++}]", values);
                }

                break;
            case JsonValueKind.String:
                values[name] = element.GetString();
                break;
            case JsonValueKind.Number:
                values[name] = element.GetRawText();
                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
                values[name] = element.GetBoolean() ? "true" : "false";
                break;
        }
    }

    // The [WidgetInput] properties, marked on the property or, for a record, on its constructor's parameter
    private static HashSet<string> GetInputNames(Type type)
    {
        var parameters = type.GetConstructors()
            .SelectMany(constructor => constructor.GetParameters())
            .Where(parameter => parameter.IsDefined(typeof(WidgetInputAttribute)))
            .Select(parameter => parameter.Name!);

        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.IsDefined(typeof(WidgetInputAttribute)))
            .Select(property => property.Name)
            .Concat(parameters)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    // "Range.From" and "Ids[0]" belong to the Range and Ids properties
    private static string TopLevelName(string key)
    {
        var end = key.IndexOfAny(['.', '[']);

        return end < 0 ? key : key[..end];
    }

    private IDataProtector CreateProtector(string page, string widget) =>
        dataProtection.CreateProtector("StellarAdmin.Widgets", page, widget);
}
