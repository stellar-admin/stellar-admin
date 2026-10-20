using Microsoft.EntityFrameworkCore;

namespace StellarAdmin.EntityFrameworkCore;

internal interface IManyToManyFieldBinder
{
    string NavigationName { get; }

    Task CopyAsync(DbContext db, object target, object source, CancellationToken cancellationToken);
}
