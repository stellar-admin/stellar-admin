using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots;

// The bookings scenarios' Travellers resource, which names each booking's traveller in the grid
internal static class Travellers
{
    public static void Add(StellarAdminBuilder admin) =>
        admin.AddEfCoreResource<Traveller>(resource =>
        {
            resource.UseTitle(traveller => traveller.FirstName + " " + traveller.LastName);
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(traveller => traveller.Email));
            });
        });
}
