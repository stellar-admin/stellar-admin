using System.Net;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

public static class TagHelperOutputExtensions
{
    public static string GetUserSuppliedClass(this TagHelperOutput output)
    {
        var value = output.Attributes["class"]?.Value;
        if (value is IHtmlContent content)
        {
            using var writer = new StringWriter();
            content.WriteTo(writer, HtmlEncoder.Default);

            return WebUtility.HtmlDecode(writer.ToString());
        }

        return value?.ToString() ?? string.Empty;
    }

    /// <summary>
    ///     Appends a declaration to the element's <c>style</c> attribute, preserving any
    ///     existing (e.g. user-supplied) value.
    /// </summary>
    public static void AppendStyle(this TagHelperOutput output, string declaration)
    {
        var existing =
            output.Attributes.ContainsName("style")
            && output.Attributes["style"].Value?.ToString() is { } value
                ? value.TrimEnd().TrimEnd(';')
                : null;

        output.Attributes.SetAttribute(
            "style",
            string.IsNullOrEmpty(existing) ? declaration : $"{existing}; {declaration}"
        );
    }
}
