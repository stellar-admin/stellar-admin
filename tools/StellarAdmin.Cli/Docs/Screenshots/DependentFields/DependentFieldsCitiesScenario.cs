using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.DependentFields;

public sealed class DependentFieldsCitiesScenario : Scenario
{
    public override string Description =>
        "The destination create page with country and city lookups, whose city lookup lists only the cities of the selected country.";

    public override string StartPath => "/stellaradmin/destinations/create";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Country>(resource => resource.Index(index => index.EnablePaging()));
        admin.AddEfCoreResource<City>(resource => resource.Index(index => index.EnablePaging()));
        admin.AddEfCoreResource<Destination>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowCreate(create =>
                create.Fields(fields =>
                {
                    fields.Add(destination => destination.CountryId);
                    fields
                        .Add(destination => destination.CityId)
                        .DependsOn(destination => destination.CountryId)
                        .UseLookupSheetEditor(lookup =>
                            lookup.UseResource<City>(items =>
                                items.Where(
                                    (city, destination) => city.CountryId == destination.CountryId
                                )
                            )
                        );
                })
            );
        });
    }
}
