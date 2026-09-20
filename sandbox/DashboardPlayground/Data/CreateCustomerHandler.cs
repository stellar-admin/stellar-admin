using DashboardPlayground.Models;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Data;

public sealed class CreateCustomerHandler(CustomerDataSource customers)
    : IResourceCreateHandler<CreateCustomerModel>
{
    public Task<ResourceOperationResult> CreateAsync(
        CreateCustomerModel model,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Password fields demonstrate form-only validation. This in-memory sample does not create login accounts.
        return Task.FromResult(customers.AddCustomer(model.Name, model.Email));
    }
}
