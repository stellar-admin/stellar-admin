using Microsoft.AspNetCore.Mvc.Rendering;

namespace StellarAdmin.TagHelpers;

internal class FieldTagBuilder : TagBuilder
{
    private static readonly Dictionary<FieldOrientation, string?[]> OrientationClasses = new()
    {
        [FieldOrientation.Vertical] =
        [
            "sa-field-orientation-vertical",
            // Child-width forcing stays in the utilities layer: it must override the
            // children's own component classes (e.g. a toggle group's w-fit).
            "[&>*]:w-full [&>.sr-only]:w-auto",
        ],
        [FieldOrientation.Horizontal] = ["sa-field-orientation-horizontal"],
        [FieldOrientation.Responsive] =
        [
            "sa-field-orientation-responsive",
            "[&>*]:w-full [&>.sr-only]:w-auto @md/field-group:[&>*]:w-auto",
        ],
    };

    public FieldTagBuilder(FieldOrientation orientation, string? userSuppliedClass)
        : base("div")
    {
        Attributes.Add("data-slot", "field");
        Attributes.Add("data-orientation", orientation.GetDataAttributeText());
        Attributes.Add(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses(
                new string?[] { "sa-field", "group/field" }
                    .Union(OrientationClasses[orientation])
                    .Append(userSuppliedClass)
                    .ToArray()
            )
        );
    }
}
