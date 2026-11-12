using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using StellarAdmin;
using VoyagerTravel.Data;
using VoyagerTravel.Data.Seeding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite("Data Source=voyager-travel.db"));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();
builder.Services.AddRazorPages();
builder.Services.AddStellarAdmin();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (db.Database.EnsureCreated())
    {
        DatabaseSeeder.Seed(db);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapStellarAdmin("/admin");
app.MapRazorPages().WithStaticAssets();

// Development sign-in: /dev-login?name=sam&roles=Sales,Finance
app.MapGet("/dev-login", async (HttpContext http, string name, string? roles) =>
{
    var claims = new List<Claim> { new(ClaimTypes.Name, name) };
    claims.AddRange((roles ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(role => new Claim(ClaimTypes.Role, role)));
    await http.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
    return Results.Redirect("/");
});
app.MapGet("/dev-logout", async (HttpContext http) =>
{
    await http.SignOutAsync();
    return Results.Redirect("/");
});

app.Run();
