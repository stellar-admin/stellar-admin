using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace StellarAdmin.Sidebar;

internal sealed class BrandingResolver(StellarAdminOptions options, IHostEnvironment environment)
{
    private const string FallbackTitle = "Admin";

    public string Title =>
        options.Branding.Title ?? (field ??= TitleFromApplicationName(environment.ApplicationName));

    public static string TitleFromApplicationName(string? applicationName)
    {
        // Voyager.Admin becomes "Voyager Admin", and AdminPlayground "Admin Playground"
        var words = (applicationName ?? "")
            .Split(['.', '_', '-', ' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Humanize(LetterCasing.Title))
            .Where(part => part.Length > 0)
            .ToArray();

        return words.Length == 0 ? FallbackTitle : string.Join(' ', words);
    }

    public SidebarBrandModel CreateBrand(IUrlHelper url)
    {
        var branding = options.Branding;
        var href = branding.Href is { } path
            ? url.Content(path)
            : url.Action("Index", "Home", new { area = "StellarAdmin" }) ?? "/";

        return new(Title, branding.Subtitle, branding.Logo, href);
    }
}
