using IdentitySimplePlayground.Data;
using IdentitySimplePlayground.Forms;
using Microsoft.EntityFrameworkCore;
using StellarAdmin;
using StellarAdmin.Pro;
using StellarAdmin.Pro.EntityFrameworkCore;
using StellarAdmin.Pro.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder
    .Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder
    .Services.AddStellarAdmin()
    .AddPro(pro =>
    {
        pro.AddStylesheet("~/css/admin.css");

        pro.AddIdentity<ApplicationUser, ApplicationRole>(identityBuilder =>
        {
            identityBuilder.ConfigureUsers(users =>
            {
                users.Index(index =>
                {
                    index.EnableSearch(
                        (q, term) =>
                            q.Where(u => u.UserName!.Contains(term) || u.Email!.Contains(term)),
                        search => search.Placeholder = "Search name or e-mail..."
                    );

                    index.Scopes(scopes =>
                    {
                        scopes.Add("All");
                        scopes.Add("Unconfirmed", u => u.EmailConfirmed == false);
                    });
                });
            });
            identityBuilder.ConfigureRoles(roles =>
            {
                roles.Index(index =>
                {
                    index.EnableSearch((q, term) => q.Where(r => r.Name!.Contains(term)));
                });
            });
            /*identityBuilder.ConfigureSidebar(sidebar =>
            {
                sidebar.Title = "User management";
            });*/
            /*identityBuilder.ConfigureUsers(users =>
            {
                users.Index(index =>
                {
                    index.Title = "Team members";
                    index.Subtitle = "People who can sign in to your application";

                    index.DefaultSortBy(u => u.Email);
                    index.TransformQuery(q => q.Include(u => u.Roles));

                    index.EnableSearch(
                        (q, term) =>
                            q.Where(u => u.UserName!.Contains(term) || u.Email!.Contains(term)),
                        search => search.Placeholder = "Search name or e-mail..."
                    );

                    index.Scopes(scopes =>
                    {
                        scopes.Add("All");
                        scopes.Add("Unconfirmed", u => u.EmailConfirmed == false);
                    });

                    index.Columns(columns =>
                    {
                        columns.Clear();
                        columns.Add(u => u.Email).Title("E-mail address").Sortable();
                        columns.Add(u => u.UserName).Sortable();
                        columns.Add(u => u.Roles);
                        columns.Add(u => u.PhoneNumber);
                        columns.Add(u => u.AccessFailedCount).Format("{0} attempts").Sortable();
                        columns.Add(u => u.EmailConfirmed).Sortable();
                    });
                });

                users.Create(create =>
                {
                    create.Subtitle = "Add a new team member";
                    create.CreateInstanceUsing(() => new ApplicationUser { EmailConfirmed = true });
                    create.Fields(fields =>
                    {
                        fields.Add(u => u.PhoneNumber);
                    });
                });

                users.Edit(edit =>
                {
                    edit.Title = "Edit team member";
                    edit.Fields(fields =>
                    {
                        fields.Clear();
                        fields.Add(u => u.UserName);
                        fields.Add(u => u.Email);
                        fields.Add(u => u.PhoneNumber).Title("Mobile number");
                        fields.Add(u => u.LockoutEnd).ReadOnly();
                    });
                });
            });*/
        });

        pro.AddEfCoreResource<ApplicationDbContext, Category>(
            "categories",
            resource =>
            {
                resource.Index(index =>
                {
                    index.Title = "Categories";

                    index.DefaultSortBy(category => category.Name);

                    index.EnableSearch(
                        (query, term) => query.Where(category => category.Name.Contains(term))
                    );

                    index.Columns(columns =>
                    {
                        columns.Add(category => category.Name).Sortable();
                        columns.Add(category => category.Description);
                    });
                });

                resource.Create(create =>
                    create.Fields(fields =>
                    {
                        fields.Add(category => category.Name);
                        fields.Add(category => category.Description);
                    })
                );

                resource.Edit(edit =>
                    edit.Fields(fields =>
                    {
                        fields.Add(category => category.Name);
                        fields.Add(category => category.Description);
                    })
                );
            }
        );

        pro.AddEfCoreResource<ApplicationDbContext, Product>(
            "products",
            resource =>
            {
                resource.AddReference(
                    product => product.CategoryId,
                    product => product.Category,
                    category => category.Name
                );

                resource.Index(index =>
                {
                    index.Title = "Products";

                    index.DefaultSortBy(product => product.Name);

                    index.EnableSearch(
                        (query, term) =>
                            query.Where(product =>
                                product.Name.Contains(term) || product.Sku.Contains(term)
                            )
                    );

                    index.Scopes(scopes =>
                    {
                        scopes.Add("All");
                        scopes.Add("Published", product => product.IsPublished);
                        scopes.Add("Draft", product => !product.IsPublished);
                    });

                    index.Columns(columns =>
                    {
                        columns.Add(product => product.Name).Sortable();
                        columns.Add(product => product.Sku).Sortable();
                        columns.Add(product => product.CategoryId).Sortable();
                        columns.Add(product => product.Price);
                        columns.Add(product => product.StockQuantity).Sortable();
                        columns.Add(product => product.IsPublished).Sortable();
                    });
                });

                resource.Create(create => create.Fields(ProductForm.Configure));
                resource.Edit(edit => edit.Fields(ProductForm.Configure));
            }
        );
    });

var app = builder.Build();

// await FakeUserCreator.CreateFakeUsers(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapStellarAdmin("/admin");

app.Run();
