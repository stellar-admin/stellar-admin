using DashboardPlayground.Data;
using DashboardPlayground.Models;
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
    .Services.AddDefaultIdentity<IdentityUser>(options =>
        options.SignIn.RequireConfirmedAccount = true
    )
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ProductDataSource>();
builder
    .Services.AddStellarAdmin()
    .AddDashboard(dashboard =>
    {
        dashboard.ConfigureResourceLabels(labels =>
        {
            labels.CreateTitle = resource => $"Add new {resource.SingularLabel}";
            labels.CreateSubmitLabel = resource => $"Add {resource.SingularLabel}";
        });

        dashboard.AddResource<Product>(resource =>
        {
            resource.UseDataSource<ProductDataSource>();
            resource.UseKey(product => product.Id);
            resource.Create(create =>
            {
                create.UseFactory(() => new Product { Price = 10m });
                create.Fields(fields =>
                {
                    fields.AddSection(
                        "Product details",
                        section =>
                        {
                            section.Description =
                                "The name and pricing shown in the product catalog.";
                            section.AddRow(row =>
                            {
                                row.AddGroup(group => group.Add(product => product.Name));
                                row.AddGroup(group => group.Add(product => product.Price));
                            });
                        }
                    );
                });
            });
            resource.Edit(edit =>
                edit.Fields(fields =>
                    fields.AddSection(
                        "Product details",
                        section =>
                            section.AddRow(row =>
                            {
                                row.Add(product => product.Name);
                                row.Add(product => product.Price);
                            })
                    )
                )
            );
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(product => product.Id);
                    columns.Add(product => product.Name);
                    columns.Add(
                        product => product.Price,
                        column =>
                        {
                            column.Title = "Unit price";
                            column.Format = "{0:0.00}";
                        }
                    );
                });
            });
        });
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
