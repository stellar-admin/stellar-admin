using Microsoft.EntityFrameworkCore;
using StellarAdmin.Pro.Resources.Options;

namespace StellarAdmin.Pro.EntityFrameworkCore;

internal sealed class EfCoreResourceOptions<TContext, TEntity>(string name)
    : ResourceOptions<TEntity>(
        new IndexPageDefaults(
            name,
            "Create " + typeof(TEntity).Name,
            "No records yet",
            "Create a record to get started.",
            "database",
            [],
            null
        ),
        new FormPageDefaults("Create " + typeof(TEntity).Name, "Create", []),
        new FormPageDefaults("Edit " + typeof(TEntity).Name, "Save changes", []),
        new DeleteDefaults<TEntity>(
            "Delete " + typeof(TEntity).Name,
            "Are you sure you want to delete this record?",
            "Delete",
            "Cancel",
            _ => null
        )
    )
    where TContext : DbContext
    where TEntity : class
{
    public string? AuthorizationPolicy { get; set; }

    public List<EfCoreReference<TEntity>> References { get; } = [];
}
