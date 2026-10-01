using Microsoft.AspNetCore.Mvc.Rendering;
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
        dashboard.AddGalleryResource<SelectGallery>("select", "Select", 20, ConfigureSelectFields);
        dashboard.AddGalleryResource<RadioGroupGallery>(
            "radio-group",
            "Radio group",
            30,
            ConfigureRadioGroupFields
        );
        dashboard.AddGalleryResource<CheckboxGroupGallery>(
            "checkbox-group",
            "Checkbox group",
            40,
            ConfigureCheckboxGroupFields
        );
        dashboard.AddGalleryResource<ToggleButtonsGallery>(
            "toggle-buttons",
            "Toggle buttons",
            50,
            ConfigureToggleButtonsFields
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

    private static void ConfigureSelectFields(ResourceFieldsBuilder<SelectGallery> fields)
    {
        fields.AddSection(
            "Data-type templates",
            section =>
            {
                section.Description =
                    "No UseEditor. The Enum and Boolean templates forward to Editors/Select.";
                section.Add(model => model.Cabin);
                section.Add(model => model.OptionalCabin);
                section.Add(model => model.RequiredCabin);
                section.Add(model => model.DescribedCabin);
                section.Add(model => model.Insured);
                section.Add(model => model.Extras);
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledCabin).Title = "Title override";
                section.Add(model => model.FieldDescribedCabin).Description =
                    "Set with the field's Description.";
                section.Add(model => model.ReadOnlyCabin);
                section.Add(model => model.ReadOnlyInsured);
            }
        );
        fields.AddSection(
            "SelectEditor settings",
            section =>
            {
                section.Description = "UseEditor<SelectEditor> with explicit settings.";
                section
                    .Add(model => model.AnyCabin)
                    .UseEditor<SelectEditor>(select => select.EmptyChoiceText = "Any cabin");
                section
                    .Add(model => model.Seat)
                    .UseEditor<SelectEditor>(select => select.UseItems(GallerySeats.Items));
                section
                    .Add(model => model.Airport)
                    .UseEditor<SelectEditor>(select =>
                    {
                        var europe = new SelectListGroup { Name = "Europe" };
                        var americas = new SelectListGroup { Name = "Americas" };
                        select.UseItems([
                            new SelectListItem("Not set", ""),
                            new SelectListItem("Lisbon", "LIS") { Group = europe },
                            new SelectListItem("Madrid", "MAD") { Group = europe },
                            new SelectListItem("Reykjavík, closed", "KEF")
                            {
                                Disabled = true,
                                Group = europe,
                            },
                            new SelectListItem("New York", "JFK") { Group = americas },
                        ]);
                    });
                section
                    .Add(model => model.StyledCabin)
                    .UseEditor<SelectEditor>(select => select.ClassNames.Control = "border-dashed");
            }
        );
    }

    private static void ConfigureRadioGroupFields(ResourceFieldsBuilder<RadioGroupGallery> fields)
    {
        fields.AddSection(
            "Inferred choices",
            section =>
            {
                section.Description =
                    "UseEditor<RadioGroupEditor> without UseItems. The property supplies the choices.";
                section.Add(model => model.Cabin).UseEditor<RadioGroupEditor>();
                section
                    .Add(model => model.CabinCards)
                    .UseEditor<RadioGroupEditor>(radio =>
                        radio.Appearance = RadioGroupAppearance.Cards
                    );
                section.Add(model => model.OptionalCabin).UseEditor<RadioGroupEditor>();
                section
                    .Add(model => model.RequiredCabin)
                    .UseEditor<RadioGroupEditor>(radio =>
                        radio.Appearance = RadioGroupAppearance.Cards
                    );
                section.Add(model => model.Insured).UseEditor<RadioGroupEditor>();
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledCabin).UseEditor<RadioGroupEditor>().Title =
                    "Title override";
                section
                    .Add(model => model.DescribedCabin)
                    .UseEditor<RadioGroupEditor>()
                    .Description = "Set with the field's Description, replacing the attribute.";
                section.Add(model => model.ReadOnlyCabin).UseEditor<RadioGroupEditor>();
                section
                    .Add(model => model.ReadOnlyCabinCards)
                    .UseEditor<RadioGroupEditor>(radio =>
                        radio.Appearance = RadioGroupAppearance.Cards
                    );
            }
        );
        fields.AddSection(
            "UseItems and class names",
            section =>
            {
                section
                    .Add(model => model.Seat)
                    .UseEditor<RadioGroupEditor>(radio => radio.UseItems(GallerySeats.Items));
                section
                    .Add(model => model.SeatCards)
                    .UseEditor<RadioGroupEditor>(radio =>
                    {
                        radio.Appearance = RadioGroupAppearance.Cards;
                        radio.ClassNames.Option.Root = "border-dashed";
                        radio.UseItems(GallerySeats.Items);
                    });
            }
        );
    }

    private static void ConfigureCheckboxGroupFields(
        ResourceFieldsBuilder<CheckboxGroupGallery> fields
    )
    {
        fields.AddSection(
            "Inferred choices",
            section =>
            {
                section.Description =
                    "UseEditor<CheckboxGroupEditor> without UseItems. The enum element type supplies the choices.";
                section.Add(model => model.Cabins).UseEditor<CheckboxGroupEditor>();
                section.Add(model => model.RequiredCabins).UseEditor<CheckboxGroupEditor>();
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledCabins).UseEditor<CheckboxGroupEditor>().Title =
                    "Title override";
                section
                    .Add(model => model.AttributeDescribedCabins)
                    .UseEditor<CheckboxGroupEditor>();
                section
                    .Add(model => model.DescribedCabins)
                    .UseEditor<CheckboxGroupEditor>()
                    .Description = "Set with the field's Description.";
                section.Add(model => model.ReadOnlyCabins).UseEditor<CheckboxGroupEditor>();
            }
        );
        fields.AddSection(
            "UseItems and class names",
            section =>
            {
                section
                    .Add(model => model.Seats)
                    .UseEditor<CheckboxGroupEditor>(group => group.UseItems(GallerySeats.Items));
                section
                    .Add(model => model.StyledCabins)
                    .UseEditor<CheckboxGroupEditor>(group =>
                        group.ClassNames.Control = "border border-dashed p-3"
                    );
            }
        );
    }

    private static void ConfigureToggleButtonsFields(
        ResourceFieldsBuilder<ToggleButtonsGallery> fields
    )
    {
        fields.AddSection(
            "Inferred choices",
            section =>
            {
                section.Description =
                    "UseEditor<ToggleButtonsEditor> without UseItems. The enum or Boolean property supplies the choices.";
                section.Add(model => model.Cabin).UseEditor<ToggleButtonsEditor>();
                section.Add(model => model.OptionalCabin).UseEditor<ToggleButtonsEditor>();
                section.Add(model => model.Insured).UseEditor<ToggleButtonsEditor>();
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledCabin).UseEditor<ToggleButtonsEditor>().Title =
                    "Title override";
                section.Add(model => model.DescribedCabin).UseEditor<ToggleButtonsEditor>();
                section
                    .Add(model => model.FieldDescribedCabin)
                    .UseEditor<ToggleButtonsEditor>()
                    .Description = "Set with the field's Description.";
                section.Add(model => model.ReadOnlyCabin).UseEditor<ToggleButtonsEditor>();
            }
        );
        fields.AddSection(
            "UseItems and class names",
            section =>
            {
                section
                    .Add(model => model.Seat)
                    .UseEditor<ToggleButtonsEditor>(toggle => toggle.UseItems(GallerySeats.Items));
                section
                    .Add(model => model.StyledCabin)
                    .UseEditor<ToggleButtonsEditor>(toggle =>
                        toggle.ClassNames.Control = "border-dashed"
                    );
            }
        );
    }
}
