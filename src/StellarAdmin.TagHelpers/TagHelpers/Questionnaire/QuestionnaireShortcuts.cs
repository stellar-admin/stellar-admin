namespace StellarAdmin.TagHelpers;

/// <summary>
///     The kind of shortcut key assigned to each choice in a questionnaire.
/// </summary>
public enum QuestionnaireShortcuts
{
    /// <summary>Assigns the letters A, B, C, and so on.</summary>
    Letters,

    /// <summary>Assigns the numbers 1, 2, 3, and so on.</summary>
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
