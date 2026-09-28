namespace StellarAdmin.TagHelpers;

internal sealed class CommandContext
{
    public required string InputId { get; init; }

    public required string? LabelId { get; init; }

    public required string ListId { get; init; }
}
