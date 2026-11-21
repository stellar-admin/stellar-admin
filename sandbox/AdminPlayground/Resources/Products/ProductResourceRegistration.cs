using AdminPlayground.Dashboard;
using AdminPlayground.Data;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Editors;
using StellarAdmin.UI;
using StellarAdmin.UI.Toasts;
using StellarAdmin.Widgets;

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
            // Under the title of the view page; the edit page shows a model of its own, so it sets its own summary
            resource.Summary(summary =>
            {
                summary.SeparateItems = true;
                summary.Add(product => product.Details.Sku);
                summary.Add(product => product.Category);
                summary.Add(product => product.Tags, item => item.UseDisplay<BadgeDisplay>());
            });
            resource.AllowCreate(create =>
                create
                    .Fields(fields =>
                    {
                        fields.Add(product => product.Name);
                        fields.Add(product => product.Price);
                        fields.Add(product => product.Details.Sku);
                        fields
                            .Add(product => product.CategoryId)
                            .UseLookupSheetEditor(UseCategories);
                        fields.Add(product => product.Tags).UseMultiLookupSheetEditor(UseTags);
                    })
                    // A tip above the form, which has no handler, so it renders with the page
                    .Widgets(widgets => widgets.Add("Category tip", "_ProductCreateTipWidget"))
            );
            // The form asks for a reason for the change, which the product doesn't store, and the events read it
            resource.AllowEdit<EditProductModel>(
                model => model.Product,
                edit =>
                {
                    edit.Events(events =>
                    {
                        // Compares the stored price with the new one, and rejects the save with an error on the field
                        events.OnUpdating = (context, _) =>
                        {
                            var original = context
                                .Entry.Property(product => product.Price)
                                .OriginalValue;
                            if (context.Entity.Price < original / 2)
                            {
                                context.Fail(
                                    model => model.Product.Price,
                                    "The price can't drop by more than half in one change."
                                );
                            }

                            return Task.CompletedTask;
                        };
                        events.OnUpdated = (context, _) =>
                        {
                            context
                                .Services.GetRequiredService<IToastNotifier>()
                                .Success($"{context.Entity.Name} changed: {context.Model.Reason}");

                            return Task.CompletedTask;
                        };
                    });
                    // The stored price, also after a rejected save
                    edit.Summary(summary =>
                    {
                        summary.AddText("SKU");
                        summary.Add(model => model.Product.Details.Sku);
                        summary.AddSeparator();
                        summary.AddText("Price");
                        summary.Add(
                            model => model.Product.Price,
                            item => item.UseDisplay<NumberDisplay>(number => number.Format = "0.00")
                        );
                    });
                    edit.Fields(fields =>
                        {
                            fields.Add(model => model.Product.Name);
                            fields.Add(model => model.Product.Price);
                            fields.Add(model => model.Product.Details.Sku);
                            // No editor: the foreign key gets a lookup sheet of the category resource
                            fields.Add(model => model.Product.CategoryId);
                            fields.AddDisplayField(
                                model => string.Join(", ", model.Product.Tags.Select(t => t.Name)),
                                field => field.Title = "Tags (2)"
                            );
                            fields
                                .Add(model => model.Product.Tags)
                                .UseCheckboxGroupEditor(checkboxes =>
                                {
                                    checkboxes.Appearance = CheckboxGroupAppearance.Cards;
                                    checkboxes.Columns(2);
                                });
                            fields.Add(model => model.Reason);
                        })
                        // The category's prices beside the form, which stay in view as the form scrolls
                        .Widgets(widgets =>
                            AddCategoryPricesWidget(widgets, model => model.Product.Id)
                        );
                }
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
                    .Widgets(widgets => AddCategoryPricesWidget(widgets, product => product.Id))
            );
            resource.AllowDelete();
            // Two tables the switcher at the end of the controls row moves between: the catalog, and prices from the
            // most expensive. The scopes, search and widgets are shared, so they carry over when switching
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
                // A price summary of the listed products above the grid, which follows the scope and search, and the
                // top categories beside it
                index.Widgets(widgets =>
                {
                    widgets.Add<ProductPriceSummaryHandler>(
                        "Price summary",
                        "_ProductPriceSummaryWidget",
                        widget =>
                            widget.Parameters(context => new ProductPriceSummaryParameters(
                                context.Scope,
                                context.Search
                            ))
                    );
                    widgets.Add<TopCategoriesHandler>(
                        "Top categories",
                        "_TopCategoriesWidget",
                        widget =>
                        {
                            widget.Placement = WidgetPlacement.Aside;
                            widget.Parameters(_ => new TopCategoriesParameters(Count: 5));
                        }
                    );
                });
                index.AddTable(
                    "catalog",
                    table =>
                    {
                        table.Title = "Catalog";
                        table.Icon = "table";
                        table.DefaultSortBy(product => product.Name);
                        // Pricing loads when it's first opened, with one query for that product. Overview renders with
                        // the index, and Related loads when it's first opened
                        table.EnableRowDetails(panels =>
                        {
                            panels.Add(
                                "Pricing",
                                "_ProductPricing",
                                product => new ProductPricing(
                                    product.Name,
                                    product.Details.Sku,
                                    product.Category != null ? product.Category.Name : null,
                                    product.Price
                                ),
                                panel => panel.Icon = "tag"
                            );
                            panels.Add(
                                "Overview",
                                "_ProductOverview",
                                panel => panel.Icon = "package"
                            );
                            panels.Add<RelatedProducts, RelatedProductsHandler>(
                                "Related",
                                "_RelatedProducts",
                                panel => panel.Icon = "link"
                            );
                        });
                        table.EnablePaging(paging =>
                        {
                            paging.PageSize = 10;
                            paging.PageSizes = [10, 25, 50];
                        });
                        table.Columns(columns =>
                        {
                            columns.Add(product => product.Id);
                            columns.Add(
                                product => product.Details.Sku,
                                column => column.Sortable()
                            );
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
                                    column.UseDisplay<NumberDisplay>(number =>
                                        number.Format = "0.00"
                                    );
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
                    }
                );
                // Its own columns, sort and page size, and no delete button
                index.AddTable(
                    "pricing",
                    table =>
                    {
                        table.Title = "Pricing";
                        table.Icon = "tag";
                        table.DefaultSortByDescending(product => product.Price);
                        table.EnablePaging(paging => paging.PageSize = 25);
                        table.RowActions(actions => actions.Delete = false);
                        table.Columns(columns =>
                        {
                            columns.Add(
                                product => product.Name,
                                column => column.Sortable(product => product.Name.ToLower())
                            );
                            columns.Add(
                                product => product.Category,
                                column => column.Sortable(product => product.Category!.Name)
                            );
                            columns.Add(
                                product => product.Price,
                                column =>
                                {
                                    column.Sortable();
                                    column.Title = "Unit price";
                                    column.UseDisplay<NumberDisplay>(number =>
                                        number.Format = "0.00"
                                    );
                                }
                            );
                        });
                    }
                );
                // Every listed product as a card, from Areas/StellarAdmin/Views/Products/_ProductCards
                index.AddPartial(
                    "cards",
                    "_ProductCards",
                    cards =>
                    {
                        cards.Title = "Cards";
                        cards.Icon = "layout-grid";
                    }
                );
                // The built-in cards layout, without images: the category as a badge, the price and id as stats,
                // and the SKU and tags in a grid. Edit is a button in the footer, and delete is in its menu
                index.AddCards(
                    "built-in-cards",
                    cards =>
                    {
                        cards.Title = "Built-in cards";
                        cards.Icon = "layout-dashboard";
                        cards.DefaultSortBy(product => product.Name);
                        cards.EnablePaging(paging => paging.PageSize = 12);
                        cards.Card(card =>
                        {
                            card.Title(product => product.Name);
                            card.Description(product => product.Details.Sku);
                            card.Badge(product => product.Category!.Name);
                            card.Stats(stats =>
                            {
                                stats.Add(
                                    product => product.Price,
                                    stat =>
                                    {
                                        stat.Title = "Unit price";
                                        stat.UseDisplay<NumberDisplay>(number =>
                                            number.Format = "0.00"
                                        );
                                    }
                                );
                                stats.Add(product => product.Id);
                            });
                            card.Fields(fields =>
                            {
                                fields.Layout = ResourceCardFieldLayout.Grid;
                                fields.Add(product => product.Category);
                                fields
                                    .Add(product => product.Tags)
                                    .UseDisplay<BadgeDisplay>(badge => badge.Limit = 2);
                            });
                            card.ActionsPlacement = ResourceCardActionsPlacement.Footer;
                        });
                    }
                );
                // Prices per category of the products the scope and search select, from the narrowed query
                index.AddPartial<CategoryPriceSummary, CategoryPriceSummaryHandler>(
                    "categories",
                    "_CategoryPriceSummary",
                    categories =>
                    {
                        categories.Title = "Prices per category";
                        categories.Icon = "chart-column";
                        // Each card's title links to its category, apart from the products without one
                        categories.LinkTo<Category>(summary => summary.CategoryId);
                    }
                );
                // Each product against its category's average, from a handler that sorts and pages the rows. Each
                // row is a product, so it has the product's row actions
                index.AddTable<ProductPriceCheck, ProductPriceCheckHandler>(
                    "price-check",
                    table =>
                    {
                        table.Title = "Price check";
                        table.Icon = "scale";
                        table.HasRecordKey(row => row.ProductId);
                        table.DefaultSortBy(row => row.Name);
                        table.EnablePaging(paging => paging.PageSize = 10);
                        table.Columns(columns =>
                        {
                            columns.Add(row => row.Name, column => column.Sortable());
                            columns.Add(
                                row => row.Category,
                                column =>
                                {
                                    column.Sortable();
                                    column.EmptyText = "No category";
                                }
                            );
                            columns.Add(
                                row => row.Price,
                                column =>
                                {
                                    column.Sortable();
                                    column.UseDisplay<NumberDisplay>(number =>
                                        number.Format = "0.00"
                                    );
                                }
                            );
                            columns.Add(
                                row => row.CategoryAverage,
                                column =>
                                    column.UseDisplay<NumberDisplay>(number =>
                                        number.Format = "0.00"
                                    )
                            );
                            columns.Add(
                                row => row.Difference,
                                column =>
                                {
                                    column.Sortable();
                                    column.UseDisplay<NumberDisplay>(number =>
                                        number.Format = "+0.00;-0.00;0.00"
                                    );
                                }
                            );
                        });
                    }
                );
                // The products per category, each row linking to its category's view page
                index.AddTable<CategorySummary, CategorySummaryHandler>(
                    "category-summary",
                    table =>
                    {
                        table.Title = "Category summary";
                        table.Icon = "tags";
                        table.LinkTo<Category>(row => row.CategoryId);
                        table.Columns(columns =>
                        {
                            columns.Add(row => row.Category, column => column.Sortable());
                            columns.Add(row => row.Products, column => column.Sortable());
                            columns.Add(
                                row => row.Average,
                                column =>
                                {
                                    column.Title = "Average price";
                                    column.Sortable();
                                    column.UseDisplay<NumberDisplay>(number =>
                                        number.Format = "0.00"
                                    );
                                }
                            );
                        });
                    }
                );
                // The price check as cards. Each card is a product, so it has the product's row actions
                index.AddCards<ProductPriceCheck, ProductPriceCheckHandler>(
                    "price-check-cards",
                    cards =>
                    {
                        cards.Title = "Price check cards";
                        cards.Icon = "wallet-cards";
                        cards.HasRecordKey(row => row.ProductId);
                        cards.DefaultSortBy(row => row.Name);
                        cards.EnablePaging(paging => paging.PageSize = 12);
                        cards.Card(card =>
                        {
                            card.Title(row => row.Name);
                            card.Badge(row => row.Category);
                            card.Stats(stats =>
                            {
                                stats.Add(
                                    row => row.Price,
                                    stat =>
                                        stat.UseDisplay<NumberDisplay>(number =>
                                            number.Format = "0.00"
                                        )
                                );
                                stats.Add(
                                    row => row.CategoryAverage,
                                    stat =>
                                    {
                                        stat.Title = "Average";
                                        stat.UseDisplay<NumberDisplay>(number =>
                                            number.Format = "0.00"
                                        );
                                    }
                                );
                                stats.Add(
                                    row => row.Difference,
                                    stat =>
                                        stat.UseDisplay<NumberDisplay>(number =>
                                            number.Format = "+0.00;-0.00;0.00"
                                        )
                                );
                            });
                        });
                    }
                );
                // The products per category as cards, each linking to its category's view page
                index.AddCards<CategorySummary, CategorySummaryHandler>(
                    "category-cards",
                    cards =>
                    {
                        cards.Title = "Category cards";
                        cards.Icon = "tag";
                        cards.LinkTo<Category>(row => row.CategoryId);
                        cards.Card(card =>
                        {
                            card.Title(row => row.Category);
                            card.Stats(stats =>
                            {
                                stats.Add(row => row.Products);
                                stats.Add(
                                    row => row.Average,
                                    stat =>
                                    {
                                        stat.Title = "Average price";
                                        stat.UseDisplay<NumberDisplay>(number =>
                                            number.Format = "0.00"
                                        );
                                    }
                                );
                            });
                        });
                    }
                );
            });
        });
    }

    // The view and edit pages compare the product's price with its category's, beside the fields. A product without
    // a category has nothing to compare, so the handler hides the widget.
    private static void AddCategoryPricesWidget<TRecord>(
        WidgetsBuilder<RecordWidgetContext<TRecord>> widgets,
        Func<TRecord, int> productId
    ) =>
        widgets.Add<ProductCategoryPricesHandler>(
            "Category prices",
            "_ProductCategoryPricesWidget",
            widget =>
            {
                widget.Placement = WidgetPlacement.Aside;
                widget.Parameters(context => new ProductCategoryPricesParameters(
                    productId(context.Record)
                ));
            }
        );

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
