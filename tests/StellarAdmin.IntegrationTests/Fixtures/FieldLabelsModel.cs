using System.ComponentModel.DataAnnotations;
using StellarAdmin.Resources;

namespace StellarAdmin.IntegrationTests.Fixtures;

public sealed class FieldLabelsModel
{
    public FieldLabelsAddress Billing { get; set; } = new();

    public DateTime DepartureDate { get; set; }

    [Required]
    public string? LeadPassenger { get; set; }

    [Range(1, 9)]
    public int PassengerCount { get; set; }

    public string ReferenceCode { get; set; } = "";

    public FieldLabelsAddress Shipping { get; set; } = new();

    public int SupplierId { get; set; }
}

public sealed class FieldLabelsAddress
{
    [Required]
    public string? Street { get; set; }
}

public sealed class FieldLabelsHandler : IResourceCreateHandler<FieldLabelsModel>
{
    public Task<ResourceOperationResult> CreateAsync(
        FieldLabelsModel model,
        CancellationToken cancellationToken
    ) => Task.FromResult(ResourceOperationResult.Success());
}
