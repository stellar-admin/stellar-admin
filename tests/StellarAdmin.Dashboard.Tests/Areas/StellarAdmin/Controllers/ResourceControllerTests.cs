using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Dashboard.Areas.StellarAdmin.Controllers;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Tests.Areas.StellarAdmin.Controllers;

public class ResourceControllerTests
{
    [Test]
    [Arguments(true, "Index")]
    [Arguments(false, "ResourceIndex")]
    public async Task Index_WithViewLookup_SelectsOverrideOrDefault(
        bool indexExists,
        string expectedView
    )
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddStellarAdmin().AddDashboard().AddResource<Product>();
        using var provider = services.BuildServiceProvider();
        var sut = new ResourceController<Product>(
            new ProductDataSource(),
            provider.GetRequiredService<IOptions<ResourceOptions<Product>>>(),
            new TestViewEngine(indexExists)
        );

        // Act
        var result = (ViewResult)await sut.Index(CancellationToken.None);

        // Assert
        await Assert.That(result.ViewName).IsEqualTo(expectedView);
    }

    public sealed record Product(int Id);

    private sealed class ProductDataSource : IResourceDataSource<Product>
    {
        public Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Product>>([]);
        }
    }

    private sealed class TestViewEngine(bool indexExists) : ICompositeViewEngine
    {
        public IReadOnlyList<IViewEngine> ViewEngines => [];

        public ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage)
        {
            return indexExists && viewName == "Index" && isMainPage
                ? ViewEngineResult.Found(viewName, new TestView())
                : ViewEngineResult.NotFound(viewName, []);
        }

        public ViewEngineResult GetView(string? executingFilePath, string viewPath, bool isMainPage)
        {
            return ViewEngineResult.NotFound(viewPath, []);
        }
    }

    private sealed class TestView : IView
    {
        public string Path => "/Areas/StellarAdmin/Views/Product/Index.cshtml";

        public Task RenderAsync(ViewContext context)
        {
            return Task.CompletedTask;
        }
    }
}
