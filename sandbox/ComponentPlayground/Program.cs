using DocsSamples;
using Microsoft.Extensions.FileProviders;
using StellarAdmin;
using StellarAdmin.TagHelpers;

var builder = WebApplication.CreateBuilder(args);
var repoRoot = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", ".."));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddStellarAdmin().AddTagHelpers();
builder.Services.AddSingleton(ThemeFixtures.Load(repoRoot));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// The theme fixtures for the theme picker, served from the repository (util/theme-check/presets).
app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(ThemeFixtures.Folder(repoRoot)),
        RequestPath = "/presets",
    }
);

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
