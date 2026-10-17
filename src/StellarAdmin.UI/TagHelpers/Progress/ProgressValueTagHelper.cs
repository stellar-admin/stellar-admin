using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the value of a progress bar.
/// </summary>
/// <remarks>
///     With no content, the value shows the percentage complete of the parent progress bar.
/// </remarks>
[HtmlTargetElement("sa-progress-value")]
public class ProgressValueTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "progress-value");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-progress-value", output.GetUserSuppliedClass())
        );

        var content = await output.GetChildContentAsync();
        if (content.IsEmptyOrWhiteSpace)
        {
            output.Content.AppendHtml(GetPercentageCompleted().ToString("P0"));
        }
        else
        {
            output.Content.AppendHtml(content);
        }
    }

    private double GetPercentageCompleted()
    {
        var (minimum, maximum, value) = GetValues();

        return (value - minimum) / (maximum - minimum);
    }

    private (double Minimum, double Maximum, double Value) GetValues()
    {
        var parentProgressTagHelper = GetParentTagHelper<ProgressTagHelper>();
        return parentProgressTagHelper == null
            ? (0, 100, 0)
            : (
                parentProgressTagHelper.Min ?? 0,
                parentProgressTagHelper.Max ?? 100,
                parentProgressTagHelper.Value ?? 0
            );
    }
}
