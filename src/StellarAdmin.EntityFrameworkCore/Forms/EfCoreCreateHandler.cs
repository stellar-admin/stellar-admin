using Microsoft.Extensions.Options;
using StellarAdmin.Resources;

namespace StellarAdmin.EntityFrameworkCore;

// Saves a create form through the EF data source, with the form's events
internal sealed class EfCoreCreateHandler<TEntity, TModel>(
    IServiceProvider services,
    EfCoreResourceDataSource<TEntity> dataSource,
    IOptions<EfCoreFormOptions<EfCoreCreateEvents<TEntity, TModel>>> options
) : IResourceCreateHandler<TModel>
    where TEntity : class
{
    public Task<ResourceOperationResult> CreateAsync(
        TModel model,
        CancellationToken cancellationToken
    ) =>
        dataSource.CreateAsync(
            model,
            options.Value.Entity!,
            options.Value.GetEvents(services),
            cancellationToken
        );

    public Task InitializeAsync(TModel model, CancellationToken cancellationToken) =>
        options.Value.GetEvents(services) is { } events
            ? dataSource.InitializeAsync(model, options.Value.Entity!, events, cancellationToken)
            : Task.CompletedTask;
}
