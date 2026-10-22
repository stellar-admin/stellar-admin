using System.ComponentModel.DataAnnotations;
using StellarAdmin.Resources;

namespace StellarAdmin.IntegrationTests.Fixtures;

public sealed class Supplier
{
    public string Country { get; set; } = "";

    public int Id { get; set; }

    public string Name { get; set; } = "";
}

// More suppliers than a lookup sheet's page, in no particular order
public sealed class SupplierStore
{
    public List<Supplier> Suppliers { get; } =
    [
        .. Enumerable
            .Range(1, 25)
            .Select(id => new Supplier
            {
                Id = id,
                Name = $"Supplier {id:00}",
                Country = id % 2 == 0 ? "Japan" : "Chile",
            })
            .Reverse(),
    ];
}

// Finds suppliers by key, searches their names, sorts by name and pages, as an application's data source would
public sealed class SupplierDataSource(SupplierStore store) : IResourceDataSource<Supplier>
{
    public List<ResourceListRequest> Requests { get; } = [];

    public Task<ResourceListResult<Supplier>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        Requests.Add(request);
        IEnumerable<Supplier> suppliers = store.Suppliers;
        if (request.Keys is { } keys)
        {
            suppliers = suppliers.Where(supplier => keys.Contains(supplier.Id.ToString()));
        }

        if (request.Search is { } term)
        {
            suppliers = suppliers.Where(supplier =>
                supplier.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
            );
        }

        if (request.Sort is { Field: nameof(Supplier.Name) } sort)
        {
            suppliers =
                sort.Direction == ResourceSortDirection.Descending
                    ? suppliers.OrderByDescending(supplier => supplier.Name)
                    : suppliers.OrderBy(supplier => supplier.Name);
        }

        var matches = suppliers.ToArray();
        var page = request.Paging is { } paging
            ? matches.Skip((paging.Page - 1) * paging.PageSize).Take(paging.PageSize)
            : matches;

        return Task.FromResult(new ResourceListResult<Supplier>(page.ToArray(), matches.Length));
    }
}

// Named by the property [DisplayColumn] selects
[DisplayColumn(nameof(Label))]
public sealed class Carrier
{
    public int Id { get; set; }

    public string Label { get; set; } = "";

    public string Name { get; set; } = "";
}

public sealed class CarrierDataSource : IResourceDataSource<Carrier>
{
    public Task<ResourceListResult<Carrier>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        Carrier[] carriers =
        [
            new()
            {
                Id = 1,
                Label = "Sea freight",
                Name = "Oceanic",
            },
        ];

        return Task.FromResult(new ResourceListResult<Carrier>(carriers, carriers.Length));
    }
}

// Has neither a title property nor a [DisplayColumn], so it is named by its key
public sealed class Dock
{
    public string Code { get; set; } = "";
}

public sealed class DockDataSource : IResourceDataSource<Dock>
{
    public Task<ResourceListResult<Dock>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        Dock[] docks = [new() { Code = "D-7" }];

        return Task.FromResult(new ResourceListResult<Dock>(docks, docks.Length));
    }
}

public sealed class ReferenceFieldsModel
{
    public int? CarrierId { get; set; }

    public string? DockCode { get; set; }

    public int? SupplierId { get; set; }

    public List<int> SupplierIds { get; set; } = [];
}

public sealed class ReferenceFieldsHandler : IResourceCreateHandler<ReferenceFieldsModel>
{
    public Task<ResourceOperationResult> CreateAsync(
        ReferenceFieldsModel model,
        CancellationToken cancellationToken
    ) => Task.FromResult(ResourceOperationResult.Success());
}
