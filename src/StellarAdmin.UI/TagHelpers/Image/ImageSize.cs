namespace StellarAdmin.UI;

/// <summary>
///     Specifies the height of an image, which is a thumbnail's height or a figure's maximum
///     height.
/// </summary>
[GenerateDataAttributeText]
public enum ImageSize
{
    /// <summary>
    ///     24px for a thumbnail and 8rem for a figure.
    /// </summary>
    [DataAttributeText("xs")]
    ExtraSmall,

    /// <summary>
    ///     32px for a thumbnail and 10rem for a figure.
    /// </summary>
    [DataAttributeText("sm")]
    Small,

    /// <summary>
    ///     40px for a thumbnail and 15rem for a figure.
    /// </summary>
    [DataAttributeText("md")]
    Medium,

    /// <summary>
    ///     64px for a thumbnail and 20rem for a figure.
    /// </summary>
    [DataAttributeText("lg")]
    Large,
}
