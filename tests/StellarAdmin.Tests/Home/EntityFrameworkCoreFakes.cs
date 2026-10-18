// Stand-ins for the EF Core types EntityCandidateScanner matches by name; this project doesn't
// reference EF Core

namespace Microsoft.EntityFrameworkCore
{
    public sealed class DbContextOptions<TContext>;

    public sealed class DbSet<TEntity>;
}

namespace Microsoft.Fakes.Identity
{
    using Microsoft.EntityFrameworkCore;

    public class IdentityContext
    {
        public DbSet<IdentityToken>? Tokens { get; set; }
    }

    public sealed class IdentityToken;
}
