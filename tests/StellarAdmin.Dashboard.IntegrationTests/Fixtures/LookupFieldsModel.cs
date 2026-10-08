using System.ComponentModel.DataAnnotations;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class Category
{
    [Required]
    public string Code { get; set; } = "";

    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = "";
}

// The categories a lookup searches and the category resource creates, shared for the lifetime of a test host
public sealed class CategoryStore
{
    public List<Category> Categories { get; } =
    [
        new()
        {
            Id = 1,
            Name = "Cameras",
            Code = "CAM",
        },
        new()
        {
            Id = 2,
            Name = "Notebooks",
            Code = "NTB",
        },
    ];
}

public sealed class CategoryLookupSource(CategoryStore store) : ILookupSource<Category, int>
{
    public Task<IReadOnlyCollection<Category>> FindAsync(
        IReadOnlyCollection<int> values,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult<IReadOnlyCollection<Category>>(
            store.Categories.Where(category => values.Contains(category.Id)).ToArray()
        );

    public Task<LookupPage<Category>> SearchAsync(
        LookupQuery query,
        CancellationToken cancellationToken
    )
    {
        var matches = store
            .Categories.Where(category =>
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

// Creates categories with the resource as the create model, so the resource's key selector returns the new key
public sealed class CategoryDataSource(CategoryStore store)
    : IResourceDataSource<Category>,
        IResourceCreateHandler<Category>
{
    public Task<ResourceOperationResult> CreateAsync(
        Category model,
        CancellationToken cancellationToken
    )
    {
        if (store.Categories.Any(category => category.Code == model.Code))
        {
            return Task.FromResult(
                ResourceOperationResult.ValidationFailed(
                    nameof(Category.Code),
                    "The code is already used."
                )
            );
        }

        model.Id = store.Categories.Max(category => category.Id) + 1;
        store.Categories.Add(model);

        return Task.FromResult(ResourceOperationResult.Success());
    }

    public Task<ResourceListResult<Category>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult(
            new ResourceListResult<Category>(store.Categories.ToArray(), store.Categories.Count)
        );
}

public sealed class CreateCategoryModel
{
    [Required]
    public string Name { get; set; } = "";
}

// Creates categories from a model other than the resource, so it returns the new key itself
public sealed class CreateCategoryHandler(CategoryStore store)
    : IResourceCreateHandler<CreateCategoryModel>
{
    public Task<ResourceOperationResult> CreateAsync(
        CreateCategoryModel model,
        CancellationToken cancellationToken
    )
    {
        var category = new Category
        {
            Id = store.Categories.Max(category => category.Id) + 1,
            Name = model.Name,
            Code = model.Name[..3].ToUpperInvariant(),
        };
        store.Categories.Add(category);

        return Task.FromResult(ResourceOperationResult.Success(category.Id.ToString()));
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
