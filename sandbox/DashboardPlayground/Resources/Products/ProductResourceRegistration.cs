using DashboardPlayground.Data;
using StellarAdmin.Dashboard;
using StellarAdmin.Dashboard.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources.Options;

namespace DashboardPlayground.Resources.Products;

internal static class ProductResourceRegistration
{
    internal static void AddProductResource(this StellarAdminDashboardBuilder dashboard)
    {
        dashboard.AddEfCoreResource<ApplicationDbContext, Product>(resource =>
        {
            resource.SidebarItem(item =>
            {
                item.Group = "Commerce";
                item.Order = 10;
            });
            resource.AllowCreate(create =>
                create.Fields(fields =>
                {
                    fields.Add(product => product.Name);
                    fields.Add(product => product.Price);
                    fields.Add(product => product.Details.Sku);
                    fields.Add(
                        product => product.CategoryId,
                        field =>
                        {
                            field.Title = "Category";
                            field.UseEditor<SelectListEditorOptions>(options =>
                                options.UseItems<ApplicationDbContext, Category, int>(
                                    category => category.Id,
                                    category => category.Name,
                                    items =>
                                    {
                                        items.OrderBy(category => category.Name);
                                        items.IncludeEmptyOption("Not set");
                                    }
                                )
                            );
                        }
                    );
                })
            );
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(product => product.Name);
                    fields.Add(product => product.Price);
                    fields.Add(product => product.Details.Sku);
                    fields.Add(
                        product => product.CategoryId,
                        field =>
                        {
                            field.Title = "Category";
                            field.UseEditor<SelectListEditorOptions>(options =>
                                options.UseItems<ApplicationDbContext, Category, int>(
                                    category => category.Id,
                                    category => category.Name,
                                    items =>
                                    {
                                        items.OrderBy(category => category.Name);
                                        items.IncludeEmptyOption("Not set");
                                    }
                                )
                            );
                        }
                    );
                })
            );
            resource.AllowDelete();
            resource.Index(index =>
            {
                index.EnableScopes(scopes =>
                {
                    scopes.Add("all", "All products");
                    scopes.Add("under-50", "Under 50", product => product.Price < 50);
                    scopes.Add("50-and-over", "50 and over", product => product.Price >= 50);
                    scopes.DefaultScope = "all";
                });
                index.EnableSearch(
                    term => product => product.Name.Contains(term),
                    search => search.Placeholder = "Search products..."
                );
                index.DefaultSortBy(product => product.Name);
                index.EnablePaging(paging =>
                {
                    paging.PageSize = 10;
                    paging.PageSizes = [10, 25, 50];
                });
                index.Columns(columns =>
                {
                    columns.Add(product => product.Id);
                    columns.Add(product => product.Details.Sku, column => column.Sortable());
                    columns.Add(product => product.CategoryId, column => column.Sortable());
                    columns.Add(
                        product => product.Name,
                        column => column.Sortable(product => product.Name.ToLower())
                    );
                    columns.Add(
                        product => product.Price,
                        column =>
                        {
                            column.Sortable();
                            column.Title = "Unit price";
                            column.Format = "{0:0.00}";
                        }
                    );
                });
            });
        });
    }
}
