using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VoyagerTravel.Data;
using VoyagerTravel.Data.Seeding;

namespace StellarAdmin.Scenarios.Fixtures;

// The seeded Voyager Travel database is built once per process and never written. Each app gets its own
// in-memory copy, which lives as long as the app, so every spec run starts from the same data and nothing
// it changes reaches another run
public static class VoyagerTravelDatabase
{
    private static readonly Lock CopyLock = new();
    private static readonly Lazy<SqliteConnection> Template = new(CreateTemplate);

    public static IServiceCollection AddVoyagerTravelDatabase(this IServiceCollection services)
    {
        // Each context opens its own connection to the copy; the held one keeps the copy alive until the
        // app's services are disposed
        services.AddSingleton(_ => new DatabaseCopy());
        services.AddDbContext<ApplicationDbContext>(
            (provider, options) =>
                options.UseSqlite(provider.GetRequiredService<DatabaseCopy>().ConnectionString)
        );

        return services;
    }

    private static SqliteConnection CreateTemplate()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var context = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options
        );
        context.Database.EnsureCreated();
        DatabaseSeeder.Seed(context);

        return connection;
    }

    private sealed class DatabaseCopy : IDisposable
    {
        private readonly SqliteConnection _connection;

        public DatabaseCopy()
        {
            ConnectionString = $"Data Source=voyager-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
            _connection = new SqliteConnection(ConnectionString);
            _connection.Open();
            lock (CopyLock)
            {
                Template.Value.BackupDatabase(_connection);
            }
        }

        public string ConnectionString { get; }

        public void Dispose() => _connection.Dispose();
    }
}
