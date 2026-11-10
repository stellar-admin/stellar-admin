using System.Globalization;
using System.Text;

namespace StellarAdmin.Resources.Displays;

// The default formats of DateDisplay, from the culture's own patterns: its first long date without a
// weekday (on ICU, CLDR's long date) with abbreviated month names, its short time, and both joined by
// a space as .NET's own date and time formats do. A culture with no such long date, such as the
// invariant culture, keeps the fixed formats.
internal sealed record DateDisplayFormats(string Date, string Time, string DateAndTime)
{
    private static readonly DateDisplayFormats Fallback = new(
        "d MMM yyyy",
        "HH:mm",
        "d MMM yyyy, HH:mm"
    );

    public static DateDisplayFormats For(DateTimeFormatInfo format)
    {
        var longDate = format
            .GetAllDateTimePatterns('D')
            .FirstOrDefault(pattern => !HasWeekday(pattern));
        if (longDate is null)
        {
            return Fallback;
        }

        var date = AbbreviateMonthNames(longDate);
        var time = format.ShortTimePattern;

        return new DateDisplayFormats(date, time, $"{date} {time}");
    }

    private static bool HasWeekday(string pattern)
    {
        var dayLetters = 0;
        foreach (var (letter, quoted) in Letters(pattern))
        {
            dayLetters = !quoted && letter == 'd' ? dayLetters + 1 : 0;
            if (dayLetters == 3)
            {
                return true;
            }
        }

        return false;
    }

    // MMMM, the full month name, becomes MMM outside quoted literals
    private static string AbbreviateMonthNames(string pattern)
    {
        var result = new StringBuilder(pattern.Length);
        var monthLetters = 0;
        foreach (var (letter, quoted) in Letters(pattern))
        {
            monthLetters = !quoted && letter == 'M' ? monthLetters + 1 : 0;
            if (monthLetters < 4)
            {
                result.Append(letter);
            }
        }

        return result.ToString();
    }

    // Each character of a custom format pattern, and whether it's literal text: inside single or
    // double quotes, or escaped with a backslash
    private static IEnumerable<(char Letter, bool Quoted)> Letters(string pattern)
    {
        char? quote = null;
        var escaped = false;
        foreach (var letter in pattern)
        {
            if (escaped)
            {
                escaped = false;
                yield return (letter, true);
            }
            else if (quote is null && letter == '\\')
            {
                escaped = true;
                yield return (letter, true);
            }
            else if (letter == quote)
            {
                quote = null;
                yield return (letter, true);
            }
            else if (quote is null && letter is '\'' or '"')
            {
                quote = letter;
                yield return (letter, true);
            }
            else
            {
                yield return (letter, quote is not null);
            }
        }
    }
}
