using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.LinkDisplays;

public sealed class LinkDisplaysGridScenario : Scenario
{
    public override string Description =>
        "The suppliers grid with each supplier's email, phone number and website as links.";

    public override string StartPath => "/stellaradmin/suppliers";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Supplier>(resource =>
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(supplier => supplier.Name);
                    columns.Add(supplier => supplier.Email);
                    columns.Add(supplier => supplier.Phone);
                    columns.Add(supplier => supplier.Website);
                });
                // Six rows, so the grid crop stays short
                index.EnablePaging(paging => paging.PageSize = 6);
            })
        );
    }
}
