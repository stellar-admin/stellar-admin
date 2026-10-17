namespace StellarAdmin.UI;

/// <summary>
///     Keeps generated element ids unique when a container renders the same markup more than once.
/// </summary>
/// <remarks>
///     A container, such as a data grid that renders its column templates for each row, shares an
///     instance with <see cref="StellarAdminTagHelperBase.SetContext{T}" />. It changes
///     <see cref="Value" /> before each pass, and
///     <see cref="StellarAdminTagHelperBase.GetUniqueId" /> adds the value to each id.
/// </remarks>
public sealed class UniqueIdDiscriminator
{
    /// <summary>
    ///     Gets or sets the value added to generated ids for the current pass.
    /// </summary>
    /// <value>
    ///     The value, or <see langword="null" /> or an empty string to leave ids unchanged. The
    ///     default is <see langword="null" />.
    /// </value>
    public string? Value { get; set; }
}
