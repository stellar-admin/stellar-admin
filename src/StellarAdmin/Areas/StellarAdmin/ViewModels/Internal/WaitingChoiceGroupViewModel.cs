namespace StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;

// A choice group waiting for a field its choices depend on: its label and the waiting text in place of its choices. It
// posts an empty value, so a value it had is cleared when the form is saved.
internal sealed record WaitingChoiceGroupViewModel(
    string Id,
    string Name,
    string Label,
    string? Description,
    string Text,
    bool Multiple
);
