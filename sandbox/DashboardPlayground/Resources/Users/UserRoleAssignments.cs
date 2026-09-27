using DashboardPlayground.Data;
using Microsoft.EntityFrameworkCore;

namespace DashboardPlayground.Resources.Users;

internal static class UserRoleAssignments
{
    internal static async Task<Dictionary<string, string>?> ResolveNamesAsync(
        ApplicationDbContext db,
        IEnumerable<string> roleIds,
        CancellationToken cancellationToken
    )
    {
        var ids = roleIds.ToHashSet(StringComparer.Ordinal);
        if (ids.Any(string.IsNullOrWhiteSpace))
        {
            return null;
        }

        var roles = await db
            .Roles.AsNoTracking()
            .Where(role => ids.Contains(role.Id))
            .Select(role => new { role.Id, role.Name })
            .ToDictionaryAsync(role => role.Id, role => role.Name, cancellationToken);

        return roles.Count == ids.Count && roles.Values.All(name => name is not null)
            ? roles.ToDictionary(role => role.Key, role => role.Value!, StringComparer.Ordinal)
            : null;
    }
}
