using Microsoft.AspNetCore.Mvc.Rendering;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

internal class FieldTagBuilder : TagBuilder
{
    private static readonly Dictionary<FieldOrientation, ClassElement[]> OrientationClasses = new()
    {
        [FieldOrientation.Vertical] =
        [
            new ThemeToken("sa-field-orientation-vertical"),
            // Child-width forcing stays in the utilities layer: it must override the
            // children's own component classes (e.g. a toggle group's w-fit).
            "[&>*]:w-full [&>.sr-only]:w-auto",
        ],
        [FieldOrientation.Horizontal] = [new ThemeToken("sa-field-orientation-horizontal")],
        [FieldOrientation.Responsive] =
        [
            new ThemeToken("sa-field-orientation-responsive"),
            "[&>*]:w-full [&>.sr-only]:w-auto @md/field-group:[&>*]:w-auto",
        ],
    };

    public FieldTagBuilder(
        ICssClassMerger classMerger,
        FieldOrientation orientation,
        string? userSuppliedClass
    )
        : base("div")
    {
        Attributes.Add("data-slot", "field");
        Attributes.Add("data-orientation", orientation.GetDataAttributeText());
        Attributes.Add(
            "class",
            classMerger.Merge(
                new ClassElement[] { new ThemeToken("sa-field"), "group/field" }
                    .Union(OrientationClasses[orientation])
                    .Append(userSuppliedClass)
                    .ToArray()
            )
        );
    }
}
