using AdminPlayground.Data;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;

namespace AdminPlayground.Resources.Tags;

internal static class TagResourceRegistration
{
    // The product tag lookups create tags with this resource's create form
    internal static void AddTagResource(this StellarAdminBuilder admin)
    {
        admin.AddEfCoreResource<ApplicationDbContext, Tag>(resource =>
        {
            resource.SidebarItem(item =>
            {
                item.Group = "Commerce";
                item.Icon = "tag";
                item.Order = 20;
            });
            resource.Index(index =>
            {
                index.Columns(columns => columns.Add(tag => tag.Name, column => column.Sortable()));
                index.DefaultSortBy(tag => tag.Name);
                index.EnableSearch(
                    term => tag => tag.Name.Contains(term),
                    search => search.Placeholder = "Search tags..."
                );
            });
            resource.AllowCreate(create => create.Fields(fields => fields.Add(tag => tag.Name)));
            resource.AllowEdit(edit => edit.Fields(fields => fields.Add(tag => tag.Name)));
            resource.AllowDelete();
        });
    }
}
