using System.ComponentModel.DataAnnotations;
using StellarAdmin.Dashboard.Resources;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class InputFieldsModel
{
    [StringLength(4)]
    public string? BookingCode { get; set; }

    public DateTimeOffset Booked { get; set; }

    public DateTime Boarding { get; set; }

    public DateOnly Departure { get; set; }

    [DataType(DataType.Date)]
    public DateTime DepartureDate { get; set; }

    public TimeOnly Gate { get; set; }

    public bool Insured { get; set; }

    [DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    public bool? OptionalInsured { get; set; }

    [Range(1, 9)]
    public int Passengers { get; set; }

    public string? PromoCode { get; set; }
}

public sealed class InputFieldsHandler : IResourceCreateHandler<InputFieldsModel>
{
    public Task<ResourceOperationResult> CreateAsync(
        InputFieldsModel model,
        CancellationToken cancellationToken
    ) => Task.FromResult(ResourceOperationResult.Success());
}
