using DashboardPlayground.Data;
using StellarAdmin.Dashboard;

namespace DashboardPlayground.Resources.Roles;

internal static class RoleResourceRegistration
{
    internal static void AddRoleResource(this StellarAdminDashboardBuilder dashboard)
    {
        dashboard.AddResource<ApplicationRole>(resource =>
        {
            resource.SingularLabel = "role";
            resource.PluralLabel = "roles";
            resource.SidebarItem(item =>
            {
                item.Label = "Roles";
                item.Group = "Identity";
                item.Order = 10;
            });
            resource.UseDataSource<RoleDataSource>();
            resource.UseKey(role => role.Id);
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(role => role.Name, column => column.Sortable());
                    columns.Add(role => role.Description);
                });
                index.DefaultSortBy(role => role.Name);
                index.EnableSearch(search => search.Placeholder = "Search roles...");
                index.EnablePaging();
            });
            resource.AllowCreate<RoleFormModel, CreateRoleHandler>(create =>
                create.Fields(fields =>
                {
                    fields.Add(model => model.Name);
                    fields.Add(model => model.Description);
                })
            );
            resource.AllowEdit<RoleFormModel, EditRoleHandler>(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(model => model.Name);
                    fields.Add(model => model.Description);
                })
            );
            resource.AllowDelete(delete =>
                delete.Message = "Delete this role? This cannot be undone."
            );
        });
    }
}
