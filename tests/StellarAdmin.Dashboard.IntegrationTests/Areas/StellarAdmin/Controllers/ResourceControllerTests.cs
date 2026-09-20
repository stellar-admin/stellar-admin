using System.ComponentModel.DataAnnotations;
using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Builders;

namespace StellarAdmin.Dashboard.IntegrationTests.Areas.StellarAdmin.Controllers;

public partial class ResourceControllerTests
{
    [Test]
    public async Task Index_AcrossRequests_ResolvesDataSourceFromEachRequestScope()
    {
        // Arrange
        var state = new ProductState([]);
        await using var sut = await CreateHost(state);
        using var client = sut.GetTestClient();

        // Act
        using var first = await client.GetAsync("/stellaradmin/Product");
        using var second = await client.GetAsync("/stellaradmin/Product");

        // Assert
        await Assert.That(first.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(second.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(state.Requests.Count).IsEqualTo(2);
        await Assert.That(state.Requests[0]).IsNotEqualTo(state.Requests[1]);
    }

    [Test]
    public async Task Index_WithEmptyDataSource_RendersEmptyStateAndColumnHeaders()
    {
        // Arrange
        await using var sut = await CreateHost(new([]));
        using var client = sut.GetTestClient();

        // Act
        var html = await client.GetStringAsync("/stellaradmin/Product");
        var document = await new HtmlParser().ParseDocumentAsync(html);

        // Assert
        await Assert
            .That(document.QuerySelector("[data-slot='empty-title']")?.TextContent.Trim())
            .IsEqualTo("No records found");
        await Assert
            .That(
                document
                    .QuerySelectorAll("thead th")
                    .Select(element => element.TextContent.Trim())
                    .ToArray()
            )
            .IsEquivalentTo(["Product name", "Unit price"]);
    }

    [Test]
    [Arguments(null, "Inventory")]
    [Arguments("Available products", "Available products")]
    public async Task Index_WithLabelOverrides_RendersEffectiveTitleAtStableRoute(
        string? title,
        string expected
    )
    {
        // Arrange
        await using var sut = await CreateHost(
            new([]),
            resource =>
            {
                resource.PluralLabel = "Inventory";
                resource.Index(index => index.Title = title);
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var html = await client.GetStringAsync("/stellaradmin/Product");
        var document = await new HtmlParser().ParseDocumentAsync(html);

        // Assert
        await Assert
            .That(document.QuerySelector("[data-slot='page-header-title']")?.TextContent.Trim())
            .IsEqualTo(expected);
    }

    [Test]
    public async Task Index_WithProducts_RendersConfiguredColumnsAndEncodedValues()
    {
        // Arrange
        await using var sut = await CreateHost(
            new([new(1, "<script>alert('test')</script>", 12.5m)])
        );
        using var client = sut.GetTestClient();

        // Act
        var html = await client.GetStringAsync("/stellaradmin/Product");
        var document = await new HtmlParser().ParseDocumentAsync(html);

        // Assert
        await Assert
            .That(document.QuerySelector("[data-slot='page-header-title']")?.TextContent.Trim())
            .IsEqualTo("Products");
        await Assert.That(document.QuerySelectorAll("tbody tr").Length).IsEqualTo(1);
        await Assert
            .That(document.QuerySelector("tbody td")?.TextContent.Trim())
            .IsEqualTo("<script>alert('test')</script>");
        await Assert.That(document.QuerySelector("tbody script")).IsNull();
        await Assert
            .That(document.QuerySelectorAll("tbody td")[1].TextContent.Trim())
            .IsEqualTo("12.50");
        await Assert
            .That(
                document.QuerySelector("a[href*='/Edit'], [hx-post], [data-slot='data-grid-pager']")
            )
            .IsNull();
    }

    [Test]
    public async Task Index_WithUnregisteredResource_ReturnsNotFound()
    {
        // Arrange
        await using var sut = await CreateHost(new([]));
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/Unknown");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    private static async Task<WebApplication> CreateHost(
        ProductState state,
        Action<ResourceBuilder<Product>>? configure = null
    )
    {
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions
            {
                ApplicationName = typeof(ResourceControllerTests).Assembly.GetName().Name,
                EnvironmentName = "Development",
            }
        );
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(state);
        builder
            .Services.AddStellarAdmin()
            .AddDashboard(dashboard =>
                dashboard.AddResource<Product>(resource =>
                {
                    resource.UseDataSource<ProductDataSource>();
                    resource.Index(index =>
                        index.Columns(columns =>
                        {
                            columns.Add(product => product.Name);
                            columns.Add(
                                product => product.Price,
                                column =>
                                {
                                    column.Title = "Unit price";
                                    column.Format = "{0:0.00}";
                                }
                            );
                        })
                    );
                    resource.Create(create =>
                        create.Fields(fields =>
                        {
                            fields.Add(product => product.Name);
                            fields.Add(product => product.Price);
                        })
                    );
                    configure?.Invoke(resource);
                })
            );

        var app = builder.Build();
        app.MapStellarAdmin();
        await app.StartAsync();

        return app;
    }

    public sealed class Product
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Product name")]
        public string Name { get; set; } = "";

        [Range(typeof(decimal), "0.01", "1000000")]
        public decimal Price { get; set; }

        public Product() { }

        public Product(int id, string name, decimal price)
        {
            Id = id;
            Name = name;
            Price = price;
        }
    }

    public sealed class ProductDataSource(ProductState state) : IResourceDataSource<Product>
    {
        private readonly Guid _id = Guid.NewGuid();

        public Task CreateAsync(Product resource, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            state.SubmittedId = resource.Id;
            resource.Id = state.Products.Count + 1;
            state.Products.Add(resource);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            state.Requests.Add(_id);

            return Task.FromResult<IReadOnlyList<Product>>(state.Products);
        }
    }

    public sealed class ProductState(IReadOnlyList<Product> products)
    {
        public List<Product> Products { get; } = [.. products];
        public List<Guid> Requests { get; } = [];
        public int? SubmittedId { get; set; }
    }
}
