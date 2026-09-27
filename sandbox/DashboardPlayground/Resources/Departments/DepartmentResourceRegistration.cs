using DashboardPlayground.Data;
using StellarAdmin.Dashboard;
using StellarAdmin.Dashboard.EntityFrameworkCore;

namespace DashboardPlayground.Resources.Departments;

internal static class DepartmentResourceRegistration
{
    internal static void AddDepartmentResource(this StellarAdminDashboardBuilder dashboard)
    {
        dashboard.AddEfCoreResource<ApplicationDbContext, Department>(resource =>
        {
            resource.SidebarItem(item =>
            {
                item.Label = "Departments";
                item.Group = "Identity";
                item.Order = 20;
            });
            resource.Index(index =>
            {
                index.Columns(columns =>
                    columns.Add(department => department.Name, column => column.Sortable())
                );
                index.DefaultSortBy(department => department.Name);
                index.EnableSearch(
                    term => department => department.Name.Contains(term),
                    search => search.Placeholder = "Search departments..."
                );
                index.EnablePaging();
            });
            resource.AllowCreate(create =>
                create.Fields(fields => fields.Add(department => department.Name))
            );
            resource.AllowEdit(edit =>
                edit.Fields(fields => fields.Add(department => department.Name))
            );
        });
    }
}
