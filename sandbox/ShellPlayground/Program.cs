using ShellPlayground;
using ShellPlayground.Sidebar;
using StellarAdmin;
using StellarAdmin.Shell;
using StellarAdmin.Shell.Sidebar;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddStellarAdmin().AddShell();
builder.Services.AddTransient<ISidebarItemsProvider, SidebarItemsProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapStellarAdmin("/admin");

app.Run();
