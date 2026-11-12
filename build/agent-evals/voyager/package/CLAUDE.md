# Voyager

Voyager Travel's internal web app: ASP.NET Core Razor Pages, EF Core with SQLite. See README.md for running it and signing in during development.

The admin panel uses StellarAdmin (NuGet packages StellarAdmin and StellarAdmin.EntityFrameworkCore). It is registered in Program.cs with `AddStellarAdmin()` and served at `/admin` by `MapStellarAdmin("/admin")`, with nothing configured yet. Documentation: https://www.stellaradmin.com/docs/admin
