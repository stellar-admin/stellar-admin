namespace StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;

internal sealed record HomePageViewModel(
    string Title,
    SetupChecklistViewModel? Checklist,
    IReadOnlyList<HomeResourceLinkViewModel> Resources
);

internal sealed record HomeResourceLinkViewModel(string Label, string? Icon, string Controller);
