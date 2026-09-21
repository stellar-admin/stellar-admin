using DashboardPlayground.Models;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Data;

public sealed class EditCustomerHandler(CustomerDataSource source)
    : IResourceEditHandler<EditCustomerModel>
{
    public async Task<EditCustomerModel?> FindAsync(string id, CancellationToken cancellationToken)
    {
        var customer = await source.FindAsync(id, cancellationToken);

        return customer is null
            ? null
            : new() { DisplayName = customer.Name, Email = customer.Email };
    }

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        EditCustomerModel model,
        CancellationToken cancellationToken
    ) =>
        source.UpdateAsync(
            id,
            new Customer { Name = model.DisplayName, Email = model.Email },
            cancellationToken
        );
}
