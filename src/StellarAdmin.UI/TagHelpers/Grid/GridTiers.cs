namespace StellarAdmin.UI;

/// <summary>
///     Holds a value for each breakpoint of a responsive grid, such as a column count or a span.
/// </summary>
/// <param name="Base">The value at the smallest width.</param>
/// <param name="Small">The value when the grid is at least 30rem wide.</param>
/// <param name="Medium">The value when the grid is at least 40rem wide.</param>
/// <param name="Large">The value when the grid is at least 56rem wide.</param>
public readonly record struct GridTiers(int Base, int Small, int Medium, int Large);
