namespace StellarAdmin.TagHelpers;

/// <summary>
///     The kind of shortcut key assigned to each choice in a questionnaire.
/// </summary>
public enum QuestionnaireShortcuts
{
    /// <summary>Assigns the letters A through Z.</summary>
    Letters,

    /// <summary>Assigns the numbers 1 through 9.</summary>
    Numbers,
}

internal static class QuestionnaireShortcutsExtensions
{
    extension(QuestionnaireShortcuts shortcuts)
    {
        public string GetDataAttributeText() =>
            shortcuts switch
            {
                QuestionnaireShortcuts.Letters => "letters",
                QuestionnaireShortcuts.Numbers => "numbers",
                _ => throw new ArgumentOutOfRangeException(nameof(shortcuts), shortcuts, null),
            };
    }
}
