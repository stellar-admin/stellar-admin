using StellarAdmin.Resources;

namespace StellarAdmin.IntegrationTests.Fixtures;

public sealed class CreateProductHandler(ProductState state)
    : IResourceCreateHandler<CreateProductModel>
{
    public Task<ResourceOperationResult> CreateAsync(
        CreateProductModel model,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        state.CreateCalls++;
        if (state.CreateResult is { } result)
        {
            return Task.FromResult(result);
        }

        state.SubmittedId = model.Id;
        state.Products.Add(new(state.Products.Count + 1, model.ProductName, model.Price));

        return Task.FromResult(ResourceOperationResult.Success());
    }
}
