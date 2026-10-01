using System.ComponentModel.DataAnnotations;
using StellarAdmin.Dashboard.Resources;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class TextInputFieldsModel
{
    [DataType(DataType.Currency)]
    public decimal Budget { get; set; }

    [Display(Description = "Shown on the invoice.")]
    public string? Code { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public Guid ExternalId { get; set; }

    public long? Population { get; set; }

    [Phone]
    public string? Phone { get; set; }

    public int Quantity { get; set; }

    public double Ratio { get; set; }

    [Url]
    public string? Website { get; set; }
}

public sealed class TextInputFieldsHandler : IResourceCreateHandler<TextInputFieldsModel>
{
    public Task<ResourceOperationResult> CreateAsync(
        TextInputFieldsModel model,
        CancellationToken cancellationToken
    ) => Task.FromResult(ResourceOperationResult.Success());
}
