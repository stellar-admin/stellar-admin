using DashboardPlayground.Data;
using DashboardPlayground.Resources.Categories;
using DashboardPlayground.Resources.Customers;
using DashboardPlayground.Resources.Departments;
using DashboardPlayground.Resources.FieldEditors;
using DashboardPlayground.Resources.FormLayouts;
using DashboardPlayground.Resources.Products;
using DashboardPlayground.Resources.Roles;
using DashboardPlayground.Resources.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StellarAdmin;
using StellarAdmin.Dashboard;
using StellarAdmin.Dashboard.Sidebar;

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
builder
    .Services.AddStellarAdmin()
    .AddDashboard(dashboard =>
    {
        dashboard.AddStylesheet(
            "https://fonts.googleapis.com/css2?family=Inter:wght@400..700&display=swap"
        );
        dashboard.AddStylesheet("~/css/dashboard.css");

        dashboard.ConfigureResourceLabels(labels =>
        {
            labels.Create(create =>
            {
                create.Title = resource => $"Add new {resource.SingularLabel}";
                create.SubmitLabel = resource => $"Add {resource.SingularLabel}";
            });
        });

        dashboard.AddUserResource();
        dashboard.AddRoleResource();
        dashboard.AddDepartmentResource();
        dashboard.AddCustomerResource();
        dashboard.AddProductResource();
        dashboard.AddCategoryResource();
        dashboard.AddFieldEditorGallery();
        dashboard.AddFormLayoutGallery();

        dashboard.AddSidebarLink(
            "Sales report",
            SidebarLinkTarget.Page("/Reports"),
            link =>
            {
                link.Group = "Commerce";
                link.Order = 30;
            }
        );
        dashboard.AddSidebarLink("Privacy policy", SidebarLinkTarget.Action("Privacy", "Home"));
        dashboard.AddSidebarLink(
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

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages().WithStaticAssets();

app.Run();
