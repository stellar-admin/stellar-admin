using System.ComponentModel.DataAnnotations;
using StellarAdmin.Dashboard.Resources;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public enum Cabin
{
    Economy,

    [Display(Name = "Premium economy", Description = "Extra legroom.")]
    PremiumEconomy,

    Business,
}

public enum Meal
{
    [Display(Name = "Pasta", GroupName = "Hot")]
    Pasta,

    [Display(Name = "Salad", GroupName = "Cold")]
    Salad,

    [Display(Name = "Curry", GroupName = "Hot")]
    Curry,
}

[Flags]
public enum Extras
{
    None = 0,
    Meals = 1,
    Lounge = 2,
}

public sealed class ChoiceFieldsModel
{
    public Cabin Cabin { get; set; }

    public List<Cabin> Cabins { get; set; } = [];

    public Extras Extras { get; set; }

    public bool? Insured { get; set; }

    public Meal Meal { get; set; }

    public Cabin? OptionalCabin { get; set; }

    [Required]
    public string? Seat { get; set; }
}

public sealed class ChoiceFieldsHandler : IResourceCreateHandler<ChoiceFieldsModel>
{
    public Task<ResourceOperationResult> CreateAsync(
        ChoiceFieldsModel model,
        CancellationToken cancellationToken
    ) => Task.FromResult(ResourceOperationResult.Success());
}
