using AdminPlayground.Data;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Widgets;

namespace AdminPlayground.Dashboard;

public sealed record TeamSummary(int Users, int Departments, DateTimeOffset CheckedAt);

// Counts the people who can sign in; the widget refreshes, so CheckedAt shows when it last loaded
public sealed class TeamHandler(ApplicationDbContext db) : IWidgetHandler<TeamSummary>
{
    public async Task<TeamSummary?> LoadAsync(
        WidgetLoadContext context,
        CancellationToken cancellationToken
    ) =>
        new(
            await db.Users.CountAsync(cancellationToken),
            await db.Departments.CountAsync(cancellationToken),
            DateTimeOffset.Now
        );
}
