namespace StellarAdmin.TagHelpers;

/// <summary>
///     A value for each container breakpoint of a responsive grid, such as column counts or spans.
/// </summary>
/// <param name="Base">
///     The value at the smallest width.
/// </param>
/// <param name="Small">
///     The value when the container is at least 30rem wide.
/// </param>
/// <param name="Medium">
///     The value when the container is at least 40rem wide.
/// </param>
/// <param name="Large">
///     The value when the container is at least 56rem wide.
/// </param>
public readonly record struct GridTiers(int Base, int Small, int Medium, int Large);
