namespace StellarAdmin.TagHelpers;

/// <summary>
///     Published as a context object by a container that executes the same child markup more
///     than once (e.g. a data grid rendering its column templates once per row), so that
///     <see cref="StellarAdminTagHelperBase.GetUniqueId" /> keeps generated DOM ids unique
///     across executions. The container mutates <see cref="Value" /> before each pass; while
///     it is null or empty, generated ids are unaffected.
/// </summary>
public sealed class UniqueIdDiscriminator
{
    public string? Value { get; set; }
}
