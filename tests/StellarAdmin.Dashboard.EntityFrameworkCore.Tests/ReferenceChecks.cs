using System.Collections.Concurrent;
using System.Data.Common;
using System.Net;
using System.Text.RegularExpressions;
using IdentitySimplePlayground.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

internal static class ReferenceChecks
{
    public static ReferenceCommands Commands { get; } = new();

    public static async Task Run(HttpClient client, ApplicationDbContext db)
    {
        var alpha = new Category { Name = "Reference Alpha" };
        var zulu = new Category { Name = "Reference Zulu" };
        var hidden = new Category { Name = "Hidden category" };
        db.Categories.AddRange(zulu, alpha, hidden);
        await db.SaveChangesAsync();

        var createHtml = await client.GetStringAsync("/admin/test-references/Create");
        Require(
            createHtml.Contains("Choose the category that best describes this product."),
            "Reference metadata help"
        );
        Require(
            createHtml.Contains("Not set") && !createHtml.Contains("Hidden category"),
            "Nullable reference and filtered choices"
        );
        Require(
            createHtml.IndexOf("Reference Alpha", StringComparison.Ordinal)
                < createHtml.IndexOf("Reference Zulu", StringComparison.Ordinal),
            "Choices sorted by display label"
        );
        var token = Token(createHtml);

        foreach (var invalid in new[] { "99999999", hidden.Id.ToString(), "not-an-id" })
        {
            var response = await Post(
                client,
                "/admin/test-references/Create",
                token,
                "Rejected",
                "REF-INVALID",
                invalid
            );
            Require(response.StatusCode == HttpStatusCode.OK, "Invalid reference redisplays");
            Require(
                (await response.Content.ReadAsStringAsync()).Contains("Reference Alpha"),
                "Invalid POST reloads choices"
            );
            Require(!await db.Products.AnyAsync(), "Invalid reference is not saved");
        }

        var invalidOtherField = await Post(
            client,
            "/admin/test-references/Create",
            token,
            "",
            "REF-INVALID",
            zulu.Id.ToString()
        );
        Require(
            Selected(await invalidOtherField.Content.ReadAsStringAsync(), zulu.Id),
            "Invalid POST preserves selected key through ModelState"
        );

        Require(
            (
                await Post(
                    client,
                    "/admin/test-references/Create",
                    token,
                    "Zulu product",
                    "REF-Z",
                    zulu.Id.ToString()
                )
            ).StatusCode == HttpStatusCode.Redirect,
            "Create with reference"
        );
        Require(
            (
                await Post(
                    client,
                    "/admin/test-references/Create",
                    token,
                    "Alpha product",
                    "REF-A",
                    alpha.Id.ToString()
                )
            ).StatusCode == HttpStatusCode.Redirect,
            "Second create with reference"
        );
        var product = await db.Products.AsNoTracking().SingleAsync(p => p.Sku == "REF-Z");
        Require(product.CategoryId == zulu.Id, "Foreign key persists");

        Commands.Sql.Clear();
        var index = await client.GetStringAsync("/admin/test-references?SortBy=CategoryId");
        Require(
            index.IndexOf("Alpha product", StringComparison.Ordinal)
                < index.IndexOf("Zulu product", StringComparison.Ordinal),
            "Reference sorting uses labels, not keys"
        );
        Require(
            index.Contains("Reference Alpha") && index.Contains("Reference Zulu"),
            "Index renders included labels"
        );
        var statements = Commands.Sql.ToArray();
        Require(
            statements.Length == 2 && statements.Any(sql => sql.Contains("JOIN \"Categories\"")),
            "Index uses count and included rows only"
        );
        Require(
            !statements.Any(sql => sql.Contains("FROM \"Categories\"")),
            "No index lookup queries"
        );

        Commands.Sql.Clear();
        var editUrl = $"/admin/test-references/Edit/{product.Id}";
        var edit = await client.GetStringAsync(editUrl);
        Require(Selected(edit, zulu.Id), "Edit selects saved reference");
        statements = Commands.Sql.ToArray();
        Require(
            statements.Length == 2 && statements.Any(sql => sql.Contains("JOIN \"Categories\"")),
            "Edit includes assigned label plus one choice query"
        );
        Require(
            statements
                .Single(sql => sql.Contains("FROM \"Categories\""))
                .Contains("SELECT \"c\".\"Id\", \"c\".\"Name\""),
            "Choices project only key and label"
        );
        token = Token(edit);

        Require(
            (await Post(client, editUrl, token, "Zulu product", "REF-Z", "")).StatusCode
                == HttpStatusCode.Redirect,
            "Nullable reference clears"
        );
        Require(
            (await db.Products.AsNoTracking().SingleAsync(p => p.Id == product.Id)).CategoryId
                is null,
            "Cleared key persists"
        );

        Require(
            (
                await Post(client, editUrl, token, "Zulu product", "REF-Z", alpha.Id.ToString())
            ).StatusCode == HttpStatusCode.Redirect,
            "Reference can change"
        );
        Require(
            (await db.Products.AsNoTracking().SingleAsync(p => p.Id == product.Id)).CategoryId
                == alpha.Id,
            "Changed key persists"
        );

        await db
            .Products.Where(p => p.Id == product.Id)
            .ExecuteUpdateAsync(set => set.SetProperty(p => p.CategoryId, hidden.Id));
        edit = await client.GetStringAsync(editUrl);
        Require(
            Selected(edit, hidden.Id) && edit.Contains("Hidden category"),
            "Filtered current label stays visible from navigation"
        );
        var rejectedEdit = await Post(
            client,
            editUrl,
            Token(edit),
            "Changed name",
            "REF-Z",
            hidden.Id.ToString()
        );
        Require(
            rejectedEdit.StatusCode == HttpStatusCode.OK
                && (await rejectedEdit.Content.ReadAsStringAsync()).Contains(
                    "Select a valid option."
                ),
            "Filtered selection is rejected on edit"
        );
        Require(
            (await db.Products.AsNoTracking().SingleAsync(p => p.Id == product.Id)).Name
                == "Zulu product",
            "Invalid edit does not save other changes"
        );

        var requiredForm = await client.GetStringAsync("/admin/test-required-reference/Create");
        Require(
            requiredForm.Contains("Select an option") && !requiredForm.Contains("Not set"),
            "Required reference has a prompt"
        );
        var requiredToken = Token(requiredForm);
        var requiredResponse = await Post(
            client,
            "/admin/test-required-reference/Create",
            requiredToken,
            "Required",
            "REF-R",
            ""
        );
        Require(
            requiredResponse.StatusCode == HttpStatusCode.OK
                && (await requiredResponse.Content.ReadAsStringAsync()).Contains(
                    "Select a valid option."
                ),
            "EF-required reference rejects empty selection"
        );
        Require(
            (
                await Post(
                    client,
                    "/admin/test-required-reference/Create",
                    requiredToken,
                    "Required",
                    "REF-R",
                    alpha.Id.ToString()
                )
            ).StatusCode == HttpStatusCode.Redirect,
            "Required reference accepts valid selection"
        );
        var required = await db.Products.AsNoTracking().SingleAsync(p => p.Sku == "REF-R");
        var requiredEditUrl = $"/admin/test-required-reference/Edit/{required.Id}";
        var readonlyForm = await client.GetStringAsync(requiredEditUrl);
        Require(
            Regex.IsMatch(readonlyForm, "<select[^>]*disabled[^>]*>")
                && Selected(readonlyForm, alpha.Id),
            "Read-only reference renders disabled with selected label"
        );
        Require(
            (
                await Post(
                    client,
                    requiredEditUrl,
                    Token(readonlyForm),
                    "Required updated",
                    "REF-R",
                    zulu.Id.ToString()
                )
            ).StatusCode == HttpStatusCode.Redirect,
            "Read-only reference permits other changes"
        );
        Require(
            (await db.Products.AsNoTracking().SingleAsync(p => p.Id == required.Id)).CategoryId
                == alpha.Id,
            "Forged read-only reference is ignored"
        );

        Commands.Sql.Clear();
        await client.GetStringAsync("/admin/test-required-reference");
        Require(
            Commands.Sql.Count == 2 && !Commands.Sql.Any(sql => sql.Contains("Categories")),
            "Unused references do not load index data"
        );

        await db.Products.ExecuteDeleteAsync();
        await db
            .Categories.Where(c => c.Id == alpha.Id || c.Id == zulu.Id || c.Id == hidden.Id)
            .ExecuteDeleteAsync();
        db.ChangeTracker.Clear();

        Console.WriteLine("All EF reference rendering, persistence, and query checks passed.");
    }

    private static Task<HttpResponseMessage> Post(
        HttpClient client,
        string url,
        string token,
        string name,
        string sku,
        string category
    )
    {
        return client.PostAsync(
            url,
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["__RequestVerificationToken"] = token,
                    ["Entity.Name"] = name,
                    ["Entity.Sku"] = sku,
                    ["Entity.CategoryId"] = category,
                }
            )
        );
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static bool Selected(string html, int key) =>
        Regex
            .Matches(html, "<option[^>]*>")
            .Select(match => match.Value)
            .Any(option => option.Contains($"value=\"{key}\"") && option.Contains("selected"));

    private static string Token(string html) =>
        WebUtility.HtmlDecode(
            Regex
                .Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"")
                .Groups[1]
                .Value
        );
}

internal sealed class ReferenceCommands : DbCommandInterceptor
{
    public ConcurrentQueue<string> Sql { get; } = new();

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default
    )
    {
        Sql.Enqueue(command.CommandText);

        return ValueTask.FromResult(result);
    }
}

internal sealed class ReferenceTestDbContext(DbContextOptions<ReferenceTestDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().ToTable("Products");
        modelBuilder.Entity<Category>().ToTable("Categories");
    }
}

internal sealed class RequiredReferenceTestDbContext(
    DbContextOptions<RequiredReferenceTestDbContext> options
) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().ToTable("Products");
        modelBuilder
            .Entity<Product>()
            .HasOne(product => product.Category)
            .WithMany()
            .HasForeignKey(product => product.CategoryId)
            .IsRequired();
        modelBuilder.Entity<Category>().ToTable("Categories");
    }
}
