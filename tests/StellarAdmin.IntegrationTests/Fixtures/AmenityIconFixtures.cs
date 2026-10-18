using StellarAdmin.Resources;

namespace StellarAdmin.IntegrationTests.Fixtures;

public sealed class AmenityIconModel
{
    // A stored name that isn't a registered icon
    public string? Badge { get; set; }

    public string? Icon { get; set; }

    public string Name { get; set; } = "";
}

// Edits product 7's icons, and keeps the model each save receives
public sealed class AmenityIconHandler(AmenityIcons icons) : IResourceEditHandler<AmenityIconModel>
{
    public Task<AmenityIconModel?> FindAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult(
            id == "7"
                ? new AmenityIconModel
                {
                    Name = "Breakfast",
                    Icon = "coffee",
                    Badge = "spa-flower",
                }
                : null
        );

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        AmenityIconModel model,
        CancellationToken cancellationToken
    )
    {
        icons.Saved = model;

        return Task.FromResult(ResourceOperationResult.Success());
    }
}

// The model the form saved
public sealed class AmenityIcons
{
    public AmenityIconModel? Saved { get; set; }
}
