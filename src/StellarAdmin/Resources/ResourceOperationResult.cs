namespace StellarAdmin.Resources;

/// <summary>
///     Represents the outcome of creating, updating or deleting a resource.
/// </summary>
public sealed class ResourceOperationResult
{
    /// <summary>
    ///     Gets the validation errors the operation returned.
    /// </summary>
    public IReadOnlyList<ResourceValidationError> Errors { get; }

    /// <summary>
    ///     Gets a value that indicates whether the resource wasn't found.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the resource wasn't found; otherwise,
    ///     <see langword="false" />.
    /// </value>
    public bool IsNotFound { get; }

    /// <summary>
    ///     Gets a value that indicates whether the operation succeeded.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the resource was found and there are no validation errors;
    ///     otherwise, <see langword="false" />.
    /// </value>
    public bool IsSuccess => !IsNotFound && Errors.Count == 0;

    /// <summary>
    ///     Gets the key of the created resource, or <see langword="null" /> when the operation
    ///     didn't return one.
    /// </summary>
    public string? Key { get; }

    private ResourceOperationResult(
        bool isNotFound,
        IReadOnlyList<ResourceValidationError> errors,
        string? key = null
    )
    {
        IsNotFound = isNotFound;
        Errors = errors;
        Key = key;
    }

    /// <summary>
    ///     Returns a result that reports the resource wasn't found.
    /// </summary>
    /// <returns>A result that reports the resource wasn't found.</returns>
    public static ResourceOperationResult NotFound() => new(true, []);

    /// <summary>
    ///     Returns a successful result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static ResourceOperationResult Success() => new(false, []);

    /// <summary>
    ///     Returns a successful result with the key of the created resource.
    /// </summary>
    /// <param name="key">The key of the created resource.</param>
    /// <returns>A successful result that carries the key.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> is empty.
    /// </exception>
    /// <remarks>
    ///     Return the key from a create handler whose model isn't the resource. The key lets a
    ///     lookup select the new resource and lets the index grid show a row created inline.
    /// </remarks>
    public static ResourceOperationResult Success(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        return new(false, [], key);
    }

    /// <summary>
    ///     Returns a failed result with validation errors.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    /// <returns>A failed result that carries the errors.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="errors" /> is <see langword="null" />, contains <see langword="null" />
    ///     or contains an error whose message is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="errors" /> is empty or contains an error whose message is empty or
    ///     white space.
    /// </exception>
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
    ///     Returns a failed result with one validation error.
    /// </summary>
    /// <param name="fieldName">
    ///     The name of the model property the error is for, or <see langword="null" /> when the
    ///     error is for the whole operation.
    /// </param>
    /// <param name="message">The error message.</param>
    /// <returns>A failed result that carries the error.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="message" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="message" /> is empty or white space.
    /// </exception>
    public static ResourceOperationResult ValidationFailed(string? fieldName, string message) =>
        ValidationFailed([new ResourceValidationError(fieldName, message)]);
}
