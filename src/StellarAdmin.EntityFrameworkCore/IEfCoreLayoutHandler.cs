using StellarAdmin.Resources;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Reshapes the entities an index lists into the items of a layout, such as a summary of them.
/// </summary>
/// <typeparam name="TEntity">The type of the entity the index lists.</typeparam>
/// <typeparam name="TItem">The type of the items the layout shows.</typeparam>
public interface IEfCoreLayoutHandler<TEntity, TItem>
    where TEntity : class
{
    /// <summary>
    ///     Returns the layout's items and their total number.
    /// </summary>
    /// <param name="entities">
    ///     A query of the entities the scope and search select, which isn't tracked.
    /// </param>
    /// <param name="request">The request with the layout's key and the applied scope and search.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task that returns the items and their total number.</returns>
    Task<ResourceListResult<TItem>> ListAsync(
        IQueryable<TEntity> entities,
        ResourceListRequest request,
        CancellationToken cancellationToken
    );
}
