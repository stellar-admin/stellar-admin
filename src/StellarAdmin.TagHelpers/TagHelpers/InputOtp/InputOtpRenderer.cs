using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Shared rendering for the Input OTP parts so the host's auto-generated markup is identical
///     to the markup produced when the author composes <c>sa-input-otp-group</c> /
///     <c>sa-input-otp-slot</c> / <c>sa-input-otp-separator</c> by hand.
/// </summary>
internal static class InputOtpRenderer
{
    internal static string GroupClass(string? userClass) =>
        StellarAdminTagHelperBase.JoinCssClasses("sa-input-otp-group", userClass) ?? string.Empty;

    internal static string SlotClass(string? userClass) =>
        StellarAdminTagHelperBase.JoinCssClasses("sa-input-otp-slot", userClass) ?? string.Empty;

    internal static string SeparatorClass(string? userClass) =>
        StellarAdminTagHelperBase.JoinCssClasses("sa-input-otp-separator", userClass)
        ?? string.Empty;

    /// <summary>
    ///     Builds a single presentational slot cell. The character (if any) seeds the first paint;
    ///     the web component re-distributes the live value once hydrated.
    /// </summary>
    internal static TagBuilder BuildSlot(int index, string? character, bool hasError)
    {
        var slot = new TagBuilder("div");
        slot.Attributes.Add("data-slot", "input-otp-slot");
        slot.Attributes.Add("data-index", index.ToString(CultureInfo.InvariantCulture));
        slot.Attributes.Add("data-active", "false");
        if (hasError)
        {
            slot.Attributes.Add("aria-invalid", "true");
        }
        slot.Attributes.Add("class", SlotClass(null));
        if (!string.IsNullOrEmpty(character))
        {
            slot.InnerHtml.Append(character);
        }
        return slot;
    }

    /// <summary>
    ///     Renders the default separator glyph (a Lucide <c>minus</c> icon) into the given content.
    /// </summary>
    internal static async Task RenderDefaultSeparatorContentAsync(
        TagHelperContent target,
        TagHelperContext context,
        IIconManager iconManager
    )
    {
        var iconOutput = new TagHelperOutput(
            "svg",
            [new TagHelperAttribute("class", "size-4")],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var iconTagHelper = new IconTagHelper(iconManager) { Name = "minus" };
        await iconTagHelper.ProcessAsync(context, iconOutput);
        target.AppendHtml(iconOutput);
    }
}
