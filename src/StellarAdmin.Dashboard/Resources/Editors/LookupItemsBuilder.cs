namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures how lookup items are displayed.
/// </summary>
public sealed class LookupItemsBuilder<TEntity>
{
    internal Func<TEntity, string?>? Description { get; private set; }

    /// <summary>
    ///     Displays secondary text below each item's text.
    /// </summary>
    public void DescribeWith(Func<TEntity, string?> description)
    {
        ArgumentNullException.ThrowIfNull(description);

        Description = description;
    }
}
