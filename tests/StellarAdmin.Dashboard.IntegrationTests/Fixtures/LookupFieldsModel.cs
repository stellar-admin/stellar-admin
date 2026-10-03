using System.ComponentModel.DataAnnotations;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed record Category(int Id, string Name, string Code);

public sealed class CategoryLookupSource : ILookupSource<Category, int>
{
    private static readonly Category[] Categories =
    [
        new(1, "Cameras", "CAM"),
        new(2, "Notebooks", "NTB"),
    ];

    public Task<Category?> FindAsync(int value, CancellationToken cancellationToken) =>
        Task.FromResult(Categories.FirstOrDefault(category => category.Id == value));

    public Task<LookupPage<Category>> SearchAsync(
        LookupQuery query,
        CancellationToken cancellationToken
    )
    {
        var matches = Categories
            .Where(category =>
                query.Term is null
                || category.Name.Contains(query.Term, StringComparison.OrdinalIgnoreCase)
            )
            .ToArray();

        return Task.FromResult(
            new LookupPage<Category>(
                matches.Skip(query.Skip).Take(query.Take).ToArray(),
                matches.Length > query.Skip + query.Take
            )
        );
    }
}

public sealed class LookupFieldsModel
{
    public int? CategoryId { get; set; }

    [Editable(false)]
    public int? FixedCategoryId { get; set; }

    public int PrimaryCategoryId { get; set; }
}

public sealed class LookupFieldsHandler : IResourceCreateHandler<LookupFieldsModel>
{
    public Task<ResourceOperationResult> CreateAsync(
        LookupFieldsModel model,
        CancellationToken cancellationToken
    ) => Task.FromResult(ResourceOperationResult.Success());
}
