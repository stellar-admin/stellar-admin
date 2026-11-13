using Microsoft.AspNetCore.Mvc.Rendering;

namespace StellarAdmin.UI;

internal class FieldTagBuilder : TagBuilder
{
    public FieldTagBuilder(FieldOrientation orientation, string? userSuppliedClass)
        : base("div")
    {
        Attributes.Add("data-slot", "field");
        Attributes.Add("data-orientation", orientation.GetDataAttributeText());
        Attributes.Add(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses("sa-field", userSuppliedClass)
        );
    }
}
