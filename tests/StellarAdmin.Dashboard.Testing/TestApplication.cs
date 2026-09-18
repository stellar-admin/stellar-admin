using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using IdentitySimplePlayground.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using StellarAdmin.Dashboard.EntityFrameworkCore;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Testing;

public sealed class TestApplication : IAsyncDisposable
{
    private readonly string _database = Path.Combine(
        Path.GetTempPath(),
        $"stellar-ef-tests-{Guid.NewGuid():N}.db"
    );
    private readonly WebApplicationFactory<Category> _factory;
    private readonly IServiceScope _scope;

    public HttpClient Client { get; }
    public ReferenceCommands Commands { get; } = new();
    public ApplicationDbContext Db { get; }
    public IServiceProvider Services => _scope.ServiceProvider;

    public TestApplication(FormSectionLayout layout = FormSectionLayout.Split)
    {
        _factory = new WebApplicationFactory<Category>().WithWebHostBuilder(builder =>
        {
            builder.UseContentRoot(
                Path.GetFullPath(
                    "../../../../../sandbox/IdentitySimplePlayground",
                    AppContext.BaseDirectory
                )
            );
            builder.UseEnvironment("Development");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureServices(services =>
            {
                services.AddDataProtection().UseEphemeralDataProtectionProvider();
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite($"Data Source={_database};Pooling=False")
                );
                services.Configure<StellarAdminFormsOptions>(options =>
                    options.SectionLayout = layout
                );
                services.AddDbContext<RequiredReferenceTestDbContext>(options =>
                    options
                        .UseSqlite($"Data Source={_database};Pooling=False")
                        .AddInterceptors(Commands)
                );

                services.AddDbContext<ReferenceTestDbContext>(options =>
                    options
                        .UseSqlite($"Data Source={_database};Pooling=False")
                        .AddInterceptors(Commands)
                );

                services
                    .AddControllersWithViews()
                    .AddApplicationPart(typeof(EditorTestController).Assembly);

                services.AddAuthorization(options =>
                    options.AddPolicy("TestAdmin", policy => policy.RequireAuthenticatedUser())
                );

                services
                    .AddStellarAdmin()
                    .AddDashboard(dashboard =>
                    {
                        dashboard.AddEfCoreResource<RequiredReferenceTestDbContext, Product>(
                            "test-required-reference",
                            resource =>
                            {
                                resource.AddReference(
                                    product => product.CategoryId,
                                    product => product.Category,
                                    category => category.Name
                                );

                                resource.Index(index =>
                                    index.Columns(columns => columns.Add(product => product.Name))
                                );
                                resource.Create(create =>
                                    create.Fields(fields =>
                                    {
                                        fields.Add(product => product.Name);
                                        fields.Add(product => product.Sku);
                                        fields.Add(product => product.CategoryId);
                                    })
                                );
                                resource.Edit(edit =>
                                    edit.Fields(fields =>
                                    {
                                        fields.Add(product => product.Name);
                                        fields.Add(product => product.Sku);
                                        fields.Add(product => product.CategoryId).ReadOnly();
                                    })
                                );
                            }
                        );

                        dashboard.AddEfCoreResource<ReferenceTestDbContext, Product>(
                            "test-references",
                            resource =>
                            {
                                resource.AddReference(
                                    product => product.CategoryId,
                                    product => product.Category,
                                    category => category.Name,
                                    reference =>
                                        reference.Choices(choices =>
                                            choices.TransformQuery(query =>
                                                query.Where(category =>
                                                    category.Name.StartsWith("Reference ")
                                                )
                                            )
                                        )
                                );

                                resource.Index(index =>
                                {
                                    index.DefaultSortBy(product => product.CategoryId);
                                    index.Columns(columns =>
                                    {
                                        columns.Add(product => product.Name);
                                        columns.Add(product => product.CategoryId).Sortable();
                                    });
                                });

                                resource.Create(create =>
                                    create.Fields(fields =>
                                    {
                                        fields.Add(product => product.Name);
                                        fields.Add(product => product.Sku);
                                        fields.Add(product => product.CategoryId);
                                    })
                                );

                                resource.Edit(edit =>
                                    edit.Fields(fields =>
                                    {
                                        fields.Add(product => product.Name);
                                        fields.Add(product => product.Sku);
                                        fields.Add(product => product.CategoryId);
                                    })
                                );
                            }
                        );

                        dashboard.AddEfCoreResource<ApplicationDbContext, ApplicationRole>(
                            "test-roles",
                            resource =>
                            {
                                resource.Index(index =>
                                    index.Columns(columns => columns.Add(role => role.Name))
                                );

                                resource.Edit(edit =>
                                    edit.Fields(fields =>
                                    {
                                        fields.AddSection(
                                            "Role details",
                                            section =>
                                            {
                                                section.Fields(fields =>
                                                {
                                                    fields.Add(role => role.Name);
                                                    fields.AddGroup(group =>
                                                        group.Fields(fields =>
                                                        {
                                                            fields
                                                                .Add(role => role.NormalizedName)
                                                                .ReadOnly();
                                                        })
                                                    );
                                                });
                                            }
                                        );
                                    })
                                );
                            }
                        );

                        dashboard.AddEfCoreResource<ApplicationDbContext, ApplicationUser>(
                            "test-protected",
                            resource => resource.AuthorizationPolicy = "TestAdmin"
                        );
                    });
            });
        });
        Client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost"),
            }
        );
        _scope = _factory.Services.CreateScope();
        Db = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    public static async Task<TestApplication> CreateAsync()
    {
        var app = new TestApplication();
        try
        {
            await app.Db.Database.MigrateAsync();
            return app;
        }
        catch
        {
            await app.DisposeAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        _scope.Dispose();
        await _factory.DisposeAsync();
        File.Delete(_database);
        File.Delete(_database + "-wal");
        File.Delete(_database + "-shm");
    }

    public async Task<IHtmlDocument> GetHtmlAsync(string path)
    {
        using var response = await Client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        return new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync());
    }

    public async Task<string> GetTokenAsync(string path)
    {
        using var html = await GetHtmlAsync(path);
        return html.QuerySelector("input[name=__RequestVerificationToken]")?.GetAttribute("value")
            ?? throw new InvalidOperationException($"No antiforgery token in {path}.");
    }

    public async Task<HttpResponseMessage> PostAsync(
        string path,
        string? token,
        params (string Key, string Value)[] fields
    )
    {
        var values = fields
            .Select(field => new KeyValuePair<string, string>(field.Key, field.Value))
            .ToList();
        if (token is not null)
        {
            values.Add(new("__RequestVerificationToken", token));
        }
        using var content = new FormUrlEncodedContent(values);
        return await Client.PostAsync(path, content);
    }
}
