using Microsoft.Extensions.FileProviders;
using StellarAdmin;
using StellarAdmin.TagHelpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddStellarAdmin().AddTagHelpers();

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
        FileProvider = new PhysicalFileProvider(
            Path.GetFullPath(
                Path.Combine(
                    app.Environment.ContentRootPath,
                    "..",
                    "..",
                    "util",
                    "theme-check",
                    "presets"
                )
            )
        ),
        RequestPath = "/presets",
    }
);

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
