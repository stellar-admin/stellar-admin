using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace StellarAdmin.Areas.StellarAdmin;

// The text of an HTML value without its markup, with a space between blocks and line breaks, so
// paragraphs, list items and table cells don't run together. Scripts and styles show nothing.
internal static class HtmlText
{
    private static readonly HashSet<string> Breaks = new(StringComparer.OrdinalIgnoreCase)
    {
        "blockquote",
        "br",
        "div",
        "h1",
        "h2",
        "h3",
        "h4",
        "h5",
        "h6",
        "hr",
        "li",
        "p",
        "pre",
        "td",
        "th",
        "tr",
    };

    private static readonly HashSet<string> Hidden = new(StringComparer.OrdinalIgnoreCase)
    {
        "script",
        "style",
        "template",
    };

    public static string ToPlainText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var document = new HtmlParser().ParseDocument(string.Empty);
        var nodes = new HtmlParser().ParseFragment(html, document.Body!);
        var text = new StringBuilder();
        foreach (var node in nodes)
        {
            Append(node, text);
        }

        return string.Join(
            ' ',
            text.ToString().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
        );
    }

    private static void Append(INode node, StringBuilder text)
    {
        if (node.NodeType == NodeType.Text)
        {
            text.Append(node.TextContent);
            return;
        }

        if (node is IElement { LocalName: var name } && Hidden.Contains(name))
        {
            return;
        }

        var breaks = node is IElement element && Breaks.Contains(element.LocalName);
        if (breaks)
        {
            text.Append(' ');
        }

        foreach (var child in node.ChildNodes)
        {
            Append(child, text);
        }

        if (breaks)
        {
            text.Append(' ');
        }
    }
}
