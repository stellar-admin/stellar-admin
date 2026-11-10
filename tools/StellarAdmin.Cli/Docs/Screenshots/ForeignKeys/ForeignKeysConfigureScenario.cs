using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Editors;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ForeignKeys;

public sealed class ForeignKeysConfigureScenario : Scenario
{
    public override string Description =>
        "The booking edit page with a traveller lookup that displays the selected traveller in a single row, and sets the sheet's title and search hint.";

    public override string StartPath => "/stellaradmin/bookings/edit/1";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Traveller>(resource =>
        {
            resource.UseTitle(traveller => traveller.FirstName + " " + traveller.LastName);
            resource.Reference(reference => reference.UseDescription(traveller => traveller.Email));
            resource.Index(index => index.EnablePaging());
        });
        admin.AddEfCoreResource<Booking>(resource =>
        {
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(booking => booking.Reference));
            });
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    // Two columns, so the lookup takes only half the form's width
                    fields.Columns(2);
                    fields
                        .Add(booking => booking.TravellerId)
                        .UseLookupSheetEditor(lookup =>
                        {
                            lookup.Field(field => field.Layout = LookupSheetEditorLayout.Input);
                            lookup.Sheet(sheet =>
                            {
                                sheet.Title = "Choose a traveller";
                                sheet.SearchPlaceholder = "Name or email";
                            });
                        });
                })
            );
        });
    }
}
