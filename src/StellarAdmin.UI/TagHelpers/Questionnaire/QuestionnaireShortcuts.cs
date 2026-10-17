namespace StellarAdmin.UI;

/// <summary>
///     Specifies the kind of shortcut key that each choice in a questionnaire gets.
/// </summary>
public enum QuestionnaireShortcuts
{
    /// <summary>
    ///     The letters A to Z.
    /// </summary>
    Letters,

    /// <summary>
    ///     The numbers 1 to 9.
    /// </summary>
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
