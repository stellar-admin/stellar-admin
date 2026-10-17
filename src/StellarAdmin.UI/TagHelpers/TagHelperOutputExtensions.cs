using System.Net;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Provides extension methods for <see cref="TagHelperOutput" />.
/// </summary>
public static class TagHelperOutputExtensions
{
    /// <summary>
    ///     Returns the <c>class</c> attribute value that the author set on the element.
    /// </summary>
    /// <param name="output">The output to read the attribute from.</param>
    /// <returns>
    ///     The decoded class names, or an empty string if the element has no <c>class</c>
    ///     attribute.
    /// </returns>
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
    ///     Adds a declaration to the end of the element's <c>style</c> attribute.
    /// </summary>
    /// <remarks>
    ///     The existing declarations, such as those the author set, are kept.
    /// </remarks>
    /// <param name="output">The output to add the declaration to.</param>
    /// <param name="declaration">The CSS declaration to add, such as <c>color: red</c>.</param>
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
