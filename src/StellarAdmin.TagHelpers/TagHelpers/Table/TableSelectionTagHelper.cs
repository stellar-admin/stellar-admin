using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Adds row selection to the table it wraps. The checkbox in the table's header row acts
///     as the select-all; a checkbox in a body row selects that row. Selected rows get
///     <c>data-state="selected"</c>, the select-all reflects the checked/indeterminate state,
///     and the selection posts as ordinary checkbox form data. Renders the
///     <c>sel-table-selection</c> web component: read the current selection from its
///     <c>selectedValues</c> property, or listen for its bubbling <c>selection-change</c>
///     event (with the values in <c>event.detail.values</c>).
/// </summary>
[HtmlTargetElement("sa-table-selection")]
public class TableSelectionTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // The wrapper renders the `sel-table-selection` web component, which owns the
        // select-all/row-highlight behavior for the checkboxes inside it.
        output.TagName = "sel-table-selection";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "table-selection");

        return Task.CompletedTask;
    }
}
