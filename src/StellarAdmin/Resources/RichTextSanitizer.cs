using Ganss.Xss;

namespace StellarAdmin.Resources;

// The default sanitizer: the elements and attributes the rich text editor produces, with links and
// images limited to http, https and mailto URLs or relative ones
internal sealed class RichTextSanitizer : IRichTextSanitizer
{
    private readonly HtmlSanitizer _sanitizer = new(
        new HtmlSanitizerOptions
        {
            AllowedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "a",
                "blockquote",
                "br",
                "code",
                "em",
                "h1",
                "h2",
                "h3",
                "h4",
                "h5",
                "h6",
                "hr",
                "img",
                "li",
                "ol",
                "p",
                "pre",
                "s",
                "strong",
                "table",
                "tbody",
                "td",
                "th",
                "thead",
                "tr",
                "u",
                "ul",
            },
            AllowedAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "alt",
                "colspan",
                "data-colwidth",
                "height",
                "href",
                "rowspan",
                "src",
                "title",
                "width",
            },
            AllowedSchemes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "http",
                "https",
                "mailto",
            },
            UriAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "href", "src" },
        }
    );

    public string Sanitize(string html) => _sanitizer.Sanitize(html);
}
