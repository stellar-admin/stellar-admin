using Microsoft.AspNetCore.Mvc.Rendering;
using StellarAdmin.Dashboard;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Editors;

namespace DashboardPlayground.Resources.FieldEditors;

// Visual harness for the built-in field editors. See docs/plans/archive/field-editor-catalog.md.
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
        dashboard.AddGalleryResource<TextareaGallery>(
            "textarea",
            "Textarea",
            60,
            ConfigureTextareaFields
        );
        dashboard.AddGalleryResource<CheckboxGallery>(
            "checkbox",
            "Checkbox",
            70,
            ConfigureCheckboxFields
        );
        dashboard.AddGalleryResource<ToggleGallery>("toggle", "Toggle", 75, ConfigureToggleFields);
        dashboard.AddGalleryResource<DateTimeGallery>(
            "date-time",
            "Date and time",
            80,
            ConfigureDateTimeFields
        );
        dashboard.AddGalleryResource<SliderGallery>("slider", "Slider", 90, ConfigureSliderFields);
        dashboard.AddGalleryResource<OneTimeCodeGallery>(
            "one-time-code",
            "One-time code",
            100,
            ConfigureOneTimeCodeFields
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

    private static void ConfigureTextareaFields(ResourceFieldsBuilder<TextareaGallery> fields)
    {
        fields.AddSection(
            "Data-type templates",
            section =>
            {
                section.Description =
                    "No UseEditor. The MultilineText template forwards to Editors/Textarea.";
                section.Add(model => model.Notes);
                section.Add(model => model.RequiredNotes);
                section.Add(model => model.DescribedNotes);
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledNotes).Title = "Title override";
                section.Add(model => model.FieldDescribedNotes).Description =
                    "Set with the field's Description.";
                section.Add(model => model.ReadOnlyNotes);
            }
        );
        fields.AddSection(
            "TextareaEditor settings",
            section =>
            {
                section.Description = "UseEditor<TextareaEditor> with explicit settings.";
                section
                    .Add(model => model.PlaceholderNotes)
                    .UseEditor<TextareaEditor>(textarea =>
                        textarea.Placeholder = "Anything the crew should know"
                    );
                section
                    .Add(model => model.TallNotes)
                    .UseEditor<TextareaEditor>(textarea => textarea.Rows = 8);
                section
                    .Add(model => model.StyledNotes)
                    .UseEditor<TextareaEditor>(textarea =>
                        textarea.ClassNames.Control = "border-dashed"
                    );
            }
        );
    }

    private static void ConfigureCheckboxFields(ResourceFieldsBuilder<CheckboxGallery> fields)
    {
        fields.AddSection(
            "Data-type template",
            section =>
            {
                section.Description =
                    "The Boolean template forwards to Editors/Checkbox. UseEditor<CheckboxEditor> renders the same.";
                section.Add(model => model.Subscribed);
                section.Add(model => model.DescribedSubscribed);
                section.Add(model => model.ExplicitSubscribed).UseEditor<CheckboxEditor>();
            }
        );
        fields.AddSection(
            "Validation",
            section =>
            {
                section.Description = "Clear these and save to see the server errors.";
                section.Add(model => model.AcceptedTerms);
                section.Add(model => model.PassportConfirmed);
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledSubscribed).Title = "Title override";
                section.Add(model => model.FieldDescribedSubscribed).Description =
                    "Set with the field's Description.";
                section.Add(model => model.ReadOnlyChecked);
                section.Add(model => model.ReadOnlyUnchecked);
            }
        );
        fields.AddSection(
            "Class names",
            section =>
            {
                section
                    .Add(model => model.LabelStyled)
                    .UseEditor<CheckboxEditor>(checkbox => checkbox.ClassNames.Label = "italic");
                section
                    .Add(model => model.RootStyled)
                    .UseEditor<CheckboxEditor>(checkbox =>
                        checkbox.ClassNames.Root = "rounded-lg border border-dashed p-3"
                    );
            }
        );
    }

    private static void ConfigureToggleFields(ResourceFieldsBuilder<ToggleGallery> fields)
    {
        fields.AddSection(
            "States",
            section =>
            {
                section.Description = "UseEditor<ToggleEditor> on Boolean properties.";
                section.Add(model => model.Notifications).UseEditor<ToggleEditor>();
                section.Add(model => model.Newsletter).UseEditor<ToggleEditor>();
                section.Add(model => model.DescribedNotifications).UseEditor<ToggleEditor>();
            }
        );
        fields.AddSection(
            "Validation",
            section =>
            {
                section.Description = "Turn these off and save to see the server errors.";
                section.Add(model => model.RequiredAlerts).UseEditor<ToggleEditor>();
                section.Add(model => model.RequiredTracking).UseEditor<ToggleEditor>();
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledNotifications).UseEditor<ToggleEditor>().Title =
                    "Title override";
                section
                    .Add(model => model.FieldDescribedNotifications)
                    .UseEditor<ToggleEditor>()
                    .Description = "Set with the field's Description.";
                section.Add(model => model.ReadOnlyOn).UseEditor<ToggleEditor>();
                section.Add(model => model.ReadOnlyOff).UseEditor<ToggleEditor>();
            }
        );
        fields.AddSection(
            "Class names",
            section =>
            {
                section
                    .Add(model => model.LabelStyled)
                    .UseEditor<ToggleEditor>(toggle => toggle.ClassNames.Label = "italic");
                section
                    .Add(model => model.RootStyled)
                    .UseEditor<ToggleEditor>(toggle =>
                        toggle.ClassNames.Root = "rounded-lg border border-dashed p-3"
                    );
            }
        );
    }

    private static void ConfigureDateTimeFields(ResourceFieldsBuilder<DateTimeGallery> fields)
    {
        fields.AddSection(
            "Data-type templates",
            section =>
            {
                section.Description =
                    "No UseEditor. The date and time templates forward to Editors/DateInput, Editors/DateTimeInput and Editors/TimeInput.";
                section.Add(model => model.Departure);
                section.Add(model => model.OptionalDeparture);
                section.Add(model => model.RequiredDeparture);
                section.Add(model => model.DepartureDate);
                section.Add(model => model.Boarding);
                section.Add(model => model.OptionalBoarding);
                section.Add(model => model.Booked);
                section.Add(model => model.Gate);
                section.Add(model => model.GateTime);
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledDeparture).Title = "Title override";
                section.Add(model => model.DescribedDeparture);
                section.Add(model => model.FieldDescribedDeparture).Description =
                    "Set with the field's Description.";
                section.Add(model => model.ReadOnlyDeparture);
                section.Add(model => model.ReadOnlyBoarding);
            }
        );
        fields.AddSection(
            "Editor settings",
            section =>
            {
                section.Description =
                    "UseEditor<DateInputEditor>, UseEditor<DateTimeInputEditor> and UseEditor<TimeInputEditor> with explicit settings.";
                section
                    .Add(model => model.BoundedDeparture)
                    .UseEditor<DateInputEditor>(date =>
                    {
                        date.Max = new DateOnly(2026, 12, 31);
                        date.Min = new DateOnly(2026, 1, 1);
                    });
                section
                    .Add(model => model.WeeklyDeparture)
                    .UseEditor<DateInputEditor>(date => date.Step = 7);
                section
                    .Add(model => model.QuarterHourBoarding)
                    .UseEditor<DateTimeInputEditor>(dateTime =>
                        dateTime.Step = TimeSpan.FromMinutes(15)
                    );
                section
                    .Add(model => model.OfficeHoursGate)
                    .UseEditor<TimeInputEditor>(time =>
                    {
                        time.Max = new TimeOnly(18, 0);
                        time.Min = new TimeOnly(8, 0);
                        time.Step = TimeSpan.FromMinutes(30);
                    });
                section
                    .Add(model => model.StyledDeparture)
                    .UseEditor<DateInputEditor>(date => date.ClassNames.Control = "border-dashed");
            }
        );
    }

    private static void ConfigureSliderFields(ResourceFieldsBuilder<SliderGallery> fields)
    {
        fields.AddSection(
            "Inferred bounds",
            section =>
            {
                section.Description =
                    "UseEditor<SliderEditor> without settings. Bounds come from [Range], or 0 to 100.";
                section.Add(model => model.Volume).UseEditor<SliderEditor>();
                section.Add(model => model.Passengers).UseEditor<SliderEditor>();
                section.Add(model => model.Rating).UseEditor<SliderEditor>();
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledVolume).UseEditor<SliderEditor>().Title =
                    "Title override";
                section.Add(model => model.DescribedVolume).UseEditor<SliderEditor>();
                section
                    .Add(model => model.FieldDescribedVolume)
                    .UseEditor<SliderEditor>()
                    .Description = "Set with the field's Description.";
                section.Add(model => model.ReadOnlyVolume).UseEditor<SliderEditor>();
            }
        );
        fields.AddSection(
            "SliderEditor settings",
            section =>
            {
                section
                    .Add(model => model.Discount)
                    .UseEditor<SliderEditor>(slider =>
                    {
                        slider.Max = 50;
                        slider.Min = 0;
                        slider.Step = 5;
                    });
                section
                    .Add(model => model.StyledVolume)
                    .UseEditor<SliderEditor>(slider => slider.ClassNames.Control = "max-w-xs");
            }
        );
    }

    private static void ConfigureOneTimeCodeFields(ResourceFieldsBuilder<OneTimeCodeGallery> fields)
    {
        fields.AddSection(
            "Inferred length",
            section =>
            {
                section.Description =
                    "UseEditor<OneTimeCodeEditor> without settings. The length comes from [StringLength], or 6.";
                section.Add(model => model.Code).UseEditor<OneTimeCodeEditor>();
                section.Add(model => model.RequiredCode).UseEditor<OneTimeCodeEditor>();
                section.Add(model => model.Pin).UseEditor<OneTimeCodeEditor>();
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledCode).UseEditor<OneTimeCodeEditor>().Title =
                    "Title override";
                section.Add(model => model.DescribedCode).UseEditor<OneTimeCodeEditor>();
                section
                    .Add(model => model.FieldDescribedCode)
                    .UseEditor<OneTimeCodeEditor>()
                    .Description = "Set with the field's Description.";
                section.Add(model => model.ReadOnlyCode).UseEditor<OneTimeCodeEditor>();
            }
        );
        fields.AddSection(
            "OneTimeCodeEditor settings",
            section =>
            {
                section
                    .Add(model => model.LongCode)
                    .UseEditor<OneTimeCodeEditor>(code => code.Length = 8);
                section
                    .Add(model => model.StyledCode)
                    .UseEditor<OneTimeCodeEditor>(code => code.ClassNames.Control = "gap-3");
            }
        );
    }
}
