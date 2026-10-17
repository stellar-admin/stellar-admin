namespace StellarAdmin.Resources;

/// <summary>
///     Represents a validation error that a resource operation returns.
/// </summary>
/// <param name="FieldName">
///     The name of the model property the error is for, or <see langword="null" /> when the error
///     is for the whole operation.
/// </param>
/// <param name="Message">The error message.</param>
public sealed record ResourceValidationError(string? FieldName, string Message);
