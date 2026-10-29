using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Builders;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots;

// The form layout scenarios' trip edit page, with the sections of the docs' first form layout example
internal static class FormLayoutTrips
{
    // The resources the trip's destination and lead guide lookups select from
    public static void AddRelatedResources(StellarAdminBuilder admin)
    {
        admin.AddEfCoreResource<Destination>(resource => resource.Index(index => index.EnablePaging()));
        admin.AddEfCoreResource<Guide>(resource =>
        {
            // A guide has no name property for the default title
            resource.UseTitle(guide => guide.FirstName + " " + guide.LastName);
            resource.Index(index => index.EnablePaging());
        });
    }

    public static void AddSections(ResourceFieldsBuilder<Trip> fields)
    {
        fields.AddSection(
            "Trip",
            section =>
            {
                section.Description = "What people see when they browse trips.";
                section.Columns(2);
                section.Add(trip => trip.Title).ColumnSpanFull();
                section.Add(trip => trip.DestinationId);
                section.Add(trip => trip.LeadGuideId);
                section.Add(trip => trip.Summary).ColumnSpanFull();
            }
        );

        fields.AddSection(
            "Pricing",
            section =>
            {
                section.Columns(3);
                section.Add(trip => trip.BasePrice);
                section.Add(trip => trip.DurationDays);
                section.Add(trip => trip.MaxGroupSize);
            }
        );
    }
}
