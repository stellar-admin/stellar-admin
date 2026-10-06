using DashboardPlayground.Data;
using DashboardPlayground.Resources.Categories;
using DashboardPlayground.Resources.Customers;
using DashboardPlayground.Resources.Departments;
using DashboardPlayground.Resources.FieldEditors;
using DashboardPlayground.Resources.Products;
using DashboardPlayground.Resources.Roles;
using DashboardPlayground.Resources.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StellarAdmin;
using StellarAdmin.Dashboard;

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
        dashboard.AddStylesheet("~/css/dashboard.css");

        dashboard.ConfigureTheme(theme =>
        {
            theme.Name = DashboardTheme.Parallax;
            theme.IncludeSuggestedFonts = true;
        });

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
