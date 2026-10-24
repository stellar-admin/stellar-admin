using AdminPlayground.Resources.Destinations;
using AdminPlayground.Resources.Guides;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Editors;
using StellarAdmin.UI;

namespace AdminPlayground.Resources.FieldEditors;

// Visual harness for the built-in field editors. See docs/plans/archive/field-editor-catalog.md.
internal static class FieldEditorGalleryRegistration
{
    private const string Group = "Field editors";

    internal static void AddFieldEditorGallery(this StellarAdminBuilder admin)
    {
        admin.AddScript("~/js/field-editor-gallery.js");
        admin.AddGalleryResource<TextInputGallery>(
            "text-input",
            "Text input",
            10,
            ConfigureTextInputFields
        );
        admin.AddGalleryResource<SelectGallery>("select", "Select", 20, ConfigureSelectFields);
        admin.Services.AddSingleton<GalleryAirportStore>();
        admin.Services.AddSingleton<GalleryAirportLookupSource>();
        admin.AddGalleryResource<LookupGallery>("lookup", "Lookup", 25, ConfigureLookupFields);
        admin.AddAirportResource();
        admin.Services.AddSingleton<GalleryGuideLookupSource>();
        admin.AddGalleryResource<MultiLookupGallery>(
            "multi-lookup",
            "Multi lookup",
            26,
            ConfigureMultiLookupFields
        );
        admin.AddGalleryResource<RadioGroupGallery>(
            "radio-group",
            "Radio group",
            30,
            ConfigureRadioGroupFields
        );
        admin.AddGalleryResource<CheckboxGroupGallery>(
            "checkbox-group",
            "Checkbox group",
            40,
            ConfigureCheckboxGroupFields
        );
        admin.AddGalleryResource<SegmentedControlGallery>(
            "segmented-control",
            "Segmented control",
            50,
            ConfigureSegmentedControlFields
        );
        admin.AddGalleryResource<ToggleGroupGallery>(
            "toggle-group",
            "Toggle group",
            55,
            ConfigureToggleGroupFields
        );
        admin.AddGalleryResource<TextareaGallery>(
            "textarea",
            "Textarea",
            60,
            ConfigureTextareaFields
        );
        admin.AddGalleryResource<CheckboxGallery>(
            "checkbox",
            "Checkbox",
            70,
            ConfigureCheckboxFields
        );
        admin.AddGalleryResource<ToggleGallery>("toggle", "Toggle", 75, ConfigureToggleFields);
        admin.AddGalleryResource<DateTimeGallery>(
            "date-time",
            "Date and time",
            80,
            ConfigureDateTimeFields
        );
        admin.AddGalleryResource<ListEditorGallery>(
            "list-editor",
            "List editor",
            85,
            ConfigureListEditorFields
        );
        admin.AddGalleryResource<DictionaryEditorGallery>(
            "dictionary-editor",
            "Dictionary editor",
            87,
            ConfigureDictionaryEditorFields
        );
        admin.AddGalleryResource<SliderGallery>("slider", "Slider", 90, ConfigureSliderFields);
        admin.AddGalleryResource<ImageGallery>("image", "Image", 95, ConfigureImageFields);
        admin.AddGalleryResource<IconGallery>("icon", "Icon", 97, ConfigureIconFields);
        admin.AddGalleryResource<OneTimeCodeGallery>(
            "one-time-code",
            "One-time code",
            100,
            ConfigureOneTimeCodeFields
        );
    }

    // The lookup gallery's EnableCreate field creates airports with this resource's create form
    private static void AddAirportResource(this StellarAdminBuilder admin)
    {
        admin.AddResource<GalleryAirport>(
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
                resource.UseKey(airport => airport.Code);
                resource.UseTitle(airport => airport.City);
                resource.Reference(reference =>
                    reference.UseDescription(airport => airport.Country)
                );
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
                            fields
                                .Add(model => model.HubCode)
                                .UseLookupSheetEditor(lookup =>
                                {
                                    lookup.UseResource<GalleryAirport>();
                                    lookup.EnableCreate();
                                });
                        })
                );
            }
        );
    }

    private static void AddGalleryResource<TRecord>(
        this StellarAdminBuilder admin,
        string slug,
        string label,
        int order,
        Action<ResourceFieldsBuilder<TRecord>> configureFields
    )
        where TRecord : FieldEditorGalleryRecord, IFieldEditorGalleryRecord<TRecord>
    {
        admin.AddResource<TRecord>(
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
                    .UseTextInputEditor(input => input.Prefix = "@")
                    .Description = "Set with the field's Description, replacing the attribute.";
                section.Add(model => model.ReadOnlyText);
                section
                    .Add(model => model.ReadOnlyAmount)
                    .UseTextInputEditor(input => input.Prefix = "$");
            }
        );
        fields.AddSection(
            "TextInputEditor settings",
            section =>
            {
                section.Description = "UseTextInputEditor with explicit settings.";
                section
                    .Add(model => model.PlaceholderText)
                    .UseTextInputEditor(input => input.Placeholder = "Seat preference");
                section
                    .Add(model => model.ExplicitEmail)
                    .UseTextInputEditor(input => input.Type = TextInputType.Email);
                section
                    .Add(model => model.TextQuantity)
                    .UseTextInputEditor(input => input.Type = TextInputType.Text);
                section
                    .Add(model => model.Constrained)
                    .UseTextInputEditor(input =>
                    {
                        input.Max = 500m;
                        input.Min = 0m;
                        input.Step = 0.25m;
                    });
                section
                    .Add(model => model.Domain)
                    .UseTextInputEditor(input => input.Prefix = "https://");
                section
                    .Add(model => model.Mass)
                    .UseTextInputEditor(input => input.Suffix = "kg");
                section
                    .Add(model => model.GroupedPrice)
                    .UseTextInputEditor(input =>
                    {
                        input.Prefix = "$";
                        input.Suffix = "USD";
                    });
                section
                    .Add(model => model.StyledCode)
                    .UseTextInputEditor(input =>
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
                section.Description = "UseSelectEditor with explicit settings.";
                section
                    .Add(model => model.AnyCabin)
                    .UseSelectEditor(select => select.EmptyChoiceText = "Any cabin");
                section
                    .Add(model => model.OmittedCabin)
                    .UseSelectEditor(select => select.EmptyChoice = EmptyChoice.Omit);
                section
                    .Add(model => model.Seat)
                    .UseSelectEditor(select => select.UseItems(GallerySeats.Items));
                section
                    .Add(model => model.Airport)
                    .UseSelectEditor(select =>
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
                    .UseSelectEditor(select => select.ClassNames.Control = "border-dashed");
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
                    "UseLookupSheetEditor with UseSource, one LookupSource per description and media. Optional fields can be cleared.";
                section
                    .Add(model => model.Airport)
                    .UseLookupSheetEditor(lookup => UseAirports(lookup));
                section
                    .Add(model => model.RequiredAirport)
                    .UseLookupSheetEditor(lookup => UseAirports(lookup));
                section
                    .Add(model => model.DescribedItemsAirport)
                    .UseLookupSheetEditor(lookup => UseAirports(lookup, describe: true));
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section
                    .Add(model => model.TitledAirport)
                    .UseLookupSheetEditor(lookup => UseAirports(lookup))
                    .Title = "Title override";
                section
                    .Add(model => model.DescribedAirport)
                    .UseLookupSheetEditor(lookup => UseAirports(lookup));
                section
                    .Add(model => model.FieldDescribedAirport)
                    .UseLookupSheetEditor(lookup => UseAirports(lookup))
                    .Description = "Set with the field's Description.";
                section
                    .Add(model => model.ReadOnlyAirport)
                    .UseLookupSheetEditor(lookup => UseAirports(lookup));
                section
                    .Add(model => model.UnknownAirport)
                    .UseLookupSheetEditor(lookup => UseAirports(lookup));
            }
        );
        fields.AddSection(
            "LookupSheetEditor settings",
            section =>
            {
                section
                    .Add(model => model.LabelledAirport)
                    .UseLookupSheetEditor(lookup =>
                    {
                        lookup.Sheet(sheet =>
                        {
                            sheet.Title = "Select a departure airport";
                            sheet.SearchPlaceholder = "City, code or country";
                        });
                        lookup.Field(editor => editor.EmptyText = "Any airport");
                        UseAirports(lookup, describe: true);
                    });
                section
                    .Add(model => model.UnclearableAirport)
                    .UseLookupSheetEditor(lookup =>
                    {
                        lookup.Field(editor => editor.AllowClear = false);
                        UseAirports(lookup);
                    });
                section
                    .Add(model => model.SearchedAirport)
                    .UseLookupSheetEditor(lookup =>
                    {
                        lookup.Sheet(sheet => sheet.MinimumSearchLength = 2);
                        UseAirports(lookup, describe: true);
                    });
                section
                    .Add(model => model.PagedAirport)
                    .UseLookupSheetEditor(lookup =>
                    {
                        lookup.Sheet(sheet => sheet.PageSize = 5);
                        UseAirports(lookup, describe: true);
                    });
                section
                    .Add(model => model.StyledAirport)
                    .UseLookupSheetEditor(lookup =>
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
                    .UseLookupSheetEditor(lookup =>
                        UseAirports(lookup, describe: true, media: AirportMedia.Code)
                    );
                section
                    .Add(model => model.CodeInputAirport)
                    .UseLookupSheetEditor(lookup =>
                        UseAirports(lookup, media: AirportMedia.Code)
                    );
                section
                    .Add(model => model.AvatarAirport)
                    .UseLookupSheetEditor(lookup =>
                        UseAirports(lookup, describe: true, media: AirportMedia.Avatar)
                    );
                section
                    .Add(model => model.AvatarInputAirport)
                    .UseLookupSheetEditor(lookup =>
                        UseAirports(lookup, media: AirportMedia.Avatar)
                    );
                section
                    .Add(model => model.ImageAirport)
                    .UseLookupSheetEditor(lookup =>
                        UseAirports(lookup, describe: true, media: AirportMedia.Image)
                    );
                section
                    .Add(model => model.IconInputAirport)
                    .UseLookupSheetEditor(lookup =>
                    {
                        lookup.ClassNames.Media = "text-muted-foreground";
                        UseAirports(lookup, media: AirportMedia.Icon);
                    });
                section
                    .Add(model => model.InputLayoutAirport)
                    .UseLookupSheetEditor(lookup =>
                    {
                        lookup.Field(editor => editor.Layout = LookupSheetEditorLayout.Input);
                        UseAirports(lookup, describe: true);
                    });
                section
                    .Add(model => model.CardLayoutAirport)
                    .UseLookupSheetEditor(lookup =>
                    {
                        lookup.Field(editor => editor.Layout = LookupSheetEditorLayout.Card);
                        UseAirports(lookup, media: AirportMedia.Code);
                    });
                section
                    .Add(model => model.EditorMediaHiddenAirport)
                    .UseLookupSheetEditor(lookup =>
                    {
                        lookup.Field(editor => editor.ShowMedia = false);
                        UseAirports(lookup, describe: true, media: AirportMedia.Code);
                    });
                section
                    .Add(model => model.SheetMediaHiddenAirport)
                    .UseLookupSheetEditor(lookup =>
                    {
                        lookup.Sheet(sheet => sheet.ShowMedia = false);
                        UseAirports(lookup, describe: true, media: AirportMedia.Code);
                    });
                section
                    .Add(model => model.CreatableAirport)
                    .UseLookupSheetEditor(lookup =>
                    {
                        lookup.EnableCreate();
                        UseAirports(lookup, describe: true, media: AirportMedia.Code);
                    });
                section
                    .Add(model => model.ReadOnlyCodeAirport)
                    .UseLookupSheetEditor(lookup =>
                        UseAirports(lookup, describe: true, media: AirportMedia.Code)
                    );
            }
        );

        static void UseAirports<TModel>(
            LookupSheetEditor<TModel> lookup,
            bool describe = false,
            AirportMedia? media = null
        )
        {
            switch (media, describe)
            {
                case (AirportMedia.Code, true):
                    lookup.UseSource<DescribedCodedAirportSource>();
                    break;
                case (AirportMedia.Code, false):
                    lookup.UseSource<CodedAirportSource>();
                    break;
                case (AirportMedia.Avatar, true):
                    lookup.UseSource<DescribedAvatarAirportSource>();
                    break;
                case (AirportMedia.Avatar, false):
                    lookup.UseSource<AvatarAirportSource>();
                    break;
                case (AirportMedia.Image, _):
                    lookup.UseSource<DescribedImageAirportSource>();
                    break;
                case (AirportMedia.Icon, _):
                    lookup.UseSource<IconAirportSource>();
                    break;
                case (null, true):
                    lookup.UseSource<DescribedAirportSource>();
                    break;
                default:
                    lookup.UseSource<GalleryAirportLookupSource>();
                    break;
            }
        }
    }

    private static void ConfigureMultiLookupFields(ResourceFieldsBuilder<MultiLookupGallery> fields)
    {
        fields.AddSection(
            "Layouts",
            section =>
            {
                section.Description =
                    "UseMultiLookupSheetEditor on a collection of keys. Add, Choose or the summary opens the sheet, where each result toggles and EnableCreate adds New; Remove and Clear edit the selection.";
                section
                    .Add(model => model.Destinations)
                    .UseMultiLookupSheetEditor(lookup =>
                    {
                        UseDestinations(lookup);
                        lookup.EnableCreate();
                    });
                section
                    .Add(model => model.Guides)
                    .UseMultiLookupSheetEditor(lookup => UseGuides(lookup));
                section
                    .Add(model => model.ListDestinations)
                    .UseMultiLookupSheetEditor(lookup =>
                    {
                        lookup.Field(editor => editor.Layout = MultiLookupSheetEditorLayout.List);
                        UseDestinations(lookup, describe: true);
                    });
                section
                    .Add(model => model.ListGuides)
                    .UseMultiLookupSheetEditor(lookup =>
                    {
                        lookup.Field(editor => editor.Layout = MultiLookupSheetEditorLayout.List);
                        UseGuides(lookup, describe: true);
                    });
                section
                    .Add(model => model.SummaryDestinations)
                    .UseMultiLookupSheetEditor(lookup =>
                    {
                        lookup.Field(editor =>
                            editor.Layout = MultiLookupSheetEditorLayout.Summary
                        );
                        UseDestinations(lookup);
                    });
                section
                    .Add(model => model.SummaryGuides)
                    .UseMultiLookupSheetEditor(lookup =>
                    {
                        lookup.Field(editor =>
                            editor.Layout = MultiLookupSheetEditorLayout.Summary
                        );
                        UseGuides(lookup);
                    });
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section
                    .Add(model => model.RequiredDestinations)
                    .UseMultiLookupSheetEditor(lookup => UseDestinations(lookup));
                section
                    .Add(model => model.LimitedGuides)
                    .UseMultiLookupSheetEditor(lookup => UseGuides(lookup));
                section
                    .Add(model => model.UnknownDestinations)
                    .UseMultiLookupSheetEditor(lookup => UseDestinations(lookup));
                section
                    .Add(model => model.LabelledDestinations)
                    .UseMultiLookupSheetEditor(lookup =>
                    {
                        lookup.Field(editor => editor.EmptyText = "Any destination");
                        lookup.Sheet(sheet => sheet.Title = "Select destinations");
                        UseDestinations(lookup);
                    });
                section
                    .Add(model => model.MediaHiddenGuides)
                    .UseMultiLookupSheetEditor(lookup =>
                    {
                        lookup.Field(editor => editor.ShowMedia = false);
                        UseGuides(lookup);
                    });
                section
                    .Add(model => model.StyledDestinations)
                    .UseMultiLookupSheetEditor(lookup =>
                    {
                        lookup.ClassNames.Control = "max-w-sm";
                        lookup.ClassNames.Media = "bg-primary text-primary-foreground";
                        UseDestinations(lookup);
                    });
            }
        );
        fields.AddSection(
            "Read-only",
            section =>
            {
                section
                    .Add(model => model.ReadOnlyDestinations)
                    .UseMultiLookupSheetEditor(lookup => UseDestinations(lookup));
                section
                    .Add(model => model.ReadOnlyListGuides)
                    .UseMultiLookupSheetEditor(lookup =>
                    {
                        lookup.Field(editor => editor.Layout = MultiLookupSheetEditorLayout.List);
                        UseGuides(lookup, describe: true);
                    });
                section
                    .Add(model => model.ReadOnlySummaryGuides)
                    .UseMultiLookupSheetEditor(lookup =>
                    {
                        lookup.Field(editor =>
                            editor.Layout = MultiLookupSheetEditorLayout.Summary
                        );
                        UseGuides(lookup, describe: true);
                    });
                section
                    .Add(model => model.ReadOnlyEmptyDestinations)
                    .UseMultiLookupSheetEditor(lookup => UseDestinations(lookup));
            }
        );

        static void UseDestinations<TModel>(
            MultiLookupSheetEditor<TModel> lookup,
            bool describe = false
        )
        {
            if (describe)
            {
                lookup.UseSource<DescribedCodedAirportSource>();
            }
            else
            {
                lookup.UseSource<CodedAirportSource>();
            }
        }

        static void UseGuides<TModel>(MultiLookupSheetEditor<TModel> lookup, bool describe = false)
        {
            if (describe)
            {
                lookup.UseSource<DescribedAvatarGuideSource>();
            }
            else
            {
                lookup.UseSource<AvatarGuideSource>();
            }
        }
    }

    private static void ConfigureRadioGroupFields(ResourceFieldsBuilder<RadioGroupGallery> fields)
    {
        fields.AddSection(
            "Inferred choices",
            section =>
            {
                section.Description =
                    "UseRadioGroupEditor without UseItems. The property supplies the choices.";
                section.Add(model => model.Cabin).UseRadioGroupEditor();
                section
                    .Add(model => model.CabinCards)
                    .UseRadioGroupEditor(radio =>
                        radio.Appearance = RadioGroupAppearance.Cards
                    );
                section.Add(model => model.OptionalCabin).UseRadioGroupEditor();
                section
                    .Add(model => model.RequiredCabin)
                    .UseRadioGroupEditor(radio =>
                        radio.Appearance = RadioGroupAppearance.Cards
                    );
                section.Add(model => model.Insured).UseRadioGroupEditor();
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledCabin).UseRadioGroupEditor().Title =
                    "Title override";
                section
                    .Add(model => model.DescribedCabin)
                    .UseRadioGroupEditor()
                    .Description = "Set with the field's Description, replacing the attribute.";
                section.Add(model => model.ReadOnlyCabin).UseRadioGroupEditor();
                section
                    .Add(model => model.ReadOnlyCabinCards)
                    .UseRadioGroupEditor(radio =>
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
                    .UseRadioGroupEditor(radio => radio.UseItems(GallerySeats.Items));
                section
                    .Add(model => model.SeatCards)
                    .UseRadioGroupEditor(radio =>
                    {
                        radio.Appearance = RadioGroupAppearance.Cards;
                        radio.ClassNames.Option.Root = "border-dashed";
                        radio.UseItems(GalleryDescribedSeats.Items);
                    });
            }
        );
        fields.AddSection(
            "Columns",
            section =>
            {
                section.Description =
                    "Columns respond to the field's width, and the choices fill each row unless Flow is Down.";
                section
                    .Add(model => model.ColumnSeat)
                    .UseRadioGroupEditor(radio =>
                    {
                        radio.UseItems(GallerySeats.Items);
                        radio.Columns(2);
                    });
                section
                    .Add(model => model.DownTransport)
                    .UseRadioGroupEditor(radio =>
                    {
                        radio.UseItems(GalleryTransport.Items);
                        radio.Columns(columns => columns.Small(2).Large(3));
                        radio.Flow = RadioGroupFlow.Down;
                    });
                section
                    .Add(model => model.CardColumnCabin)
                    .UseRadioGroupEditor(radio =>
                    {
                        radio.Appearance = RadioGroupAppearance.Cards;
                        radio.Columns(2);
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
                    .UseRadioGroupEditor(radio => radio.UseItems(GalleryTransport.Items));
                section
                    .Add(model => model.AirportCode)
                    .UseRadioGroupEditor(radio =>
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
                    .UseRadioGroupEditor(radio =>
                    {
                        radio.Appearance = RadioGroupAppearance.Cards;
                        radio.UseItems(GalleryGuides.Items);
                    });
                section
                    .Add(model => model.City)
                    .UseRadioGroupEditor(radio =>
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
                    "UseCheckboxGroupEditor without UseItems. The enum element type supplies the choices.";
                section.Add(model => model.Cabins).UseCheckboxGroupEditor();
                section.Add(model => model.RequiredCabins).UseCheckboxGroupEditor();
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledCabins).UseCheckboxGroupEditor().Title =
                    "Title override";
                section
                    .Add(model => model.AttributeDescribedCabins)
                    .UseCheckboxGroupEditor();
                section
                    .Add(model => model.DescribedCabins)
                    .UseCheckboxGroupEditor()
                    .Description = "Set with the field's Description.";
                section.Add(model => model.ReadOnlyCabins).UseCheckboxGroupEditor();
            }
        );
        fields.AddSection(
            "UseItems and class names",
            section =>
            {
                section
                    .Add(model => model.Seats)
                    .UseCheckboxGroupEditor(group => group.UseItems(GallerySeats.Items));
                section
                    .Add(model => model.StyledCabins)
                    .UseCheckboxGroupEditor(group =>
                        group.ClassNames.Control = "border border-dashed p-3"
                    );
            }
        );
        fields.AddSection(
            "Appearance and columns",
            section =>
            {
                section.Description =
                    "Columns respond to the field's width, and the choices fill each row unless Flow is Down.";
                section
                    .Add(model => model.CardCabins)
                    .UseCheckboxGroupEditor(group =>
                        group.Appearance = CheckboxGroupAppearance.Cards
                    );
                section
                    .Add(model => model.Amenities)
                    .UseCheckboxGroupEditor(group =>
                    {
                        group.UseItems(GalleryAmenities.Items);
                        group.Columns(2);
                    });
                section
                    .Add(model => model.DownAmenities)
                    .UseCheckboxGroupEditor(group =>
                    {
                        group.UseItems(GalleryAmenities.Items);
                        group.Columns(columns => columns.Small(2).Large(3));
                        group.Flow = CheckboxGroupFlow.Down;
                    });
                section
                    .Add(model => model.CardColumnCabins)
                    .UseCheckboxGroupEditor(group =>
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
                    .UseCheckboxGroupEditor(group =>
                    {
                        group.UseItems(GalleryAmenities.Items);
                        group.Columns(2);
                    });
                section
                    .Add(model => model.NarrowAmenitiesBeside)
                    .UseCheckboxGroupEditor(group =>
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
                    .UseCheckboxGroupEditor(group =>
                        group.UseItems(
                            GalleryTransport.Items.Select(item => item with { Description = null })
                        )
                    );
                section
                    .Add(model => model.AirportCodes)
                    .UseCheckboxGroupEditor(group =>
                        group.UseItems(GalleryAirportCodes.Items)
                    );
                section
                    .Add(model => model.Guides)
                    .UseCheckboxGroupEditor(group =>
                    {
                        group.Appearance = CheckboxGroupAppearance.Cards;
                        group.UseItems(GalleryGuides.Items);
                    });
                section
                    .Add(model => model.Cities)
                    .UseCheckboxGroupEditor(group =>
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
                    "UseSegmentedControlEditor without UseItems. The enum or Boolean property supplies the choices.";
                section.Add(model => model.Cabin).UseSegmentedControlEditor();
                section.Add(model => model.OptionalCabin).UseSegmentedControlEditor();
                section.Add(model => model.Insured).UseSegmentedControlEditor();
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledCabin).UseSegmentedControlEditor().Title =
                    "Title override";
                section.Add(model => model.DescribedCabin).UseSegmentedControlEditor();
                section
                    .Add(model => model.FieldDescribedCabin)
                    .UseSegmentedControlEditor()
                    .Description = "Set with the field's Description.";
                section.Add(model => model.ReadOnlyCabin).UseSegmentedControlEditor();
            }
        );
        fields.AddSection(
            "UseItems and class names",
            section =>
            {
                section
                    .Add(model => model.Seat)
                    .UseSegmentedControlEditor(toggle =>
                        toggle.UseItems(GallerySeats.Items)
                    );
                section
                    .Add(model => model.StyledCabin)
                    .UseSegmentedControlEditor(toggle =>
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
                    .UseSegmentedControlEditor(toggle =>
                        toggle.UseItems(GalleryTransport.Items)
                    );
                section
                    .Add(model => model.Guide)
                    .UseSegmentedControlEditor(toggle =>
                        toggle.UseItems(GalleryGuides.Items)
                    );
                section
                    .Add(model => model.AirportCode)
                    .UseSegmentedControlEditor(toggle =>
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
                section.Add(model => model.Cabins).UseToggleGroupEditor();
                section
                    .Add(model => model.Amenities)
                    .UseToggleGroupEditor(group => group.UseItems(GalleryAmenities.Items));
                section.Add(model => model.Cabin).UseToggleGroupEditor();
                section
                    .Add(model => model.Days)
                    .UseToggleGroupEditor(group =>
                    {
                        group.UseItems(GalleryDays.Items);
                        group.Appearance = ToggleGroupAppearance.Joined;
                    });
                section
                    .Add(model => model.OptionalCabin)
                    .UseToggleGroupEditor(group =>
                        group.Appearance = ToggleGroupAppearance.Joined
                    );
                section
                    .Add(model => model.Meals)
                    .UseToggleGroupEditor(group =>
                    {
                        group.UseItems(GalleryMeals.Items);
                        group.Appearance = ToggleGroupAppearance.Buttons;
                    });
                section
                    .Add(model => model.Insured)
                    .UseToggleGroupEditor(group =>
                        group.Appearance = ToggleGroupAppearance.Buttons
                    );
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledCabins).UseToggleGroupEditor().Title =
                    "Title override";
                section.Add(model => model.DescribedCabins).UseToggleGroupEditor();
                section.Add(model => model.ReadOnlyCabins).UseToggleGroupEditor();
                section
                    .Add(model => model.StyledCabins)
                    .UseToggleGroupEditor(group =>
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
                    .UseToggleGroupEditor(group => group.UseItems(GalleryTransport.Items));
                section
                    .Add(model => model.Guides)
                    .UseToggleGroupEditor(group =>
                    {
                        group.CheckPlacement = ToggleGroupCheckPlacement.Start;
                        group.UseItems(GalleryGuides.Items);
                    });
                section
                    .Add(model => model.AirportCodes)
                    .UseToggleGroupEditor(group =>
                    {
                        group.CheckPlacement = ToggleGroupCheckPlacement.End;
                        group.UseItems(GalleryAirportCodes.Items);
                    });
                section
                    .Add(model => model.City)
                    .UseToggleGroupEditor(group =>
                    {
                        group.Appearance = ToggleGroupAppearance.Joined;
                        group.UseItems(GalleryCities.Items);
                    });
                section
                    .Add(model => model.ButtonTransports)
                    .UseToggleGroupEditor(group =>
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
                section.Description = "UseTextareaEditor with explicit settings.";
                section
                    .Add(model => model.PlaceholderNotes)
                    .UseTextareaEditor(textarea =>
                        textarea.Placeholder = "Anything the crew should know"
                    );
                section
                    .Add(model => model.TallNotes)
                    .UseTextareaEditor(textarea => textarea.Rows = 8);
                section
                    .Add(model => model.StyledNotes)
                    .UseTextareaEditor(textarea =>
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
                    "The Boolean template forwards to Editors/Checkbox. UseCheckboxEditor renders the same.";
                section.Add(model => model.Subscribed);
                section.Add(model => model.DescribedSubscribed);
                section.Add(model => model.ExplicitSubscribed).UseCheckboxEditor();
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
                    .UseCheckboxEditor(checkbox => checkbox.ClassNames.Label = "italic");
                section
                    .Add(model => model.RootStyled)
                    .UseCheckboxEditor(checkbox =>
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
                section.Description = "UseToggleEditor on Boolean properties.";
                section.Add(model => model.Notifications).UseToggleEditor();
                section.Add(model => model.Newsletter).UseToggleEditor();
                section.Add(model => model.DescribedNotifications).UseToggleEditor();
            }
        );
        fields.AddSection(
            "Validation",
            section =>
            {
                section.Description = "Turn these off and save to see the server errors.";
                section.Add(model => model.RequiredAlerts).UseToggleEditor();
                section.Add(model => model.RequiredTracking).UseToggleEditor();
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledNotifications).UseToggleEditor().Title =
                    "Title override";
                section
                    .Add(model => model.FieldDescribedNotifications)
                    .UseToggleEditor()
                    .Description = "Set with the field's Description.";
                section.Add(model => model.ReadOnlyOn).UseToggleEditor();
                section.Add(model => model.ReadOnlyOff).UseToggleEditor();
            }
        );
        fields.AddSection(
            "Class names",
            section =>
            {
                section
                    .Add(model => model.LabelStyled)
                    .UseToggleEditor(toggle => toggle.ClassNames.Label = "italic");
                section
                    .Add(model => model.RootStyled)
                    .UseToggleEditor(toggle =>
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
                    "UseDateInputEditor, UseDateTimeInputEditor and UseTimeInputEditor with explicit settings.";
                section
                    .Add(model => model.BoundedDeparture)
                    .UseDateInputEditor(date =>
                    {
                        date.Max = new DateOnly(2026, 12, 31);
                        date.Min = new DateOnly(2026, 1, 1);
                    });
                section
                    .Add(model => model.WeeklyDeparture)
                    .UseDateInputEditor(date => date.Step = 7);
                section
                    .Add(model => model.QuarterHourBoarding)
                    .UseDateTimeInputEditor(dateTime =>
                        dateTime.Step = TimeSpan.FromMinutes(15)
                    );
                section
                    .Add(model => model.OfficeHoursGate)
                    .UseTimeInputEditor(time =>
                    {
                        time.Max = new TimeOnly(18, 0);
                        time.Min = new TimeOnly(8, 0);
                        time.Step = TimeSpan.FromMinutes(30);
                    });
                section
                    .Add(model => model.StyledDeparture)
                    .UseDateInputEditor(date => date.ClassNames.Control = "border-dashed");
            }
        );
    }

    private static void ConfigureIconFields(ResourceFieldsBuilder<IconGallery> fields)
    {
        fields.AddSection(
            "Every registered icon",
            section =>
            {
                section.Description =
                    "No Icons list: the picker loads every registered icon from the panel's icons endpoint when it first opens.";
                section.Add(model => model.Icon).UseIconEditor();
                section.Add(model => model.TourIcon).UseIconEditor();
            }
        );
        fields.AddSection(
            "Icons lists and layouts",
            section =>
            {
                section.Description =
                    "Auto shows tiles for 24 icons or fewer and the picker for more.";
                section
                    .Add(model => model.Amenity)
                    .UseIconEditor(editor => editor.Icons = GalleryIcons.Amenities);
                section
                    .Add(model => model.Activity)
                    .UseIconEditor(editor => editor.Icons = GalleryIcons.Activities);
                section
                    .Add(model => model.PickerAmenity)
                    .UseIconEditor(editor =>
                    {
                        editor.Icons = GalleryIcons.Amenities;
                        editor.Layout = IconEditorLayout.Picker;
                    });
                section
                    .Add(model => model.Transport)
                    .UseIconEditor(editor =>
                    {
                        editor.Icons = GalleryIcons.Transport;
                        editor.Layout = IconEditorLayout.Inline;
                    });
            }
        );
        fields.AddSection(
            "Stored values",
            section =>
            {
                section.Description =
                    "A stored name that isn't an icon shows a dashed box and saves unchanged; a new value must be one the field offers.";
                section.Add(model => model.LegacyIcon).UseIconEditor();
                section
                    .Add(model => model.LegacyAmenity)
                    .UseIconEditor(editor => editor.Icons = GalleryIcons.Amenities);
                section.Add(model => model.ReadOnlyIcon).UseIconEditor();
            }
        );
    }

    private static void ConfigureImageFields(ResourceFieldsBuilder<ImageGallery> fields)
    {
        fields.AddSection(
            "Layouts",
            section =>
            {
                section.Description =
                    "A picked file uploads straight away to the playground's image store; Save keeps the returned value.";
                section.Add(model => model.Photo).UseImageEditor(ConfigureEditor);
                section.Add(model => model.Cover).UseImageEditor(ConfigureEditor);
                section
                    .Add(model => model.Thumbnail)
                    .UseImageEditor(editor =>
                    {
                        ConfigureEditor(editor);
                        editor.Layout = ImageEditorLayout.Compact;
                    });
                section
                    .Add(model => model.Attachment)
                    .UseImageEditor(editor =>
                    {
                        ConfigureEditor(editor);
                        editor.Layout = ImageEditorLayout.Attachment;
                    });
            }
        );
        fields.AddSection(
            "ImageEditor settings",
            section =>
            {
                section
                    .Add(model => model.Limited)
                    .UseImageEditor(editor =>
                    {
                        ConfigureEditor(editor);
                        editor.MaxFileSize = 100 * 1024;
                        editor.AcceptedTypes = ["image/png", "image/webp"];
                    });
                section
                    .Add(model => model.Styled)
                    .UseImageEditor(editor =>
                    {
                        ConfigureEditor(editor);
                        editor.Layout = ImageEditorLayout.Compact;
                        editor.ClassNames.Image = "ring-2 ring-primary";
                        editor.ClassNames.Note = "italic";
                    });
                section.Add(model => model.ReadOnlyPhoto).UseImageEditor(ConfigureEditor);
            }
        );

        static void ConfigureEditor(ImageEditor editor) =>
            DestinationResourceRegistration.ConfigureEditor(editor);
    }

    private static void ConfigureListEditorFields(ResourceFieldsBuilder<ListEditorGallery> fields)
    {
        fields.AddSection(
            "Defaults",
            section =>
            {
                section.Description =
                    "No UseEditor. A list of strings or numbers gets the list editor, with limits from [Length].";
                section.Add(model => model.Highlights);
                section.Add(model => model.DepartureDays);
                section.Add(model => model.Themes);
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledHighlights).Title = "Title override";
                section.Add(model => model.DescribedHighlights);
                section.Add(model => model.ReadOnlyHighlights);
            }
        );
        fields.AddSection(
            "ListEditor settings",
            section =>
            {
                section.Description =
                    "UseListEditor with explicit settings. Turn on Skip client validation to see the server's checks.";
                section
                    .Add(model => model.Itinerary)
                    .UseListEditor(list =>
                    {
                        list.MinItems = 1;
                        list.MaxItems = 8;
                        list.Sortable = true;
                        list.Numbered = true;
                        list.MaxLength = 120;
                    });
                section
                    .Add(model => model.ArrivalAirports)
                    .UseListEditor(list =>
                    {
                        list.MaxItems = 6;
                        list.Unique = true;
                        list.Casing = ListEditorCasing.Upper;
                        list.Pattern = "^[A-Z]{3}$";
                        list.PatternMessage = "Use a three-letter IATA code, like LIS.";
                    });
                section
                    .Add(model => model.MonthlyDepartures)
                    .UseListEditor(list =>
                    {
                        list.Min = 1;
                        list.Max = 31;
                        list.Unique = true;
                    });
                section
                    .Add(model => model.AddOnPrices)
                    .UseListEditor(list =>
                    {
                        list.Prefix = "€";
                        list.Step = 0.01;
                    });
                section
                    .Add(model => model.BagWeights)
                    .UseListEditor(list =>
                    {
                        list.Suffix = "kg";
                        list.Align = ListEditorAlign.Start;
                    });
                section
                    .Add(model => model.PackingList)
                    .UseListEditor(list =>
                    {
                        list.AddText = "Add something to pack";
                        list.EmptyText = "Nothing to pack yet";
                        list.Placeholder = "e.g. Comfortable walking shoes";
                    });
            }
        );
    }

    private static void ConfigureDictionaryEditorFields(
        ResourceFieldsBuilder<DictionaryEditorGallery> fields
    )
    {
        fields.AddSection(
            "Defaults",
            section =>
            {
                section.Description =
                    "No UseEditor. A dictionary of strings, numbers or enums gets the dictionary editor; an enum key is a select of its members.";
                section.Add(model => model.Notes);
                section.Add(model => model.SeasonalPrices);
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledNotes).Title = "Title override";
                section.Add(model => model.ReadOnlyNotes);
            }
        );
        fields.AddSection(
            "DictionaryEditor settings",
            section =>
            {
                section.Description =
                    "UseDictionaryEditor with explicit settings. Turn on Skip client validation to see the server's checks.";
                section
                    .Add(model => model.LocalNames)
                    .UseDictionaryEditor(dictionary =>
                    {
                        dictionary.EmptyText = "No translations yet. Pages use the English name.";
                        dictionary.Key(key =>
                        {
                            key.Label = "Language";
                            key.Casing = DictionaryEditorCasing.Lower;
                            key.Pattern = "^[a-z]{2}$";
                            key.PatternMessage = "Use a two-letter language code, like pt.";
                        });
                        dictionary.Value(value => value.Label = "Local name");
                    });
                section
                    .Add(model => model.BaggageAllowance)
                    .UseDictionaryEditor(dictionary =>
                    {
                        dictionary.Key(key => key.Label = "Cabin");
                        dictionary.Value(value =>
                        {
                            value.Label = "Allowance";
                            value.Min = 0;
                            value.Max = 50;
                            value.Suffix = "kg";
                        });
                    });
                section
                    .Add(model => model.GuidesByLanguage)
                    .UseDictionaryEditor(dictionary =>
                    {
                        dictionary.Key(key =>
                        {
                            key.Label = "Language";
                            key.UseItems([
                                new ChoiceItem("en", "English"),
                                new ChoiceItem("pt", "Portuguese"),
                                new ChoiceItem("es", "Spanish"),
                            ]);
                        });
                        dictionary.Value(value =>
                        {
                            value.Label = "Guide";
                            value.UseResource<Guide>();
                        });
                    });
                section
                    .Add(model => model.TourPrices)
                    .UseDictionaryEditor(dictionary =>
                    {
                        dictionary.MinItems = 1;
                        dictionary.Numbered = true;
                        dictionary.Key(key => key.Label = "Season");
                        dictionary.Value(value =>
                        {
                            value.Label = "Price per traveller";
                            value.Prefix = "€";
                        });
                    });
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
                    "UseSliderEditor without settings. Bounds come from [Range], or 0 to 100.";
                section.Add(model => model.Volume).UseSliderEditor();
                section.Add(model => model.Passengers).UseSliderEditor();
                section.Add(model => model.Rating).UseSliderEditor();
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledVolume).UseSliderEditor().Title =
                    "Title override";
                section.Add(model => model.DescribedVolume).UseSliderEditor();
                section
                    .Add(model => model.FieldDescribedVolume)
                    .UseSliderEditor()
                    .Description = "Set with the field's Description.";
                section.Add(model => model.ReadOnlyVolume).UseSliderEditor();
            }
        );
        fields.AddSection(
            "SliderEditor settings",
            section =>
            {
                section
                    .Add(model => model.Discount)
                    .UseSliderEditor(slider =>
                    {
                        slider.Max = 50;
                        slider.Min = 0;
                        slider.Step = 5;
                    });
                section
                    .Add(model => model.StyledVolume)
                    .UseSliderEditor(slider => slider.ClassNames.Control = "max-w-xs");
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
                    .UseSliderEditor(slider =>
                    {
                        slider.Max = 50;
                        slider.ValueFormat = "{0} km";
                        slider.MarkInterval = 10;
                    });
                section
                    .Add(model => model.Nights)
                    .UseSliderEditor(slider =>
                    {
                        slider.MarkInterval = 1;
                        slider.MarkLabels = SliderMarkLabels.All;
                    });
                section
                    .Add(model => model.GuestRating)
                    .UseSliderEditor(slider =>
                    {
                        slider.AddMark(1, "Poor");
                        slider.AddMark(3, "Good");
                        slider.AddMark(5, "Excellent");
                    });
                section
                    .Add(model => model.PlainVolume)
                    .UseSliderEditor(slider =>
                    {
                        slider.ShowValue = false;
                        slider.MarkLabels = SliderMarkLabels.None;
                    });
                section
                    .Add(model => model.StyledMarkVolume)
                    .UseSliderEditor(slider =>
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
                    "UseOneTimeCodeEditor without settings. The length comes from [StringLength], or 6.";
                section.Add(model => model.Code).UseOneTimeCodeEditor();
                section.Add(model => model.RequiredCode).UseOneTimeCodeEditor();
                section.Add(model => model.Pin).UseOneTimeCodeEditor();
            }
        );
        fields.AddSection(
            "Field configuration",
            section =>
            {
                section.Add(model => model.TitledCode).UseOneTimeCodeEditor().Title =
                    "Title override";
                section.Add(model => model.DescribedCode).UseOneTimeCodeEditor();
                section
                    .Add(model => model.FieldDescribedCode)
                    .UseOneTimeCodeEditor()
                    .Description = "Set with the field's Description.";
                section.Add(model => model.ReadOnlyCode).UseOneTimeCodeEditor();
            }
        );
        fields.AddSection(
            "OneTimeCodeEditor settings",
            section =>
            {
                section
                    .Add(model => model.LongCode)
                    .UseOneTimeCodeEditor(code => code.Length = 8);
                section
                    .Add(model => model.StyledCode)
                    .UseOneTimeCodeEditor(code => code.ClassNames.Control = "gap-3");
            }
        );
    }
}
