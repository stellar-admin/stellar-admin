using DashboardPlayground.Resources.FieldEditors;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard;
using StellarAdmin.Dashboard.Resources.Builders;
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
            "Columns(columns => columns.Default(2))",
            section =>
            {
                section.Description = "Two columns at every width, even on a phone.";
                section.Columns(columns => columns.Default(2));
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
