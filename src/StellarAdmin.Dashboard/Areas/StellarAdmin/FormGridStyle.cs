using System.Globalization;
using System.Text;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin;

// Writes resolved grid values as the inline variables the form grid CSS reads. A tier at 1 is left out, since the
// variables are registered with 1 as their initial value and don't inherit.
internal static class FormGridStyle
{
    public static string? Columns(FormGridTiers columns) => Format("--sa-cols", columns);

    public static string? Rows(FormGridTiers rows) => Format("--sa-rows", rows);

    public static string? Span(FormGridTiers span) => Format("--sa-span", span);

    private static string? Format(string variable, FormGridTiers tiers)
    {
        var style = new StringBuilder();
        Append(style, variable, tiers.Default);
        Append(style, $"{variable}-sm", tiers.Small);
        Append(style, $"{variable}-md", tiers.Medium);
        Append(style, $"{variable}-lg", tiers.Large);

        return style.Length > 0 ? style.ToString() : null;
    }

    private static void Append(StringBuilder style, string variable, int value)
    {
        if (value == 1)
        {
            return;
        }

        if (style.Length > 0)
        {
            style.Append(';');
        }

        style.Append(variable).Append(':').Append(value.ToString(CultureInfo.InvariantCulture));
    }
}
