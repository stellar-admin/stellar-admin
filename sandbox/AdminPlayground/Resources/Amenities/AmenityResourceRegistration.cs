using StellarAdmin;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Editors;

namespace AdminPlayground.Resources.Amenities;

// Icon fields: the icon alone in the grid, the icon edited in an inline row and on the create form,
// and the icon with its name on the view page.
internal static class AmenityResourceRegistration
{
    internal static void AddAmenityResource(this StellarAdminBuilder admin)
    {
        admin.Services.AddSingleton<AmenityDataSource>();
        admin.AddResource<Amenity>(resource =>
        {
            resource.SidebarItem(item =>
            {
                item.Group = "Commerce";
                item.Icon = "sparkles";
                item.Order = 26;
            });
            resource.UseDataSource<AmenityDataSource>();
            resource.UseKey(amenity => amenity.Id);
            resource.AllowDelete();
            resource.Index(index =>
                index.Columns(columns =>
                {
                    columns.Add(
                        amenity => amenity.Icon,
                        column => column.UseDisplay<IconDisplay>()
                    );
                    columns.Add(amenity => amenity.Name);
                })
            );
            resource.AllowView(view =>
                view.Fields(fields =>
                {
                    fields.Add(amenity => amenity.Name);
                    fields.Add(amenity => amenity.Icon).UseDisplay<IconDisplay>();
                })
            );
            resource.AllowCreate(create =>
                create.Fields(fields =>
                {
                    fields.Add(amenity => amenity.Name);
                    fields.Add(amenity => amenity.Icon).UseEditor<IconEditor>();
                })
            );
            resource.AllowEdit(edit =>
            {
                edit.Fields(fields =>
                {
                    fields.Add(amenity => amenity.Icon).UseEditor<IconEditor>();
                    fields.Add(amenity => amenity.Name);
                });
                edit.UseInlineRow();
            });
        });
    }
}
