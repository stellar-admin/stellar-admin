using Microsoft.AspNetCore.Mvc.Rendering;

namespace StellarAdmin.TagHelpers;

internal class FieldTagBuilder : TagBuilder
{
    private static readonly Dictionary<FieldOrientation, string?[]> OrientationClasses = new()
    {
        [FieldOrientation.Vertical] = ["sa-field-orientation-vertical"],
        [FieldOrientation.Horizontal] = ["sa-field-orientation-horizontal"],
        [FieldOrientation.Responsive] = ["sa-field-orientation-responsive"],
    };

    public FieldTagBuilder(FieldOrientation orientation, string? userSuppliedClass)
        : base("div")
    {
        Attributes.Add("data-slot", "field");
        Attributes.Add("data-orientation", orientation.GetDataAttributeText());
        Attributes.Add(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses(
                new string?[] { "sa-field" }
                    .Union(OrientationClasses[orientation])
                    .Append(userSuppliedClass)
                    .ToArray()
            )
        );
    }
}
