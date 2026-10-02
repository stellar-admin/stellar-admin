using Microsoft.EntityFrameworkCore.Metadata;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

internal interface IEfCoreLookupReference
{
    INavigation? FindNavigation(IEntityType model, string fieldName);
}
