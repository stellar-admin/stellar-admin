using Microsoft.AspNetCore.Razor.TagHelpers;
using FrameworkInputTagHelper = Microsoft.AspNetCore.Mvc.TagHelpers.InputTagHelper;

namespace StellarAdmin.UI;

// The framework gives a bound checkbox a hidden input that posts false while it's unchecked, after the checkbox or at
// the end of the form. A disabled checkbox doesn't post, so it leaves that input out too; otherwise it would post false
// and clear a checked value.
internal static class CheckboxHiddenInput
{
    public static void Process(
        FrameworkInputTagHelper inputTagHelper,
        TagHelperContext context,
        TagHelperOutput output
    )
    {
        var endOfForm = inputTagHelper.ViewContext.FormContext.EndOfFormContent;
        var count = endOfForm.Count;

        inputTagHelper.Process(context, output);

        if (output.Attributes.ContainsName("disabled"))
        {
            while (endOfForm.Count > count)
            {
                endOfForm.RemoveAt(endOfForm.Count - 1);
            }

            output.PostElement.Clear();
        }
    }
}
