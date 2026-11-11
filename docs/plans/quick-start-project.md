# Downloadable quick start project

Status: proposed (2026-10-25).

Give readers of the admin panel quick start (`/docs/admin/quick-start`) a project to download and start from, so they don't have to set up an ASP.NET Core app, an EF Core `DbContext` and the `Customer` and `Booking` entities before they can follow along.

The quick start page (`content/docs/admin/quick-start.mdx` in the website repo) would link to the download, and its steps would build on that project.

Groundwork (2026-11-11): the generator in `../sample-app-generator` (renamed from `voyager-travel-generator`) now builds apps from scenarios, each a host (a `dotnet new` template and its arguments) and a self-contained dataset. Its `quick-start` scenario generates a plain MVC app without auth, with the page's `Trip` and `Departure` entities and seeded trips (Voyager Travel codes and titles) and departures. Verified by generating it on the `mvc` and `webapp` hosts and running the MVC app, and by comparing the `voyager-travel` scenario's output with the previous generator's (same data and files, apart from template ports, user secrets ids, migration timestamps and password hashes). The download itself is still open.

Open questions to settle with Jerrie before implementing: what the project contains (only the starting point, or the finished result as well), where it lives and how it's downloaded (a folder in this repo, a separate repo, a zip or a `dotnet new` template), the database (SQLite with seed data is the obvious default), and how it's kept in step with releases.
