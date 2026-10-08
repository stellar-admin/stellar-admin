using System.ComponentModel.DataAnnotations;
using StellarAdmin.Dashboard.Resources;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class MultiLookupFieldsModel
{
    public List<int> CategoryIds { get; set; } = [];

    [Editable(false)]
    public int[] FixedCategoryIds { get; set; } = [];

    [MaxLength(2)]
    public int[] LimitedCategoryIds { get; set; } = [];
}

// The record the multi-select lookup tests edit; a successful save replaces it
public sealed class MultiLookupFieldsState
{
    public MultiLookupFieldsModel Model { get; set; } = new();
}

public sealed class MultiLookupFieldsHandler(MultiLookupFieldsState state)
    : IResourceEditHandler<MultiLookupFieldsModel>
{
    public Task<MultiLookupFieldsModel?> FindAsync(
        string id,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult<MultiLookupFieldsModel?>(
            id == "7"
                ? new()
                {
                    CategoryIds = [.. state.Model.CategoryIds],
                    FixedCategoryIds = [.. state.Model.FixedCategoryIds],
                    LimitedCategoryIds = [.. state.Model.LimitedCategoryIds],
                }
                : null
        );

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        MultiLookupFieldsModel model,
        CancellationToken cancellationToken
    )
    {
        state.Model = model;

        return Task.FromResult(ResourceOperationResult.Success());
    }
}
