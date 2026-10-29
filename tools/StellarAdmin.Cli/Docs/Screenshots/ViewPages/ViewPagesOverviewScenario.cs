using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Displays;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ViewPages;

public sealed class ViewPagesOverviewScenario : Scenario
{
    public override string Description =>
        "A booking's view page with a Trip section and a Traveller section, and Edit and Delete buttons.";

    public override string StartPath => "/stellaradmin/bookings/view/1";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource => resource.Index(index => index.EnablePaging()));
        admin.AddEfCoreResource<Booking>(resource =>
        {
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns =>
                {
                    columns.Add(booking => booking.Reference);
                    columns.Add(booking => booking.Traveller);
                });
            });
            resource.AllowView(view =>
                view.Fields(fields =>
                {
                    fields.Columns(2);
                    fields.AddSection(
                        "Trip",
                        section =>
                        {
                            section.Add(booking => booking.Reference);
                            section.Add(booking => booking.Departure.Trip);
                            section.Add(booking => booking.Departure.StartDate);
                            section.Add(
                                booking => booking.Passengers.Count,
                                field => field.Title = "Travelers"
                            );
                            section.Add(
                                booking => booking.TotalAmount,
                                field =>
                                    field.UseDisplay<NumberDisplay>(number => number.Format = "C")
                            );
                        }
                    );
                    fields.AddSection(
                        "Traveller",
                        section =>
                        {
                            section.Add(booking => booking.Traveller);
                            section.Add(
                                booking => booking.Traveller.Email,
                                field => field.Title = "Email"
                            );
                            section.Add(
                                booking => booking.SpecialRequests,
                                field => field.EmptyText = "No special requests"
                            );
                        }
                    );
                })
            );
            resource.AllowEdit(edit =>
                edit.Fields(fields => fields.Add(booking => booking.SpecialRequests))
            );
            resource.AllowDelete();
        });
        Travellers.Add(admin);
    }
}
