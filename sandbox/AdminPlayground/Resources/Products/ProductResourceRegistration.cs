using AdminPlayground.Data;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Editors;

namespace AdminPlayground.Resources.Products;

internal static class ProductResourceRegistration
{
    internal static void AddProductResource(this StellarAdminBuilder admin)
    {
        admin.AddEfCoreResource<Product>(resource =>
        {
            resource.UseTitle(product =>
                product.Category != null
                    ? product.Category.Name + " / " + product.Name
                    : product.Name
            );
            resource.SidebarItem(item =>
            {
                item.Group = "Commerce";
                item.Icon = "package";
                item.Order = 10;
            });
            resource.AllowCreate(create =>
                create.Fields(fields =>
                {
                    fields.Add(product => product.Name);
                    fields.Add(product => product.Price);
                    fields.Add(product => product.Details.Sku);
                    fields
                        .Add(product => product.CategoryId)
                        .UseLookupSheetEditor(UseCategories);
                    fields.Add(product => product.Tags).UseMultiLookupSheetEditor(UseTags);
                })
            );
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(product => product.Name);
                    fields.Add(product => product.Price);
                    fields.Add(product => product.Details.Sku);
                    // No editor: the foreign key gets a lookup sheet of the category resource
                    fields.Add(product => product.CategoryId);
                    fields.AddDisplayField(
                        product => string.Join(", ", product.Tags.Select(t => t.Name)),
                        field => field.Title = "Tags (2)"
                    );
                    fields
                        .Add(product => product.Tags)
                        .UseCheckboxGroupEditor(checkboxes =>
                        {
                            checkboxes.Appearance = CheckboxGroupAppearance.Cards;
                            checkboxes.Columns(2);
                        });
                })
            );
            resource.AllowView(view =>
                view.Fields(fields =>
                {
                    fields.AddSection(
                        "Product",
                        section =>
                        {
                            section.Columns(2);
                            section.Add(product => product.Name);
                            section.Add(product => product.Category);
                            section.Add(
                                product => string.Join(", ", product.Tags.Select(t => t.Name)),
                                field => field.Title = "Tags (2)"
                            );
                            section.Add(product => product.Tags).UseDisplay<BadgeDisplay>();
                        }
                    );
                    fields.AddSection(
                        "Pricing",
                        section =>
                        {
                            section.Columns(2);
                            section
                                .Add(product => product.Price)
                                .UseDisplay<NumberDisplay>(number => number.Format = "0.00");
                            section.Add(product => product.Details.Sku);
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
                // Pricing loads when a row first expands, with one query for that product. Overview renders with
                // the index, and Related loads when its tab is first shown
                index.EnableRowDetails(panels =>
                {
                    panels.Add(
                        "Pricing",
                        "_ProductPricing",
                        product => new ProductPricing(
                            product.Name,
                            product.Details.Sku,
                            product.Category != null ? product.Category.Name : null,
                            product.Price
                        )
                    );
                    panels.Add("Overview", "_ProductOverview");
                    panels.Add<RelatedProducts, RelatedProductsHandler>(
                        "Related",
                        "_RelatedProducts"
                    );
                });
                index.EnablePaging(paging =>
                {
                    paging.PageSize = 10;
                    paging.PageSizes = [10, 25, 50];
                });
                index.Columns(columns =>
                {
                    columns.Add(product => product.Id);
                    columns.Add(product => product.Details.Sku, column => column.Sortable());
                    columns.Add(
                        product => product.Category,
                        column => column.Sortable(product => product.Category!.Name)
                    );
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
                            column.UseDisplay<NumberDisplay>(number => number.Format = "0.00");
                        }
                    );
                    // columns.Add(product => product.Tags, column => column.UseDisplay<BadgeDisplay>());
                    columns.Add(
                        product => string.Join(",", product.Tags.Select(t => t.Name)),
                        column =>
                        {
                            column.Title = "Tags";
                            column.Sortable(c => c.Tags.Count);
                        }
                    );
                });
            });
        });
    }

    // The items come from the category resource, which the foreign key names. New opens its create form, and selects
    // the category it creates
    private static void UseCategories(LookupSheetEditor lookup)
    {
        lookup.Sheet(sheet =>
        {
            sheet.Title = "Select category";
            sheet.SearchPlaceholder = "Search categories...";
        });
        lookup.EnableCreate();
    }

    // The items come from the tag resource, which the many-to-many navigation names. New opens its create form, and
    // adds the tag it creates to the selection
    private static void UseTags(MultiLookupSheetEditor lookup)
    {
        lookup.Sheet(sheet =>
        {
            sheet.Title = "Select tags";
            sheet.SearchPlaceholder = "Search tags...";
        });
        lookup.Field(field => field.Layout = MultiLookupSheetEditorLayout.Chips);
        lookup.EnableCreate();
    }
}
