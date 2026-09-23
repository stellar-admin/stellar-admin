using DashboardPlayground.Data;
using DashboardPlayground.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StellarAdmin;
using StellarAdmin.Dashboard;
using StellarAdmin.Dashboard.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources.Editors;

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

// Keep the demo catalog separate from the existing Identity database.
builder.Services.AddSingleton(_ =>
{
    var connection = new SqliteConnection("Data Source=:memory:");
    connection.Open();
    return connection;
});
builder.Services.AddDbContext<ProductDbContext>(
    (services, options) => options.UseSqlite(services.GetRequiredService<SqliteConnection>())
);
builder.Services.AddSingleton<CustomerDataSource>();
builder.Services.AddScoped<CategoryLookupProvider>();
builder
    .Services.AddStellarAdmin()
    .AddDashboard(dashboard =>
    {
        dashboard.ConfigureResourceLabels(labels =>
        {
            labels.CreateTitle = resource => $"Add new {resource.SingularLabel}";
            labels.CreateSubmitLabel = resource => $"Add {resource.SingularLabel}";
        });

        dashboard.AddResource<Customer>(resource =>
        {
            resource.UseDataSource<CustomerDataSource>();
            resource.AllowDelete();
            resource.UseKey(customer => customer.Id);
            resource.Index(index =>
                index.Columns(columns =>
                {
                    columns.Add(customer => customer.Name);
                    columns.Add(customer => customer.Email);
                })
            );
            resource.AllowCreate<CreateCustomerModel, CreateCustomerHandler>(create =>
                create.Fields(fields =>
                {
                    fields.AddSection(
                        "Customer details",
                        section =>
                            section.AddRow(row =>
                            {
                                row.Add(model => model.Name);
                                row.Add(model => model.Email);
                            })
                    );
                    fields.AddSection(
                        "Password",
                        section =>
                            section.AddRow(row =>
                            {
                                row.Add(model => model.Password);
                                row.Add(model => model.PasswordConfirmation);
                            })
                    );
                })
            );
            resource.AllowEdit<EditCustomerModel, EditCustomerHandler>(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(model => model.DisplayName);
                    fields.Add(model => model.Email);
                })
            );
        });

        dashboard.AddEfCoreResource<ProductDbContext, Product>(resource =>
        {
            resource.AllowCreate(create =>
                create.Fields(fields =>
                {
                    fields.Add(product => product.Name);
                    fields.Add(product => product.Price);
                    fields.Add(product => product.Details.Sku);
                    fields.Add(product => product.CategoryId, field =>
                    {
                        field.Title = "Category";
                        field.UseEditor<ReferenceLookupEditor>(editor =>
                            editor.UseLookup<CategoryLookupProvider>()
                        );
                    });
                })
            );
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(product => product.Name);
                    fields.Add(product => product.Price);
                    fields.Add(product => product.Details.Sku);
                    fields.Add(product => product.CategoryId, field =>
                    {
                        field.Title = "Category";
                        field.UseEditor<ReferenceLookupEditor>(editor =>
                            editor.UseLookup<CategoryLookupProvider>()
                        );
                    });
                })
            );
            resource.AllowDelete();
            resource.Index(index =>
            {
                index.EnableScopes(scopes =>
                {
                    scopes.Add("all", "All products");
                    scopes.Add("under-50", "Under 50", product => product.Price < 50);
                    scopes.Add("50-and-over", "50 and over", product => product.Price >= 50);
                    scopes.DefaultScope = "all";
                });
                index.EnableSearch(
                    term => product => product.Name.Contains(term),
                    search => search.Placeholder = "Search products..."
                );
                index.DefaultSortBy(product => product.Name);
                index.EnablePaging(paging =>
                {
                    paging.PageSize = 10;
                    paging.PageSizes = [10, 25, 50];
                });
                index.Columns(columns =>
                {
                    columns.Add(product => product.Id);
                    columns.Add(product => product.Details.Sku, column => column.Sortable());
                    columns.Add(product => product.CategoryId, column => column.Sortable());
                    columns.Add(
                        product => product.Name,
                        column => column.Sortable(product => product.Name.ToLower())
                    );
                    columns.Add(
                        product => product.Price,
                        column =>
                        {
                            column.Sortable();
                            column.Title = "Unit price";
                            column.Format = "{0:0.00}";
                        }
                    );
                });
            });
        });
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    await db.Database.EnsureCreatedAsync();
    db.Categories.AddRange(
        new Category { Id = 1, Name = "Stationery" },
        new Category { Id = 2, Name = "Lighting" },
        new Category { Id = 3, Name = "Drinkware" }
    );
    db.Products.AddRange(ProductSeed.Create());
    await db.SaveChangesAsync();
}

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
