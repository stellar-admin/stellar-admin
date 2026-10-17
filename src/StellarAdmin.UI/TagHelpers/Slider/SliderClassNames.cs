namespace StellarAdmin.UI;

/// <summary>
///     Holds additional CSS classes for the parts of a slider.
/// </summary>
public class SliderClassNames : FieldClassNames
{
    /// <summary>
    ///     Gets or sets additional CSS classes for the slider element that holds the track and
    ///     thumbs.
    /// </summary>
    public string? Control { get; set; }

    /// <summary>
    ///     Gets or sets additional CSS classes for each mark.
    /// </summary>
    public string? Mark { get; set; }

    /// <summary>
    ///     Gets or sets additional CSS classes for each mark's label.
    /// </summary>
    public string? MarkLabel { get; set; }

    /// <summary>
    ///     Gets or sets additional CSS classes for the container of the marks.
    /// </summary>
    public string? Marks { get; set; }

    /// <summary>
    ///     Gets or sets additional CSS classes for the filled part of the track.
    /// </summary>
    public string? Range { get; set; }

    /// <summary>
    ///     Gets or sets additional CSS classes for each thumb.
    /// </summary>
    public string? Thumb { get; set; }

    /// <summary>
    ///     Gets or sets additional CSS classes for the track.
    /// </summary>
    public string? Track { get; set; }
}
