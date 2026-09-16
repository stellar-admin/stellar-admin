using System.Net;
using System.Text.RegularExpressions;
using IdentitySimplePlayground.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin;
using StellarAdmin.Pro;
using StellarAdmin.Pro.EntityFrameworkCore;

var database = Path.Combine(Path.GetTempPath(), $"stellar-ef-tests-{Guid.NewGuid():N}.db");
Environment.SetEnvironmentVariable(
    "ConnectionStrings__DefaultConnection",
    $"Data Source={database}"
);

try
{
    await using var factory = new WebApplicationFactory<Category>().WithWebHostBuilder(builder =>
    {
        builder.UseContentRoot(
            Path.GetFullPath(
                "../../../../../sandbox/IdentitySimplePlayground",
                AppContext.BaseDirectory
            )
        );

        builder.ConfigureServices(services =>
        {
            services.AddDataProtection().UseEphemeralDataProtectionProvider();

            services.AddDbContext<RequiredReferenceTestDbContext>(options =>
                options
                    .UseSqlite($"Data Source={database}")
                    .AddInterceptors(ReferenceChecks.Commands)
            );

            services.AddDbContext<ReferenceTestDbContext>(options =>
                options
                    .UseSqlite($"Data Source={database}")
                    .AddInterceptors(ReferenceChecks.Commands)
            );

            services
                .AddControllersWithViews()
                .AddApplicationPart(typeof(EditorTestController).Assembly);

            services.AddAuthorization(options =>
                options.AddPolicy("TestAdmin", policy => policy.RequireAuthenticatedUser())
            );

            services
                .AddStellarAdmin()
                .AddPro(pro =>
                {
                    pro.AddEfCoreResource<RequiredReferenceTestDbContext, Product>(
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

                    pro.AddEfCoreResource<ReferenceTestDbContext, Product>(
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

                    pro.AddEfCoreResource<ApplicationDbContext, ApplicationRole>(
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

                    pro.AddEfCoreResource<ApplicationDbContext, ApplicationUser>(
                        "test-protected",
                        resource => resource.AuthorizationPolicy = "TestAdmin"
                    );
                });
        });
    });

    using var client = factory.CreateClient(
        new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost"),
        }
    );

    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.GetService<IMigrator>().MigrateAsync("00000000000000_CreateIdentitySchema");

    var existingRole = new ApplicationRole { Name = "Existing", NormalizedName = "EXISTING" };
    db.Roles.Add(existingRole);
    await db.SaveChangesAsync();

    await db.Database.MigrateAsync();
    Check(
        !db.Database.HasPendingModelChanges(),
        "Fluent EF configuration preserves the migration model"
    );

    var metadataProvider = scope.ServiceProvider.GetRequiredService<IModelMetadataProvider>();
    foreach (var entityType in new[] { typeof(Category), typeof(Product) })
    {
        foreach (var property in metadataProvider.GetMetadataForProperties(entityType))
        {
            Check(
                !string.IsNullOrWhiteSpace(property.DisplayName)
                    && !string.IsNullOrWhiteSpace(property.Description),
                $"{entityType.Name}.{property.PropertyName} has a metadata label and help text"
            );
        }
    }

    Check(
        await db.Roles.AnyAsync(role => role.Id == existingRole.Id),
        "Migration preserves existing Identity data"
    );

    var index = await client.GetAsync("/admin/categories");
    Check(index.StatusCode == HttpStatusCode.OK, "Index renders");
    Check(
        (await index.Content.ReadAsStringAsync()).Contains("Categories"),
        "Configured title renders"
    );
    Check(
        (await client.GetAsync("/admin/Users")).StatusCode == HttpStatusCode.OK,
        "Identity users still render"
    );
    Check(
        (await client.GetAsync("/admin/Roles")).StatusCode == HttpStatusCode.OK,
        "Identity roles still render"
    );

    Check(
        (await client.GetAsync("/admin/test-roles")).StatusCode == HttpStatusCode.OK,
        "Second EF resource has independent routes and configuration"
    );

    var roleToken = await Token($"/admin/test-roles/Edit/{existingRole.Id}");
    Check(
        (
            await Post(
                $"/admin/test-roles/Edit/{existingRole.Id}",
                roleToken,
                ("Entity.Name", "Changed"),
                ("Entity.NormalizedName", "TAMPERED")
            )
        ).StatusCode == HttpStatusCode.Redirect,
        "String-key edit succeeds"
    );
    Check(
        (await db.Roles.AsNoTracking().SingleAsync()).NormalizedName == "EXISTING",
        "Read-only field is excluded from binding"
    );

    foreach (var path in new[] { "", "/Create", "/Edit/1" })
    {
        var denied = await client.GetAsync("/admin/test-protected" + path);
        Check(
            denied.StatusCode == HttpStatusCode.Redirect
                && denied.Headers.Location!.ToString().Contains("Login"),
            "Resource policy protects GET " + path
        );
    }

    foreach (var path in new[] { "/Create", "/Edit/1", "/Delete/1" })
    {
        var denied = await Post("/admin/test-protected" + path, roleToken);
        Check(
            denied.StatusCode == HttpStatusCode.Redirect
                && denied.Headers.Location!.ToString().Contains("Login"),
            "Resource policy protects POST " + path
        );
    }

    var token = await Token("/admin/categories/Create");
    var categoryForm = await client.GetStringAsync("/admin/categories/Create");
    Check(
        categoryForm.Contains("Category name")
            && categoryForm.Contains("Explain what kinds of products belong in this category."),
        "Category metadata renders labels and help text"
    );
    Check(
        Regex.IsMatch(categoryForm, """<textarea[^>]*name="Entity.Description" """.Trim()),
        "Category metadata selects the multiline editor"
    );

    var tooLong = await Post(
        "/admin/categories/Create",
        token,
        ("Entity.Name", new string('n', 101)),
        ("Entity.Description", new string('d', 501))
    );
    Check(
        tooLong.StatusCode == HttpStatusCode.OK && await db.Categories.CountAsync() == 0,
        "Category metadata enforces maximum lengths"
    );

    var invalid = await Post("/admin/categories/Create", token, ("Entity.Name", ""));
    Check(
        invalid.StatusCode == HttpStatusCode.OK
            && (await invalid.Content.ReadAsStringAsync()).Contains("required"),
        "Validation redisplays"
    );
    Check(await db.Categories.CountAsync() == 0, "Invalid create does not persist");

    Check(
        (await Post("/admin/categories/Create", null, ("Entity.Name", "Rejected"))).StatusCode
            == HttpStatusCode.BadRequest,
        "Antiforgery enforced"
    );

    Check(
        (
            await Post(
                "/admin/categories/Create",
                token,
                ("Entity.Name", "Books"),
                ("Entity.Description", "Read me"),
                ("Entity.Id", "999")
            )
        ).StatusCode == HttpStatusCode.Redirect,
        "Create redirects"
    );
    var category = await db.Categories.AsNoTracking().SingleAsync();
    Check(
        category.Name == "Books" && category.Id != 999,
        "Create persists only configured properties"
    );

    token = await Token($"/admin/categories/Edit/{category.Id}");
    Check(
        (
            await Post(
                $"/admin/categories/Edit/{category.Id}",
                token,
                ("Entity.Name", "Edited"),
                ("Entity.Id", "999")
            )
        ).StatusCode == HttpStatusCode.Redirect,
        "Edit redirects"
    );
    Check((await db.Categories.AsNoTracking().SingleAsync()).Name == "Edited", "Edit persists");

    Check(
        (await client.GetAsync("/admin/categories/Edit/not-a-key")).StatusCode
            == HttpStatusCode.NotFound,
        "Invalid key returns 404"
    );
    Check(
        (await client.GetAsync("/admin/categories/Edit/999999")).StatusCode
            == HttpStatusCode.NotFound,
        "Missing record returns 404"
    );

    db.Categories.AddRange(
        Enumerable.Range(0, 30).Select(i => new Category { Name = $"Item {i:D2}" })
    );
    await db.SaveChangesAsync();

    var page = await (
        await client.GetAsync("/admin/categories?PageSize=5&PageNo=999&SortBy=Name&SortDir=desc")
    ).Content.ReadAsStringAsync();
    Check(
        page.Contains("Edited") && !page.Contains("Item 29"),
        "Sorting and last-page clamping execute in EF"
    );

    var search = await (
        await client.GetAsync("/admin/categories?Search=Item%2029")
    ).Content.ReadAsStringAsync();
    Check(search.Contains("Item 29") && !search.Contains("Item 28"), "Search executes in EF");

    Check(
        (await Post($"/admin/categories/Delete/{category.Id}", null)).StatusCode
            == HttpStatusCode.BadRequest,
        "Delete antiforgery enforced"
    );

    Check(
        (await Post($"/admin/categories/Delete/{category.Id}", token)).StatusCode
            == HttpStatusCode.Redirect,
        "Delete redirects"
    );
    Check(!await db.Categories.AnyAsync(c => c.Id == category.Id), "Delete persists");

    await ReferenceChecks.Run(client, db);

    await EditorChecks.Run(client);
    FormLayoutChecks.CheckConfiguration();

    var formsOptions = factory
        .Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<StellarAdminFormsOptions>>()
        .Value;
    var originalLayout = formsOptions.SectionLayout;
    try
    {
        foreach (var editForm in new[] { false, true })
        {
            foreach (
                var (app, form, expected) in new[]
                {
                    ("Split", "", "split"),
                    ("Card", "", "card"),
                    ("Card", "Stacked", "stacked"),
                    ("Card", "Split", "split"),
                    ("Split", "Card", "card"),
                }
            )
            {
                formsOptions.SectionLayout = Enum.Parse<StellarAdmin.TagHelpers.FormSectionLayout>(
                    app
                );
                var html = await client.GetStringAsync(
                    $"/test-form-layout?edit={editForm}&form={form}"
                );
                Check(
                    html.Contains($"data-layout=\"{expected}\""),
                    "Form > app cascade on create and edit inside a group"
                );
                Check(
                    html.Contains($"data-form-layout=\"{expected}\""),
                    "Form and sections share the effective layout"
                );
                Check(
                    html.Contains("name=\"Entity.Name\"") && html.Contains("name=\"Entity.Sku\""),
                    "Layout cascade preserves row field binding"
                );
            }
        }
    }
    finally
    {
        formsOptions.SectionLayout = originalLayout;
    }

    var productForm = await client.GetStringAsync("/admin/products/Create");
    FormLayoutChecks.CheckProductMarkup(productForm);
    FormLayoutChecks.CheckActions(productForm, "/admin/products", false);
    foreach (var route in new[] { "users", "roles" })
    {
        FormLayoutChecks.CheckActions(
            await client.GetStringAsync($"/admin/{route}/Create"),
            $"/admin/{route}",
            false
        );
    }
    FormLayoutChecks.CheckActions(
        await client.GetStringAsync($"/admin/roles/Edit/{existingRole.Id}"),
        "/admin/roles",
        true
    );
    var layoutUser = new ApplicationUser
    {
        UserName = "layout@example.com",
        Email = "layout@example.com",
    };
    db.Users.Add(layoutUser);
    await db.SaveChangesAsync();
    FormLayoutChecks.CheckActions(
        await client.GetStringAsync($"/admin/users/Edit/{layoutUser.Id}"),
        "/admin/users",
        true
    );
    FormLayoutChecks.CheckCondition(productForm, "New");
    Check(
        productForm.Contains("Compare-at price")
            && productForm.Contains("The cost to purchase or produce one unit.")
            && productForm.Contains("Record stock levels and inventory preferences.")
            && productForm.Contains("Describe shipping requirements and physical measurements.")
            && productForm.Contains("Record the publication status and date."),
        "Product metadata and section descriptions render"
    );
    var createdAtControl = Regex
        .Match(productForm, """<input[^>]*name="Entity.CreatedAt"[^>]*>""")
        .Value;
    Check(createdAtControl.Contains("readonly"), "Configured read-only date renders read-only");

    var productToken = await Token("/admin/products/Create");
    var invalidProduct = await Post(
        "/admin/products/Create",
        productToken,
        ("Entity.Name", "Invalid"),
        ("Entity.Sku", ""),
        ("Entity.Price", "-1"),
        ("Entity.Condition", "Used")
    );
    FormLayoutChecks.CheckProductMarkup(await invalidProduct.Content.ReadAsStringAsync());
    FormLayoutChecks.CheckCondition(await invalidProduct.Content.ReadAsStringAsync(), "Used");

    Check(
        invalidProduct.StatusCode == HttpStatusCode.OK && await db.Products.CountAsync() == 0,
        "Product rejects missing SKU and negative price"
    );

    var invalidCondition = await Post(
        "/admin/products/Create",
        productToken,
        ("Entity.Name", "Invalid condition"),
        ("Entity.Sku", "INVALID"),
        ("Entity.Condition", "999")
    );
    Check(
        invalidCondition.StatusCode == HttpStatusCode.OK && await db.Products.CountAsync() == 0,
        "Undefined enum value is rejected"
    );

    var createdProduct = await Post(
        "/admin/products/Create",
        productToken,
        ("Entity.Name", "Desk lamp"),
        ("Entity.Sku", "LAMP-001"),
        ("Entity.Description", "A reading lamp"),
        ("Entity.Condition", "Refurbished"),
        ("Entity.Price", "29.95"),
        ("Entity.CompareAtPrice", "39.95"),
        ("Entity.CostPrice", "12.50"),
        ("Entity.StockQuantity", "15"),
        ("Entity.TrackInventory", "true"),
        ("Entity.AllowBackorders", "false"),
        ("Entity.RequiresShipping", "true"),
        ("Entity.WeightKg", "1.25"),
        ("Entity.WidthCm", "12.5"),
        ("Entity.HeightCm", "40"),
        ("Entity.DepthCm", "15"),
        ("Entity.IsPublished", "true"),
        ("Entity.PublishedAt", "2026-09-08T12:00:00"),
        ("Entity.CreatedAt", "2000-01-01"),
        ("Entity.CategoryId", (await db.Categories.OrderBy(c => c.Id).FirstAsync()).Id.ToString())
    );
    Check(createdProduct.StatusCode == HttpStatusCode.Redirect, "Product create succeeds");

    var product = await db.Products.AsNoTracking().SingleAsync();
    Check(product.Condition == ProductCondition.Refurbished, "Product condition persists");
    FormLayoutChecks.CheckCondition(
        await client.GetStringAsync($"/admin/products/Edit/{product.Id}"),
        "Refurbished"
    );
    FormLayoutChecks.CheckProductMarkup(
        await client.GetStringAsync($"/admin/products/Edit/{product.Id}")
    );
    Check(
        product.Price == 29.95m
            && product.WeightKg == 1.25m
            && product.StockQuantity == 15
            && product.IsPublished
            && product.PublishedAt.HasValue,
        "Product decimal, integer, boolean, and date fields persist"
    );
    Check(
        product.CategoryId is not null && product.CreatedAt.Year != 2000,
        "Product reference persists and creation timestamp stays read-only"
    );

    var productIndex = await client.GetAsync("/admin/products?Search=LAMP-001&Scope=published");
    Check(
        productIndex.StatusCode == HttpStatusCode.OK
            && (await productIndex.Content.ReadAsStringAsync()).Contains("Desk lamp"),
        "Product index searches SKU and renders the published scope"
    );

    FormLayoutChecks.CheckActions(
        await client.GetStringAsync($"/admin/products/Edit/{product.Id}"),
        "/admin/products",
        true
    );
    productToken = await Token($"/admin/products/Edit/{product.Id}");
    var editedProduct = await Post(
        $"/admin/products/Edit/{product.Id}",
        productToken,
        ("Entity.Name", "Updated lamp"),
        ("Entity.Condition", "Used"),
        ("Entity.Sku", "LAMP-001"),
        ("Entity.CompareAtPrice", ""),
        ("Entity.WeightKg", ""),
        ("Entity.PublishedAt", "")
    );
    Check(editedProduct.StatusCode == HttpStatusCode.Redirect, "Product edit succeeds");

    var updatedProduct = await db.Products.AsNoTracking().SingleAsync();
    Check(
        updatedProduct.Name == "Updated lamp"
            && updatedProduct.Condition == ProductCondition.Used
            && updatedProduct.CompareAtPrice is null
            && updatedProduct.WeightKg is null
            && updatedProduct.PublishedAt is null,
        "Optional product fields can be cleared"
    );

    Check(
        (await Post($"/admin/products/Delete/{product.Id}", productToken)).StatusCode
            == HttpStatusCode.Redirect
            && !await db.Products.AnyAsync(),
        "Product delete persists"
    );

    Console.WriteLine("All EF resource integration checks passed.");

    async Task<string> Token(string path)
    {
        var response = await client.GetAsync(path);
        Check(response.StatusCode == HttpStatusCode.OK, $"Form renders: {path}");

        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Check(match.Success, "Antiforgery token renders");

        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    Task<HttpResponseMessage> Post(
        string path,
        string? token,
        params (string Key, string Value)[] fields
    )
    {
        var values = fields.Select(f => new KeyValuePair<string, string>(f.Key, f.Value)).ToList();
        if (token is not null)
        {
            values.Add(new("__RequestVerificationToken", token));
        }

        return client.PostAsync(path, new FormUrlEncodedContent(values));
    }
}
finally
{
    Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
    File.Delete(database);
}

static void Check(bool condition, string description)
{
    if (!condition)
    {
        throw new InvalidOperationException(description);
    }

    Console.WriteLine($"PASS: {description}");
}
