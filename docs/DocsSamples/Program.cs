using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.FileProviders;
using StellarAdmin.UI;
using StellarAdmin.UI.Icons;

namespace DocsSamples;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var repoRoot = Path.GetFullPath(
            Path.Combine(builder.Environment.ContentRootPath, "..", "..")
        );

        // Add services to the container.
        builder.Services.AddSingleton<IUrlHelperFactory, DemoUrlHelperFactory>(
            _ => new DemoUrlHelperFactory(new UrlHelperFactory())
        );
        builder.Services.AddRazorPages();
        builder.Services.AddSingleton(ThemeFixtures.Load(repoRoot));
        builder.Services.AddStellarAdminUI(ui =>
        {
            ui.AddIconPack<VoyagerIconPack>();
            ui.AddIconPack<LucideIconPack>(pack =>
            {
                pack.Prefix = "preview-lucide-";
                pack.ImportSemanticMappings = false;
            });
            ui.AddIconPack<TablerOutlineIconPack>(pack =>
            {
                pack.Prefix = "preview-tabler-outline-";
                pack.ImportSemanticMappings = false;
            });
            ui.AddIconPack<TablerFilledIconPack>(pack =>
            {
                pack.Prefix = "preview-tabler-filled-";
                pack.ImportSemanticMappings = false;
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        // The theme fixtures for the theme picker, served from the repository (util/theme-check/presets).
        app.UseStaticFiles(
            new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(ThemeFixtures.Folder(repoRoot)),
                RequestPath = "/presets",
            }
        );

        app.UseRouting();

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapRazorPages().WithStaticAssets();

        app.Run();
    }
}

internal class DemoUrlHelperFactory(IUrlHelperFactory wrappedFactory) : IUrlHelperFactory
{
    public IUrlHelper GetUrlHelper(ActionContext context)
    {
        var urlHelper = wrappedFactory.GetUrlHelper(context);

        return new DemoUrlHelper(urlHelper);
    }
}

/// <summary>
/// Overrides URL generation for fictitious booking pages used in demo
/// </summary>
/// <param name="wrappedUrlHelper"></param>
internal class DemoUrlHelper(IUrlHelper wrappedUrlHelper) : IUrlHelper
{
    public string? Action(UrlActionContext actionContext)
    {
        if (
            new[] { "Booking", "Search", "User" }.Contains(
                actionContext.Controller,
                StringComparer.OrdinalIgnoreCase
            )
        )
        {
            return "#";
        }

        return wrappedUrlHelper.Action(actionContext);
    }

    [return: NotNullIfNotNull("contentPath")]
    public string? Content(string? contentPath)
    {
        return wrappedUrlHelper.Content(contentPath);
    }

    public bool IsLocalUrl([NotNullWhen(true)] string? url)
    {
        return wrappedUrlHelper.IsLocalUrl(url);
    }

    public string? RouteUrl(UrlRouteContext routeContext)
    {
        if (
            routeContext.Values is RouteValueDictionary routeValues
            && routeValues.TryGetValue("page", out var page)
            && page is string pageName
            && pageName.StartsWith("/Booking", StringComparison.OrdinalIgnoreCase)
        )
        {
            return "#";
        }

        return wrappedUrlHelper.RouteUrl(routeContext);
    }

    public string? Link(string? routeName, object? values)
    {
        return wrappedUrlHelper.Link(routeName, values);
    }

    public ActionContext ActionContext => wrappedUrlHelper.ActionContext;
}
