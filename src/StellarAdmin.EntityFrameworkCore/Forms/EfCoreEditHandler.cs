using Microsoft.Extensions.Options;
using StellarAdmin.Resources;

namespace StellarAdmin.EntityFrameworkCore;

// Loads and saves an edit form through the EF data source, with the form's events
internal sealed class EfCoreEditHandler<TEntity, TModel>(
    IServiceProvider services,
    EfCoreResourceDataSource<TEntity> dataSource,
    IOptions<EfCoreFormOptions<EfCoreEditEvents<TEntity, TModel>>> options
) : IResourceEditHandler<TModel>
    where TEntity : class
{
    public Task<TModel?> FindAsync(string id, CancellationToken cancellationToken) =>
        dataSource.FindAsync(
            id,
            options.Value.Entity!,
            options.Value.GetEvents(services),
            cancellationToken
        );

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        TModel model,
        CancellationToken cancellationToken
    ) =>
        dataSource.UpdateAsync(
            id,
            model,
            options.Value.Entity!,
            options.Value.GetEvents(services),
            cancellationToken
        );
}
