using AdminPlayground.Data;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;

namespace AdminPlayground.Resources.Departments;

internal static class DepartmentResourceRegistration
{
    internal static void AddDepartmentResource(this StellarAdminBuilder admin)
    {
        admin.AddEfCoreResource<ApplicationDbContext, Department>(resource =>
        {
            resource.SidebarItem(item =>
            {
                item.Label = "Departments";
                item.Icon = "building";
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
