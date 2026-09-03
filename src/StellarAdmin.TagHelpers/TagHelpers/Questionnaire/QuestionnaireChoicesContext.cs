using System.Globalization;

namespace StellarAdmin.TagHelpers;

internal sealed class QuestionnaireChoicesContext
{
    private int _assigned;

    public QuestionnaireShortcuts? Shortcuts { get; init; }

    /// Hands out the next auto-assigned shortcut, or null when the container assigns none.
    /// Letters run out after Z; a longer list simply stops showing badges.
    public string? TakeNextShortcut()
    {
        if (Shortcuts is not { } shortcuts)
        {
            return null;
        }

        var index = _assigned++;

        return shortcuts switch
        {
            QuestionnaireShortcuts.Letters => index < 26
                ? ((char)('A' + index)).ToString(CultureInfo.InvariantCulture)
                : null,
            QuestionnaireShortcuts.Numbers => (index + 1).ToString(CultureInfo.InvariantCulture),
            _ => null,
        };
    }
}
