using AdminPlayground.Dashboard;
using AdminPlayground.Data;
using AdminPlayground.Resources.Amenities;
using AdminPlayground.Resources.Categories;
using AdminPlayground.Resources.Customers;
using AdminPlayground.Resources.Departments;
using AdminPlayground.Resources.Destinations;
using AdminPlayground.Resources.Excursions;
using AdminPlayground.Resources.FieldEditors;
using AdminPlayground.Resources.FormLayouts;
using AdminPlayground.Resources.Guides;
using AdminPlayground.Resources.Products;
using AdminPlayground.Resources.Roles;
using AdminPlayground.Resources.Tags;
using AdminPlayground.Resources.Trips;
using AdminPlayground.Resources.Users;
using Microsoft.EntityFrameworkCore;
using StellarAdmin;
using StellarAdmin.Sidebar;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder
    .Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<CustomerDataSource>();
builder.Services.AddStellarAdmin(admin =>
{
    admin.AddStylesheet(
        "https://fonts.googleapis.com/css2?family=Inter:wght@400..700&display=swap"
    );
    admin.AddStylesheet("~/css/playground.css");

    admin.ConfigureBranding(branding =>
    {
        branding.Title = "Voyager Travel";
        branding.Subtitle = "Admin Playground";
        branding.Logo = BrandingLogo.Icon("compass");
    });

    // Log in through Identity UI to see the user menu; Log out signs out of the Identity cookie
    admin.ConfigureUserMenu(menu =>
    {
        menu.Add(
                UserMenuItem.Page(
                    "Account",
                    "/Account/Manage/Index",
                    "Identity",
                    item => item.Icon = "badge-check"
                )
            )
            .Add(UserMenuItem.Separator());
    });

    admin.ConfigureResourceLabels(labels =>
    {
        labels.Create(create =>
        {
            create.Title = resource => $"Add new {resource.SingularLabel}";
            create.SubmitLabel = resource => $"Add {resource.SingularLabel}";
        });
    });

    admin.AddHomeDashboard();
    admin.AddUserResource();
    admin.AddRoleResource();
    admin.AddDepartmentResource();
    admin.AddCustomerResource();
    admin.AddDestinationResource();
    admin.AddTripResource();
    admin.AddGuideResource();
    admin.AddAmenityResource();
    admin.AddProductResource();
    admin.AddCategoryResource();
    admin.AddTagResource();
    admin.AddFieldEditorGallery();
    admin.AddFormLayoutGallery();
    admin.AddMisconfiguredExcursionResource();

    admin.AddSidebarLink(
        "Sales report",
        SidebarLinkTarget.Page("/Reports"),
        link =>
        {
            link.Group = "Commerce";
            link.Icon = "chart-line";
            link.Order = 30;
        }
    );
    admin.AddSidebarLink("Privacy policy", SidebarLinkTarget.Action("Privacy", "Home"));
    admin.AddSidebarLink(
        "ASP.NET Core docs",
        SidebarLinkTarget.Url("https://learn.microsoft.com/aspnet/core/"),
        link => link.OpenInNewTab = true
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapStellarAdmin();

// Serves the images uploaded to the playground's image fields
app.MapGet(
    "/playground-uploads/{name}",
    (string name, PlaygroundImageStore store) =>
        store.GetPath(name) is { } path
            ? Results.File(path, PlaygroundImageStore.GetContentType(path))
            : Results.NotFound()
);

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages().WithStaticAssets();

app.Run();
