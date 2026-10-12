using DashboardPlayground.Resources.FieldEditors;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Displays;
using StellarAdmin.Dashboard.Resources.Editors;
using StellarAdmin.TagHelpers;

namespace DashboardPlayground.Resources.FormLayouts;

// Visual harness for form grid layouts. See docs/plans/archive/dashboard-form-grid.md. Resize the window, or open
// the create forms from a lookup's New button, to see each breakpoint.
internal static class FormLayoutGalleryRegistration
{
    private const string Group = "Form layouts";

    internal static void AddFormLayoutGallery(this StellarAdminDashboardBuilder dashboard)
    {
        dashboard.Services.AddSingleton<LayoutDestinationStore>();
        dashboard.Services.AddSingleton<LayoutDestinationLookupSource>();
        dashboard.AddLayoutResource<ColumnsLayoutGallery>(
            "form-columns",
            "Columns",
            10,
            FormSectionLayout.Stacked,
            ConfigureColumnsFields
        );
        dashboard.AddLayoutResource<SpansLayoutGallery>(
            "form-spans",
            "Spans",
            20,
            FormSectionLayout.Stacked,
            ConfigureSpansFields
        );
        dashboard.AddLayoutResource<SectionsLayoutGallery>(
            "form-sections",
            "Sections and groups",
            30,
            FormSectionLayout.Card,
            ConfigureSectionsFields
        );
        dashboard.AddLayoutResource<SectionLayoutsGallery>(
            "form-section-layouts",
            "Section layouts",
            40,
            FormSectionLayout.Split,
            ConfigureSectionLayoutsFields
        );
        dashboard.AddLayoutResource<MixedEditorsLayoutGallery>(
            "form-mixed-editors",
            "Mixed editors",
            50,
            FormSectionLayout.Stacked,
            ConfigureMixedEditorsFields
        );
        dashboard.AddLayoutResource<DisplayFieldsLayoutGallery>(
            "form-display-fields",
            "Display fields",
            55,
            FormSectionLayout.Card,
            ConfigureDisplayFieldsFields
        );
        dashboard.AddDestinationResource();
    }

    private static void AddLayoutResource<TRecord>(
        this StellarAdminDashboardBuilder dashboard,
        string slug,
        string label,
        int order,
        FormSectionLayout sectionLayout,
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
                resource.AllowCreate(create =>
                {
                    create.SectionLayout = sectionLayout;
                    create.Fields(configureFields);
                });
                resource.AllowEdit(edit =>
                {
                    edit.SectionLayout = sectionLayout;
                    edit.Fields(configureFields);
                });
            }
        );
    }

    // The section create form is Split with two columns, so the sheet shows it stacked and narrow
    private static void AddDestinationResource(this StellarAdminDashboardBuilder dashboard)
    {
        dashboard.AddResource<LayoutDestination>(
            "form-destinations",
            resource =>
            {
                resource.PluralLabel = "Destinations";
                resource.SingularLabel = "Destination";
                resource.SidebarItem(item =>
                {
                    item.Group = Group;
                    item.Order = 60;
                });
                resource.UseDataSource<LayoutDestinationDataSource>();
                resource.Index(index =>
                    index.Columns(columns =>
                    {
                        columns.Add(destination => destination.Name);
                        columns.Add(destination => destination.Code);
                        columns.Add(destination => destination.Country);
                    })
                );
                resource.AllowCreate<CreateLayoutDestinationModel, CreateLayoutDestinationHandler>(
                    create =>
                        create.Fields(fields =>
                            fields.AddSection(
                                "Destination",
                                section =>
                                {
                                    section.Description = "This is a description for destination.";

                                    section.Layout = FormSectionLayout.Split;
                                    section.Columns(columns => columns.Small(2));
                                    section.Add(model => model.Name);
                                    section.Add(model => model.Code);
                                    section.Add(model => model.Country);
                                    section.Add(model => model.Region);
                                    section
                                        .Add(model => model.Notes)
                                        .UseEditor<TextareaEditor>(textarea => textarea.Rows = 3)
                                        .ColumnSpanFull();
                                }
                            )
                        )
                );
            }
        );
    }

    private static void ConfigureColumnsFields(ResourceFieldsBuilder<ColumnsLayoutGallery> fields)
    {
        fields.AddSection(
            "Columns(3)",
            section =>
            {
                section.Description = "One column below Medium (40rem), three from Medium.";
                section.Columns(3);
                section.Add(model => model.TripName);
                section.Add(model => model.Destination);
                section.Add(model => model.Departure);
                section.Add(model => model.ReturnDate);
                section.Add(model => model.Travellers);
                section.Add(model => model.Budget);
            }
        );
        fields.AddSection(
            "Columns(columns => columns.Small(2).Medium(3).Large(4))",
            section =>
            {
                section.Description =
                    "One column by default, two from Small (30rem), three from Medium (40rem) and four from Large (56rem).";
                section.Columns(columns => columns.Small(2).Medium(3).Large(4));
                section.Add(model => model.FirstName);
                section.Add(model => model.LastName);
                section.Add(model => model.Email);
                section.Add(model => model.Phone);
                section.Add(model => model.PassportNumber);
                section.Add(model => model.Nationality);
                section.Add(model => model.LoyaltyNumber);
                section.Add(model => model.SeatPreference);
            }
        );
        fields.AddSection(
            "Columns(columns => columns.Base(2))",
            section =>
            {
                section.Description = "Two columns at every width, even on a phone.";
                section.Columns(columns => columns.Base(2));
                section.Add(model => model.Street);
                section.Add(model => model.City);
                section.Add(model => model.PostalCode);
                section.Add(model => model.Country);
            }
        );
        fields.AddSection(
            "No Columns",
            section =>
            {
                section.Description = "The default: one column at every width.";
                section.Add(model => model.EmergencyContact);
                section.Add(model => model.EmergencyPhone);
            }
        );
    }

    private static void ConfigureSpansFields(ResourceFieldsBuilder<SpansLayoutGallery> fields)
    {
        fields.AddSection(
            "Spans in Columns(4)",
            section =>
            {
                section.Description =
                    "Items fill the grid in order and wrap. Below Medium the grid has one column, so every item takes a row.";
                section.Columns(4);
                section.Add(model => model.Airline);
                section.Add(model => model.FlightNumber);
                section.Add(model => model.Route).ColumnSpan(2);
                section.Add(model => model.Terminal).ColumnSpan(3);
                section.Add(model => model.Gate);
                section.Add(model => model.Remarks).ColumnSpanFull();
                section.Add(model => model.Departs).ColumnSpan(2);
                section.Add(model => model.Arrives).ColumnSpan(2);
            }
        );
        fields.AddSection(
            "Spans per breakpoint in Small(2).Medium(4)",
            section =>
            {
                section.Description =
                    "An unset breakpoint uses the next smaller one, and each span is capped at the grid's columns.";
                section.Columns(columns => columns.Small(2).Medium(4));
                section.Add(model => model.Hotel).ColumnSpan(span => span.SmallFull().Medium(2));
                section.Add(model => model.CheckIn);
                section.Add(model => model.CheckOut);
                section.Add(model => model.RoomType).ColumnSpan(span => span.Small(2).Medium(3));
                section.Add(model => model.Guests);
                section
                    .Add(model => model.SpecialRequests)
                    .ColumnSpan(span => span.SmallFull().Medium(2).LargeFull());
            }
        );
        fields.AddSection(
            "Spans larger than Columns(2)",
            section =>
            {
                section.Description = "A span of 4 or 12 is reduced to the grid's two columns.";
                section.Columns(2);
                section.Add(model => model.CarRental).ColumnSpan(4);
                section.Add(model => model.PickUp);
                section.Add(model => model.DropOff);
                section.Add(model => model.Insurance).ColumnSpan(12);
            }
        );
        fields.AddSection(
            "Fewer items than Columns(4)",
            section =>
            {
                section.Description = "The remaining columns stay empty.";
                section.Columns(4);
                section.Add(model => model.Visa);
                section.Add(model => model.VisaExpiry);
            }
        );
        fields.AddSection(
            "Starts in Columns(3)",
            section =>
            {
                section.Description =
                    "A start of 1 begins a new row, and a later start leaves the columns before it empty. Starts are dropped below Medium, where the grid has one column.";
                section.Columns(3);
                section.Add(model => model.LeadTraveller);
                section.Add(model => model.Total).ColumnStart(3);
                section.Add(model => model.Outbound).ColumnStart(1);
                section.Add(model => model.Inbound);
                section.Add(model => model.PartySize);
                section
                    .Add(model => model.Notes)
                    .ColumnSpan(2)
                    .ColumnStart(start => start.Base(2).Large(1));
            }
        );
    }

    private static void ConfigureSectionsFields(ResourceFieldsBuilder<SectionsLayoutGallery> fields)
    {
        fields.Columns(columns => columns.Medium(2).Large(3));
        fields.AddSection(
            "Traveller",
            section =>
            {
                section.Description =
                    "The form has two columns from Medium and three from Large. This section spans two from Large and has Columns(2) of its own.";
                section.ColumnSpan(span => span.Large(2));
                section.Columns(2);
                section.Add(model => model.FirstName);
                section.Add(model => model.LastName);
                section.Add(model => model.Email);
                section.Add(model => model.Phone);
            }
        );
        fields.AddSection(
            "Loyalty",
            section =>
            {
                section.Description = "Span 1.";
                section.Add(model => model.LoyaltyProgramme);
                section.Add(model => model.LoyaltyNumber);
            }
        );
        fields.AddSection(
            "Itinerary",
            section =>
            {
                section.Description =
                    "ColumnSpanFull with Small(2).Large(4): a group stacks two fields in one cell, and a group spanning two columns has Small(2). A plain Columns(2) would keep one column, since the group is narrower than Medium (40rem).";
                section.ColumnSpanFull();
                section.Columns(columns => columns.Small(2).Large(4));
                section.AddGroup(group =>
                {
                    group.Add(model => model.Origin);
                    group.Add(model => model.Destination);
                });
                section.AddGroup(group =>
                {
                    group.ColumnSpan(2);
                    group.Columns(columns => columns.Small(2));
                    group.Add(model => model.Departure);
                    group.Add(model => model.ReturnDate);
                });
                section.Add(model => model.Travellers);
            }
        );
        fields.AddSection(
            "Preferences",
            section =>
            {
                section.Description =
                    "Spans two columns. A group with Columns(2) holds a stacked group and a field.";
                section.ColumnSpan(2);
                section.AddGroup(group =>
                {
                    group.Columns(2);
                    group.AddGroup(inner =>
                    {
                        inner.Add(model => model.Seat);
                        inner.Add(model => model.Meal);
                    });
                    group.Add(model => model.Notes);
                });
            }
        );
        fields.AddSection(
            "Emergency contact",
            section =>
            {
                section.Description = "Span 1, filling the last cell.";
                section.Add(model => model.EmergencyContact);
                section.Add(model => model.EmergencyPhone);
            }
        );
    }

    private static void ConfigureSectionLayoutsFields(
        ResourceFieldsBuilder<SectionLayoutsGallery> fields
    )
    {
        fields.AddSection(
            "Form layout",
            section =>
            {
                section.Description = "No Layout, so the section uses the form's Split layout.";
                section.Columns(2);
                section.Add(model => model.TripName);
                section.Add(model => model.Budget);
            }
        );
        fields.AddSection(
            "Layout = Stacked",
            section =>
            {
                section.Description = "Overrides the form's Split layout.";
                section.Layout = FormSectionLayout.Stacked;
                section.Columns(2);
                section.Add(model => model.Airline);
                section.Add(model => model.FlightNumber);
            }
        );
        fields.AddSection(
            "Layout = Card",
            section =>
            {
                section.Description = "Overrides the form's Split layout.";
                section.Layout = FormSectionLayout.Card;
                section.Columns(2);
                section.Add(model => model.Hotel);
                section.Add(model => model.RoomType);
            }
        );
        fields.AddSection(
            "Create sheet",
            section =>
            {
                section.Description =
                    "New opens the destination create form in the sheet. Its section is Split with Small(2), and the sheet stacks it.";
                section
                    .Add(model => model.Destination)
                    .UseEditor<LookupSheetEditor>(UseDestinations);
            }
        );
    }

    private static void ConfigureMixedEditorsFields(
        ResourceFieldsBuilder<MixedEditorsLayoutGallery> fields
    )
    {
        fields.AddSection(
            "Gallery options",
            section =>
            {
                section.Description =
                    "Turn on Reject every field to see error messages in every column.";
                section.Columns(2);
                section.Add(record => record.RejectEveryField);
                section.Add(record => record.SkipClientValidation);
            }
        );
        fields.AddSection(
            "Editors in Columns(3)",
            section =>
            {
                section.Description =
                    "Text, select, lookup, date, number, radio group, textarea and toggle editors of different heights.";
                section.Columns(3);
                section.Add(model => model.TravellerName);
                section.Add(model => model.Cabin).UseEditor<SelectEditor>();
                section
                    .Add(model => model.Destination)
                    .UseEditor<LookupSheetEditor>(UseDestinations);
                section.Add(model => model.Departure);
                section.Add(model => model.CheckedBags);
                section.Add(model => model.SeatCabin).UseEditor<RadioGroupEditor>();
                section
                    .Add(model => model.Notes)
                    .UseEditor<TextareaEditor>(textarea => textarea.Rows = 4)
                    .ColumnSpan(2);
                section.Add(model => model.Insurance).UseEditor<ToggleEditor>();
            }
        );
        fields.AddSection(
            "Horizontal fields in Columns(2)",
            section =>
            {
                section.Description = "Toggles and checkboxes keep their label beside the control.";
                section.Columns(2);
                section.Add(model => model.EmailUpdates).UseEditor<ToggleEditor>();
                section.Add(model => model.TextMessages).UseEditor<ToggleEditor>();
                section.Add(model => model.AcceptTerms).UseEditor<CheckboxEditor>();
                section.Add(model => model.SharePartners).UseEditor<CheckboxEditor>();
            }
        );
    }

    private static void ConfigureDisplayFieldsFields(
        ResourceFieldsBuilder<DisplayFieldsLayoutGallery> fields
    )
    {
        fields.AddDisplayField(model => model.Summary);
        fields.AddSection(
            "Display fields among inputs in Columns(3)",
            section =>
            {
                section.Description =
                    "AddDisplayField beside Add: labels and rows line up with the inputs, and nothing is bound.";
                section.Columns(3);
                section.Add(model => model.TravellerName);
                section.Add(model => model.Destination);
                section.AddDisplayField(model => model.BookingReference);
                section.Add(model => model.Departure);
                section
                    .AddDisplayField(model => model.Total)
                    .UseDisplay<NumberDisplay>(number => number.Format = "€#,##0.00");
                section.AddDisplayField(model => model.IsPaid);
            }
        );
        fields.AddDetails(
            "AddDetails in Columns(3)",
            details =>
            {
                details.Description =
                    "Stacked labels; the cabin uses UseDisplay<BadgeDisplay>, the status a [UIHint] template that picks the badge by value, and the notes are empty.";
                details.Columns(3);
                details.Add(model => model.BookingReference);
                details
                    .Add(model => model.Cabin)
                    .UseDisplay<BadgeDisplay>(badge => badge.Variant = BadgeVariant.Outline);
                details.Add(model => model.Status);
                details.Add(model => model.IsPaid);
                details
                    .Add(model => model.Total)
                    .UseDisplay<NumberDisplay>(number => number.Format = "€#,##0.00");
                details.Add(model => model.ContactEmail).ColumnSpan(2);
                var notes = details.Add(model => model.AgentNotes).ColumnSpanFull();
                notes.EmptyText = "No notes";
            }
        );
        fields.AddDetails(
            "Standard displays in Columns(3)",
            details =>
            {
                details.Description =
                    "No UseDisplay: each value renders with the default display for its type or data type, and the created date with its [DisplayFormat].";
                details.Columns(3);
                details.Add(model => model.HotelWebsite);
                details.Add(model => model.HotelPhone);
                details.Add(model => model.ContactEmail);
                details.Add(model => model.Departure);
                details.Add(model => model.CheckIn);
                details.Add(model => model.CreatedAt);
                details.Add(model => model.Cabin);
                details.Add(model => model.Nights);
                details.Add(model => model.IsPaid);
                details.Add(model => model.Itinerary).ColumnSpanFull();
            }
        );
        fields.AddDetails(
            "Display settings in Columns(3)",
            details =>
            {
                details.Description =
                    "Formats on NumberDisplay and DateDisplay, BooleanDisplay text, a link that opens in the same tab, a clamped multiline text, and UseBuiltInRendering.";
                details.Columns(3);
                details
                    .Add(model => model.Total)
                    .UseDisplay<NumberDisplay>(number => number.Format = "N2");
                details
                    .Add(model => model.Departure)
                    .UseDisplay<DateDisplay>(date => date.Format = "dddd d MMMM yyyy");
                details
                    .Add(model => model.IsPaid)
                    .UseDisplay<BooleanDisplay>(boolean =>
                    {
                        boolean.Appearance = BooleanDisplayAppearance.IconAndText;
                        boolean.TrueText = "Paid in full";
                    });
                details
                    .Add(model => model.HotelWebsite)
                    .UseDisplay<LinkDisplay>(link => link.OpenInNewTab = false);
                details.Add(
                    model => model.IsPaid,
                    field =>
                    {
                        field.Title = "Paid (text)";
                        field.UseDisplay<BooleanDisplay>(boolean =>
                            boolean.Appearance = BooleanDisplayAppearance.Text
                        );
                    }
                );
                details.Add(
                    model => model.CreatedAt,
                    field =>
                    {
                        field.Title = "Created (built-in)";
                        field.UseBuiltInRendering();
                    }
                );
                details
                    .Add(model => model.Itinerary)
                    .ColumnSpanFull()
                    .UseDisplay<TextDisplay>(text => text.LineClamp = 2);
            }
        );
        fields.AddDetails(
            "Audit: horizontal and divided in Columns(2)",
            details =>
            {
                details.Description = "Orientation = Horizontal and Variant = Divided.";
                details.Orientation = FieldOrientation.Horizontal;
                details.Variant = DetailsVariant.Divided;
                details.Columns(2);
                details.Add(model => model.CreatedAt);
                details.Add(model => model.CreatedBy);
                details.Add(model => model.UpdatedAt, field => field.EmptyText = "Never");
            }
        );
    }

    private static void UseDestinations(LookupSheetEditor lookup)
    {
        lookup.UseItems<LayoutDestinationLookupSource, LayoutDestination, string>(
            destination => destination.Code,
            destination => destination.Name,
            items => items.UseDescription(destination => destination.Country)
        );
        lookup.EnableCreate();
    }
}
