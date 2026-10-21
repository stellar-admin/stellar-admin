using AdminPlayground.Resources.Destinations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StellarAdmin;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Editors;

namespace AdminPlayground.Resources.Guides;

// Rich text: a bio edited with the rich text editor, with images stored in the playground's upload
// folder, shown as plain text in the grid and formatted on the view page. Sofia's bio is plain text
// from before the field was rich text.
internal static class GuideResourceRegistration
{
    internal static void AddGuideResource(this StellarAdminBuilder admin)
    {
        admin.Services.AddSingleton<GuideDataSource>();
        admin.Services.TryAddSingleton<PlaygroundImageStore>();
        admin.AddResource<Guide>(resource =>
        {
            resource.SidebarItem(item =>
            {
                item.Group = "Commerce";
                item.Icon = "user-round";
                item.Order = 26;
            });
            resource.UseDataSource<GuideDataSource>();
            resource.UseKey(guide => guide.Id);
            resource.AllowDelete();
            resource.Index(index =>
                index.Columns(columns =>
                {
                    columns.Add(guide => guide.Name);
                    columns.Add(guide => guide.Bio);
                })
            );
            resource.AllowView(view =>
                view.Fields(fields =>
                {
                    fields.Add(guide => guide.Name);
                    fields.Add(guide => guide.Bio);
                })
            );
            resource.AllowCreate(create => create.Fields(ConfigureFields));
            resource.AllowEdit(edit => edit.Fields(ConfigureFields));
        });
    }

    private static void ConfigureFields(ResourceFieldsBuilder<Guide> fields)
    {
        fields.Add(guide => guide.Name);
        fields
            .Add(guide => guide.Bio)
            .UseEditor<RichTextEditor>(editor =>
            {
                editor.Placeholder = "Tell travellers about this guide…";
                editor.StoreImage(
                    async (file, services, cancellationToken) =>
                        PlaygroundImageStore.GetUrl(
                            await services
                                .GetRequiredService<PlaygroundImageStore>()
                                .SaveAsync(file, cancellationToken)
                        )
                );
            });
    }
}
