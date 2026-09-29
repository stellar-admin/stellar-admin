namespace StellarAdmin.TagHelpers;

internal sealed class CommandContext
{
    public int GroupCount { get; set; }

    public required string InputId { get; init; }

    public required string? LabelId { get; init; }

    public required string ListId { get; init; }
}
