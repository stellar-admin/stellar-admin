using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

internal static class AttachmentTriggerRenderingHelper
{
    public static void Render(TagHelperOutput output)
    {
        output.Attributes.SetAttribute("data-slot", "attachment-trigger");
        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses(
                "sa-attachment-trigger",
                output.GetUserSuppliedClass()
            )
        );
    }
}
