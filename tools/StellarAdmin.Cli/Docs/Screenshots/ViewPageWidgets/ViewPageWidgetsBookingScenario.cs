using Microsoft.EntityFrameworkCore;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Displays;
using StellarAdmin.ScenarioTesting.Host.Models;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.Widgets;
using VoyagerTravel.Data;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ViewPageWidgets;

public sealed class ViewPageWidgetsBookingScenario : Scenario
{
    public override string Description =>
        "A booking's view page with the traveller's other bookings beside its fields, and its notes below them.";

    public override string StartPath => "/stellaradmin/bookings/view/1";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource => resource.Index(index => index.EnablePaging()));
        admin.AddEfCoreResource<Booking>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowView(view =>
            {
                view.Fields(fields =>
                {
                    fields.Add(booking => booking.Reference);
                    fields.Add(booking => booking.Traveller);
                    fields.Add(booking => booking.Departure.Trip);
                    fields.Add(booking => booking.Departure.StartDate);
                    fields.Add(
                        booking => booking.TotalAmount,
                        field => field.UseDisplay<NumberDisplay>(number => number.Format = "C")
                    );
                });
                view.Widgets(widgets =>
                {
                    widgets.Add<OtherBookingsHandler>(
                        "Other bookings",
                        "_OtherBookingsWidget",
                        widget =>
                        {
                            widget.Placement = WidgetPlacement.Aside;
                            widget.Parameters(context => new OtherBookingsParameters(
                                context.Record.Id
                            ));
                        }
                    );
                    widgets.Add(
                        "Notes",
                        "_BookingNotesWidget",
                        widget => widget.Placement = WidgetPlacement.Bottom
                    );
                });
            });
        });
        Travellers.Add(admin);
    }

    public record OtherBookingsParameters(int BookingId);

    public sealed class OtherBookingsHandler(ApplicationDbContext db)
        : IWidgetHandler<OtherBookingsParameters, IReadOnlyList<OtherBooking>>
    {
        public async Task<IReadOnlyList<OtherBooking>?> LoadAsync(
            OtherBookingsParameters parameters,
            WidgetLoadContext context,
            CancellationToken cancellationToken
        )
        {
            var travellerId = await db
                .Bookings.Where(booking => booking.Id == parameters.BookingId)
                .Select(booking => booking.TravellerId)
                .SingleAsync(cancellationToken);

            return await db
                .Bookings.Where(booking =>
                    booking.TravellerId == travellerId && booking.Id != parameters.BookingId
                )
                .OrderBy(booking => booking.Reference)
                .Select(booking => new OtherBooking(
                    booking.Reference,
                    booking.Departure.Trip.Title
                ))
                .ToListAsync(cancellationToken);
        }
    }
}
