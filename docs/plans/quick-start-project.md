# Downloadable quick start project

Status: proposed (2026-10-25).

Give readers of the admin panel quick start (`/docs/admin/quick-start`) a project to download and start from, so they don't have to set up an ASP.NET Core app, an EF Core `DbContext` and the `Customer` and `Booking` entities before they can follow along.

The quick start page (`content/docs/admin/quick-start.mdx` in the website repo) would link to the download, and its steps would build on that project.

Open questions to settle with Jerrie before implementing: what the project contains (only the starting point, or the finished result as well), where it lives and how it's downloaded (a folder in this repo, a separate repo, a zip or a `dotnet new` template), the database (SQLite with seed data is the obvious default), and how it's kept in step with releases.
