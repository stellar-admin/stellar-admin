using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Areas.StellarAdmin;

// Finds editor templates in EditorTemplates/ for a form and GridEditorTemplates/ for a grid cell. A field without an
// editor goes through the names MVC tries for its property's [UIHint], [DataType] and type, most specific first: a
// template the application adds under a name wins, and otherwise the built-in editor the name maps to, when the folder
// has one. A grid has no textarea or rich text editor, so multiline text and HTML fall through to a text input there.
internal static class EditorTemplateFinder
{
    public const string FormFolder = "EditorTemplates";

    public const string GridFolder = "GridEditorTemplates";

    private const string TextInput = "Editors/TextInput";

    // The built-in editor for each name, as some depend on the property's metadata
    private static readonly Dictionary<string, Func<ModelMetadata, string>> BuiltInEditors = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        ["Boolean"] = metadata =>
            metadata.IsNullableValueType ? "Editors/Select" : "Editors/Checkbox",
        ["Byte"] = _ => TextInput,
        ["Currency"] = _ => TextInput,
        ["Date"] = _ => "Editors/DateInput",
        ["DateOnly"] = _ => "Editors/DateInput",
        ["DateTime"] = _ => "Editors/DateTimeInput",
        ["DateTimeOffset"] = _ => "Editors/DateTimeInput",
        ["Decimal"] = _ => TextInput,
        ["Double"] = _ => TextInput,
        ["EmailAddress"] = _ => TextInput,
        ["Enum"] = metadata => metadata.IsFlagsEnum ? TextInput : "Editors/Select",
        ["Guid"] = _ => TextInput,
        ["Html"] = _ => "Editors/RichText",
        ["Int16"] = _ => TextInput,
        ["Int32"] = _ => TextInput,
        ["Int64"] = _ => TextInput,
        ["MultilineText"] = _ => "Editors/Textarea",
        ["Password"] = _ => TextInput,
        ["PhoneNumber"] = _ => TextInput,
        ["SByte"] = _ => TextInput,
        ["Single"] = _ => TextInput,
        ["String"] = _ => TextInput,
        ["Time"] = _ => "Editors/TimeInput",
        ["TimeOnly"] = _ => "Editors/TimeInput",
        ["UInt16"] = _ => TextInput,
        ["UInt32"] = _ => TextInput,
        ["UInt64"] = _ => TextInput,
        ["Url"] = _ => TextInput,
    };

    // The template of a field without an editor, or null when only a template the application adds can edit it
    public static EditorTemplate? FindDefault(
        ViewContext viewContext,
        string folder,
        ModelMetadata metadata
    )
    {
        foreach (var name in GetTemplateNames(metadata))
        {
            if (FindView(viewContext, folder, name) is { } view)
            {
                return new EditorTemplate(name, view);
            }

            if (
                BuiltInEditors.TryGetValue(name, out var builtIn)
                && builtIn(metadata) is var builtInName
                && FindView(viewContext, folder, builtInName) is { } builtInView
            )
            {
                return new EditorTemplate(builtInName, builtInView);
            }
        }

        return null;
    }

    public static IView? FindView(ViewContext viewContext, string folder, string templateName)
    {
        var viewEngine =
            viewContext.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();
        var viewName = $"{folder}/{templateName}";
        var result = viewEngine.GetView(viewContext.ExecutingFilePath, viewName, false);
        if (!result.Success)
        {
            result = viewEngine.FindView(viewContext, viewName, false);
        }

        return result.Success ? result.View : null;
    }

    // The names MVC tries for an editor template without a template name, most specific first
    public static IEnumerable<string> GetTemplateNames(ModelMetadata metadata)
    {
        if (!string.IsNullOrEmpty(metadata.TemplateHint))
        {
            yield return metadata.TemplateHint;
        }

        if (!string.IsNullOrEmpty(metadata.DataTypeName))
        {
            yield return metadata.DataTypeName;
        }

        var type = metadata.UnderlyingOrModelType;
        yield return type.Name;
        if (type == typeof(string) || metadata.IsComplexType)
        {
            yield break;
        }

        if (type.IsEnum)
        {
            yield return "Enum";
        }
        else if (type == typeof(DateTimeOffset))
        {
            yield return "DateTime";
        }

        yield return "String";
    }
}

// A template's name in its folder, and its view
internal sealed record EditorTemplate(string Name, IView View);
