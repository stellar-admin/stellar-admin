using StellarAdmin;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Editors;

namespace AdminPlayground.Resources.Destinations;

// Image fields: thumbnails in the grid, a photo edited in an inline row and on the create form,
// and the photo as a figure on the view page. Valparaíso's photo is missing, to show a broken image.
// The create form saves and adds another, keeping the country. A second layout lists the destinations per country.
internal static class DestinationResourceRegistration
{
    internal static void AddDestinationResource(this StellarAdminBuilder admin)
    {
        admin.Services.AddSingleton<DestinationDataSource>();
        admin.Services.AddSingleton<PlaygroundImageStore>();
        admin.AddResource<Destination>(resource =>
        {
            resource.SidebarItem(item =>
            {
                item.Group = "Commerce";
                item.Icon = "map-pin";
                item.Order = 25;
            });
            resource.UseDataSource<DestinationDataSource>();
            resource.UseKey(destination => destination.Id);
            resource.AllowDelete();
            // The destinations in a table, as cards, and grouped by country with a handler that loads its own data
            resource.Index(index =>
            {
                index.AddTable(
                    "destinations",
                    table =>
                    {
                        table.Title = "Destinations";
                        table.Icon = "table";
                        table.Columns(columns =>
                        {
                            columns.Add(
                                destination => destination.Photo,
                                column => column.UseDisplay<ImageDisplay>(ConfigureDisplay)
                            );
                            columns.Add(destination => destination.Name);
                            columns.Add(destination => destination.Country);
                        });
                        table.EnableInlineEditing();
                    }
                );
                // The built-in cards layout: each destination's photo, with its country under its name
                index.AddCards(
                    "cards",
                    cards =>
                    {
                        cards.Title = "Cards";
                        cards.Icon = "layout-grid";
                        cards.DefaultSortBy(destination => destination.Name);
                        cards.EnablePaging(paging => paging.PageSize = 12);
                        cards.Card(card =>
                        {
                            card.Description(destination => destination.Country);
                            card.Image(destination =>
                                destination.Photo == null
                                    ? null
                                    : PlaygroundImageStore.GetUrl(destination.Photo)
                            );
                        });
                    }
                );
                // The title and country over a square photo, with the country as a badge too
                index.AddCards(
                    "overlay",
                    cards =>
                    {
                        cards.Title = "Overlay";
                        cards.Icon = "image";
                        cards.DefaultSortBy(destination => destination.Name);
                        cards.Card(card =>
                        {
                            card.Arrangement = ResourceCardArrangement.Overlay;
                            card.Description(destination => destination.Country);
                            card.Image(
                                destination =>
                                    destination.Photo == null
                                        ? null
                                        : PlaygroundImageStore.GetUrl(destination.Photo),
                                image => image.Shape = ResourceCardImageShape.Square
                            );
                            card.Badge(destination => destination.Country);
                        });
                    }
                );
                // The photo beside the content, with a stat, a field and the actions in the footer
                index.AddCards(
                    "horizontal",
                    cards =>
                    {
                        cards.Title = "Horizontal";
                        cards.Icon = "layout-list";
                        cards.DefaultSortBy(destination => destination.Name);
                        cards.Card(card =>
                        {
                            card.Arrangement = ResourceCardArrangement.Horizontal;
                            card.ActionsPlacement = ResourceCardActionsPlacement.Footer;
                            card.Image(
                                destination =>
                                    destination.Photo == null
                                        ? null
                                        : PlaygroundImageStore.GetUrl(destination.Photo),
                                image => image.Shape = ResourceCardImageShape.Square
                            );
                            card.Badge(destination => destination.Country);
                            card.Stats(stats => stats.Add(destination => destination.Id));
                            card.Fields(fields => fields.Add(destination => destination.Country));
                        });
                    }
                );
                index.AddPartial<DestinationCountry, DestinationCountriesHandler>(
                    "countries",
                    "_DestinationCountries",
                    countries =>
                    {
                        countries.Title = "Countries";
                        countries.Icon = "globe";
                    }
                );
            });
            resource.AllowView(view =>
                view.Fields(fields =>
                {
                    fields.Add(destination => destination.Name);
                    fields.Add(destination => destination.Country);
                    fields
                        .Add(destination => destination.Photo)
                        .UseDisplay<ImageDisplay>(ConfigureDisplay);
                })
            );
            // Save and add another keeps the country, for entering several destinations in one country
            resource.AllowCreate(create =>
            {
                create.Fields(ConfigureFields);
                create.EnableAddAnother(another =>
                    another.KeepForNext(destination => destination.Country)
                );
            });
            resource.AllowEdit(edit =>
            {
                edit.Fields(ConfigureFields);
            });
        });
    }

    internal static void ConfigureEditor(ImageEditor editor)
    {
        editor.ResolveUrl(PlaygroundImageStore.GetUrl);
        editor.StoreImage(
            (file, services, cancellationToken) =>
                services
                    .GetRequiredService<PlaygroundImageStore>()
                    .SaveAsync(file, cancellationToken)
        );
    }

    private static void ConfigureDisplay(ImageDisplay display) =>
        display.ResolveUrl(PlaygroundImageStore.GetUrl);

    private static void ConfigureFields(ResourceFieldsBuilder<Destination> fields)
    {
        fields.Add(destination => destination.Photo).UseImageEditor(ConfigureEditor);
        fields.Add(destination => destination.Name);
        fields.Add(destination => destination.Country);
    }
}
