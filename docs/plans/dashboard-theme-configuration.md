# Dashboard theme configuration

Status: implemented and verified.

Dashboard previously hardcoded the shadcn Nova stylesheet in its shared layout. The requested app-wide API is `dashboard.ConfigureTheme(theme => { theme.Name = DashboardTheme.Ice; theme.IncludeSuggestedFonts = true; })`. No call keeps Nova and does not request web fonts.

The Dashboard theme enum covers all fifteen TagHelpers bundles. The layout resolves the selected bundle name and the corresponding suggested font families from one Dashboard catalog. Suggested fonts are opt-in and load from Google Fonts with `display=swap`; the CSS bundles already contain local and system fallback stacks. Theme options use the standard options pipeline and reject enum values outside the catalog.

Verification on 2026-09-24: the Release solution build passed with 12 existing warnings and no errors. The Release solution test run passed all 393 cases, including 165 Dashboard HTTP cases and 29 Dashboard unit cases. The default layout, all fifteen stylesheet choices, suggested font links, and invalid theme values are covered. A catalog check found fifteen entries matching all fifteen theme CSS sources. CSharpier formatted touched C# files and `git diff --check` passed. The first solution test run hit the sandbox's named-pipe socket restriction; the same command passed with local IPC access.
