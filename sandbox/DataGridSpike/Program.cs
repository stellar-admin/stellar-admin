using StellarAdmin;
using StellarAdmin.TagHelpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddStellarAdmin().AddTagHelpers();

var app = builder.Build();

// Spike 7: exercise URL generation under a sub-path (requests to /admin/... get a PathBase).
app.UsePathBase("/admin");
app.UseRouting();

app.MapRazorPages();

app.Run();
