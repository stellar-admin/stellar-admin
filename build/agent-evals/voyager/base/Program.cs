using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Voyager.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<VoyagerDbContext>(options => options.UseSqlite("Data Source=voyager.db"));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();
builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    SeedData.Initialize(scope.ServiceProvider.GetRequiredService<VoyagerDbContext>());
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
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
