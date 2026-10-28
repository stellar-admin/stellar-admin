using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.Sidebar;
using VoyagerTravel.Models;

namespace StellarAdmin.ScenarioTesting.Scenarios.Authz;

public sealed class AuthorizationScenario : Scenario
{
    public override string Description =>
        "Only Admin and Agent staff can open the admin panel; Payments and the Audit log link are for Admins only.";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.RequireAuthorization(policy => policy.RequireRole("Admin", "Agent"));
        // Links to the resources each user can open instead of the setup checklist
        admin.ConfigureHomePage(home => home.ShowSetupChecklist = false);
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.UseTitle(trip => trip.Title);
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(trip => trip.Title));
            });
        });
        admin.AddEfCoreResource<Payment>(resource =>
        {
            resource.RequireAuthorization(policy => policy.RequireRole("Admin"));
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns =>
                {
                    columns.Add(payment => payment.Amount);
                    columns.Add(payment => payment.Status);
                });
            });
        });
        admin.AddSidebarLink(
            "Audit log",
            SidebarLinkTarget.Url("/audit-log"),
            link => link.RequireAuthorization(policy => policy.RequireRole("Admin"))
        );
    }
}
