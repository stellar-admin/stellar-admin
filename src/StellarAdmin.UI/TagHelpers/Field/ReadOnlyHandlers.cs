using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

// Browsers ignore readonly on checkboxes, radio buttons and selects, so a read-only one gets inline
// handlers that keep its value, and aria-readonly where its role has it
internal static class ReadOnlyHandlers
{
    // Cancels a click, which a label and Space also cause, and in a radio group an arrow key's
    public const string Choice = "return false";

    // Takes focus without opening
    public const string SelectMouseDown = "this.focus(); return false";

    // Ignores the keys that open the select or change its option, and typed letters; Tab and
    // shortcuts still work
    public const string SelectKeyDown =
        "return event.ctrlKey || event.metaKey || !/^(.|Arrow.*|Page.*|Home|End|Enter|F4)$/.test(event.key)";

    public static void ApplyToCheckbox(TagHelperAttributeList attributes)
    {
        if (!attributes.ContainsName("readonly"))
        {
            return;
        }

        attributes.SetAttribute("onclick", Choice);
        if (!attributes.ContainsName("aria-readonly"))
        {
            attributes.SetAttribute("aria-readonly", "true");
        }
    }

    // A radio button's role has no read-only state; its group's has
    public static void ApplyToRadio(TagHelperAttributeList attributes)
    {
        if (attributes.ContainsName("readonly"))
        {
            attributes.SetAttribute("onclick", Choice);
        }
    }

    public static void ApplyToSelect(TagHelperAttributeList attributes)
    {
        if (!attributes.ContainsName("readonly"))
        {
            return;
        }

        attributes.SetAttribute("onmousedown", SelectMouseDown);
        attributes.SetAttribute("onkeydown", SelectKeyDown);
        if (!attributes.ContainsName("aria-readonly"))
        {
            attributes.SetAttribute("aria-readonly", "true");
        }
    }
}
