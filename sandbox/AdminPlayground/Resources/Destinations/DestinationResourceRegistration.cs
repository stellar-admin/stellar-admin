using StellarAdmin;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Editors;

namespace AdminPlayground.Resources.Destinations;

// Image fields: thumbnails in the grid, a photo edited in an inline row and on the create form,
// and the photo as a figure on the view page. Valparaíso's photo is missing, to show a broken image.
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
            resource.Index(index =>
                index.Columns(columns =>
                {
                    columns.Add(
                        destination => destination.Photo,
                        column => column.UseDisplay<ImageDisplay>(ConfigureDisplay)
                    );
                    columns.Add(destination => destination.Name);
                    columns.Add(destination => destination.Country);
                })
            );
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
            resource.AllowCreate(create => create.Fields(ConfigureFields));
            resource.AllowEdit(edit =>
            {
                edit.Fields(ConfigureFields);
                edit.UseInlineRow();
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
        fields.Add(destination => destination.Photo).UseEditor<ImageEditor>(ConfigureEditor);
        fields.Add(destination => destination.Name);
        fields.Add(destination => destination.Country);
    }
}
