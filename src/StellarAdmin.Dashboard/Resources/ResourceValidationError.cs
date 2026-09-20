namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     A validation message for a model property, or the whole operation when FieldName is null.
/// </summary>
public sealed record ResourceValidationError(string? FieldName, string Message);
