using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the checkboxes of the table selection cells through the library input tag helper,
///     so they match a hand-written <c>&lt;sa-input type="checkbox"&gt;</c>.
/// </summary>
internal static class TableSelectionRendering
{
    public static async Task<IHtmlContent> RenderCheckboxAsync(
        IHtmlGenerator htmlGenerator,
        IconOptions iconOptions,
        ViewContext viewContext,
        TagHelperContext context,
        IEnumerable<TagHelperAttribute> attributes
    )
    {
        // The checkbox gets its own context, so it does not see the cell's attributes or
        // place itself in a layout grid around the table.
        var checkboxAttributes = new TagHelperAttributeList(attributes);
        var checkboxContext = new TagHelperContext(
            "sa-input",
            checkboxAttributes,
            new Dictionary<object, object>(),
            context.UniqueId
        );
        var checkboxOutput = new TagHelperOutput(
            "sa-input",
            checkboxAttributes,
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var inputTagHelper = new InputTagHelper(htmlGenerator, iconOptions)
        {
            ViewContext = viewContext,
        };
        await inputTagHelper.ProcessAsync(checkboxContext, checkboxOutput);

        return checkboxOutput;
    }
}
