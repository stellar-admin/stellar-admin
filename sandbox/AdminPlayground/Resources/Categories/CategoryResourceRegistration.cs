using AdminPlayground.Data;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.UI;

namespace AdminPlayground.Resources.Categories;

internal static class CategoryResourceRegistration
{
    // The product category lookups create categories with this resource's create form
    internal static void AddCategoryResource(this StellarAdminBuilder admin)
    {
        admin.AddEfCoreResource<ApplicationDbContext, Category>(resource =>
        {
            resource.SidebarItem(item =>
            {
                item.Group = "Commerce";
                item.Icon = "tags";
                item.Order = 15;
            });
            // Fields that refer to a category show its name, its title by convention, with a description and an
            // avatar. Categories have no image, so the avatar shows the name's initials
            resource.Reference(reference =>
                reference
                    .UseDescription(category => "Category #" + category.Id.ToString())
                    .UseAvatar(category => null)
            );
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
                index.EnableRowDetails<CategoryProducts, CategoryProductsHandler>(
                    "_CategoryProducts",
                    details =>
                    {
                        details.Toggle = DataGridRowDetailToggle.End;
                        details.ExpandMode = DataGridRowDetailExpandMode.Single;
                    }
                );
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
