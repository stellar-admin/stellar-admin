using AdminPlayground.Data;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;

namespace AdminPlayground.Resources.Departments;

internal static class DepartmentResourceRegistration
{
    internal static void AddDepartmentResource(this StellarAdminBuilder admin)
    {
        admin.AddEfCoreResource<Department>(resource =>
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
                index.EnableSearch(
                    term => department => department.Name.Contains(term),
                    search => search.Placeholder = "Search departments..."
                );
                index.AddTable(
                    "departments",
                    table =>
                    {
                        table.Columns(columns =>
                            columns.Add(department => department.Name, column => column.Sortable())
                        );
                        table.DefaultSortBy(department => department.Name);
                        table.EnablePaging();
                        table.EnableInlineCreate();
                        table.EnableInlineEditing();
                    }
                );
            });
            // Departments are edited and added in the index grid's rows, several in a row with add another
            resource.AllowCreate(create =>
            {
                create.Fields(fields => fields.Add(department => department.Name));
                create.EnableAddAnother();
            });
            resource.AllowEdit(edit =>
            {
                edit.Fields(fields => fields.Add(department => department.Name));
            });
        });
    }
}
