namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     The outcome of a resource write operation.
/// </summary>
public sealed class ResourceOperationResult
{
    /// <summary>
    ///     The validation errors returned by the operation.
    /// </summary>
    public IReadOnlyList<ResourceValidationError> Errors { get; }

    /// <summary>
    ///     Whether the requested resource was not found.
    /// </summary>
    public bool IsNotFound { get; }

    /// <summary>
    ///     Whether the operation succeeded.
    /// </summary>
    public bool IsSuccess => !IsNotFound && Errors.Count == 0;

    private ResourceOperationResult(bool isNotFound, IReadOnlyList<ResourceValidationError> errors)
    {
        IsNotFound = isNotFound;
        Errors = errors;
    }

    /// <summary>
    ///     Returns a missing-resource result.
    /// </summary>
    public static ResourceOperationResult NotFound() => new(true, []);

    /// <summary>
    ///     Returns a successful result.
    /// </summary>
    public static ResourceOperationResult Success() => new(false, []);

    /// <summary>
    ///     Returns a failed result with the supplied validation errors.
    /// </summary>
    public static ResourceOperationResult ValidationFailed(
        IEnumerable<ResourceValidationError> errors
    )
    {
        ArgumentNullException.ThrowIfNull(errors);
        var snapshot = errors.ToArray();
        if (snapshot.Length == 0)
        {
            throw new ArgumentException(
                "At least one validation error is required.",
                nameof(errors)
            );
        }

        foreach (var error in snapshot)
        {
            ArgumentNullException.ThrowIfNull(error);
            ArgumentException.ThrowIfNullOrWhiteSpace(error.Message);
        }

        return new(false, Array.AsReadOnly(snapshot));
    }

    /// <summary>
    ///     Returns a validation error for a property, or the whole operation when fieldName is null.
    /// </summary>
    public static ResourceOperationResult ValidationFailed(string? fieldName, string message) =>
        ValidationFailed([new ResourceValidationError(fieldName, message)]);
}
