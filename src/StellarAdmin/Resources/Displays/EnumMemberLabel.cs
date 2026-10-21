using Humanizer;

namespace StellarAdmin.Resources.Displays;

// An enum member's label: its [Display] name, or else its name made readable, so NorthAmerica shows as "North America"
internal static class EnumMemberLabel
{
    public static string Get(string memberName, string displayName) =>
        displayName == memberName ? memberName.Humanize(LetterCasing.Title) : displayName;
}
