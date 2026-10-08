using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Editors;
using StellarAdmin.TagHelpers;

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
        dashboard.Services.AddSingleton<GalleryAirportStore>();
        dashboard.Services.AddSingleton<GalleryAirportLookupSource>();
        dashboard.AddGalleryResource<LookupGallery>("lookup", "Lookup", 25, ConfigureLookupFields);
        dashboard.AddAirportResource();
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
        dashboard.AddGalleryResource<SegmentedControlGallery>(
            "segmented-control",
            "Segmented control",
            50,
            ConfigureSegmentedControlFields
        );
        dashboard.AddGalleryResource<ToggleGroupGallery>(
            "toggle-group",
            "Toggle group",
            55,
            ConfigureToggleGroupFields
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

    // The lookup gallery's EnableCreate field creates airports with this resource's create form
    private static void AddAirportResource(this StellarAdminDashboardBuilder dashboard)
    {
        dashboard.AddResource<GalleryAirport>(
            "airports",
            resource =>
            {
                resource.PluralLabel = "Airports";
                resource.SingularLabel = "Airport";
                resource.SidebarItem(item =>
                {
                    item.Group = Group;
                    item.Order = 26;
                });
                resource.UseDataSource<GalleryAirportDataSource>();
                resource.Index(index =>
                    index.Columns(columns =>
                    {
                        columns.Add(airport => airport.Code);
                        columns.Add(airport => airport.City);
                        columns.Add(airport => airport.Country);
                    })
                );
                resource.AllowCreate<CreateGalleryAirportModel, CreateGalleryAirportHandler>(
                    create =>
                        create.Fields(fields =>
                        {
                            fields.Add(model => model.Code);
                            fields.Add(model => model.City);
                            fields.Add(model => model.Country);
                        })
                );
            }
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
                section.Add(model => model.UnsetRequiredCabin);
                section.Add(model => model.DescribedCabin);
                section.Add(model => model.Fare);
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
                    .Add(model => model.OmittedCabin)
                    .UseEditor<SelectEditor>(select => select.EmptyChoice = EmptyChoice.Omit);
                section
                    .Add(model => model.Seat)
                    .UseEditor<SelectEditor>(select => select.UseItems(GallerySeats.Items));
                section
                    .Add(model => model.Airport)
                    .UseEditor<SelectEditor>(select =>
                    {
                        var europe = new ChoiceGroup("Europe");
                        select.UseItems([
                            new ChoiceItem("LIS", "Lisbon") { Group = europe },
                            new ChoiceItem("JFK", "New York") { Group = new("Americas") },
                            new ChoiceItem("MAD", "Madrid") { Group = europe },
                            new ChoiceItem("KEF", "Reykjavík, closed")
                            {
                                Disabled = true,
                                Group = europe,
                            },
                            new ChoiceItem("NRT", "Tokyo")
                            {
                                Group = new("Asia, closed") { Disabled = true },
                            },
                        ]);
                    });
                section
                    .Add(model => model.StyledCabin)
                    .UseEditor<SelectEditor>(select => select.ClassNames.Control = "border-dashed");
            }
        );
    }

    private static void ConfigureLookupFields(ResourceFieldsBuilder<LookupGallery> fields)
    {
        fields.AddSection(
            "Items",
            section =>
            {
                section.Description =
                    "UseEditor<LookupEditor> with UseItems from a registered ILookupSource. Optional fields can be cleared.";
                section
                    .Add(model => model.Airport)
                    .UseEditor<LookupEditor>(lookup => UseAirports(lookup));
                section
                    .Add(model => model.RequiredAirport)
                    .UseEditor<LookupEditor>(lookup => UseAirports(lookup));
                section
                    .Add(model => model.DescribedItemsAirport)
                    .UseEditor<LookupEditor>(lookup => UseAirports(lookup, describe: true));
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section
                    .Add(model => model.TitledAirport)
                    .UseEditor<LookupEditor>(lookup => UseAirports(lookup))
                    .Title = "Title override";
                section
                    .Add(model => model.DescribedAirport)
                    .UseEditor<LookupEditor>(lookup => UseAirports(lookup));
                section
                    .Add(model => model.FieldDescribedAirport)
                    .UseEditor<LookupEditor>(lookup => UseAirports(lookup))
                    .Description = "Set with the field's Description.";
                section
                    .Add(model => model.ReadOnlyAirport)
                    .UseEditor<LookupEditor>(lookup => UseAirports(lookup));
                section
                    .Add(model => model.UnknownAirport)
                    .UseEditor<LookupEditor>(lookup => UseAirports(lookup));
            }
        );
        fields.AddSection(
            "LookupEditor settings",
            section =>
            {
                section
                    .Add(model => model.LabelledAirport)
                    .UseEditor<LookupEditor>(lookup =>
                    {
                        lookup.Sheet(sheet =>
                        {
                            sheet.Title = "Select a departure airport";
                            sheet.SearchPlaceholder = "City, code or country";
                        });
                        lookup.Editor(editor => editor.EmptyText = "Any airport");
                        UseAirports(lookup, describe: true);
                    });
                section
                    .Add(model => model.UnclearableAirport)
                    .UseEditor<LookupEditor>(lookup =>
                    {
                        lookup.Editor(editor => editor.AllowClear = false);
                        UseAirports(lookup);
                    });
                section
                    .Add(model => model.SearchedAirport)
                    .UseEditor<LookupEditor>(lookup =>
                    {
                        lookup.Sheet(sheet => sheet.MinimumSearchLength = 2);
                        UseAirports(lookup, describe: true);
                    });
                section
                    .Add(model => model.PagedAirport)
                    .UseEditor<LookupEditor>(lookup =>
                    {
                        lookup.Sheet(sheet => sheet.PageSize = 5);
                        UseAirports(lookup, describe: true);
                    });
                section
                    .Add(model => model.StyledAirport)
                    .UseEditor<LookupEditor>(lookup =>
                    {
                        lookup.ClassNames.Control = "max-w-xs";
                        UseAirports(lookup);
                    });
            }
        );
        fields.AddSection(
            "Media and layout",
            section =>
            {
                section.Description =
                    "UseCode, UseAvatar, UseImage and UseIcon display media beside each item. The layout is a card when the items have a description, otherwise a button. Only Lisbon and Cape Town have images; the other airports have no media.";
                section
                    .Add(model => model.CodeAirport)
                    .UseEditor<LookupEditor>(lookup =>
                        UseAirports(lookup, describe: true, media: AirportMedia.Code)
                    );
                section
                    .Add(model => model.CodeInputAirport)
                    .UseEditor<LookupEditor>(lookup =>
                        UseAirports(lookup, media: AirportMedia.Code)
                    );
                section
                    .Add(model => model.AvatarAirport)
                    .UseEditor<LookupEditor>(lookup =>
                        UseAirports(lookup, describe: true, media: AirportMedia.Avatar)
                    );
                section
                    .Add(model => model.AvatarInputAirport)
                    .UseEditor<LookupEditor>(lookup =>
                        UseAirports(lookup, media: AirportMedia.Avatar)
                    );
                section
                    .Add(model => model.ImageAirport)
                    .UseEditor<LookupEditor>(lookup =>
                        UseAirports(lookup, describe: true, media: AirportMedia.Image)
                    );
                section
                    .Add(model => model.IconInputAirport)
                    .UseEditor<LookupEditor>(lookup =>
                    {
                        lookup.ClassNames.Media = "text-muted-foreground";
                        UseAirports(lookup, media: AirportMedia.Icon);
                    });
                section
                    .Add(model => model.InputLayoutAirport)
                    .UseEditor<LookupEditor>(lookup =>
                    {
                        lookup.Editor(editor => editor.Layout = LookupEditorLayout.Input);
                        UseAirports(lookup, describe: true);
                    });
                section
                    .Add(model => model.CardLayoutAirport)
                    .UseEditor<LookupEditor>(lookup =>
                    {
                        lookup.Editor(editor => editor.Layout = LookupEditorLayout.Card);
                        UseAirports(lookup, media: AirportMedia.Code);
                    });
                section
                    .Add(model => model.EditorMediaHiddenAirport)
                    .UseEditor<LookupEditor>(lookup =>
                    {
                        lookup.Editor(editor => editor.ShowMedia = false);
                        UseAirports(lookup, describe: true, media: AirportMedia.Code);
                    });
                section
                    .Add(model => model.SheetMediaHiddenAirport)
                    .UseEditor<LookupEditor>(lookup =>
                    {
                        lookup.Sheet(sheet => sheet.ShowMedia = false);
                        UseAirports(lookup, describe: true, media: AirportMedia.Code);
                    });
                section
                    .Add(model => model.CreatableAirport)
                    .UseEditor<LookupEditor>(lookup =>
                    {
                        lookup.EnableCreate();
                        UseAirports(lookup, describe: true, media: AirportMedia.Code);
                    });
                section
                    .Add(model => model.ReadOnlyCodeAirport)
                    .UseEditor<LookupEditor>(lookup =>
                        UseAirports(lookup, describe: true, media: AirportMedia.Code)
                    );
            }
        );

        // The gallery has no airport photos, so avatars show the title's initials and only two airports have images
        static void UseAirports(
            LookupEditor lookup,
            bool describe = false,
            AirportMedia? media = null
        ) =>
            lookup.UseItems<GalleryAirportLookupSource, GalleryAirport, string>(
                airport => airport.Code,
                airport => airport.City,
                items =>
                {
                    // A code chip already shows the code
                    if (describe && media == AirportMedia.Code)
                    {
                        items.UseDescription(airport => airport.Country);
                    }
                    else if (describe)
                    {
                        items.UseDescription(airport => $"{airport.Code} · {airport.Country}");
                    }

                    switch (media)
                    {
                        case AirportMedia.Code:
                            items.UseCode(airport => airport.Code);
                            break;
                        case AirportMedia.Avatar:
                            items.UseAvatar(_ => null);
                            break;
                        case AirportMedia.Image:
                            items.UseImage(airport =>
                                airport.Code switch
                                {
                                    "LIS" => "/images/lisbon.jpg",
                                    "CPT" => "/images/cape-town.jpg",
                                    _ => null,
                                }
                            );
                            break;
                        case AirportMedia.Icon:
                            items.UseIcon(_ => "plane-landing");
                            break;
                    }
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
                        radio.UseItems(GalleryDescribedSeats.Items);
                    });
            }
        );
        fields.AddSection(
            "Media",
            section =>
            {
                section.Description =
                    "Choices with ItemMedia: icons, avatars, images or codes. Media sits before the text, and leads cards at a larger size.";
                section.Columns(2);
                section
                    .Add(model => model.Transport)
                    .UseEditor<RadioGroupEditor>(radio => radio.UseItems(GalleryTransport.Items));
                section
                    .Add(model => model.AirportCode)
                    .UseEditor<RadioGroupEditor>(radio =>
                        radio.UseItems(
                            GalleryAirportCodes.Items.Select(item =>
                                item with
                                {
                                    Description = null,
                                }
                            )
                        )
                    );
                section
                    .Add(model => model.Guide)
                    .UseEditor<RadioGroupEditor>(radio =>
                    {
                        radio.Appearance = RadioGroupAppearance.Cards;
                        radio.UseItems(GalleryGuides.Items);
                    });
                section
                    .Add(model => model.City)
                    .UseEditor<RadioGroupEditor>(radio =>
                    {
                        radio.Appearance = RadioGroupAppearance.Cards;
                        radio.ClassNames.Media = "size-10";
                        radio.UseItems(GalleryCities.Items);
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
        fields.AddSection(
            "Appearance and columns",
            section =>
            {
                section.Description =
                    "Columns respond to the field's width, and the choices flow down each column unless Flow is Across.";
                section
                    .Add(model => model.CardCabins)
                    .UseEditor<CheckboxGroupEditor>(group =>
                        group.Appearance = CheckboxGroupAppearance.Cards
                    );
                section
                    .Add(model => model.Amenities)
                    .UseEditor<CheckboxGroupEditor>(group =>
                    {
                        group.UseItems(GalleryAmenities.Items);
                        group.Columns(2);
                    });
                section
                    .Add(model => model.AcrossAmenities)
                    .UseEditor<CheckboxGroupEditor>(group =>
                    {
                        group.UseItems(GalleryAmenities.Items);
                        group.Columns(columns => columns.Small(2).Large(3));
                        group.Flow = CheckboxGroupFlow.Across;
                    });
                section
                    .Add(model => model.CardColumnCabins)
                    .UseEditor<CheckboxGroupEditor>(group =>
                    {
                        group.Appearance = CheckboxGroupAppearance.Cards;
                        group.Columns(2);
                    });
            }
        );
        fields.AddSection(
            "Columns in narrow fields",
            section =>
            {
                section.Description =
                    "The same Columns(2) stays at one column while the field is narrower than the medium breakpoint.";
                section.Columns(2);
                section
                    .Add(model => model.NarrowAmenities)
                    .UseEditor<CheckboxGroupEditor>(group =>
                    {
                        group.UseItems(GalleryAmenities.Items);
                        group.Columns(2);
                    });
                section
                    .Add(model => model.NarrowAmenitiesBeside)
                    .UseEditor<CheckboxGroupEditor>(group =>
                    {
                        group.UseItems(GalleryAmenities.Items);
                        group.Columns(2);
                    });
            }
        );
        fields.AddSection(
            "Media",
            section =>
            {
                section.Description =
                    "Choices with ItemMedia: icons, avatars, images or codes. Media sits before the text, and leads cards at a larger size.";
                section.Columns(2);
                section
                    .Add(model => model.Transports)
                    .UseEditor<CheckboxGroupEditor>(group =>
                        group.UseItems(
                            GalleryTransport.Items.Select(item => item with { Description = null })
                        )
                    );
                section
                    .Add(model => model.AirportCodes)
                    .UseEditor<CheckboxGroupEditor>(group =>
                        group.UseItems(GalleryAirportCodes.Items)
                    );
                section
                    .Add(model => model.Guides)
                    .UseEditor<CheckboxGroupEditor>(group =>
                    {
                        group.Appearance = CheckboxGroupAppearance.Cards;
                        group.UseItems(GalleryGuides.Items);
                    });
                section
                    .Add(model => model.Cities)
                    .UseEditor<CheckboxGroupEditor>(group =>
                    {
                        group.Appearance = CheckboxGroupAppearance.Cards;
                        group.UseItems(GalleryCities.Items);
                        group.Columns(2);
                    });
            }
        );
    }

    private static void ConfigureSegmentedControlFields(
        ResourceFieldsBuilder<SegmentedControlGallery> fields
    )
    {
        fields.AddSection(
            "Inferred choices",
            section =>
            {
                section.Description =
                    "UseEditor<SegmentedControlEditor> without UseItems. The enum or Boolean property supplies the choices.";
                section.Add(model => model.Cabin).UseEditor<SegmentedControlEditor>();
                section.Add(model => model.OptionalCabin).UseEditor<SegmentedControlEditor>();
                section.Add(model => model.Insured).UseEditor<SegmentedControlEditor>();
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledCabin).UseEditor<SegmentedControlEditor>().Title =
                    "Title override";
                section.Add(model => model.DescribedCabin).UseEditor<SegmentedControlEditor>();
                section
                    .Add(model => model.FieldDescribedCabin)
                    .UseEditor<SegmentedControlEditor>()
                    .Description = "Set with the field's Description.";
                section.Add(model => model.ReadOnlyCabin).UseEditor<SegmentedControlEditor>();
            }
        );
        fields.AddSection(
            "UseItems and class names",
            section =>
            {
                section
                    .Add(model => model.Seat)
                    .UseEditor<SegmentedControlEditor>(toggle =>
                        toggle.UseItems(GallerySeats.Items)
                    );
                section
                    .Add(model => model.StyledCabin)
                    .UseEditor<SegmentedControlEditor>(toggle =>
                        toggle.ClassNames.Control = "border-dashed"
                    );
            }
        );
        fields.AddSection(
            "Media",
            section =>
            {
                section.Description =
                    "Choices with ItemMedia display it before the text. A segmented control has no descriptions.";
                section
                    .Add(model => model.Transport)
                    .UseEditor<SegmentedControlEditor>(toggle =>
                        toggle.UseItems(GalleryTransport.Items)
                    );
                section
                    .Add(model => model.Guide)
                    .UseEditor<SegmentedControlEditor>(toggle =>
                        toggle.UseItems(GalleryGuides.Items)
                    );
                section
                    .Add(model => model.AirportCode)
                    .UseEditor<SegmentedControlEditor>(toggle =>
                    {
                        toggle.ClassNames.Media = "border bg-transparent";
                        toggle.UseItems(GalleryAirportCodes.Items);
                    });
            }
        );
    }

    private static void ConfigureToggleGroupFields(ResourceFieldsBuilder<ToggleGroupGallery> fields)
    {
        fields.AddSection(
            "Appearances",
            section =>
            {
                section.Description =
                    "A collection property selects multiple values and any other property selects one. Chips is the default appearance.";
                section.Add(model => model.Cabins).UseEditor<ToggleGroupEditor>();
                section
                    .Add(model => model.Amenities)
                    .UseEditor<ToggleGroupEditor>(group => group.UseItems(GalleryAmenities.Items));
                section.Add(model => model.Cabin).UseEditor<ToggleGroupEditor>();
                section
                    .Add(model => model.Days)
                    .UseEditor<ToggleGroupEditor>(group =>
                    {
                        group.UseItems(GalleryDays.Items);
                        group.Appearance = ToggleGroupAppearance.Joined;
                    });
                section
                    .Add(model => model.OptionalCabin)
                    .UseEditor<ToggleGroupEditor>(group =>
                        group.Appearance = ToggleGroupAppearance.Joined
                    );
                section
                    .Add(model => model.Meals)
                    .UseEditor<ToggleGroupEditor>(group =>
                    {
                        group.UseItems(GalleryMeals.Items);
                        group.Appearance = ToggleGroupAppearance.Buttons;
                    });
                section
                    .Add(model => model.Insured)
                    .UseEditor<ToggleGroupEditor>(group =>
                        group.Appearance = ToggleGroupAppearance.Buttons
                    );
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledCabins).UseEditor<ToggleGroupEditor>().Title =
                    "Title override";
                section.Add(model => model.DescribedCabins).UseEditor<ToggleGroupEditor>();
                section.Add(model => model.ReadOnlyCabins).UseEditor<ToggleGroupEditor>();
                section
                    .Add(model => model.StyledCabins)
                    .UseEditor<ToggleGroupEditor>(group =>
                        group.ClassNames.Control = "rounded-lg border border-dashed p-3"
                    );
            }
        );
        fields.AddSection(
            "Media",
            section =>
            {
                section.Description =
                    "Choices with ItemMedia display it before the text. A chip's check mark replaces its media while on, unless CheckPlacement moves it.";
                section
                    .Add(model => model.Transports)
                    .UseEditor<ToggleGroupEditor>(group => group.UseItems(GalleryTransport.Items));
                section
                    .Add(model => model.Guides)
                    .UseEditor<ToggleGroupEditor>(group =>
                    {
                        group.CheckPlacement = ToggleGroupCheckPlacement.Start;
                        group.UseItems(GalleryGuides.Items);
                    });
                section
                    .Add(model => model.AirportCodes)
                    .UseEditor<ToggleGroupEditor>(group =>
                    {
                        group.CheckPlacement = ToggleGroupCheckPlacement.End;
                        group.UseItems(GalleryAirportCodes.Items);
                    });
                section
                    .Add(model => model.City)
                    .UseEditor<ToggleGroupEditor>(group =>
                    {
                        group.Appearance = ToggleGroupAppearance.Joined;
                        group.UseItems(GalleryCities.Items);
                    });
                section
                    .Add(model => model.ButtonTransports)
                    .UseEditor<ToggleGroupEditor>(group =>
                    {
                        group.Appearance = ToggleGroupAppearance.Buttons;
                        group.UseItems(GalleryTransport.Items);
                    });
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
        fields.AddSection(
            "Value and marks",
            section =>
            {
                section.Description =
                    "The value shows beside the label and the bounds under the track by default. ValueFormat formats both.";
                section
                    .Add(model => model.DistanceFromCenter)
                    .UseEditor<SliderEditor>(slider =>
                    {
                        slider.Max = 50;
                        slider.ValueFormat = "{0} km";
                        slider.MarkInterval = 10;
                    });
                section
                    .Add(model => model.Nights)
                    .UseEditor<SliderEditor>(slider =>
                    {
                        slider.MarkInterval = 1;
                        slider.MarkLabels = SliderMarkLabels.All;
                    });
                section
                    .Add(model => model.GuestRating)
                    .UseEditor<SliderEditor>(slider =>
                    {
                        slider.AddMark(1, "Poor");
                        slider.AddMark(3, "Good");
                        slider.AddMark(5, "Excellent");
                    });
                section
                    .Add(model => model.PlainVolume)
                    .UseEditor<SliderEditor>(slider =>
                    {
                        slider.ShowValue = false;
                        slider.MarkLabels = SliderMarkLabels.None;
                    });
                section
                    .Add(model => model.StyledMarkVolume)
                    .UseEditor<SliderEditor>(slider =>
                    {
                        slider.ClassNames.Value = "font-semibold text-foreground";
                        slider.ClassNames.MarkLabel = "text-primary";
                    });
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
