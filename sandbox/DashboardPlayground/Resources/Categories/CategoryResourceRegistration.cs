using DashboardPlayground.Data;
using StellarAdmin.Dashboard;
using StellarAdmin.Dashboard.EntityFrameworkCore;

namespace DashboardPlayground.Resources.Categories;

internal static class CategoryResourceRegistration
{
    // The product category lookups create categories with this resource's create form
    internal static void AddCategoryResource(this StellarAdminDashboardBuilder dashboard)
    {
        dashboard.AddEfCoreResource<ApplicationDbContext, Category>(resource =>
        {
            resource.SidebarItem(item =>
            {
                item.Group = "Commerce";
                item.Order = 15;
            });
            resource.Index(index =>
            {
                index.Columns(columns =>
                    columns.Add(category => category.Name, column => column.Sortable())
                );
                index.DefaultSortBy(category => category.Name);
                index.EnableSearch(
                    term => category => category.Name.Contains(term),
                    search => search.Placeholder = "Search categories..."
                );
                index.EnablePaging();
            });
            resource.AllowCreate(create =>
                create.Fields(fields => fields.Add(category => category.Name))
            );
            resource.AllowEdit(edit =>
                edit.Fields(fields => fields.Add(category => category.Name))
            );
        });
    }
}
