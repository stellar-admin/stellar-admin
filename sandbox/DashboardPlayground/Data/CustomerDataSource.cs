using DashboardPlayground.Models;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Data;

public sealed class CustomerDataSource
    : IResourceDataSource<Customer>,
        IResourceDeleteHandler<Customer>
{
    private readonly List<Customer> _customers =
    [
        new()
        {
            Id = 1,
            Name = "Alex Morgan",
            Email = "alex@example.com",
        },
    ];
    private readonly Lock _lock = new();
    private int _nextId = 2;

    public ResourceOperationResult AddCustomer(string name, string email)
    {
        lock (_lock)
        {
            if (
                _customers.Any(customer =>
                    string.Equals(customer.Email, email, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                return ResourceOperationResult.ValidationFailed(
                    nameof(Customer.Email),
                    "A customer with this email already exists."
                );
            }

            _customers.Add(
                new()
                {
                    Id = _nextId++,
                    Name = name,
                    Email = email,
                }
            );

            return ResourceOperationResult.Success();
        }
    }

    public Task<ResourceOperationResult> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            return Task.FromResult(
                int.TryParse(id, out var key)
                && _customers.RemoveAll(customer => customer.Id == key) > 0
                    ? ResourceOperationResult.Success()
                    : ResourceOperationResult.NotFound()
            );
        }
    }

    public Task<Customer?> FindAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            var customer = int.TryParse(id, out var key)
                ? _customers.Find(customer => customer.Id == key)
                : null;

            return Task.FromResult(customer is null ? null : Copy(customer));
        }
    }

    public Task<IReadOnlyList<Customer>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<Customer>>(_customers.Select(Copy).ToArray());
        }
    }

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        Customer model,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            var index = int.TryParse(id, out var key)
                ? _customers.FindIndex(customer => customer.Id == key)
                : -1;
            if (index < 0)
            {
                return Task.FromResult(ResourceOperationResult.NotFound());
            }

            if (
                _customers.Any(customer =>
                    customer.Id != key
                    && string.Equals(
                        customer.Email,
                        model.Email,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
            )
            {
                return Task.FromResult(
                    ResourceOperationResult.ValidationFailed(
                        nameof(Customer.Email),
                        "A customer with this email already exists."
                    )
                );
            }

            _customers[index] = new()
            {
                Id = key,
                Name = model.Name,
                Email = model.Email,
            };

            return Task.FromResult(ResourceOperationResult.Success());
        }
    }

    private static Customer Copy(Customer customer) =>
        new()
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
        };
}
