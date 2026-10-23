namespace StellarAdmin.EntityFrameworkCore;

// The EF Core settings of a resource that aren't part of its admin panel options
internal sealed class EfCoreResourceOptions<TEntity>
    where TEntity : class
{
    // The context UseDbContext pins; otherwise, the one that maps the entity is found
    internal Type? ContextType { get; set; }
}
