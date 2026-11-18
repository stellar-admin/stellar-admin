using StellarAdmin.Resources;

namespace StellarAdmin.IntegrationTests.Fixtures;

// The products as rows in the order the request sorts them, with a last row that isn't a product, and the page the
// request asks for. Records the requests it receives
public sealed class ProductRowsHandler(ProductState state) : IResourceLayoutHandler<ProductRow>
{
    public Task<ResourceListResult<ProductRow>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        state.HandlerRequests.Add(request);
        IEnumerable<ProductRow> rows = state
            .Products.Select(product => new ProductRow(
                product.Id,
                product.Id + 10,
                product.Name,
                product.Price
            ))
            .Append(
                new ProductRow(null, null, "Total", state.Products.Sum(product => product.Price))
            );
        if (request.Sort is { Field: nameof(ProductRow.Price) } sort)
        {
            rows =
                sort.Direction == ResourceSortDirection.Descending
                    ? rows.OrderByDescending(row => row.Price)
                    : rows.OrderBy(row => row.Price);
        }

        var all = rows.ToList();
        if (request.Paging is { } paging)
        {
            rows = all.Skip((paging.Page - 1) * paging.PageSize).Take(paging.PageSize);
        }

        return Task.FromResult(new ResourceListResult<ProductRow>(rows.ToList(), all.Count));
    }
}
