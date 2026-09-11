using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace StellarAdmin.TagHelpers;

internal sealed class SegmentedControlContext
{
    public required string? DescribedBy { get; init; }
    public required bool Disabled { get; init; }
    public string? FirstInputId { get; set; }
    public required ModelExpression? For { get; init; }
    public required bool Invalid { get; init; }
    public required string Name { get; init; }
    public required bool Required { get; init; }
    public required string? Value { get; init; }
}
