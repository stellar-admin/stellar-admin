using System.ComponentModel.DataAnnotations;
using AdminPlayground.Data;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources;

namespace AdminPlayground.Resources.Products;

public sealed record ProductPriceCheck(
    int ProductId,
    string Name,
    string? Category,
    [property: Display(Name = "Unit price")] decimal Price,
    [property: Display(Name = "Category average")] decimal CategoryAverage
)
{
    public decimal Difference => Price - CategoryAverage;
}

// Each product the scope and search select, against the average price of its category among them. The table sorts and
// pages through the handler. SQLite can't aggregate or order decimals, so the rows are worked out here
public sealed class ProductPriceCheckHandler : IEfCoreDataLayoutHandler<Product, ProductPriceCheck>
{
    public async Task<ResourceListResult<ProductPriceCheck>> ListAsync(
        IQueryable<Product> entities,
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        var products = await entities
            .Select(product => new
            {
                product.Id,
                product.Name,
                Category = product.Category != null ? product.Category.Name : null,
                product.Price,
            })
            .ToListAsync(cancellationToken);
        var averages = products
            .GroupBy(product => product.Category ?? "")
            .ToDictionary(
                category => category.Key,
                category => category.Average(product => product.Price)
            );
        var rows = products.Select(product => new ProductPriceCheck(
            product.Id,
            product.Name,
            product.Category,
            product.Price,
            Math.Round(averages[product.Category ?? ""], 2)
        ));

        rows = request.Sort switch
        {
            { Field: nameof(ProductPriceCheck.Name) } sort => Order(rows, row => row.Name, sort),
            { Field: nameof(ProductPriceCheck.Category) } sort => Order(
                rows,
                row => row.Category,
                sort
            ),
            { Field: nameof(ProductPriceCheck.Price) } sort => Order(rows, row => row.Price, sort),
            { Field: nameof(ProductPriceCheck.Difference) } sort => Order(
                rows,
                row => row.Difference,
                sort
            ),
            _ => rows.OrderBy(row => row.Name),
        };
        if (request.Paging is { } paging)
        {
            rows = rows.Skip((paging.Page - 1) * paging.PageSize).Take(paging.PageSize);
        }

        return new(rows.ToList(), products.Count);
    }

    private static IEnumerable<ProductPriceCheck> Order<TKey>(
        IEnumerable<ProductPriceCheck> rows,
        Func<ProductPriceCheck, TKey> key,
        ResourceSort sort
    ) =>
        sort.Direction == ResourceSortDirection.Descending
            ? rows.OrderByDescending(key).ThenBy(row => row.Name)
            : rows.OrderBy(key).ThenBy(row => row.Name);
}
