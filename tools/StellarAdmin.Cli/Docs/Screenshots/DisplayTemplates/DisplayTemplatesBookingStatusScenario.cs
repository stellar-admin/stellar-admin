using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.DisplayTemplates;

// The bookings grid and a booking's view page with the host's BookingStatus display templates. The seeded
// Booking has no [UIHint], so a metadata provider gives its Status the template hint instead.
public sealed class DisplayTemplatesBookingStatusScenario : Scenario
{
    public override string Description =>
        "The bookings grid and a booking's view page showing its status with the BookingStatus display templates.";

    public override string StartPath => "/stellaradmin/bookings";

    public override void ConfigureServices(IServiceCollection services) =>
        services.Configure<MvcOptions>(options =>
            options.ModelMetadataDetailsProviders.Add(new BookingStatusHint())
        );

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        Travellers.Add(admin);
        admin.AddEfCoreResource<Booking>(resource =>
        {
            resource.UseTitle(booking => booking.Reference);
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(booking => booking.Reference);
                    columns.Add(booking => booking.Traveller);
                    columns.Add(booking => booking.BookedAt, column => column.Sortable());
                    columns.Add(booking => booking.Status);
                });
                index.DefaultSortByDescending(booking => booking.BookedAt);
                // Five rows, so the grid crop stays short
                index.EnablePaging(paging => paging.PageSize = 5);
            });
            resource.AllowView(view =>
                view.Fields(fields =>
                {
                    fields.Add(booking => booking.Reference);
                    fields.Add(booking => booking.Traveller);
                    fields.Add(booking => booking.BookedAt);
                    fields.Add(booking => booking.Status);
                })
            );
        });
    }

    private sealed class BookingStatusHint : IDisplayMetadataProvider
    {
        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            if (context.Key.ContainerType == typeof(Booking) && context.Key.Name == nameof(Booking.Status))
            {
                context.DisplayMetadata.TemplateHint = "BookingStatus";
            }
        }
    }
}
