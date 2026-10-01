using StellarAdmin.Dashboard;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Editors;

namespace DashboardPlayground.Resources.FieldEditors;

// Visual harness for the built-in field editors. See docs/plans/field-editor-catalog.md.
internal static class FieldEditorGalleryRegistration
{
    private const string Group = "Field editors";

    internal static void AddFieldEditorGallery(this StellarAdminDashboardBuilder dashboard)
    {
        dashboard.AddScript("~/js/field-editor-gallery.js");
        dashboard.AddGalleryResource<TextInputGallery>(
            "text-input",
            "Text input",
            10,
            ConfigureTextInputFields
        );
    }

    private static void AddGalleryResource<TRecord>(
        this StellarAdminDashboardBuilder dashboard,
        string slug,
        string label,
        int order,
        Action<ResourceFieldsBuilder<TRecord>> configureFields
    )
        where TRecord : FieldEditorGalleryRecord, IFieldEditorGalleryRecord<TRecord>
    {
        dashboard.AddResource<TRecord>(
            slug,
            resource =>
            {
                resource.PluralLabel = label;
                resource.SingularLabel = label;
                resource.SidebarItem(item =>
                {
                    item.Group = Group;
                    item.Order = order;
                });
                resource.UseDataSource<FieldEditorGalleryDataSource<TRecord>>();
                resource.UseKey(record => record.Id);
                resource.Index(index => index.Columns(columns => columns.Add(record => record.Id)));
                resource.AllowCreate(create => create.Fields(ConfigureFields));
                resource.AllowEdit(edit => edit.Fields(ConfigureFields));
            }
        );

        void ConfigureFields(ResourceFieldsBuilder<TRecord> fields)
        {
            fields.AddSection(
                "Gallery options",
                section =>
                {
                    section.Add(record => record.RejectEveryField);
                    section.Add(record => record.SkipClientValidation);
                }
            );
            configureFields(fields);
        }
    }

    private static void ConfigureTextInputFields(ResourceFieldsBuilder<TextInputGallery> fields)
    {
        fields.AddSection(
            "Data-type templates",
            section =>
            {
                section.Description =
                    "No UseEditor. MVC resolves the data-type template, which forwards to Editors/TextInput.";
                section.Add(model => model.PlainText);
                section.Add(model => model.RequiredText);
                section.Add(model => model.DescribedText);
                section.Add(model => model.Email);
                section.Add(model => model.Phone);
                section.Add(model => model.Website);
                section.Add(model => model.Password);
                section.Add(model => model.ExternalId);
                section.Add(model => model.Level);
                section.Add(model => model.Quantity);
                section.Add(model => model.OptionalQuantity);
                section.Add(model => model.Population);
                section.Add(model => model.Ratio);
                section.Add(model => model.Weight);
                section.Add(model => model.Price);
                section.Add(model => model.Budget);
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledText).Title = "Title override";
                section.Add(model => model.FieldDescribedText).Description =
                    "Set with the field's Description.";
                section
                    .Add(model => model.OverriddenDescription)
                    .UseEditor<TextInputEditor>(input => input.Prefix = "@")
                    .Description = "Set with the field's Description, replacing the attribute.";
                section.Add(model => model.ReadOnlyText);
                section
                    .Add(model => model.ReadOnlyAmount)
                    .UseEditor<TextInputEditor>(input => input.Prefix = "$");
            }
        );
        fields.AddSection(
            "TextInputEditor settings",
            section =>
            {
                section.Description = "UseEditor<TextInputEditor> with explicit settings.";
                section
                    .Add(model => model.PlaceholderText)
                    .UseEditor<TextInputEditor>(input => input.Placeholder = "Seat preference");
                section
                    .Add(model => model.ExplicitEmail)
                    .UseEditor<TextInputEditor>(input => input.Type = TextInputType.Email);
                section
                    .Add(model => model.TextQuantity)
                    .UseEditor<TextInputEditor>(input => input.Type = TextInputType.Text);
                section
                    .Add(model => model.Constrained)
                    .UseEditor<TextInputEditor>(input =>
                    {
                        input.Max = 500m;
                        input.Min = 0m;
                        input.Step = 0.25m;
                    });
                section
                    .Add(model => model.Domain)
                    .UseEditor<TextInputEditor>(input => input.Prefix = "https://");
                section
                    .Add(model => model.Mass)
                    .UseEditor<TextInputEditor>(input => input.Suffix = "kg");
                section
                    .Add(model => model.GroupedPrice)
                    .UseEditor<TextInputEditor>(input =>
                    {
                        input.Prefix = "$";
                        input.Suffix = "USD";
                    });
                section
                    .Add(model => model.StyledCode)
                    .UseEditor<TextInputEditor>(input =>
                    {
                        input.ClassNames.Control = "border-dashed";
                        input.Placeholder = "AAA-00";
                        input.Prefix = "#";
                    });
            }
        );
    }
}
