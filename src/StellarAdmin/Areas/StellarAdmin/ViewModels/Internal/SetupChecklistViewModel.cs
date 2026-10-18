using StellarAdmin.UI;

namespace StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;

internal sealed record SetupChecklistViewModel(IReadOnlyList<SetupStepViewModel> Steps)
{
    public int DoneCount => Steps.Count(step => step.Done);

    // The first step still to do starts expanded
    public string? FirstOpenId => Steps.FirstOrDefault(step => !step.Done)?.Id;
}

internal sealed record SetupStepViewModel(
    string Id,
    string Title,
    string Description,
    string Code,
    string AgentPrompt,
    SetupStepLinkViewModel Docs
)
{
    public string? Detail { get; init; }

    public bool Done { get; init; }

    public bool Essential { get; init; }

    public SetupStepLinkViewModel? ExtraLink { get; init; }

    public string? Install { get; init; }

    public bool Optional { get; init; }

    public bool Warn { get; init; }
}

internal sealed record SetupStepLinkViewModel(string Label, string Url);

internal sealed record SetupTabsViewModel(
    string Id,
    string? Install,
    string Code,
    string AgentPrompt
);

internal sealed record SetupCodeViewModel(string Text, bool IsPrompt);

internal sealed record SetupCopyButtonViewModel(
    string Label,
    string? Text,
    ButtonSize Size,
    string? Class
);
