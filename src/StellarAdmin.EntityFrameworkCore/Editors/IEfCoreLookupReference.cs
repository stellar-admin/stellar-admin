using Microsoft.EntityFrameworkCore.Metadata;

namespace StellarAdmin.EntityFrameworkCore;

internal interface IEfCoreLookupReference
{
    IEnumerable<string> FindIncludePaths(IEntityType model, string fieldName);
}
