using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures a checkbox group editor with application-supplied choices.
/// </summary>
public sealed class CheckboxGroupEditorOptions
    : ChoiceItemsEditorOptions,
        IFieldEditorOptions<CheckboxGroupEditor>;
