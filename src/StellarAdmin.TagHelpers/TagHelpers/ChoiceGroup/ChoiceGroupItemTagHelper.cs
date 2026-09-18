using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Shared attributes for radio and checkbox group options.
/// </summary>
public abstract class ChoiceGroupItemTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Supporting text below the option label.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Whether this option is disabled.
    /// </summary>
    public bool? Disabled { get; set; }

    /// <summary>
    ///     The value submitted when this option is selected.
    /// </summary>
    public string? Value { get; set; }

    protected async Task RenderAsync(
        TagHelperContext context,
        TagHelperOutput output,
        bool multiple
    )
    {
        var group = GetContext<ChoiceGroupContext>(context);
        if (group == null || group.Multiple != multiple)
        {
            throw new InvalidOperationException(
                "Group items require a matching radio-group or checkbox-group parent."
            );
        }

        if (Value == null)
        {
            throw new InvalidOperationException("Group items require a non-null value.");
        }

        group.ItemCount++;
        var content = await group.RenderItem(
            Value,
            await output.GetChildContentAsync(),
            Description,
            Disabled == true,
            output.GetUserSuppliedClass()
        );
        output.TagName = null;
        output.Attributes.Clear();
        output.Content.SetHtmlContent(content);
    }
}
