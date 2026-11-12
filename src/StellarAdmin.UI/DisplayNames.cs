using System.Text;

namespace StellarAdmin.UI;

/// <summary>
///     Provides the display names that StellarAdmin gives properties without a display name of their own.
/// </summary>
public static class DisplayNames
{
    /// <summary>
    ///     Returns a property name split into words, such as <c>Departure Date</c> for <c>DepartureDate</c>.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <returns>The display name.</returns>
    /// <remarks>
    ///     The words keep their casing, and an acronym stays whole, so <c>HTMLCode</c> reads <c>HTML Code</c>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="name" /> is <see langword="null" />.
    /// </exception>
    public static string FromPropertyName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var builder = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            if (
                i > 0
                && char.IsUpper(name[i])
                && (char.IsLower(name[i - 1]) || (i + 1 < name.Length && char.IsLower(name[i + 1])))
            )
            {
                builder.Append(' ');
            }

            builder.Append(name[i]);
        }

        return builder.ToString();
    }
}
