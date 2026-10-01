namespace StellarAdmin.TagHelpers;

/// <summary>
///     The shared state a <c>sa-slider-marks</c> publishes for its authored marks.
/// </summary>
internal sealed class SliderMarksContext
{
    public required bool ShowTicks { get; init; }
}
