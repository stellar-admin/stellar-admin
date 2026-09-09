namespace StellarAdmin.TagHelpers;

/// <summary>
///     Additional CSS classes for the parts of a slider.
/// </summary>
public class SliderClassNames : FieldClassNames
{
    /// <summary>
    ///     Additional CSS classes for the slider container.
    /// </summary>
    public string? Control { get; set; }

    /// <summary>
    ///     Additional CSS classes for the filled range.
    /// </summary>
    public string? Range { get; set; }

    /// <summary>
    ///     Additional CSS classes for each slider thumb.
    /// </summary>
    public string? Thumb { get; set; }

    /// <summary>
    ///     Additional CSS classes for the slider track.
    /// </summary>
    public string? Track { get; set; }
}
