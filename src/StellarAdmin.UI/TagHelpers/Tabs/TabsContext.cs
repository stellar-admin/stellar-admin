namespace StellarAdmin.UI;

internal sealed class TabsContext
{
    public required string TabsId { get; init; }

    public string? SelectedPanelId { get; set; }

    public Dictionary<string, string> TabIdsByPanelId { get; } = new(StringComparer.Ordinal);
}
