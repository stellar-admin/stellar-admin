using System.Collections.Concurrent;
using System.Data.Common;
using IdentitySimplePlayground.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace StellarAdmin.Dashboard.Testing;

public sealed class ReferenceCommands : DbCommandInterceptor
{
    public ConcurrentQueue<string> Sql { get; } = new();

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default
    )
    {
        Sql.Enqueue(command.CommandText);

        return ValueTask.FromResult(result);
    }
}
