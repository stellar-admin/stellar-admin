using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Field;

public partial class FieldInputBaseTagHelperTests
{
    private static readonly HashSet<string> FieldParts =
    [
        "field-label",
        "field-description",
        "field-error",
    ];

    private static FieldInputBaseTagHelper CreateSut(string kind, RenderingContext context)
    {
        List<SelectListItem> items = [new("One", "1"), new("Two", "2")];

        return kind switch
        {
            "input" => new InputTagHelper(context.Generator, context.Icons)
            {
                ViewContext = context.ViewContext,
            },
            "input-checkbox" => new InputTagHelper(context.Generator, context.Icons)
            {
                ViewContext = context.ViewContext,
                InputTypeName = "checkbox",
            },
            "input-radio" => new InputTagHelper(context.Generator, context.Icons)
            {
                ViewContext = context.ViewContext,
                InputTypeName = "radio",
                Value = "1",
            },
            "textarea" => new TextareaTagHelper(context.Generator)
            {
                ViewContext = context.ViewContext,
            },
            "select" => new SelectTagHelper(context.Generator, context.Icons)
            {
                ViewContext = context.ViewContext,
                Items = items,
            },
            "input-otp" => new InputOtpTagHelper(context.Generator, context.Icons)
            {
                ViewContext = context.ViewContext,
            },
            "slider" => new SliderTagHelper(context.Generator)
            {
                ViewContext = context.ViewContext,
            },
            "switch" => new SwitchTagHelper(context.Generator)
            {
                ViewContext = context.ViewContext,
            },
            "toggle" => new ToggleTagHelper(context.Generator)
            {
                ViewContext = context.ViewContext,
            },
            "toggle-group" => new ToggleGroupTagHelper(context.Generator)
            {
                ViewContext = context.ViewContext,
            },
            "segmented-control" => new SegmentedControlTagHelper(context.Generator)
            {
                ViewContext = context.ViewContext,
            },
            "checkbox-group" => new CheckboxGroupTagHelper(context.Generator, context.Icons)
            {
                ViewContext = context.ViewContext,
                Items = items,
            },
            "radio-group" => new RadioGroupTagHelper(context.Generator, context.Icons)
            {
                ViewContext = context.ViewContext,
                Items = items,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    private static ModelExpression CreateFor(RenderingContext context, string kind)
    {
        var modelType = kind switch
        {
            "input-checkbox" or "switch" or "toggle" => typeof(bool),
            "slider" => typeof(int),
            "checkbox-group" => typeof(List<string>),
            _ => typeof(string),
        };

        return new ModelExpression(
            "Value",
            context.Metadata.GetModelExplorerForType(modelType, null)
        );
    }

    // Describes the automatic field wrapper's direct children in document order, e.g.
    // "field-label control field-error". Field parts are named by their data-slot, a field
    // content container lists its children in brackets, and anything else is the control.
    // Only the rendered root counts as the wrapper: choice group items render nested fields.
    private static string DescribeField(IHtmlDocument html)
    {
        var root = html.Body?.FirstElementChild;

        return root?.GetAttribute("data-slot") == "field" ? Describe(root.Children) : "(no field)";
    }

    private static string Describe(IEnumerable<IElement> elements)
    {
        return string.Join(
            " ",
            elements.Select(element =>
                element.GetAttribute("data-slot") switch
                {
                    "field-content" => $"field-content[{Describe(element.Children)}]",
                    { } slot when FieldParts.Contains(slot) => slot,
                    _ => "control",
                }
            )
        );
    }
}
