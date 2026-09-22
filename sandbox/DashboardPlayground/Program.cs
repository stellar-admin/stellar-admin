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
builder.Services.AddSingleton<CustomerDataSource>();
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

        dashboard.AddResource<Product>(resource =>
        {
            resource.UseDataSource<ProductDataSource>();
            resource.AllowDelete();
            resource.UseKey(product => product.Id);
            resource.AllowCreate(create =>
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
            resource.AllowEdit(edit =>
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
                index.EnableSearch();
                index.EnableScopes(scopes =>
                {
                    scopes.Add("all", "All products");
                    scopes.Add("under-50", "Under 50");
                    scopes.Add("50-and-over", "50 and over");
                    scopes.DefaultScope = "all";
                });
                index.DefaultSortBy(product => product.Name);
                index.EnablePaging(paging =>
                {
                    paging.PageSize = 10;
                    paging.PageSizes = [10, 25, 50];
                });
                index.Columns(columns =>
                {
                    columns.Add(product => product.Id);
                    columns.Add(product => product.Name, column => column.Sortable = true);
                    columns.Add(
                        product => product.Price,
                        column =>
                        {
                            column.Sortable = true;
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
