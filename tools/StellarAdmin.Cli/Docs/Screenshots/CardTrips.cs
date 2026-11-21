using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Displays;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots;

// The cards scenarios' trips index, with the Voyager Travel branding and the resources the cards' fields name
internal static class CardTrips
{
    public static void Add(
        StellarAdminBuilder admin,
        Action<ResourceCardsDataLayoutBuilder<Trip>> cards,
        bool actions = false
    )
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index => index.AddCards("trips", cards));
            if (actions)
            {
                resource.AllowView(view => view.Fields(fields => fields.Add(trip => trip.Title)));
                resource.AllowEdit(edit => edit.Fields(fields => fields.Add(trip => trip.Title)));
                resource.AllowDelete();
            }
        });
        admin.AddEfCoreResource<Destination>(resource =>
            resource.Index(index => index.AddTable("destinations", table => table.EnablePaging()))
        );
        admin.AddEfCoreResource<Guide>(resource =>
        {
            // A guide has no name property for the default title
            resource.UseTitle(guide => guide.FirstName + " " + guide.LastName);
            resource.Index(index => index.AddTable("guides", table => table.EnablePaging()));
        });
    }

    // The card of the cards page's first example: cover image, title, summary, difficulty, three stats and two fields
    public static void FirstExample(ResourceCardBuilder<Trip> card)
    {
        card.Image(trip => trip.CoverImageUrl);
        card.Title(trip => trip.Title);
        card.Description(trip => trip.Summary);
        card.Badge(trip => trip.Difficulty);
        card.Stats(stats =>
        {
            stats.Add(trip => trip.DurationDays);
            stats.Add(
                trip => trip.BasePrice,
                stat => stat.UseDisplay<NumberDisplay>(number => number.Format = "C0")
            );
            stats.Add(trip => trip.MaxGroupSize);
        });
        card.Fields(fields =>
        {
            fields.Add(trip => trip.Destination);
            fields.Add(trip => trip.LeadGuide);
        });
    }
}
