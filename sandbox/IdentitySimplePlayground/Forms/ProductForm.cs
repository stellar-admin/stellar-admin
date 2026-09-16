using IdentitySimplePlayground.Data;
using StellarAdmin.Pro.Resources.Builders;
using StellarAdmin.Pro.Resources.Options;

namespace IdentitySimplePlayground.Forms;

internal static class ProductForm
{
    public static void Configure(FormFieldsBuilder<Product> fields)
    {
        fields.Clear();

        fields.AddSection(
            "Details",
            section =>
            {
                section.Description = "How this product appears in the catalog.";

                section.Fields(fields =>
                {
                    fields.AddRow(row =>
                        row.Fields(fields =>
                        {
                            fields.Add(product => product.Name);
                            fields.Add(product => product.Sku);
                        })
                    );

                    fields
                        .Add(product => product.Condition)
                        .Editor<RadioEditorOptions>(editor =>
                        {
                            editor.ClassNames.Control =
                                "grid grid-cols-1 @[40rem]/field-group:grid-cols-3";
                            editor.ClassNames.Option.Root = "[&>[data-slot=field]]:h-full";
                        });
                    fields.Add(product => product.CategoryId);
                    fields.Add(product => product.Description);
                });
            }
        );

        fields.AddSection(
            "Pricing",
            section =>
            {
                section.Description = "Set the selling price, reference price, and cost per unit.";

                section.Fields(fields =>
                {
                    fields.AddRow(row =>
                        row.Fields(fields =>
                        {
                            fields.Add(product => product.Price);
                            fields.Add(product => product.CompareAtPrice);
                            fields.Add(product => product.CostPrice);
                        })
                    );
                });
            }
        );

        fields.AddSection(
            "Inventory",
            section =>
            {
                section.Description = "Record stock levels and inventory preferences.";

                section.Fields(fields =>
                {
                    fields.Add(product => product.StockQuantity);

                    fields.AddGroup(group =>
                        group.Fields(fields =>
                        {
                            fields.Add(product => product.TrackInventory);
                            fields.Add(product => product.AllowBackorders);
                        })
                    );
                });
            }
        );

        fields.AddSection(
            "Shipping",
            section =>
            {
                section.Description = "Describe shipping requirements and physical measurements.";

                section.Fields(fields =>
                {
                    fields.Add(product => product.RequiresShipping);
                    fields.Add(product => product.WeightKg);

                    fields.AddRow(row =>
                        row.Fields(fields =>
                        {
                            fields.Add(product => product.WidthCm);
                            fields.Add(product => product.HeightCm);
                            fields.Add(product => product.DepthCm);
                        })
                    );
                });
            }
        );

        fields.AddSection(
            "Publishing",
            section =>
            {
                section.Description = "Record the publication status and date.";

                section.Fields(fields =>
                {
                    fields.Add(product => product.IsPublished);
                    fields.Add(product => product.PublishedAt);
                });
            }
        );

        fields.Add(product => product.CreatedAt).ReadOnly();
    }
}
