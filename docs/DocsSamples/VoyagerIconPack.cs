using System.Collections.Immutable;
using StellarAdmin.TagHelpers.Icons;

namespace DocsSamples;

/// <summary>
///     A small custom icon pack used by the Icons docs page to demonstrate
///     <c>AddIconPack&lt;T&gt;()</c>. Icons are drawn on the same 24x24 stroke grid as Lucide so
///     they mix with the built-in icons.
/// </summary>
public class VoyagerIconPack : IIconPack
{
    private static readonly Dictionary<string, string> SvgAttributes = new()
    {
        ["xmlns"] = "http://www.w3.org/2000/svg",
        ["width"] = "24",
        ["height"] = "24",
        ["viewBox"] = "0 0 24 24",
        ["fill"] = "none",
        ["stroke"] = "currentColor",
        ["stroke-width"] = "2",
        ["stroke-linecap"] = "round",
        ["stroke-linejoin"] = "round",
    };

    public IDictionary<string, IconDefinition> GetIcons()
    {
        return new Dictionary<string, IconDefinition>
        {
            // A suitcase with a luggage tag
            ["voyager-suitcase"] = new IconDefinition(
                SvgAttributes,
                [
                    Shape(
                        "rect",
                        ("x", "3"),
                        ("y", "7"),
                        ("width", "18"),
                        ("height", "13"),
                        ("rx", "2")
                    ),
                    Shape("path", ("d", "M8 7V5a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2")),
                    Shape("path", ("d", "M8 7v13")),
                    Shape("path", ("d", "M16 7v13")),
                ]
            ),
            // A stylised compass rose
            ["voyager-compass"] = new IconDefinition(
                SvgAttributes,
                [
                    Shape("circle", ("cx", "12"), ("cy", "12"), ("r", "9")),
                    Shape("path", ("d", "m15.5 8.5-2 5-5 2 2-5z")),
                    Shape("path", ("d", "M12 3v2")),
                    Shape("path", ("d", "M12 19v2")),
                    Shape("path", ("d", "M3 12h2")),
                    Shape("path", ("d", "M19 12h2")),
                ]
            ),
        };
    }

    private static SvgShape Shape(string name, params (string Name, string Value)[] attributes)
    {
        return new SvgShape(name, attributes.ToImmutableDictionary(a => a.Name, a => a.Value));
    }
}
