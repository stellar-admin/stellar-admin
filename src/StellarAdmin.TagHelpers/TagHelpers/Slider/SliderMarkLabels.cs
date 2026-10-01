namespace StellarAdmin.TagHelpers;

/// <summary>
///     Which generated marks of a <c>&lt;sa-slider-marks&gt;</c> show their formatted value.
/// </summary>
public enum SliderMarkLabels
{
    /// <summary>No mark shows a label.</summary>
    None,

    /// <summary>The marks at the minimum and maximum show their value.</summary>
    Ends,

    /// <summary>Every mark shows its value.</summary>
    All,
}
