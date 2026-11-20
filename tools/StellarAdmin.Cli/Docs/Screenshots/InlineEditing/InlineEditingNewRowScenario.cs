using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.InlineEditing;

public sealed class InlineEditingNewRowScenario : Scenario
{
    public override string Description =>
        "The destinations grid adding a destination in a new row, with the Create button in the grid.";

    public override string StartPath => "/stellaradmin/destinations";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Country>(resource =>
            resource.Index(index => index.AddTable("countries", table => table.EnablePaging()))
        );
        admin.AddEfCoreResource<Destination>(resource =>
        {
            resource.Index(index =>
            {
                index.AddTable(
                    "destinations",
                    table =>
                    {
                        table.Columns(columns =>
                        {
                            columns.Add(destination => destination.Name);
                            columns.Add(
                                destination => destination.Country!.Name,
                                column =>
                                {
                                    column.Title = "Country";
                                    column.EditsField(destination => destination.CountryId);
                                }
                            );
                            columns.Add(destination => destination.IsFeatured);
                        });
                        // Ten rows, so the grid crop stays short
                        table.EnablePaging(paging => paging.PageSize = 10);
                        table.EnableInlineEditing();
                        table.EnableInlineCreate(inline => inline.AddButton = InlineAddButton.Grid);
                    }
                );
            });

            resource.AllowEdit(edit =>
            {
                edit.Fields(fields =>
                {
                    fields.Add(destination => destination.Name);
                    fields.Add(destination => destination.CountryId);
                    fields.Add(destination => destination.IsFeatured);
                });
            });

            resource.AllowCreate(create =>
            {
                create.Fields(fields =>
                {
                    fields.Add(destination => destination.Name);
                    fields.Add(destination => destination.CountryId);
                    fields.Add(destination => destination.IsFeatured);
                });
                create.UseFactory(() => new Destination { Name = "", IsFeatured = true });
            });
        });
    }
}
