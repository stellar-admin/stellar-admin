# Voyager Travel

Voyager Travel's internal web app: Razor Pages, EF Core with SQLite (`voyager-travel.db`, created and seeded with sample data on first run). The model is in `Models/` and `Data/ApplicationDbContext.cs`; `Data/Seeding/` generates the sample data.

The admin panel (StellarAdmin) is at `/admin`.

The database holds staff accounts (ASP.NET Core Identity tables) for later; during development, sign in with `/dev-login?name=sam&roles=Sales` (comma-separated roles) and sign out with `/dev-logout`.
