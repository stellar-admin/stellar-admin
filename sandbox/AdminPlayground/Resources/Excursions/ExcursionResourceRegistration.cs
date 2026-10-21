using AdminPlayground.Data;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Sidebar;

namespace AdminPlayground.Resources.Excursions;

// Deliberately misconfigured, so the playground always has configuration problems for the Development indicator to
// show
internal static class ExcursionResourceRegistration
{
    internal static void AddMisconfiguredExcursionResource(this StellarAdminBuilder admin)
    {
        admin.AddResource<Excursion>(resource =>
        {
            resource.SidebarItem(item =>
            {
                item.Group = "Trips";
                item.Icon = "map";
            });
            resource.UseDataSource<ExcursionDataSource>();
            resource.UseKey(excursion => excursion.Id);
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(excursion => excursion.Name);
                    columns.Add(excursion => excursion.Destination);
                    columns.Add(excursion => excursion.Price);

                    // Computed without a title: left out
                    columns.Add(excursion => excursion.Price * 1.2m);
                });

                // Price isn't sortable: ignored
                index.DefaultSortBy(excursion => excursion.Price);

                // 15 isn't one of the page sizes: added to them
                index.EnablePaging(paging =>
                {
                    paging.PageSize = 15;
                    paging.PageSizes = [10, 25];
                });
            });

            // The data source can't create: create is turned off
            resource.AllowCreate(create =>
                create.Fields(fields => fields.Add(excursion => excursion.Name))
            );
        });

        // The booking isn't an entity of the context: the resource is turned off, and Development shows why at
        // /stellaradmin/excursion-bookings
        admin.AddEfCoreResource<ApplicationDbContext, ExcursionBooking>();

        // A blank page name: the link is left out
        admin.AddSidebarLink("Excursion report", SidebarLinkTarget.Page(" "));
    }
}
