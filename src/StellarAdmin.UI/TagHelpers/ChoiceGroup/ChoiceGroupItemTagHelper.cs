using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Provides the attributes shared by radio and checkbox group options.
/// </summary>
public abstract class ChoiceGroupItemTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the supporting text shown below the option's label.
    /// </summary>
    [HtmlAttributeName("description")]
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the option is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the option is disabled and can't be selected; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Gets or sets the value that the group posts when the option is selected.
    /// </summary>
    /// <remarks>
    ///     Required. Each option in a group needs a different value.
    /// </remarks>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    private protected ChoiceGroupItemTagHelper() { }

    /// <summary>
    ///     Renders the option through the group that contains it.
    /// </summary>
    /// <param name="context">The context of the element being rendered.</param>
    /// <param name="output">The output of the element being rendered.</param>
    /// <param name="multiple">
    ///     <see langword="true" /> if the option belongs to a checkbox group; otherwise,
    ///     <see langword="false" />.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">
    ///     The element isn't inside a matching radio or checkbox group, or <see cref="Value" /> is
    ///     <see langword="null" />.
    /// </exception>
    private protected async Task RenderAsync(
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
        var item = new ChoiceGroupItemContext { Multiple = multiple };
        SetContext(context, item);
        var label = await output.GetChildContentAsync();
        var content = await group.RenderItem(
            Value,
            label,
            item.Media,
            Description,
            Disabled == true,
            output.GetUserSuppliedClass()
        );
        output.TagName = null;
        output.Attributes.Clear();
        output.Content.SetHtmlContent(content);
    }
}
