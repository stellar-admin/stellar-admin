namespace StellarAdmin.ScenarioTesting.Host;

// A plain host application with a page, a controller and static files. The scenario hosts add the
// database, sign-in, the scenario's services and the admin panel.
public sealed class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        var app = builder.Build();
        app.MapStaticAssets();
        app.MapRazorPages();
        app.MapDefaultControllerRoute();
        app.Run();
    }
}
