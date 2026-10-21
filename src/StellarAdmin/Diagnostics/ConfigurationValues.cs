namespace StellarAdmin.Diagnostics;

// Why a value object can't be used, as the end of a "because" clause, and how to fix it
internal sealed record ConfigurationValueProblem(string Cause, string Fix);

internal static class ConfigurationValues
{
    // What each argument of a value object's factory method is called in a message, and what to pass instead
    private static readonly Dictionary<string, (string Noun, string Expected)> Arguments = new()
    {
        ["action"] = ("action name", "the name of an action"),
        ["area"] = ("area name", "the name of an area"),
        ["color"] = ("colour", "a CSS colour"),
        ["controller"] = ("controller name", "the name of a controller"),
        ["darkColor"] = ("dark mode colour", "a CSS colour for dark mode"),
        ["darkPath"] = ("dark mode path", "the path of the image for dark mode"),
        ["label"] = ("label", "a label"),
        ["name"] = ("icon name", "the name of an icon"),
        ["page"] = ("page name", "the name of a Razor page"),
        ["path"] = ("path", "the path of the image"),
        ["text"] = ("text", "the text of the item"),
        ["url"] = ("URL", "a URL"),
    };

    // Describes the first blank value, for a value object that its builder method reports and leaves out
    public static ConfigurationValueProblem? FindBlank(
        string method,
        params (string Name, string Value)[] values
    )
    {
        foreach (var (name, value) in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                var (noun, expected) = Arguments.TryGetValue(name, out var argument)
                    ? argument
                    : (name, $"a {name}");

                return new($"its {noun} is blank", $"Pass {expected} to `{method}`.");
            }
        }

        return null;
    }
}
