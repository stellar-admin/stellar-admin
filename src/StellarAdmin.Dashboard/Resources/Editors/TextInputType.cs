namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     The kind of value a text input editor accepts.
/// </summary>
public enum TextInputType
{
    /// <summary>
    ///     Plain text.
    /// </summary>
    Text,

    /// <summary>
    ///     An email address.
    /// </summary>
    Email,

    /// <summary>
    ///     A telephone number.
    /// </summary>
    Tel,

    /// <summary>
    ///     A URL.
    /// </summary>
    Url,

    /// <summary>
    ///     A password, with its characters obscured.
    /// </summary>
    Password,

    /// <summary>
    ///     A number.
    /// </summary>
    Number,
}
