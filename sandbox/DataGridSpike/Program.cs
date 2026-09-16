using StellarAdmin;
using StellarAdmin.Pro;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
// StellarAdmin.Pro opts out of automatic application part discovery; AddPro()
// registers its part (and the UI services), which the sa-data-grid tag helpers need.
builder.Services.AddStellarAdmin().AddPro();

var app = builder.Build();

// Spike 7: exercise URL generation under a sub-path (requests to /admin/... get a PathBase).
app.UsePathBase("/admin");
app.UseRouting();

app.MapRazorPages();

app.Run();
