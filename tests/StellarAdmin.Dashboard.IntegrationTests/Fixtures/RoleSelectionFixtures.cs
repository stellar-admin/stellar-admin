using StellarAdmin.Dashboard.Resources;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class RoleSelectionModel
{
    public string[] RoleIds { get; set; } = [];
}

public sealed class RoleSelectionState
{
    public string[] RoleIds { get; set; } = ["auditor"];

    public ResourceOperationResult? UpdateResult { get; set; }
}

public sealed class RoleSelectionEditHandler(RoleSelectionState state)
    : IResourceEditHandler<RoleSelectionModel>
{
    public Task<RoleSelectionModel?> FindAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult<RoleSelectionModel?>(
            id == "7" ? new() { RoleIds = state.RoleIds } : null
        );
    }

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        RoleSelectionModel model,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (state.UpdateResult is { } result)
        {
            return Task.FromResult(result);
        }

        state.RoleIds = model.RoleIds;

        return Task.FromResult(ResourceOperationResult.Success());
    }
}
