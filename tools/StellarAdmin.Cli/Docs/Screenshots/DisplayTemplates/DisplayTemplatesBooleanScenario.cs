using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.DisplayTemplates;

// The trips grid with the Boolean grid template overridden. The override lives in the host's
// Views/DisplayTemplatesBoolean folder, which only this scenario searches, so other scenarios keep the built-in one.
public sealed class DisplayTemplatesBooleanScenario : Scenario
{
    public override string Description =>
        "The trips grid with the published flag shown as a badge by an overridden Boolean grid template.";

    public override string StartPath => "/stellaradmin/trips";

    public override void ConfigureServices(IServiceCollection services) =>
        services.Configure<RazorViewEngineOptions>(options =>
            options.AreaViewLocationFormats.Insert(0, "/Areas/{2}/Views/DisplayTemplatesBoolean/{0}.cshtml")
        );

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(trip => trip.Title, column => column.Sortable());
                    columns.Add(trip => trip.TripType);
                    columns.Add(trip => trip.DurationDays);
                    columns.Add(trip => trip.IsPublished);
                });
                index.DefaultSortBy(trip => trip.Title);
                // Five rows, so the grid crop stays short
                index.EnablePaging(paging => paging.PageSize = 5);
            })
        );
    }
}
