using Microsoft.EntityFrameworkCore.Metadata;

namespace StellarAdmin.EntityFrameworkCore;

internal interface IEfCoreLookupReference
{
    INavigation? FindNavigation(IEntityType model, string fieldName);
}
